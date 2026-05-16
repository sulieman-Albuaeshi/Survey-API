using System.Data;
using System.Text.Json;
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
            while (await reader.ReadAsync())
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
                        : JsonSerializer.Deserialize<JsonElement>(reader.GetString(reader.GetOrdinal("SettingsJSON"))),
                    QuestionType = (QuestionType)reader.GetInt32(reader.GetOrdinal("QuestionType"))
                });
            }
            return list;
        }
        catch (Exception ex)
        {
          throw new Exception("Error fetching questions for survey id " + surveyId + ": " + ex.Message, ex);
        }
    }
    
    public async Task<Question?> GetQuestionByIdAsync(int id, int surveyid)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand("SELECT * FROM Questions where Id = @id and SurveyId = @surveyId", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@surveyId", surveyid);
        try
        { 
            await conn.OpenAsync();
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Question
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    SurveyId = reader.GetInt32(reader.GetOrdinal("SurveyId")),
                    QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                    IsRequired = reader.GetBoolean(reader.GetOrdinal("IsRequired")),
                    OrderIndex = reader.GetInt32(reader.GetOrdinal("OrderIndex")),
                    SettingsJSON = reader.IsDBNull(reader.GetOrdinal("SettingsJSON"))
                        ? null
                        : JsonSerializer.Deserialize<JsonElement>(reader.GetString(reader.GetOrdinal("SettingsJSON"))),
                    QuestionType = (QuestionType)reader.GetInt32(reader.GetOrdinal("QuestionType"))
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching question for id " + id, ex);
        }
    }

    public async Task<int> CreateQuestionAsync(int surveyid, Question que, IEnumerable<Choice> choices)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        await conn.OpenAsync();
        var tx = (SqlTransaction)await conn.BeginTransactionAsync();
        try{
            using var cmd = new SqlCommand(
                @"INSERT INTO Questions (SurveyId, QuestionText, IsRequired, OrderIndex, SettingsJSON, QuestionType) 
             VALUES (@SurveyId, @QuestionText, @IsRequired, @OrderIndex, @SettingsJSON, @QuestionType); 
             SELECT SCOPE_IDENTITY();", conn, tx);

            cmd.Parameters.AddWithValue("@SurveyId", surveyid);
            cmd.Parameters.AddWithValue("@QuestionText", que.QuestionText);
            cmd.Parameters.AddWithValue("@IsRequired", que.IsRequired);
            cmd.Parameters.AddWithValue("@OrderIndex", que.OrderIndex);
            cmd.Parameters.AddWithValue("@SettingsJSON", que.SettingsJSON == null? DBNull.Value : JsonSerializer.Serialize(que.SettingsJSON));
            cmd.Parameters.AddWithValue("@QuestionType", que.QuestionType);
            
            
            var newQuestionId =Convert.ToInt32(await cmd.ExecuteScalarAsync());;
            
            if(choices != null && choices.Any())
            {
                foreach(var choice in choices)
                {
                    using var choiceCmd = new SqlCommand(
                        @"INSERT INTO Choices (QuestionId, ChoiceText, OrderIndex) 
                          VALUES (@QuestionId, @ChoiceText, @OrderIndex);", conn, tx);
                    choiceCmd.Parameters.Add("@ChoiceText", SqlDbType.Text).Value = choice.ChoiceText;      
                    choiceCmd.Parameters.Add("@OrderIndex", SqlDbType.Int).Value = choice.OrderIndex;
                    choiceCmd.Parameters.Add("@QuestionId", SqlDbType.Int).Value = newQuestionId; // set the question id for the choice
                    
                    await choiceCmd.ExecuteNonQueryAsync();
                }
            }
            
            await tx.CommitAsync();
            return newQuestionId;
        }
        catch(Exception e)
        {
            await tx.RollbackAsync();
            throw new Exception("Error Creating  Question " , e);
        }
    }

    public async Task<int> UpdateQuestionAsync(Question que)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(
            @"UPDATE Questions SET QuestionText = @QuestionText,
                     IsRequired = @IsRequired, 
                     OrderIndex = @OrderIndex,
                     SettingsJSON = @SettingsJSON,
                     QuestionType = @QuestionType
              WHERE Id = @Id", conn);
        
        cmd.Parameters.AddWithValue("@QuestionText", que.QuestionText);
        cmd.Parameters.AddWithValue("@IsRequired", que.IsRequired);
        cmd.Parameters.AddWithValue("@OrderIndex", que.OrderIndex);
        cmd.Parameters.AddWithValue("@SettingsJSON", que.SettingsJSON == null? DBNull.Value : JsonSerializer.Serialize(que.SettingsJSON));
        cmd.Parameters.AddWithValue("@QuestionType", que.QuestionType);
        cmd.Parameters.AddWithValue("@Id", que.Id); 
        
       await conn.OpenAsync();
        var rowsAffected = await cmd.ExecuteNonQueryAsync(); 
        return rowsAffected;    
    }
    
    public async Task<bool> DeleteQuestionAsync(int id)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString()); 
        
        using var cmd = new SqlCommand("DELETE FROM Questions WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);
        
        await conn.OpenAsync();
        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}