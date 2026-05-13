namespace SurveyBusinessLayer;
using SurveyDataAccessLayer;
using Entities;


public class SurveyBL
{
    private Survey survey = new Survey();
    
    public SurveyBL(int surveyId, string title, string? description, bool isAnonymous, DateTime createdDate, bool isActive = true,
        int userId = 0, SurveyStatus status = SurveyStatus.Draft, int questionCount = 0)
    {
        survey.Id = surveyId;
        survey.Title = title;
        survey.Description = description;
        survey.IsAnonymous = isAnonymous;
        survey.CreatedDate = createdDate;
        survey.IsActive = isActive;
        survey.UserId = userId;
        survey.Status = status;
        survey.QuestionCount = questionCount;
        survey.Mode = EnMode.AddNew;
    }

    private async Task<int> _AddNewSurvey()
    {
        survey.Id = await SurveyDAL.AddNewSurveyAsync(survey);
        return survey.Id;
    }
    
    private async Task<int> _updateSurvey()
    {
        return 1;
    }

    public async Task<int> Save()
    {
        switch (survey.Mode)
        {
            case EnMode.AddNew:
                return await _AddNewSurvey();
            case EnMode.Update:
                return await _updateSurvey();
            default:
                return await _updateSurvey();
        }
    }
    
    public static async Task<List<Survey>> GetAllSurveysAsync()
    {
        return await SurveyDAL.GetAllSurveysAsync();
    }
}