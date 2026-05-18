using SurveyBusinessLayer.Interface;
using SurveyDataAccessLayer.Interface;
using SurveyDataAccessLayer.rowDTO;

namespace SurveyBusinessLayer;

public class ResponseService : IResponseService
{
    private readonly IResponseRepository _responseRepository;
    public ResponseService(IResponseRepository responseRepository)
    {
        _responseRepository = responseRepository;
    }
    
    public Task<List<SurveyResponseRow>> GetAllResponsesDetailsAsync()
    {
        return _responseRepository.GetAllResponsesDetailsAsync();
    }
}