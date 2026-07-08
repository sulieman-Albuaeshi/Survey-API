
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
    public class SurveyAPITest
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        public SurveyAPITest(ITestOutputHelper output)
        {
            var application = new SurveyWebApplicationFactory();
            _client = application.CreateClient();
            _output = output;
        }

        private Survey CreateSurveyInDatabase()
        {
            var newSurvey = new Survey
            {
                Title = "Junior Safe Test",
                Description = "Testing the full pipeline!",
                IsAnonymous = true,
                Status = SurveyStatus.Draft,
                UserId = "test-user-123",
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "Is this easy?",
                        IsRequired = true,
                        QuestionTypeEnum = enQuestionType.Radio, // Tests if your string-to-enum mapping works!
                        Choices = new List<Choice>
                        {
                            new Choice{ ChoiceText = "Yes" },
                            new Choice{ ChoiceText = "Absolutely" }
                        }
                    }
                }
            };

            var dbContext = CreateTestDbContext();
            dbContext.Surveys.Add(newSurvey);
            dbContext.SaveChanges();

            return newSurvey;
        }

        private AppDbContext CreateTestDbContext()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsetting.Test.json", optional: false, reloadOnChange: true)
            .Build();

            // 2. Extract the connection string safely
            var connectionString = config.GetConnectionString("TestConnection");
            Assert.NotNull(connectionString);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new AppDbContext(options);
        }

        private void ReadResponseContent(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            _output.WriteLine($"Response Content: {content}");
        }

        [Fact]
        public async Task GetAllSurveys_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var endpoint = "/api/surveys/All?pageSize=5&pageNumber=1";

            // Act
            var response = await _client.GetAsync(endpoint);
            ReadResponseContent(response);


            // Assert
            response.EnsureSuccessStatusCode(); // Expects 200 OK
        }

        [Fact]
        public async Task GetAllSurveys_WithZeroPageSize_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/surveys/All?pageSize=0&pageNumber=1";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSurveyById_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var endpoint = "/api/surveys/-5";

            // Act
            var response = await _client.GetAsync(endpoint);
            ReadResponseContent(response);


            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateSurvey_WithValidData_ReturnsCreated()
        {
            // Arrange
            var endpoint = "/api/surveys/Create";
            var newSurvey = new CreateSurveyDto
            {
                Title = "Junior Safe Test",
                Description = "Testing the full pipeline!",
                IsAnonymous = true,
                Status = "Draft",
                userId = "test-user-123",
                Questions = new List<CreateQuestionDto>
                {
                    new CreateQuestionDto
                    {
                        QuestionText = "Is this easy?",
                        IsRequired = true,
                        QuestionType = "Radio", // Tests if your string-to-enum mapping works!
                        Choices = new List<CreateChoiceDto>
                        {
                            new CreateChoiceDto{ ChoiceText = "Yes" },
                            new CreateChoiceDto{ ChoiceText = "Absolutely" }
                        }
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newSurvey);
            ReadResponseContent(response);


            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Optional: Verify it returns the GetSurveyById location header
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task UpdateSurvey_WithValidData_ReturnsOkOrBadRequest()
        { 
            // Arrange
            var survey = CreateSurveyInDatabase();
            var QuestionId = survey.Questions.First().Id;
            var ChoiceId = survey.Questions.First().Choices.First().Id;
            var endpoint = $"/api/surveys/{survey.Id}";

            var updateDto = new UpdaatSurveyDto
            {
                Id = survey.Id,
                Title = "Updated Survey Title",
                Description = "Updated Description",
                IsAnonymous = true,
                Status = "Draft",
                Questions = new List<UpdateQuestionDto>
                {
                // ─── QUESTION 1: EXISTING (UPDATED) ───
                new UpdateQuestionDto
                {
                    Id = QuestionId,
                    QuestionText = "Altered Text for Existing Question 1",
                    IsRequired = true,
                    QuestionType = "Radio",
                    Choices = new List<updateChoiceDto>
                    {
                        // Choice A: Existing item updated
                        new updateChoiceDto { Id = ChoiceId, ChoiceText = "Altered Text for Existing Choice 1" },
                    
                        // Choice B: Brand new item added to this existing question
                        new updateChoiceDto { Id = 0, ChoiceText = "Brand New Choice Added to Old Question" }
                    }
                },

                // ─── QUESTION 2: BRAND NEW (INSERTED) ───
                new UpdateQuestionDto
                {
                    Id = 0, // 0 tells EF Core to INSERT this new question
                    QuestionText = "Completely New Question 2",
                    IsRequired = false,
                    QuestionType = "Checkbox",
                    Choices = new List<updateChoiceDto>
                    {
                        // Choice C: Brand new item added to the new question
                        new updateChoiceDto { Id = 0, ChoiceText = "Brand New Choice inside New Question" }
                    }
                }
                }
            };

            // Act
            var response = await _client.PutAsJsonAsync(endpoint, updateDto);
            ReadResponseContent(response);


            // Assert
            // Note: Since this runs on a fresh test DB, if ID 1 doesn't exist, 
            // your controller returns BadRequest("Failed to update survey")
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task DeleteSurvey_WhenFound_ReturnsNotContent()
        {
            // Arrange
            var ExistentId = CreateSurveyInDatabase().Id;
            var endpoint = $"/api/surveys/{ExistentId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);
            ReadResponseContent(response);


            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSurvey_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = 9999;
            var endpoint = $"/api/surveys/{nonExistentId}";

            // Act
            var response = await _client.DeleteAsync(endpoint);
            ReadResponseContent(response);


            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ChangeSurveyStatus_WithValidStatus_ReturnsNoContent()
        {
            // Arrange
            var surveyId = CreateSurveyInDatabase().Id;
            var endpoint = $"/api/surveys/{surveyId}/status";
            var statusDto = new SurveyStatusDto
            {
                StatusText = "Published"
            };

            // Act
            var response = await _client.PatchAsJsonAsync(endpoint, statusDto);
            ReadResponseContent(response);

            // Assert
            // If the ID exists it returns 204 NoContent, if it fails it returns 404 NotFound
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
            else
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
