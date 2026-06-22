using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Interface;
using Repository.Data;
using Microsoft.Data.SqlClient.DataClassification;
namespace Repository
{
    public class ResponseRepository : IResponseRepository
    {
        private readonly AppDbContext _context;

        public ResponseRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> DeleteResponseAsync(int responseId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Response>> GetAllResponsesDetailsAsync(int pageSize, int pageNumber)
        {
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;

            int recordToSkip = (pageNumber - 1) * pageSize;

            return _context.Responses
                .Include(r => r.Answers)
                    .ThenInclude(a => a.AnswerSelections)
                    //.ThenInclude(an => an.Choice)
                .Include(r => r.Survey)
                    .ThenInclude(a => a.Questions)
                .OrderByDescending(r => r.Id)
                .Skip(recordToSkip)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<Response> GetResponseByIdAsync(int responseId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Response>> GetResponsesBySurveyIdAsync(int surveyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Response>> GetResponsesByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseValidationDataDto?> GetValidationDataForSurveyAsync(int surveyId)
        {
            return await _context.Surveys
                .Where(s => s.Id == surveyId)
                .Select(s => new ResponseValidationDataDto
                {
                    IsAnonymous = (bool?)s.IsAnonymous,

                    RequiredQuestionIds = s.Questions
                        .Where(q => q.IsRequired)
                        .Select(q => q.Id)
                        .ToList(),

                    ValidChoiceIds = s.Questions
                        .SelectMany(q => q.Choices)
                        .Select(c => c.Id)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }


        public async Task<Response> SubmitResponseAsync(Response response)
        {

            _context.Add(response);
            await _context.SaveChangesAsync();

            return await _context.Responses
                .Include(s => s.Survey)
                .Include(s => s.Answers)
                    .ThenInclude(a => a.Question)
                .FirstAsync(r => r.Id == response.Id);  
        }
    }
}
