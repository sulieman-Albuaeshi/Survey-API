using Microsoft.Data.SqlClient;

namespace SurveyDataAccessLayer;

using SurveyDataAccessLayer.Interface;
using Entities;

public class QuestionRepository : IQuestionRepository
{
    public async Task<List<Question>> GetAllQuestionsAsync(int surveyId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand("SELECT * FROM Questions where SurveyId = @surveyid", conn);
        cmd.Parameters.AddWithValue("@surveyid", surveyId);
        try
        {
            await conn.OpenAsync();
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            var list = new List<Question>();
            while (reader.Read())
            {
                list.Add(new Question
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    SurveyId = reader.GetInt32(reader.GetOrdinal("SurveyId")),
                    QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                    IsRequired = reader.GetBoolean(reader.GetOrdinal("IsRequired")),
                    OrderIndex = reader.GetInt32(reader.GetOrdinal("OrderIndex")),
                    SettingsJSON = reader.IsDBNull(reader.GetOrdinal("SettingsJSON"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("SettingsJSON")),
                    QuestionType = (QuestionType)reader[reader.GetOrdinal("QuestionType")]
                });
            }
            return list;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching questions for survey id " + surveyId, ex);
        }
    }
    
    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateQuestionAsync(Question que)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdateQuestionAsync(Question que)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        throw new NotImplementedException();
    }
}