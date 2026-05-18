using Entities;
using Microsoft.Data.SqlClient;
using SurveyDataAccessLayer.Interface;
using SurveyDataAccessLayer.rowDTO;

namespace SurveyDataAccessLayer;

public class ResponseRepository : IResponseRepository
{
    public async Task<List<SurveyResponseRow>> GetAllResponsesDetailsAsync()
    {   
        using var conn = new SqlConnection(DbHelperLocal.GetConnectionString());
        using var cmd = new SqlCommand(@" SELECT R.Id AS ResponseId, R.SurveyId, R.UserId, R.SubmittedAt,
                                    A.Id AS AnswerId, A.QuestionId,  A.AnswerType, 
                                    CASE
                                        WHEN A.AnswerType = 3 THEN A.TextValue
                                        ELSE NULL
                                        END AS TextAnswer,
                                    CASE
                                        WHEN A.AnswerType = 6 THEN CAST(A.NumberValue AS NVARCHAR(50))
                                        ELSE NULL
                                        END AS RatingAnswer,
                                    ASel.ChoiceId,
                                    ASel.RankOrder
                                FROM Response R
                                         INNER JOIN Answers A ON R.Id = A.ResponseId
                                         LEFT JOIN AnswerSelections ASel ON A.Id = ASel.AnswerId
                                ORDER BY R.Id, A.Id, ASel.RankOrder;", conn);
        try
        {
            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            var responsesList = new List<SurveyResponseRow>();
            while (await reader.ReadAsync())
            {
                var response = new SurveyResponseRow {
                    ResponseId = reader.GetInt32(reader.GetOrdinal("ResponseId")),
                    SurveyId = reader.GetInt32(reader.GetOrdinal("SurveyId")),
                    UserId = reader.GetString(reader.GetOrdinal("UserId")),
                    SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                    AnswerId = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                    QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                    AnswerType = (QuestionType)reader.GetByte(reader.GetOrdinal("AnswerType")),

                    // Nullable fields - requires IsDBNull check
                    TextAnswer = reader.IsDBNull(reader.GetOrdinal("TextAnswer")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("TextAnswer")),
            
                    RatingAnswer = reader.IsDBNull(reader.GetOrdinal("RatingAnswer")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("RatingAnswer")),
            
                    ChoiceId = reader.IsDBNull(reader.GetOrdinal("ChoiceId")) 
                        ? null 
                        : reader.GetInt32(reader.GetOrdinal("ChoiceId")),
            
                    RankOrder = reader.IsDBNull(reader.GetOrdinal("RankOrder")) 
                        ? null 
                        : reader.GetInt32(reader.GetOrdinal("RankOrder"))
                };
                responsesList.Add(response);
            }

            return responsesList;
        }
        catch
        {
            throw new Exception("An error occurred while retrieving responses.");
        }
        
    }

    public async Task<Responses?> GetResponseByIdAsync(int responseId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Responses>> GetResponsesBySurveyIdAsync(int surveyId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Responses>> GetResponsesByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateResponseAsync(Responses response)
    {
        throw new NotImplementedException();
    }

    public async Task<int> UpdateResponseAsync(Responses response)
    {
        throw new NotImplementedException();
    }

    public async Task<int> DeleteResponseAsync(int responseId)
    {
        throw new NotImplementedException();
    }
}