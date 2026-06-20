using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository.Models;

namespace Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;
        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }
        public Task<List<Question>> GetAllQuestions(int surveyId)
        {
            return _context.Questions
                .Include(q => q.Choices.OrderBy(c => c.OrderIndex))
                .Where(q => q.SurveyId == surveyId)
                .ToListAsync();
            
        }

        public Task<Question?> GetQuestionByIdAsync(int id)
        {
            return _context.Questions
                .Include(q => q.Choices.OrderBy(c => c.OrderIndex))
                .FirstOrDefaultAsync(q => q.Id == id);
        }
    }
}
