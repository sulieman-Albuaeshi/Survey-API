using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using DTOs;
using Entities;
using Microsoft.Data.SqlClient;
using SurveyDataAccessLayer.Interface;

namespace SurveyDataAccessLayer;

public class ResponseRepository : IResponseRepository
{
    public async Task<List<ResponseDto>> GetAllResponsesDetailsAsync()
    {   
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(@"
                        SELECT R.Id AS ResponseId, S.Title AS SurveyTitle, R.SubmittedAt,
                        Q.QuestionType, A.Id, Q.QuestionText,
                            IIF(A.AnswerValue is not null, A.AnswerValue, 
                            (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           )) AS AnswerValue
                         FROM Response R
                         INNER JOIN Answers A ON R.Id = A.ResponseId
                         join dbo.Surveys S on S.Id = R.SurveyId
                             JOIN dbo.Questions Q on Q.Id = A.QuestionId
                         where R.isActive = 1", conn);
        return await HandleSQLRetriver(conn, cmd);
    }

    public async Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(@"
                        SELECT R.Id AS ResponseId, S.Title AS SurveyTitle, R.SubmittedAt,
                        Q.QuestionType, A.Id, Q.QuestionText,
                        IIF(A.AnswerValue is not null, A.AnswerValue, 
                            (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           )) AS AnswerValue
                         FROM Response R
                         INNER JOIN Answers A ON R.Id = A.ResponseId
                         join dbo.Surveys S on S.Id = R.SurveyId
                              JOIN dbo.Questions Q on Q.Id = A.QuestionId
                         where R.SurveyId = @SurveyId and R.isActive = 1", conn);
        cmd.Parameters.AddWithValue("@SurveyId", surveyId);
        return await HandleSQLRetriver(conn, cmd);

    }

    public async Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(@"
                        SELECT R.Id AS ResponseId, S.Title AS SurveyTitle, R.SubmittedAt,
                        Q.QuestionType, A.Id, Q.QuestionText,
                        IIF(A.AnswerValue is not null, A.AnswerValue, 
                            (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           )) AS AnswerValue
                         FROM Response R
                         INNER JOIN Answers A ON R.Id = A.ResponseId
                         join dbo.Surveys S on S.Id = R.SurveyId
                              JOIN dbo.Questions Q on Q.Id = A.QuestionId
                         where R.UserId = @UserId and R.isActive = 1", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        return await HandleSQLRetriver(conn, cmd);     
    }
    
    public async Task<ResponseDto> GetResponseByIdAsync(int responseId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(@"
                        SELECT R.Id AS ResponseId, S.Title AS SurveyTitle, R.SubmittedAt,
                        Q.QuestionType, A.Id, Q.QuestionText,
                        IIF(A.AnswerValue is not null, A.AnswerValue, 
                            (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           )) AS AnswerValue
                         FROM Response R
                         INNER JOIN Answers A ON R.Id = A.ResponseId
                         join dbo.Surveys S on S.Id = R.SurveyId
                              JOIN dbo.Questions Q on Q.Id = A.QuestionId
                         where R.Id = @ResponseId and R.isActive = 1", conn);
        cmd.Parameters.AddWithValue("@ResponseId", responseId);
        var responses = await HandleSQLRetriver(conn, cmd);
        return responses.FirstOrDefault();
    }

    public async Task<ResponseCreateDto> CreateResponseAsync(ResponseCreateDto response)
    {
        using SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        await conn.OpenAsync();
        await using var tx =  (SqlTransaction) await conn.BeginTransactionAsync();
        try
        {
            using SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO RESPONSE (UserId, SurveyId, SubmittedAt, isActive)
                        VALUES (@UserId, @SurveyId, @SubmittedAt, @isActive)
                        SELECT SCOPE_IDENTITY();", conn, tx);
            cmd.Parameters.AddWithValue("@UserId", response.UserId);
            cmd.Parameters.AddWithValue("@SurveyId", response.SurveyId);
            cmd.Parameters.AddWithValue("@SubmittedAt",DateTime.UtcNow);
            cmd.Parameters.Add("@isActive", SqlDbType.Bit).Value = true;
            var responseId =  Convert.ToInt32(await cmd.ExecuteScalarAsync());

            if (responseId > 0)
            {
                response.ResponseId = responseId;
                foreach (var answer in response.Answers)
                {
                    using SqlCommand answerCmd = new SqlCommand(@"
                        INSERT INTO Answers (ResponseId, QuestionId, AnswerType, AnswerValue)
                        VALUES (@ResponseId, @QuestionId, @AnswerType, @AnswerValue);
                        SELECT SCOPE_IDENTITY();", conn, tx);
                    answerCmd.Parameters.AddWithValue("@ResponseId", responseId);
                    answerCmd.Parameters.AddWithValue("@QuestionId", answer.QuestionId);
                    answerCmd.Parameters.AddWithValue("@AnswerType", answer.AnswerType);
                    if (answer.AnswerType is QuestionType.Text or QuestionType.Rating or QuestionType.Radio )
                    {
                        answerCmd.Parameters.AddWithValue("@AnswerValue", answer.Value.ToString());
                    }
                    else
                    {
                        answerCmd.Parameters.AddWithValue("@AnswerValue", DBNull.Value);
                    }
                   
                    var answerId = await answerCmd.ExecuteScalarAsync();

                    if (answer.AnswerType is QuestionType.Checkbox)
                    {
                        foreach (var item in answer.Value.EnumerateArray())
                        {
                                var id = item.GetProperty("id").GetInt32();
                                var val = item.GetProperty("value").GetInt32();
                                using SqlCommand selectionCmd = new SqlCommand(@"
                                INSERT INTO AnswerSelections (AnswerId, ChoiceId, RankOrder)
                                VALUES (@AnswerId, @ChoiceId, @RankOrder);", conn, tx);
                                selectionCmd.Parameters.AddWithValue("@AnswerId", Convert.ToInt32(answerId));
                                selectionCmd.Parameters.AddWithValue("@RankOrder", id); 
                                selectionCmd.Parameters.AddWithValue("@ChoiceId", val);
                                await selectionCmd.ExecuteNonQueryAsync();
                            
                        }
                    }
                }
            }
            
            await tx.CommitAsync();
            return response;
            
        }
        catch (Exception e)
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<int> DeleteResponseAsync(int surveyId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(@"UPDATE Response SET isActive = 0 WHERE SurveyId = @surveyId", conn);
        cmd.Parameters.AddWithValue("@surveyId", surveyId);
        try
        {
            await conn.OpenAsync();
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            
            return rowsAffected;
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while deleting the response with surveyId " + surveyId, e);
        }
    }

    private async Task<List<ResponseDto>> HandleSQLRetriver(SqlConnection conn, SqlCommand cmd)
    {
        try
        {
            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            var responses = new Dictionary<int, ResponseDto>();
            while (await reader.ReadAsync())
            {
                var id =  reader.GetInt32(reader.GetOrdinal("ResponseId"));
                object? answerValue;
                var AnswerRow = reader.GetString(reader.GetOrdinal("AnswerValue"));
                try
                {
                    answerValue = JsonSerializer.Deserialize<object>(AnswerRow);
                }
                catch (JsonException e)
                {
                    answerValue = AnswerRow;
                }
                if (responses.TryGetValue(id, out var existingResponse))
                {
                    var answer = new AnswerQuestionDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        QuestionText= reader.GetString(reader.GetOrdinal("QuestionText")),
                        AnswerType = (QuestionType)reader.GetInt32(reader.GetOrdinal("QuestionType")),
                        Value = answerValue
                    };
                    existingResponse.Answers.Add(answer);
                }
                else
                {
                    responses.Add(id, new ResponseDto
                    {
                        ResponseId = id,
                        SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                        Title = reader.GetString(reader.GetOrdinal("SurveyTitle")),
                        Answers =
                        [
                            new AnswerQuestionDto
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                                AnswerType = (QuestionType)reader.GetInt32(reader.GetOrdinal("QuestionType")),
                                
                                Value = answerValue
                            }
                        ]
                    });
                }
              
            }
            return responses.Values.ToList();
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while retrieving responses for the survey.", e);
        }
    }
}