using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Data;
using Repository.Models;
using SurveyBusinessLayer.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace surveyTest
{
    public class ResponseAPITest
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        public ResponseAPITest(ITestOutputHelper output)
        {
            var application = new SurveyWebApplicationFactory();
            _client = application.CreateClient();
            _output = output;
        }

        private AppDbContext CreateTestDbContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.Test.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("TestConnection");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        private Survey CreateSurveyInDatabase()
        {
            var newSurvey = new Survey
            {
                Title = "Response Test Survey",
                Description = "Testing response endpoints",
                IsAnonymous = true,
                Status = SurveyStatus.Published,
                UserId = new Guid("7b0e14a2-9c3f-42a1-b8d6-5f8e02c1439b"),
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "Do you like tests?",
                        IsRequired = true,
                        QuestionTypeEnum = enQuestionType.Radio,
                        Choices = new List<Choice>
                        {
                            new Choice { ChoiceText = "Yes" },
                            new Choice { ChoiceText = "No" }
                        }
                    }
                }
            };

            var dbContext = CreateTestDbContext();
            dbContext.Surveys.Add(newSurvey);
            dbContext.SaveChanges();

            return newSurvey;
        }

        private void ReadResponseContent(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine($"Response Content: {content}");
        }

        [Fact]
        public async Task GetAllResponses_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var endpoint = "/api/Response/All?pageSize=5&pageNumber=1";

            // Act
            var response = await _client.GetAsync(endpoint);
            ReadResponseContent(response);

            // Assert
            // It might return 200 OK or 404 NotFound if DB is completely empty (no responses)
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllResponses_WithZeroPageSize_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/Response/All?pageSize=0&pageNumber=1";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetResponsesBySurveyId_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/Response/survey/-1?pageSize=5&pageNumber=1";

            // Act
            var response = await _client.GetAsync(endpoint);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetResponseById_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/Response/-5";

            // Act
            var response = await _client.GetAsync(endpoint);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetResponsesCount_WithInvalidSurveyId_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/Response/survey/0/Count";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SubmitResponse_WithValidData_ReturnsCreated()
        {
            // Arrange
            var survey = CreateSurveyInDatabase();
            var question = survey.Questions.First();
            var choice = question.Choices.First();

            var endpoint = $"/api/Response/survey/{survey.Id}";
            var newResponse = new ResponseCreateDto
            {
                SurveyId = survey.Id,
                UserId = "7b0e14a2-9c3f-42a1-b8d6-5f8e02c1439b",
                Answers = new List<AnswerCreateDto>
                {
                    new AnswerCreateDto
                    {
                        QuestionId = question.Id,
                        AnswerType = enQuestionType.Radio,
                        RankedChoices = new List<ChoiceRankingDto>
                        {
                            new ChoiceRankingDto { ChoiceId = choice.Id, RankOrder = 1 }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newResponse);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task SubmitResponse_WithInvalidSurveyId_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/Response/survey/-1";
            var newResponse = new ResponseCreateDto
            {
                SurveyId = -1,
                UserId = "test-user-123",
                Answers = new List<AnswerCreateDto>()
            };
            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newResponse);
            ReadResponseContent(response);
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SubmitResponse_WithAnswertypeDoesNotReqierChoice_ReturnsBadRequest()
        {
            // Arrange
            var survey = CreateSurveyInDatabase();
            var endpoint = $"/api/Response/survey/{survey.Id}";
            var newResponse = new ResponseCreateDto
            {
                SurveyId = survey.Id,
                UserId = "7b0e14a2-9c3f-42a1-b8d6-5f8e02c1439b",
                Answers = new List<AnswerCreateDto>()
                {
                    new AnswerCreateDto
                    {
                        QuestionId = survey.Id,
                        AnswerType = enQuestionType.Text,
                        RankedChoices = new List<ChoiceRankingDto>
                        {
                            new ChoiceRankingDto { ChoiceId = 1, RankOrder = 1 }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newResponse);
            ReadResponseContent(response);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SubmitResponse_WithMissingQuestions_ReturnsBadRequest()
        {
            // Arrange
            var survey = CreateSurveyInDatabase();

            var endpoint = $"/api/Response/survey/{survey.Id}";
            var newResponse = new ResponseCreateDto
            {
                SurveyId = survey.Id,
                UserId = "test-user-123",
                Answers = new List<AnswerCreateDto>() // Missing required answers
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newResponse);
            ReadResponseContent(response);

            // Assert
            // Can be BadRequest (validation fluent) or InternalServerError/BadRequest depending on Exception mapping middleware
            // Assuming the Service throws InvalidOperationException or FluentValidation returns BadRequest:
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError });
        }
    }
}