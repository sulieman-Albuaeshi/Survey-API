using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.Interface;
using Repository.Data;

namespace Repository
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly AppDbContext _context;

        public SurveyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Survey>> GetAllSurveysAsync()
        {
            return await _context.Surveys.ToListAsync();
        }

        public async Task<Survey?> GetSurveyByIdAsync(int surveyId)
        {
            return await _context.Surveys.FindAsync(surveyId);
        }

        public async Task<Survey> CreateSurveyAsync(Survey survey)
        {
            await _context.AddAsync(survey);
            await _context.SaveChangesAsync();
            return survey;
        }

        public async Task<Survey> UpdateSurveyAsync(Survey survey)
        {
            var existingSurvey = await _context.Surveys.FindAsync(survey.Id);
            if (existingSurvey != null)
            {
                existingSurvey.Title = survey.Title;
                existingSurvey.Description = survey.Description;
                existingSurvey.IsActive = survey.IsActive;
                existingSurvey.IsAnonymous = survey.IsAnonymous;
                existingSurvey.Status = survey.Status;
                // TODO is QeustionCount something we should update here? .
                existingSurvey.QuestionCount = survey.QuestionCount;
                await _context.SaveChangesAsync();
                return existingSurvey;
            }
            else
            {
                throw new KeyNotFoundException($"Survey with ID {survey.Id} not found.");
            }
        }

        public async Task<int> DeleteSurveyAsync(int surveyId)
        {
            var survey = await _context.Surveys.FindAsync(surveyId);

            if (survey == null)
            {
                throw new KeyNotFoundException($"Survey with ID {surveyId} not found.");
            }

            if (survey.Status == SurveyStatus.Published)
            {
                throw new InvalidOperationException("Cannot delete a Published survey. Please close the survey before deletion.");
            }


            _context.Surveys.Remove(survey);
            return await _context.SaveChangesAsync();
        }
        

        public Task<(List<Question>, List<Choice>)> GetQuestionsForSurveyAsync(int surveyId)
        {
            throw new NotImplementedException();
        }
    }
}
