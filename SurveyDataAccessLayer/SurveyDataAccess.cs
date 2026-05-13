using Entities;
using Microsoft.Data.SqlClient;
namespace SurveyDataAccessLayer;

public class SurveyDAL
{
    public static async Task<List<Survey>> GetAllSurveysAsync()
    {
        // the DbHelperLocal is a hidden class 
        // you need to update the connection string in the DbHelper class to connect to your local database
        using (SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString()))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, Status, CreatedAt  FROM Surveys", conn))
            {
                conn.Open();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    List<Survey> surveys = new List<Survey>();
                    while (reader.Read())
                    {
                        surveys.Add(new Survey
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Status = (SurveyStatus)reader["Status"],
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            // add responses 
                            // question count 
                        });
                    }
                    return surveys;
                }
            }
        }
    }

    public static async Task<int> AddNewSurveyAsync(Survey survey)
    {
        using (SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString()))
        {
            using (SqlCommand cmd =
                   new SqlCommand(
                       @"INSERT INTO Surveys (Title, Description, IsAnonymous, IsActive, CreatedAt, UserId, Status) 
                                VALUES (@Title, @Description, @IsAnonymous, @IsActive, @CreatedAt, @UserId, @Status);
                                select SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Title", survey.Title);
                cmd.Parameters.AddWithValue("@Description", survey.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsAnonymous", survey.IsAnonymous);
                cmd.Parameters.AddWithValue("@CreatedAt", survey.CreatedDate);
                cmd.Parameters.AddWithValue("@UserId", 1); // for demo purposes 
                cmd.Parameters.AddWithValue("@Status", (byte)survey.Status);
                cmd.Parameters.AddWithValue("@IsActive", survey.IsActive);

                conn.Open();
                int rowsAffected_ID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return rowsAffected_ID;
            }
        }
    }
}