using Repository.Data;
using Repository.Interface;
using Repository.Models;

namespace Repository
{
    public class ChoiceRepository : IChoiceRepository
    {
        readonly AppDbContext _context;
        public ChoiceRepository(AppDbContext context) 
        {
            _context = context;
        }

        public Task<bool> CreateChoiceAsync(Choice choice)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteChoiceAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Choice>> GetChoicesByQuestionIdAsync(int questionId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateChoiceAsync(Choice choice)
        {
            throw new NotImplementedException();
        }
    }
}
