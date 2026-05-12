using Entities;
using Microsoft.Data.SqlClient;

namespace SurveyDataAccessLayer;

public class SurveyDAL
{
    public static List<Survey> GetAllSurveys()
    {
        // the DbHelperLocal is a hidden class 
        // you need to update the connection string in the DbHelper class to connect to your local database
        using (SqlConnection conn = new SqlConnection(DbHelperLocal.GetConnectionString()))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Id, Title, Description, Status, CreatedAt  FROM Surveys", conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
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
                        });
                    }
                    return surveys;
                }
            }
        }
    }
}