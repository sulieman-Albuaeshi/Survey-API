using  SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Interface;

public interface IResponseService
{
    public Task<List<ResponseDto>> GetAllResponsesDetailsAsync(int pageSize, int pageNumber);
    
    public Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId, int pageSize, int pageNumber);
    
    public Task<List<ResponseDto>> GetResponsesByUserIdAsync(Guid userId, int pageSize, int pageNumber);
    public Task<ResponseDto> GetResponseByIdAsync(int responseId, Guid UserId);
    public Task<int> GetResponsesCountAsync(int surveyId);
    public Task<ResponseDetailsDto> SubmitResponseAsync(ResponseCreateDto responseCreateDto, bool isAuthenticated);
}   