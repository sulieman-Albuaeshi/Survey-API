using SurveyBusinessLayer.Interface;
using Repository.Interface;
using Repository.Models;
using SurveyBusinessLayer.DTOs;
using SurveyBusinessLayer.Mapper;

namespace SurveyBusinessLayer;

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository _surveyRepository;
    
    public SurveyService(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }
    
    public async Task<List<SurveyDto>> GetAllSurveysAsync(int pageSize, int pageNumber)
    {
        if (pageSize <= 0 || pageNumber <= 0)
            throw new ArgumentException("Page size and page number must be greater than zero.");

        var surveyList = await _surveyRepository.GetAllSurveysAsync(pageSize, pageNumber);
        if (surveyList == null || !surveyList.Any())
            throw new KeyNotFoundException("Survey not found.");

        List<SurveyDto> dto = surveyList.Select(s => s.ToDto()).ToList();
        return dto;
    }
    
    public async Task<SurveyDetailsDto?> GetSurveyByIdAsync(int surveyId)
    {
        var survey = await _surveyRepository.GetSurveyByIdAsync(surveyId);
        if (survey == null)
            throw new KeyNotFoundException("Survey not found.");

        return survey.ToDetailsDto();
    }

    public async Task<SurveyDetailsDto> CreateSurveyWithQuestionsAsync(CreateSurveyDto dto)
    {

        var MapedSurvey = dto.ToDominEntity();


        if (string.IsNullOrEmpty(dto.Title))
            throw new ArgumentException("Survey title is required.");

        foreach (var question in MapedSurvey.Questions)
        {
            if (!question.IsValid())
            {
                throw new ArgumentException("Invalid question data.");
            }
        }

        var createdSurvey = await _surveyRepository.CreateSurveyAsync(MapedSurvey);

        if (createdSurvey == null)
            throw new KeyNotFoundException("Survey was not created.");

        return createdSurvey.ToDetailsDto();
    }

    public async Task<SurveyDetailsDto> UpdateSurveyWithQuestionsAsync(UpdaatSurveyDto dto)
    {
        // TODO : need to check if the same user trying to update the same survey
        var MapedSurvey = dto.ToDominEntity();

        if (MapedSurvey.Status == SurveyStatus.Published)
            throw new InvalidOperationException("Cannot update a published survey.");
        
        if(MapedSurvey.Id <= 0)
            throw new ArgumentException("Invalid survey ID.");


        if (string.IsNullOrEmpty(dto.Title))
            throw new ArgumentException("Survey title is required.");

        foreach (var question in MapedSurvey.Questions)
        {
            if (!question.IsValid())
            {
                throw new ArgumentException("Invalid question data.");
            }
        }

        var createdSurvey = await _surveyRepository.UpdateSurveyAsync(MapedSurvey);

        if (createdSurvey == null)
            throw new KeyNotFoundException("Survey was not updated.");

        return createdSurvey.ToDetailsDto();
    }
    
    public async Task<bool> DeleteSurveyAsync(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");

        return await _surveyRepository.DeleteSurveyAsync(surveyId) == 1;
    }

    public async Task<bool> ChangeSurveyStatusAsync(int surveyId, string statusText)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");
        var status = await _surveyRepository.ChangeSurveyStatusAsync(surveyId, statusText);
        if (!status)
            throw new KeyNotFoundException("there is no survey with the specified ID.");
        return status;
    }

    public async Task<bool?> IsSurveyAnonymous(int surveyId)
    {
        if (surveyId <= 0)
            throw new ArgumentException("Invalid survey id.");

        return await _surveyRepository.IsSurveyAnonymousAsync(surveyId);
    }
}