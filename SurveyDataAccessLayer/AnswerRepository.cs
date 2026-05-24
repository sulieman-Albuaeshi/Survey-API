using System.Text.Json;
using DTOs;
using Entities;
using Microsoft.Data.SqlClient;
using SurveyDataAccessLayer.Interface;

namespace SurveyDataAccessLayer;

public class AnswerRepository : IAnswerRepository
{
    public async Task<List<AnswerQuestionDto>> GetAllAnswersBySurveyAsync(int surveyId, int userId)
    {
        await using var SqlConnection = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var sqlcommand = new SqlCommand(@"
                            SELECT A.Id, Q.QuestionText, A.AnswerType,
                                   value = IIF(A.AnswerValue is not null, A.AnswerValue,            
                                   (SELECT ROW_NUMBER() OVER (ORDER BY RankOrder) AS [id],
                                           ChoiceId AS [value]
                                    FROM AnswerSelections ASel
                                    WHERE ASel.AnswerId = A.Id
                                    ORDER BY RankOrder
                                    FOR JSON PATH
                                   ))
                            FROM Answers as A
                                     join dbo.Response R2 on R2.Id = A.ResponseId
                                     join dbo.Questions Q on Q.Id = A.QuestionId
                            where R2.SurveyId = @SurveyId  and userId = @UserId" , SqlConnection);
        sqlcommand.Parameters.AddWithValue("@SurveyId", surveyId);
        sqlcommand.Parameters.AddWithValue("@UserId", userId); // Assuming you have a userId parameter

        await SqlConnection.OpenAsync();
        var reader = await sqlcommand.ExecuteReaderAsync(); 
        
        var answers = new List<AnswerQuestionDto>();
        object AnswerValue = null;
        
        while (reader.Read())
        {
            var value = reader.GetString(reader.GetOrdinal("value"));
            var AnswerType = (QuestionType)reader.GetByte(reader.GetOrdinal("AnswerType"));
            try
            {
                if(AnswerType == QuestionType.Checkbox || AnswerType == QuestionType.Rank)
                {
                    AnswerValue = JsonSerializer.Deserialize<object>(value);
                }
                else
                {
                    AnswerValue = value;
                }
            }
            catch
            {
                AnswerValue = value;
            }
            answers.Add(new AnswerQuestionDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                AnswerType = AnswerType,
                Value = AnswerValue
            });
        }

        return answers;
    }
}