using DTOs;
using Entities;
using SurveyDataAccessLayer.rowDTO;

namespace SurveyApplication.Mapper;

public class ResponseMapper
{
    public static IEnumerable<ResponseDto> ToResponseDto(IEnumerable<SurveyResponseRow> rows)
    {
        var responseDict = new Dictionary<int, ResponseDto>();

        foreach (var row in rows)
        {
            // 1. Get the existing Response, or create a new one if it doesn't exist
            if (!responseDict.TryGetValue(row.ResponseId, out var currentResponse))
            {
                currentResponse = new ResponseDto
                {
                    ResponseId = row.ResponseId,
                    UserId = row.UserId,
                    SubmittedAt = row.SubmittedAt,
                    Answers = new List<AnswerDto>()
                };
                responseDict.Add(row.ResponseId, currentResponse);
            }

            // 2. Look for the exact Answer inside this Response
            var currentAnswer = currentResponse.Answers.FirstOrDefault(a => a.AnswerId == row.AnswerId);
        
            // If the Answer doesn't exist yet, create it!
            if (currentAnswer == null)
            {
                currentAnswer = new AnswerDto
                {
                    AnswerId = row.AnswerId,
                    QuestionId = row.QuestionId,
                    AnswerType = Enum.Parse<QuestionType>(row.AnswerType.ToString()),
                };
                
                if (row.AnswerType == QuestionType.Text)
                {
                    currentAnswer.AnswerValue = row.TextAnswer;
                }
                else if (row.AnswerType == QuestionType.Rating) 
                {
                    currentAnswer.AnswerValue = row.RatingAnswer;
                }
                else // Radio, Checkbox, Matrix, Rank (Types that use choices)
                {
                    currentAnswer.AnswerValue = new List<AnswerSelectionDto>();
                }
                
                currentResponse.Answers.Add(currentAnswer);
            }

            if (currentAnswer.AnswerValue is List<AnswerSelectionDto> selectionList)
            {
                selectionList.Add(new AnswerSelectionDto
                {
                    ChoiceId = row.ChoiceId.Value,
                    RankOrder = row.RankOrder.Value
                });
            }
        }

        return responseDict.Values;
    }
}