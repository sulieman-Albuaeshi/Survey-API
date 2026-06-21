using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Interface;
using Repository.Data;
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

        public Task<List<Response>> GetAllResponsesDetailsAsync()
        {
            throw new NotImplementedException();
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

        public async Task<Response> SubmitResponseAsync(Response response)
        {
            var isAnonymous = await _context.Surveys
                .Where(s => s.Id == response.SurveyId)
                .Select(s => (bool?)s.IsAnonymous)
                .FirstOrDefaultAsync();

            if (isAnonymous == null)
                throw new KeyNotFoundException($"Survey with ID {response.SurveyId} not found.");

            if (!isAnonymous.Value && string.IsNullOrEmpty(response.UserId))
                throw new InvalidOperationException("Survey is not anonymous, userId must be provided.");

            var questionIds = response.Answers.Select(a => a.QuestionId).ToHashSet();

            var requiredQuestionIds = await _context.Questions
            .Where(q => q.SurveyId == response.SurveyId && q.IsRequired)
            .Select(q => q.Id)
            .ToListAsync();

            var missingRequiredIds = requiredQuestionIds
                .Where(id => !questionIds.Contains(id))
                .Count();

            if (missingRequiredIds != 0)
                throw new InvalidOperationException("All required questions must be answered.");

            // NEW: validate ChoiceIds belong to the questions being answered
            var validChoiceIds = _context.Choices
                .Where(c => questionIds.Contains(c.QuestionId))
                .Select(c => c.Id)
                .ToHashSet();

            var submittedChoiceIds = response.Answers
                .SelectMany(a => a.AnswerSelections)
                .Select(s => s.ChoiceId)
                .ToHashSet();

            if (!submittedChoiceIds.All(id => validChoiceIds.Contains(id)))
                throw new InvalidOperationException("One or more selected choices are invalid.");


            _context.Add(response);
            await _context.SaveChangesAsync();

            return response;
        }
    }
}
