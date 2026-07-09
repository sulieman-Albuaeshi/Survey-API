namespace SurveyBusinessLayer.utility
{
    public static class Utility
    {
        public static string hashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);   
        }
    }
}
