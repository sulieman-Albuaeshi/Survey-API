using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Repository;
using Repository.Data;      
using Repository.Models;
using SurveyBusinessLayer;
using SurveyBusinessLayer.DTOs;

public class SurveyServiceTests
{
    // 1. This helper helper creates a fresh, empty database in RAM for every test
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

    [Fact]
    public async Task CreateSurvey_ShouldWorkFromDtoToDatabase()
    {
        // ==========================================
        //  ARRANGE: Set up your real classes & data
        // ==========================================
        using var context = CreateTestDbContext();

        // Pass the real DB context into your real repository and service
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // Create the exact DTO your frontend would send
        var incomingDto = new CreateSurveyDto
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


        var result = await service.CreateSurveyWithQuestionsAsync(incomingDto);

        // ==========================================
        //  ASSERT: Check if it actually worked
        // ==========================================
        var savedSurvey = await context.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(s => s.Id == result.Id);

        // 1. Core Survey Level Assertions
        Assert.NotNull(savedSurvey);
        Assert.Equal("Junior Safe Test", savedSurvey.Title);
        Assert.Equal("Testing the full pipeline!", savedSurvey.Description);
        Assert.True(savedSurvey.IsAnonymous);
        Assert.Equal(SurveyStatus.Draft, savedSurvey.Status);
        Assert.Equal("test-user-123", savedSurvey.UserId);
        Assert.Equal(1, savedSurvey.QuestionCount); 

        // 2. Question Level Assertions
        Assert.Single(savedSurvey.Questions); 
        var question = savedSurvey.Questions.First();
        Assert.Equal("Is this easy?", question.QuestionText);
        Assert.True(question.IsRequired);
        Assert.Equal(1, question.OrderIndex);
        //Assert.Equal(2, question.QuestionTypeId);

        // 3. Nested Choices Level Assertions
        Assert.Equal(2, question.Choices.Count);

        var choice1 = question.Choices.First(c => c.ChoiceText == "Yes");
        Assert.Equal(1, choice1.OrderIndex);

        var choice2 = question.Choices.First(c => c.ChoiceText == "Absolutely");
        Assert.Equal(2, choice2.OrderIndex);
    }

