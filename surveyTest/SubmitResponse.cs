using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository;
using Repository.Data;
using Repository.Models;
using SurveyBusinessLayer;
using SurveyBusinessLayer.DTOs;
using System;
using System.IO;

public class SubmitResponseAsyncTests
{
    // Same helper pattern as your CreateSurvey test —
    // points at a real (test) SQL Server database defined in appsetting.Test.json
    private AppDbContext CreateTestDbContext()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsetting.Test.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = config.GetConnectionString("TestConnection");
        Assert.NotNull(connectionString);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .EnableDetailedErrors()
            .Options;

        return new AppDbContext(options);
    }

    // Small helper: seeds a Survey + Question + Choices directly via EF
    // so each test has known, valid IDs to work with.
    private async Task<(Survey survey, Question question, List<Choice> choices)> SeedSurveyWithChoiceQuestion(
        AppDbContext context, bool isAnonymous, bool isRequired)
    {
        var survey = new Survey
        {
            Title = "Seeded Survey",
            UserId = new  Guid("7b0e14a2-9c3f-42a1-b8d6-5f8e02c1439b"),
            IsAnonymous = isAnonymous,
            Status = SurveyStatus.Draft
        };
        context.Surveys.Add(survey);
        await context.SaveChangesAsync();

        var question = new Question
        {
            SurveyId = survey.Id,
            QuestionText = "Pick one",
            IsRequired = isRequired,
            QuestionTypeId = 2 // Radio, matching your existing test's convention
        };
        context.Questions.Add(question);
        await context.SaveChangesAsync();

        var choices = new List<Choice>
        {
            new Choice { QuestionId = question.Id, ChoiceText = "Yes", OrderIndex = 1 },
            new Choice { QuestionId = question.Id, ChoiceText = "No", OrderIndex = 2 }
        };
        context.Choices.AddRange(choices);
        await context.SaveChangesAsync();

        return (survey, question, choices);
    }

    [Fact]
    public async Task SubmitResponseAsync_ValidAnonymousResponse_SavesSuccessfully()
    {
        // ARRANGE
        using var context = CreateTestDbContext();
        var responseRepository = new ResponseRepository(context); // adjust to your real repo type
        var userRepository = new UserRepository(context); // adjust to your real repo type
        var service = new ResponseService(responseRepository, userRepository);     // adjust to your real service type

        var (survey, question, choices) = await SeedSurveyWithChoiceQuestion(
            context, isAnonymous: true, isRequired: true);

        var response = new ResponseCreateDto
        {
            SurveyId = survey.Id,
            UserId = null, // allowed: survey is anonymous
            SubmittedAt = DateTime.UtcNow,
            Answers = new List<AnswerCreateDto>
            {
                new AnswerCreateDto
                {
                    QuestionId = question.Id,
                    AnswerType = enQuestionType.Checkbox,
                    AnswerValue = "",
                    RankedChoices = new List<ChoiceRankingDto>
                    {
                        new ChoiceRankingDto { ChoiceId = choices[0].Id, RankOrder = 0 }
                    }
                }
            }
        };
        // ACT
        var result = await service.SubmitResponseAsync(response);

        // ASSERT
        Assert.NotNull(result);
        Assert.True(result.ResponseId > 0);

        var saved = await context.Responses
            .Include(r => r.Answers)
            .ThenInclude(a => a.AnswerSelections)
            .FirstOrDefaultAsync(r => r.Id == result.ResponseId);

        Assert.NotNull(saved);
        Assert.Equal(survey.Id, saved.SurveyId);
        Assert.Single(saved.Answers);
        Assert.Single(saved.Answers.First().AnswerSelections);
        Assert.Equal(choices[0].Id, saved.Answers.First().AnswerSelections.First().ChoiceId);
    }

    [Fact]
    public async Task SubmitResponseAsync_ValidNotAnonymousResponse_SavesSuccessfully()
    {
        int surveyId, questionId, choiceId;
        using (var context = CreateTestDbContext()) // create a new context to avoid EF tracking issues
        {
            var (survey, question, choices) = await SeedSurveyWithChoiceQuestion(
                context, isAnonymous: false, isRequired: true);

            surveyId = survey.Id;
            questionId = question.Id;
            choiceId = choices[0].Id;

        }


        var response = new ResponseCreateDto
        {
            SurveyId = surveyId,
            UserId = "7b0e14a2-9c3f-42a1-b8d6-5f8e02c1439b", // required: survey is not anonymous
            SubmittedAt = DateTime.UtcNow,
            Answers = new List<AnswerCreateDto>
            {
                new AnswerCreateDto
                {
                    QuestionId = questionId,
                    AnswerType = enQuestionType.Checkbox,
                    AnswerValue = "",
                    RankedChoices = new List<ChoiceRankingDto>
                    {
                        new ChoiceRankingDto { ChoiceId = choiceId, RankOrder = 0 }
                    }
                }
            }
        };

        var context2 = CreateTestDbContext(); // create a new context to avoid EF tracking issues

        var repository = new ResponseRepository(context2); // adjust to your real repo type
        var userRepository = new UserRepository(context2); // adjust to your real repo type
        var service2 = new ResponseService(repository, userRepository);

        // ACT
        var result = await service2.SubmitResponseAsync(response);

        // ASSERT
        Assert.NotNull(result);
        Assert.True(result.ResponseId > 0);

        
        // Survey navigation
        Assert.NotNull(result.Title); // crashes here if Survey was null

        // Answer navigation
        Assert.NotNull(result.Answers);
        Assert.Single(result.Answers);

        var answer = result.Answers.First();
        Assert.NotNull(answer.QuestionText);
        Assert.NotNull(answer.AnswerType);   

        // AnswerSelections navigation
        Assert.NotNull(answer.RankedChoices);
        Assert.Single(answer.RankedChoices);
        Assert.Equal(choiceId, answer.RankedChoices.First().ChoiceId);
        Assert.Equal(0, answer.RankedChoices.First().RankOrder);
    }

    [Fact]
    public async Task SubmitResponseAsync_SurveyDoesNotExist_ThrowsKeyNotFoundException()
    {
        using var context = CreateTestDbContext();
        var repository = new ResponseRepository(context); // adjust to your real repo 
        var userRepository = new UserRepository(context); // adjust to your real repo type
        var service = new ResponseService(repository, userRepository);

        var (survey, question, choices) = await SeedSurveyWithChoiceQuestion(
            context, isAnonymous: true, isRequired: false);
    

        var response = new ResponseCreateDto
        {
            SurveyId = -999, // guaranteed not to exist
            Answers = new List<AnswerCreateDto>()
            {
                new AnswerCreateDto
                {
                    QuestionId = question.Id,
                    AnswerType = enQuestionType.Checkbox,
                    AnswerValue = "",
                    RankedChoices = new List<ChoiceRankingDto>
                    {
                        new ChoiceRankingDto { ChoiceId = choices[0].Id, RankOrder = 0 }
                    }
                }
            }
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SubmitResponseAsync(response));
    }

    [Fact]
    public async Task SubmitResponseAsync_NonAnonymousSurveyMissingUserId_ThrowsInvalidOperationException()
    {
        using var context = CreateTestDbContext();
        var repository = new ResponseRepository(context); // adjust to your real repo 
        var userRepository = new UserRepository(context); // adjust to your real repo type
        var service = new ResponseService(repository, userRepository);

        var (survey, question, choices) = await SeedSurveyWithChoiceQuestion(
            context, isAnonymous: false, isRequired: false);

        var response = new ResponseCreateDto
        {
            SurveyId = survey.Id,
            UserId = null, // missing, but required since survey is NOT anonymous
            Answers = new List<AnswerCreateDto>
            {
                new AnswerCreateDto
                {
                    QuestionId = question.Id,
                    AnswerType = enQuestionType.Checkbox,
                    AnswerValue = "",
                    RankedChoices = new List<ChoiceRankingDto>
                    {
                        new ChoiceRankingDto { ChoiceId = choices[0].Id, RankOrder = 0 }
                    }
                }
            }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitResponseAsync(response));

        Assert.Contains("userId must be provided", ex.Message);
    }

    [Fact]
    public async Task SubmitResponseAsync_MissingRequiredQuestion_ThrowsInvalidOperationException()
    {
        using var context = CreateTestDbContext();
        var repository = new ResponseRepository(context); // adjust to your real repo 
        var userRepository = new UserRepository(context); // adjust to your real repo type
        var service = new ResponseService(repository, userRepository);

        var (survey, question, choices) = await SeedSurveyWithChoiceQuestion(
            context, isAnonymous: true, isRequired: true);

        // Response answers NO questions at all, but `question` is required
        var response = new ResponseCreateDto
        {
            SurveyId = survey.Id,
            Answers = new List<AnswerCreateDto>()
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitResponseAsync(response));

        Assert.Contains("required questions must be answered", ex.Message);
    }

    [Fact]
    public async Task SubmitResponseAsync_InvalidChoiceId_ThrowsInvalidOperationException()
    {
        // This test directly covers the bug from your SQL FK error (ChoiceId = 15 that doesn't exist)
        using var context = CreateTestDbContext();
        var repository = new ResponseRepository(context); // adjust to your real repo 
        var userRepository = new UserRepository(context); // adjust to your real repo type
        var service = new ResponseService(repository, userRepository);

        var (survey, question, choices) = await SeedSurveyWithChoiceQuestion(
            context, isAnonymous: true, isRequired: true);

        var response = new ResponseCreateDto
        {
            SurveyId = survey.Id,
            Answers = new List<AnswerCreateDto>
            {
                new AnswerCreateDto
                {
                    QuestionId = question.Id,
                    RankedChoices = new List<ChoiceRankingDto>
                    {
                        // ChoiceId that does NOT exist in the seeded Choices table
                        new ChoiceRankingDto { ChoiceId = 999999, RankOrder = 0 }
                    }
                }
            }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitResponseAsync(response));

        Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Just as important: confirm NOTHING was saved, since validation should
        // reject this before _context.Add/_context.SaveChangesAsync ever runs
        var savedCount = await context.Responses.CountAsync(r => r.SurveyId == survey.Id);
        Assert.Equal(0, savedCount);
    }
}