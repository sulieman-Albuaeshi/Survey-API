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
                SELECT A.Id, Q.QuestionText, Q.QuestionType, 
                       value = CASE
                            WHEN A.AnswerType = 3 THEN CAST(A.TextValue AS NVARCHAR(100))
                            WHEN A.AnswerType = 6 THEN CAST(A.NumberValue AS NVARCHAR(100))
                            ELSE ( SELECT STRING_AGG(CAST(ChoiceId AS NVARCHAR(100)), ',') WITHIN GROUP (ORDER BY RankOrder)
                                   FROM AnswerSelections ASel
                                   WHERE ASel.AnswerId = A.Id ) 
                            END
                FROM Answers as A
                    join dbo.Response R2 on R2.Id = A.ResponseId
                    join dbo.Questions Q on Q.Id = A.QuestionId
                where R2.SurveyId  = @SurveyId and userId = @UserId" , SqlConnection);
        sqlcommand.Parameters.AddWithValue("@SurveyId", surveyId);
        sqlcommand.Parameters.AddWithValue("@UserId", userId); // Assuming you have a userId parameter

        await SqlConnection.OpenAsync();
        var reader = await sqlcommand.ExecuteReaderAsync(); 
        
        var answers = new List<AnswerQuestionDto>();
        while (reader.Read())
        {
            answers.Add(new AnswerQuestionDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                AnswerType = (QuestionType)reader.GetInt32(reader.GetOrdinal("QuestionType")),
                Value = reader.GetString(reader.GetOrdinal("Value"))
            });
        }

        return answers;
    }
}