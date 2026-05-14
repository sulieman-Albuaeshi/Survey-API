namespace SurveyDataAccessLayer;

using Entities;
using Microsoft.Data.SqlClient;
using SurveyDataAccessLayer.Interface;

public class SurveyRepository : ISurveyRepository
{
    public async Task<List<Survey>> GetAllSurveysAsync()
    {
        // the DbHelperLocal is a hidden class 
        // you need to update the connection string in the DbHelper class to connect to your local database
        using SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, Status, CreatedAt  FROM Surveys", conn);
        await conn.OpenAsync();
        await using SqlDataReader reader = await cmd.ExecuteReaderAsync();
        List<Survey> surveys = new List<Survey>();
        while (reader.Read())
        {
            surveys.Add(new Survey
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description")),
                Status = (SurveyStatus)reader["Status"],
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                // add responses 
                // question count 
            });
        }
        return surveys;
    }
    
    public  async Task<Survey?> GetSurveyByIdAsync(int surveyId)
    {
        using SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, IsAnonymous, IsActive, Status, CreatedAt, UserId FROM Surveys WHERE Id = @Id",conn);
        
        cmd.Parameters.AddWithValue("@Id", surveyId);
        await conn.OpenAsync();
        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        {
            if (!reader.Read())
                return null;

            Survey survey = new Survey
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Description")),
                IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Status = (SurveyStatus)reader["Status"],
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UserId = reader.GetString(reader.GetOrdinal("UserId"))
            };
            return survey;
        }
            
        
    }

    public async Task<int> CreateSurveyAsync(Survey survey)
    {
        using SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString());

        using SqlCommand cmd = new SqlCommand(@"INSERT INTO Surveys (Title, Description, IsAnonymous, IsActive, CreatedAt, UserId, Status) 
                                VALUES (@Title, @Description, @IsAnonymous, @IsActive, @CreatedAt, @UserId, @Status);
                                select SCOPE_IDENTITY();", conn);
    
        cmd.Parameters.AddWithValue("@Title", survey.Title);
        cmd.Parameters.AddWithValue("@Description", survey.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@IsAnonymous", survey.IsAnonymous);
        cmd.Parameters.AddWithValue("@CreatedAt", survey.CreatedDate);
        cmd.Parameters.AddWithValue("@UserId", 1); // for demo purposes 
        cmd.Parameters.AddWithValue("@Status", (byte)survey.Status);
        cmd.Parameters.AddWithValue("@IsActive", survey.IsActive);

        conn.Open();
        int rowsAffectedId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return rowsAffectedId;
            
        
    }
    
    public  async Task<int> UpdateSurveyAsync(Survey survey)
    {
        using SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using SqlCommand cmd = new SqlCommand(
            @"UPDATE Surveys SET Title = @Title, Description = @Description, IsAnonymous = @IsAnonymous, IsActive = @IsActive, Status = @Status
                       WHERE Id = @Id", conn);
        
        cmd.Parameters.AddWithValue("@Id", survey.Id);
        cmd.Parameters.AddWithValue("@Title", survey.Title);
        cmd.Parameters.AddWithValue("@Description", survey.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@IsAnonymous", survey.IsAnonymous);
        cmd.Parameters.AddWithValue("@IsActive", survey.IsActive);
        cmd.Parameters.AddWithValue("@Status", (byte)survey.Status);

        conn.Open();
        int rowsAffectedId = Convert.ToInt32(await cmd.ExecuteNonQueryAsync());
        return rowsAffectedId;
            
        
    }
    
    public  async Task<int> DeleteSurveyAsync(int surveyId)
    {
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());

        using var cmd = new SqlCommand( @"DELETE FROM Surveys WHERE Id = @Id", conn);
            
        cmd.Parameters.AddWithValue("@Id", surveyId);

        conn.Open();
        int rowsAffectedId = Convert.ToInt32(await cmd.ExecuteNonQueryAsync());
        return rowsAffectedId;
    }
}