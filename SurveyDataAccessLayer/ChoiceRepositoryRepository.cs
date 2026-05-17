using Entities;
using Microsoft.Data.SqlClient;
using SurveyDataAccessLayer.Interface;

namespace SurveyDataAccessLayer;

public class ChoiceRepository : IChoiceRepository
{
    public async Task<List<Choice>> GetChoicesByQuestionIdAsync(int questionId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand("SELECT * FROM Choices where QuestionId = @questionId", conn);
        cmd.Parameters.AddWithValue("@questionId", questionId);
        try
        {
            await conn.OpenAsync();
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            var list = new List<Choice>();
            while (await reader.ReadAsync())
            {
                list.Add(new Choice
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                    ChoiceText = reader.GetString(reader.GetOrdinal("ChoiceText")),
                    OrderIndex = reader.GetInt32(reader.GetOrdinal("OrderIndex"))
                });
            }
            return list;
        }
        catch (Exception e)
        {
            throw new Exception("Error fetching choices for question id " + questionId + ": " + e.Message, e);
        }
    }

    public async Task<bool> CreateChoiceAsync(Choice choice)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(
            @"INSERT INTO Choices (QuestionId, ChoiceText, OrderIndex) 
              VALUES (@questionId, @choiceText, @orderIndex)", conn);
        cmd.Parameters.AddWithValue("@questionId", choice.QuestionId);
        cmd.Parameters.AddWithValue("@choiceText", choice.ChoiceText);
        cmd.Parameters.AddWithValue("@orderIndex", choice.OrderIndex);
        try
        {
            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected != 0;
        }
        catch (Exception e)
        {
            throw new Exception("Error creating choice for question id " + choice.QuestionId + ": " + e.Message, e);
        }
    }
    
    public async Task<bool> UpdateChoiceAsync(Choice choice)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(
            @"UPDATE Choices SET ChoiceText = @choiceText,
                                        OrderIndex = @orderIndex
               WHERE Id = @id AND QuestionId = @questionId", conn);
        cmd.Parameters.AddWithValue("@choiceText", choice.ChoiceText);
         cmd.Parameters.AddWithValue("@orderIndex", choice.OrderIndex);
         cmd.Parameters.AddWithValue("@id", choice.Id);
         cmd.Parameters.AddWithValue("@questionId", choice.QuestionId);
         try
         {
             await conn.OpenAsync();
             int rowsAffected = await cmd.ExecuteNonQueryAsync();
             if (rowsAffected != 0)
                 return true;
             
            throw new KeyNotFoundException("Choice not found for update");
         }
         catch (Exception e)
         {
             throw new Exception("Error updating choice with id " + choice.Id + ": " + e.Message, e);
         }
    }

    public async Task<bool> DeleteChoiceAsync(int id)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand("DELETE FROM Choices WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        try
        {
            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected != 0)
                return true;
            
            throw new KeyNotFoundException("Choice not found for deletion");
        }
        catch (Exception e)
        {
            throw new Exception("Error deleting choice with id " + id + ": " + e.Message, e);
        }
    }
}