using System.Text.Json;
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
                        CASE 
                           WHEN A.AnswerType = 3 THEN A.TextValue
                           when A.AnswerType = 6 THEN CAST(A.NumberValue AS NVARCHAR(50))
                           else (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           ) END AS AnswerValue
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
                        CASE 
                           WHEN A.AnswerType = 3 THEN A.TextValue
                           when A.AnswerType = 6 THEN CAST(A.NumberValue AS NVARCHAR(50))
                           else (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           ) END AS AnswerValue
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
                        CASE 
                           WHEN A.AnswerType = 3 THEN A.TextValue
                           when A.AnswerType = 6 THEN CAST(A.NumberValue AS NVARCHAR(50))
                           else (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                     ChoiceId AS [value]
                                 FROM AnswerSelections ASel
                                 WHERE ASel.AnswerId = A.Id
                                 ORDER BY RankOrder
                                 FOR JSON PATH
                           ) END AS AnswerValue
                         FROM Response R
                         INNER JOIN Answers A ON R.Id = A.ResponseId
                         join dbo.Surveys S on S.Id = R.SurveyId
                              JOIN dbo.Questions Q on Q.Id = A.QuestionId
                         where R.UserId = @UserId and R.isActive = 1", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        return await HandleSQLRetriver(conn, cmd);     
    }

    public async Task<int> CreateResponseAsync(Responses response)
    {
        throw new NotImplementedException();
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