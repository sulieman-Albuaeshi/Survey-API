using SurveyDataAccessLayer.rowDTO;

namespace SurveyBusinessLayer.Interface;

public interface IResponseService
{
    public Task<List<SurveyResponseRow>> GetAllResponsesDetailsAsync();
}