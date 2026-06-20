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
            return await _context.Surveys
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Survey?> GetSurveyByIdAsync(int surveyId)
        {
            return await _context.Surveys
                        .Include(s => s.Questions)
                        .ThenInclude(q => q.Choices)
                        .AsSplitQuery()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == surveyId);
        }

        public async Task<Survey> CreateSurveyAsync(Survey newSurvey)
        {
            foreach(var (question, index) in newSurvey.Questions.Select((q, i) => (q, i+1)))
            {
                question.OrderIndex = index;
                foreach(var (choice, choiceIndex) in question.Choices.Select((c, i) => (c, i+1)))
                {
                    choice.OrderIndex = choiceIndex;
                }
            }
            newSurvey.QuestionCount = newSurvey.Questions.Count;

            try
            {
                await _context.AddAsync(newSurvey);
                await _context.SaveChangesAsync();

            }
            catch(Exception e)
            {
                throw new Exception($"Database save failed: {e.InnerException?.Message ?? e.Message}", e);
            }
            return newSurvey;
        }

        public async Task<Survey> UpdateSurveyAsync(Survey Updatedsurvey)
        {
            var newQuestionIDs = Updatedsurvey.Questions
                                 .Where(q => q.Id > 0)
                                 .Select(q => q.Id)
                                 .ToHashSet();
            var incomingChoiceIDs = Updatedsurvey.Questions
                                 .SelectMany(q => q.Choices)
                                 .Where(c => c.Id > 0)
                                 .Select(c => c.Id)
                                 .ToHashSet();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                await _context.Choices
                    .Where(c => newQuestionIDs.Contains(c.QuestionId) && !incomingChoiceIDs.Contains(c.Id))
                    .ExecuteDeleteAsync();

                await _context.Questions
                    .Where(q => q.SurveyId == Updatedsurvey.Id && !newQuestionIDs.Contains(q.Id))
                    .ExecuteDeleteAsync();

                var existingSurvey = await _context.Surveys
                    .Include(q => q.Questions)
                    .ThenInclude(c => c.Choices)
                    .FirstOrDefaultAsync(s => s.Id == Updatedsurvey.Id);

                if (existingSurvey == null) throw new KeyNotFoundException($"Survey with ID {Updatedsurvey.Id} not found.");

                existingSurvey.Title = Updatedsurvey.Title;
                existingSurvey.Description = Updatedsurvey.Description;
                existingSurvey.IsAnonymous = Updatedsurvey.IsAnonymous;
                existingSurvey.Status = Updatedsurvey.Status;
                existingSurvey.QuestionCount = Updatedsurvey.Questions.Count;

                int QuestionInsex = 0;
                foreach (var question in Updatedsurvey.Questions)
                {
                    var existingQuestion = existingSurvey.Questions.FirstOrDefault(q => q.Id == question.Id);
                    if (existingQuestion != null)
                    {
                        existingQuestion.QuestionText = question.QuestionText;
                        existingQuestion.IsRequired = question.IsRequired;
                        existingQuestion.OrderIndex = ++QuestionInsex;

                        int ChoiceIndex = 0;
                        foreach (var choice in question.Choices)
                        {
                            var existingChoice = existingQuestion.Choices.FirstOrDefault(c => c.Id == choice.Id);
                            if (existingChoice != null)
                            {
                                existingChoice.ChoiceText = choice.ChoiceText;
                                existingChoice.OrderIndex = ++ChoiceIndex;
                            }
                            else
                            {
                                choice.OrderIndex = ++ChoiceIndex;
                                existingQuestion.Choices.Add(choice);
                            }
                        }
                    }
                    else
                    {
                        question.OrderIndex = ++QuestionInsex;

                        int choiceIndex = 0;
                        foreach (var choice in question.Choices)
                        {
                            choice.OrderIndex = ++choiceIndex;
                        }
                        existingSurvey.Questions.Add(question);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return existingSurvey;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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
        
        public Task<int> ContextSaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<(List<Question>, List<Choice>)> GetQuestionsForSurveyAsync(int surveyId)
        {
            throw new NotImplementedException();
        }
    }
}
