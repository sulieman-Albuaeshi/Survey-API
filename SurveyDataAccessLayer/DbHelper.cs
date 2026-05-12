namespace SurveyDataAccessLayer;

public class DbHelper
{
    public static string GetConnectionString()
    {
        return "Server=localhost;Database=ExampleDB;User Id=sa;Password=1234;TrustServerCertificate=True;";
    }
}