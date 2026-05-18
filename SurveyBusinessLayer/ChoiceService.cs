using SurveyBusinessLayer.Interface;

namespace SurveyBusinessLayer;
using Entities;
using SurveyDataAccessLayer.Interface;

public class ChoiceService : IChoiceService
{
        private readonly IChoiceRepository _choiceRepository;
        private readonly IQuestionRepository _questionRepository;
    
        public ChoiceService(IChoiceRepository choiceRepository, IQuestionRepository questionRepository)
        {
            _choiceRepository = choiceRepository;
            _questionRepository = questionRepository;
        }
    
        public async Task<List<Choice>> GetChoicesByQuestionIdAsync(int questionId)
        {
            if (questionId < 1)
                throw new ArgumentException("Invalid question id");
            
            var choices = await _choiceRepository.GetChoicesByQuestionIdAsync(questionId);
            
            if(choices == null || !choices.Any())
                throw new KeyNotFoundException("No choices found");
            
            return choices;
        }
        
        public async Task<bool> CreateChoiceAsync(Choice choice)
        {
            if (choice == null)
                throw new ArgumentNullException(nameof(choice));
            
            if (choice.QuestionId < 1)
                throw new ArgumentException("Invalid question id");
            
            var question = await _questionRepository.GetQuestionByIdAsync(choice.QuestionId);
            if (question == null)
                throw new KeyNotFoundException("No question found");
            
            return await _choiceRepository.CreateChoiceAsync(choice);
        }
    
        public async Task<bool> UpdateChoiceAsync(Choice choice)
        {
            if (choice == null)
                throw new ArgumentNullException(nameof(choice));
            
            if (choice.Id < 1 || choice.QuestionId < 1)
                throw new ArgumentException("Invalid choice id or question id");
            
            var question = await _questionRepository.GetQuestionByIdAsync(choice.QuestionId);
            if (question == null)
                throw new KeyNotFoundException("No question found");
            
            return await _choiceRepository.UpdateChoiceAsync(choice);
        }
    
        public async Task<bool> DeleteChoiceAsync(int id)
        {
            if (id < 1)
                throw new ArgumentException("Invalid choice id");
            
            return await _choiceRepository.DeleteChoiceAsync(id);
        }
}