    [Fact]
    public async Task CreateSurvey_withValidQuestionTypeAndChoices_WhenValidQuestionType()
    {
        // ==========================================
        //  ARRANGE: Set up your real classes & data
        // ==========================================
        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);
        var incomingDto = new CreateSurveyDto
        {
            Title = "Nested Test Survey",
            Description = "Testing nested questions and choices.",
            IsAnonymous = false,
            Status = "Draft",
            userId = "test-user-456",
            Questions = new List<CreateQuestionDto>
            {
                new CreateQuestionDto
                {
                    QuestionText = "What is your favorite color?",
                    IsRequired = true,
                    QuestionType = "Radio",
                    Choices = new List<CreateChoiceDto>
                    {
                        new CreateChoiceDto{ ChoiceText = "Red" },
                        new CreateChoiceDto{ ChoiceText = "Blue" },
                        new CreateChoiceDto{ ChoiceText = "Green" }
                    }
                }
            }
        };
        // ==========================================
        //  ACT: Call the service to create the survey
        // ==========================================
        var result = await service.CreateSurveyWithQuestionsAsync(incomingDto);
        // ==========================================
        //  ASSERT: Verify the survey and its nested data persisted correctly
        // ==========================================
        var savedSurvey = await context.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(s => s.Id == result.Id);
        Assert.NotNull(savedSurvey);
        Assert.Equal("Nested Test Survey", savedSurvey.Title);
        Assert.Equal(1, savedSurvey.QuestionCount);
        var question = savedSurvey.Questions.First();
        Assert.Equal("What is your favorite color?", question.QuestionText);
        Assert.Equal(3, question.Choices.Count);
    }

    [Fact]
    public async Task CreateSurvey_withValidQuestionTypeAndChoices_WhenValidQuestionTypeDoesNotRequireChoices()
    {
        // ==========================================
        //  ARRANGE: Set up your real classes & data
        // ==========================================
        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);
        var incomingDto = new CreateSurveyDto
        {
            Title = "Nested Test Survey",
            Description = "Testing nested questions and choices.",
            IsAnonymous = false,
            Status = "Draft",
            userId = "test-user-456",
            Questions = new List<CreateQuestionDto>
            {
                new CreateQuestionDto
                {
                    QuestionText = "What is your favorite color?",
                    IsRequired = true,
                    QuestionType = "Text",
                    Choices = new List<CreateChoiceDto>
                    {
                        new CreateChoiceDto{ ChoiceText = "Red" },
                        new CreateChoiceDto{ ChoiceText = "Blue" },
                        new CreateChoiceDto{ ChoiceText = "Green" }
                    }
                }
            }
        };
        // ==========================================
        //  ACT: Call the service to create the survey
        // ==========================================
        var exsiption = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSurveyWithQuestionsAsync(incomingDto));

        Assert.Contains("Text question must not have choices.", exsiption.Message);
    }
    [Fact]
    public async Task CreateSurvey_ShouldThrowArgumentException_WhenTitleIsMissing()
    {
        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);
        var incomingDto = new CreateSurveyDto
        {
            Title = "", // Missing title
            Description = "This survey has no title.",
            IsAnonymous = true,
            Status = "Draft",
            userId = "test-user-789",
            Questions = new List<CreateQuestionDto>()
        };
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSurveyWithQuestionsAsync(incomingDto));
    }

    [Fact]
    public async Task CreateSurvey_ShouldThrowArgumentException_WhenQuestionTypeIsInvalid()
    {
        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);
        var incomingDto = new CreateSurveyDto
        {
            Title = "Invalid Question Type Survey",
            Description = "This survey has an invalid question type.",
            IsAnonymous = false,
            Status = "Draft",
            userId = "test-user-101",
            Questions = new List<CreateQuestionDto>
            {
                new CreateQuestionDto
                {
                    QuestionText = "What is your favorite fruit?",
                    IsRequired = true,
                    QuestionType = "InvalidType", // Invalid question type
                    Choices = new List<CreateChoiceDto>
                    {
                        new CreateChoiceDto{ ChoiceText = "Apple" },
                        new CreateChoiceDto{ ChoiceText = "Banana" }
                    }
                }
            }
        };
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSurveyWithQuestionsAsync(incomingDto));
    }

    [Fact]
    public async Task CreateSurvey_ShouldThrowArgumentException_WhenQuestionRequiredChoices()
    {
        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);
        var incomingDto = new CreateSurveyDto
        {
            Title = "No Choices Survey",
            Description = "This survey has a question with no choices.",
            IsAnonymous = true,
            Status = "Draft",
            userId = "test-user-202",
            Questions = new List<CreateQuestionDto>
            {
                new CreateQuestionDto
                {
                    QuestionText = "What is your favorite season?",
                    IsRequired = true,
                    QuestionType = "Radio", // Valid type
                    Choices = new List<CreateChoiceDto>() // No choices provided
                }
            }
        };
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSurveyWithQuestionsAsync(incomingDto));
    }

    [Fact]
    public async Task UpdateSurveyWithQuestionsAsync_ShouldModifyExistingSurveyAndMaintainDataGraph()
    {

        // To seed the memmory DB with an existing survey
        int existingSurveyId;
        int existingQuestionId;
        int existingChoiceId;
        using (var seedContext = CreateTestDbContext())
        {
            var existingSurvey = new Survey
            {
                Title = "Original Survey Title",
                Description = "Original Description",
                IsAnonymous = false,
                Status = SurveyStatus.Draft,
                UserId = "test-user-123",
                QuestionCount = 1,
                Questions = new List<Question>
            {
                new Question
                {
                    QuestionText = "Original Question 1",
                    IsRequired = false,
                    OrderIndex = 1,
                    QuestionTypeId = 1,
                    Choices = new List<Choice>
                    {
                        new Choice { ChoiceText = "Original Choice 1", OrderIndex = 1 },
                        new Choice { ChoiceText = "deleted Choice", OrderIndex = 2 }
                    }
                }
            }
            };

            await seedContext.Surveys.AddAsync(existingSurvey);
            await seedContext.SaveChangesAsync();

            // Capture the real database IDs generated by EF Core
            existingSurveyId = existingSurvey.Id;
            existingQuestionId = existingSurvey.Questions.First().Id;
            existingChoiceId = existingSurvey.Questions.First().Choices.First().Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // Build the mixed payload: updating old items + adding new items
        var updateDto = new UpdaatSurveyDto
        {
            Id = existingSurveyId,
            Title = "Updated Survey Title",
            Description = "Updated Description",
            IsAnonymous = true,
            Status = "Draft",
            Questions = new List<UpdateQuestionDto>
            {
                // ─── QUESTION 1: EXISTING (UPDATED) ───
                new UpdateQuestionDto
                {
                    Id = existingQuestionId,
                    QuestionText = "Altered Text for Existing Question 1",
                    IsRequired = true,
                    QuestionType = "Radio",
                    Choices = new List<updateChoiceDto>
                    {
                        // Choice A: Existing item updated
                        new updateChoiceDto { Id = existingChoiceId, ChoiceText = "Altered Text for Existing Choice 1" },
                    
                        // Choice B: Brand new item added to this existing question
                        new updateChoiceDto { Id = 0, ChoiceText = "Brand New Choice Added to Old Question" }
                    }
                },

                // ─── QUESTION 2: BRAND NEW (INSERTED) ───
                new UpdateQuestionDto
                {
                    Id = 0, // 👈 0 tells EF Core to INSERT this new question
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

        // ==========================================
        //  ACT: Call the update pipeline
        // ==========================================
        var responseDto = await service.UpdateSurveyWithQuestionsAsync(updateDto);

        // ==========================================
        //  ASSERT: Check if modifications persisted safely
        // ==========================================
        var dbRecord = await context.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Choices)
            .FirstOrDefaultAsync(s => s.Id == existingSurveyId);

        // 1. Root Survey Asserts
        Assert.NotNull(dbRecord);
        Assert.Equal("Updated Survey Title", dbRecord.Title);
        Assert.Equal(2, dbRecord.QuestionCount); // 👈 Verify counter tracked both questions

        // 2. Question Graph Structure Asserts
        Assert.Equal(2, dbRecord.Questions.Count); // Should have exactly 2 questions total

        // Find the original updated question by its identity
        var updatedQuestion = dbRecord.Questions.FirstOrDefault(q => q.Id == existingQuestionId);
        Assert.NotNull(updatedQuestion);
        Assert.Equal("Altered Text for Existing Question 1", updatedQuestion.QuestionText);
        Assert.Equal(1, updatedQuestion.OrderIndex); 
        Assert.Equal(2, updatedQuestion.Choices.Count); 

        // Verify choice modifications inside the old question
        var modifiedChoice = updatedQuestion.Choices.FirstOrDefault(c => c.Id == existingChoiceId);
        Assert.NotNull(modifiedChoice);
        Assert.Equal("Altered Text for Existing Choice 1", modifiedChoice.ChoiceText);
        Assert.Equal(1, modifiedChoice.OrderIndex);

        var newlyAddedChoice = updatedQuestion.Choices.FirstOrDefault(c => c.Id != existingChoiceId);
        Assert.NotNull(newlyAddedChoice);
        Assert.Equal("Brand New Choice Added to Old Question", newlyAddedChoice.ChoiceText);
        Assert.Equal(2, newlyAddedChoice.OrderIndex); 

        var deletedChoice = updatedQuestion.Choices.FirstOrDefault(c => c.ChoiceText == "deleted Choice");
        Assert.Null(deletedChoice); 

        // Find the brand new question by looking for the one that isn't the old ID
        var insertedQuestion = dbRecord.Questions.FirstOrDefault(q => q.Id != existingQuestionId);
        Assert.NotNull(insertedQuestion);
        Assert.Equal("Completely New Question 2", insertedQuestion.QuestionText);
        Assert.Equal(2, insertedQuestion.OrderIndex);

        // Verify choice inside the brand new question
        Assert.Single(insertedQuestion.Choices);
        var targetNewChoice = insertedQuestion.Choices.First();
        Assert.Equal("Brand New Choice inside New Question", targetNewChoice.ChoiceText);
        Assert.Equal(1, targetNewChoice.OrderIndex); // First choice in the new question gets index 1
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldSuccessfullyPublishDraft_WhenSurveyHasQuestions()
    {
        // ==========================================
        //  ARRANGE: Seed a Draft survey with a question
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var draftSurvey = new Survey
            {
                Title = "Ready to Publish Survey",
                Description = "Has questions, ready to go live.",
                IsAnonymous = false,
                Status = SurveyStatus.Draft,
                UserId = "test-user-123",
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionText = "Do you like writing integration tests?",
                        OrderIndex = 1,
                        QuestionTypeId = 1
                    }
                }
            };
            await seedContext.Surveys.AddAsync(draftSurvey);
            await seedContext.SaveChangesAsync();
            testSurveyId = draftSurvey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT: Transition Draft -> Published
        // ==========================================
        // (Assuming your Service maps this call down to your Repository method)
        var result = await service.ChangeSurveyStatusAsync(testSurveyId, "Published");

        // ==========================================
        //  ASSERT: Verify DB entry switched cleanly
        // ==========================================
        Assert.True(result);

        var updatedSurvey = await context.Surveys.FindAsync(testSurveyId);
        Assert.NotNull(updatedSurvey);
        Assert.Equal(SurveyStatus.Published, updatedSurvey.Status);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowInvalidOperationException_WhenPublishingDraftWithNoQuestions()
    {
        // ==========================================
        //  ARRANGE: Seed an empty Draft survey (0 questions)
        // ==========================================
        int ghostSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var ghostSurvey = new Survey
            {
                Title = "Empty Ghost Survey",
                Description = "Accidentally left blank.",
                Status = SurveyStatus.Draft,
                UserId = "test-user-123"
            };
            await seedContext.Surveys.AddAsync(ghostSurvey);
            await seedContext.SaveChangesAsync();
            ghostSurveyId = ghostSurvey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Validation rule must halt execution
        // ==========================================
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeSurveyStatusAsync(ghostSurveyId, "Published"));

        Assert.Contains("cannot publish a survey that has no questions", exception.Message);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowInvalidOperationException_WhenMakingIllegalStateJump()
    {
        // ==========================================
        //  ARRANGE: Seed a Draft survey
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var draftSurvey = new Survey
            {
                Title = "Strict State Survey",
                Status = SurveyStatus.Draft,
                UserId = "test-user-123"
            };
            await seedContext.Surveys.AddAsync(draftSurvey);
            await seedContext.SaveChangesAsync();
            testSurveyId = draftSurvey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Cannot jump from Draft straight to Archived
        // ==========================================
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeSurveyStatusAsync(testSurveyId, "Archived"));

        Assert.Contains("Cannot change status of a Draft survey directly to Archived", exception.Message);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowArgumentException_WhenStatusTextIsGarbage()
    {
        // ==========================================
        //  ARRANGE: Seed a survey
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var survey = new Survey { Title = "Typo Test", Status = SurveyStatus.Draft, UserId = "test-user-123" };
            await seedContext.Surveys.AddAsync(survey);
            await seedContext.SaveChangesAsync();
            testSurveyId = survey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Text parsing must fail gracefully
        // ==========================================
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ChangeSurveyStatusAsync(testSurveyId, "NotAValidStatusName"));

        Assert.Contains("Invalid status", exception.Message);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
    {
        // ==========================================
        //  ARRANGE: Empty environment
        // ==========================================
        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Handled database miss
        // ==========================================
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ChangeSurveyStatusAsync(99999, "Published"));
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowInvalidOperationException_WhenTargetStatusMatchesCurrentStatus()
    {
        // ==========================================
        //  ARRANGE: Seed a survey in Published status
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var survey = new Survey { Title = "Status Match Test", Status = SurveyStatus.Published, UserId = "test-user-123" };
            await seedContext.Surveys.AddAsync(survey);
            await seedContext.SaveChangesAsync();
            testSurveyId = survey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Trying to change to "Published" again must throw
        // ==========================================
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeSurveyStatusAsync(testSurveyId, "Published"));

        Assert.Contains("already in the Published status", exception.Message);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowInvalidOperationException_WhenChangingPublishedToAnythingButArchived()
    {
        // ==========================================
        //  ARRANGE: Seed a Published survey
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var survey = new Survey { Title = "Published Guard Test", Status = SurveyStatus.Published, UserId = "test-user-123" };
            await seedContext.Surveys.AddAsync(survey);
            await seedContext.SaveChangesAsync();
            testSurveyId = survey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Changing Published -> Draft must throw
        // ==========================================
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeSurveyStatusAsync(testSurveyId, "Draft"));

        Assert.Contains("Cannot change status of a Published survey to anything other than Archived", exception.Message);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldThrowInvalidOperationException_WhenChangingArchivedToAnythingButDraft()
    {
        // ==========================================
        //  ARRANGE: Seed an Archived survey
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var survey = new Survey { Title = "Archived Guard Test", Status = SurveyStatus.Archived, UserId = "test-user-123" };
            await seedContext.Surveys.AddAsync(survey);
            await seedContext.SaveChangesAsync();
            testSurveyId = survey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT & ASSERT: Changing Archived -> Published must throw
        // ==========================================
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeSurveyStatusAsync(testSurveyId, "Published"));

        Assert.Contains("Cannot change status of an Archived survey to anything other than Draft", exception.Message);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldSuccessfullyArchive_WhenSurveyIsPublished()
    {
        // ==========================================
        //  ARRANGE: Seed a Published survey
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var survey = new Survey { Title = "Live Survey", Status = SurveyStatus.Published, UserId = "test-user-123" };
            await seedContext.Surveys.AddAsync(survey);
            await seedContext.SaveChangesAsync();
            testSurveyId = survey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT: Transition Published -> Archived
        // ==========================================
        var result = await service.ChangeSurveyStatusAsync(testSurveyId, "Archived");

        // ==========================================
        //  ASSERT: Ensure state changed successfully
        // ==========================================
        Assert.True(result);

        var updatedSurvey = await context.Surveys.FindAsync(testSurveyId);
        Assert.NotNull(updatedSurvey);
        Assert.Equal(SurveyStatus.Archived, updatedSurvey.Status);
    }

    [Fact]
    public async Task ChangeSurveyStatusAsync_ShouldSuccessfullyRevertToDraft_WhenSurveyIsArchived()
    {
        // ==========================================
        //  ARRANGE: Seed an Archived survey
        // ==========================================
        int testSurveyId;
        using (var seedContext = CreateTestDbContext())
        {
            var survey = new Survey { Title = "Old Archived Survey", Status = SurveyStatus.Archived, UserId = "test-user-123" };
            await seedContext.Surveys.AddAsync(survey);
            await seedContext.SaveChangesAsync();
            testSurveyId = survey.Id;
        }

        using var context = CreateTestDbContext();
        var repository = new SurveyRepository(context);
        var service = new SurveyService(repository);

        // ==========================================
        //  ACT: Transition Archived -> Draft
        // ==========================================
        var result = await service.ChangeSurveyStatusAsync(testSurveyId, "Draft");

        // ==========================================
        //  ASSERT: Ensure state reverted back to Draft
        // ==========================================
        Assert.True(result);

        var updatedSurvey = await context.Surveys.FindAsync(testSurveyId);
        Assert.NotNull(updatedSurvey);
        Assert.Equal(SurveyStatus.Draft, updatedSurvey.Status);
    }
}