namespace SurveyBusinessLayer;

using SurveyDataAccessLayer;
using Entities;


public class SurveyBL
{
    public static List<Survey> GetAllSurveys()
    {
        return SurveyDAL.GetAllSurveys();
    }
}