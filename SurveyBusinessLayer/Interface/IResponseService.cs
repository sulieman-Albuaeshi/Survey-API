using  SurveyBusinessLayer.DTOs;

namespace SurveyBusinessLayer.Interface;

public interface IResponseService
{
    public Task<List<ResponseDto>> GetAllResponsesDetailsAsync(int pageSize, int pageNumber);
    
    public Task<List<ResponseDto>> GetResponsesBySurveyIdAsync(int surveyId, int pageSize, int pageNumber);
    
    public Task<List<ResponseDto>> GetResponsesByUserIdAsync(string userId, int pageSize, int pageNumber);
    public Task<ResponseDto> GetResponseByIdAsync(int responseId);
    public Task<int> GetResponsesCountAsync();
    public Task<ResponseDetailsDto> SubmitResponseAsync(ResponseCreateDto responseCreateDto);
}   