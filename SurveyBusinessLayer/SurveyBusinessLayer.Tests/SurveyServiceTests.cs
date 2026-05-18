using Entities;
using Moq;
using SurveyDataAccessLayer.Interface;
using Xunit;

namespace SurveyBusinessLayer.Tests;

public class SurveyServiceTests
{
    private readonly Mock<ISurveyRepository> _mockRepository;
    private readonly SurveyService _surveyService;

    public SurveyServiceTests()
    {
        _mockRepository = new Mock<ISurveyRepository>();
        _surveyService = new SurveyService(_mockRepository.Object);
    }

    #region GetAllSurveysAsync Tests

    [Fact]
    public async Task GetAllSurveysAsync_WithValidSurveys_ReturnsListOfSurveys()
    {
        // Arrange
        var surveys = new List<Survey>
        {
            new() { Id = 1, Title = "Survey 1", UserId = "user1", Status = SurveyStatus.Draft },
            new() { Id = 2, Title = "Survey 2", UserId = "user2", Status = SurveyStatus.Published }
        };
        _mockRepository.Setup(r => r.GetAllSurveysAsync())
            .ReturnsAsync(surveys);

        // Act
        var result = await _surveyService.GetAllSurveysAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Survey 1", result[0].Title);
        _mockRepository.Verify(r => r.GetAllSurveysAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllSurveysAsync_WithEmptyList_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllSurveysAsync())
            .ReturnsAsync(new List<Survey>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _surveyService.GetAllSurveysAsync());
        Assert.Equal("Survey not found.", exception.Message);
    }

    [Fact]
    public async Task GetAllSurveysAsync_WithNullList_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllSurveysAsync())
            .ReturnsAsync((List<Survey>?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _surveyService.GetAllSurveysAsync());
        Assert.Equal("Survey not found.", exception.Message);
    }

    #endregion

    #region GetSurveyByIdAsync Tests

    [Fact]
    public async Task GetSurveyByIdAsync_WithValidId_ReturnsSurvey()
    {
        // Arrange
        var survey = new Survey
        {
            Id = 1,
            Title = "Test Survey",
            UserId = "user1",
            Status = SurveyStatus.Draft,
            CreatedDate = DateTime.UtcNow
        };
        _mockRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);

        // Act
        var result = await _surveyService.GetSurveyByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Survey", result.Title);
        _mockRepository.Verify(r => r.GetSurveyByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetSurveyByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetSurveyByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Survey?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _surveyService.GetSurveyByIdAsync(999));
        Assert.Equal("Survey not found.", exception.Message);
    }

    #endregion

    #region CreateSurveyAsync Tests

    [Fact]
    public async Task CreateSurveyAsync_WithValidSurvey_ReturnsCreatedId()
    {
        // Arrange
        var survey = new Survey
        {
            Title = "New Survey",
            UserId = "user1",
            Description = "Test Description",
            IsActive = true,
            IsAnonymous = false
        };
        _mockRepository.Setup(r => r.CreateSurveyAsync(It.IsAny<Survey>()))
            .ReturnsAsync(1);

        // Act
        var result = await _surveyService.CreateSurveyAsync(survey);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(SurveyStatus.Draft, survey.Status);
        Assert.Equal(0, survey.QuestionCount);
        _mockRepository.Verify(r => r.CreateSurveyAsync(It.IsAny<Survey>()), Times.Once);
    }

    [Fact]
    public async Task CreateSurveyAsync_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var survey = new Survey
        {
            Title = null!,
            UserId = "user1"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.CreateSurveyAsync(survey));
        Assert.Equal("Survey title is required.", exception.Message);
    }

    [Fact]
    public async Task CreateSurveyAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var survey = new Survey
        {
            Title = "   ",
            UserId = "user1"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.CreateSurveyAsync(survey));
        Assert.Equal("Survey title is required.", exception.Message);
    }

    [Fact]
    public async Task CreateSurveyAsync_WithNullUserId_ThrowsArgumentException()
    {
        // Arrange
        var survey = new Survey
        {
            Title = "Test Survey",
            UserId = null!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.CreateSurveyAsync(survey));
        Assert.Equal("User ID is required.", exception.Message);
    }

    #endregion

    #region UpdateSurveyAsync Tests

    [Fact]
    public async Task UpdateSurveyAsync_WithValidSurvey_ReturnsRowsAffected()
    {
        // Arrange
        var existingSurvey = new Survey
        {
            Id = 1,
            Title = "Old Title",
            UserId = "user1",
            Status = SurveyStatus.Draft
        };
        var updateSurvey = new Survey
        {
            Id = 1,
            Title = "New Title",
            UserId = "user1",
            Status = SurveyStatus.Draft
        };

        _mockRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(existingSurvey);
        _mockRepository.Setup(r => r.UpdateSurveyAsync(updateSurvey))
            .ReturnsAsync(1);

        // Act
        var result = await _surveyService.UpdateSurveyAsync(updateSurvey);

        // Assert
        Assert.Equal(1, result);
        _mockRepository.Verify(r => r.UpdateSurveyAsync(updateSurvey), Times.Once);
    }

    [Fact]
    public async Task UpdateSurveyAsync_WithPublishedSurvey_ThrowsInvalidOperationException()
    {
        // Arrange
        var publishedSurvey = new Survey
        {
            Id = 1,
            Title = "Test",
            UserId = "user1",
            Status = SurveyStatus.Published
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _surveyService.UpdateSurveyAsync(publishedSurvey));
        Assert.Equal("Cannot update a published survey.", exception.Message);
    }

    [Fact]
    public async Task UpdateSurveyAsync_WithMismatchedUserId_ThrowsArgumentException()
    {
        // Arrange
        var existingSurvey = new Survey
        {
            Id = 1,
            Title = "Test",
            UserId = "user1",
            Status = SurveyStatus.Draft
        };
        var updateSurvey = new Survey
        {
            Id = 1,
            Title = "Test",
            UserId = "user2",
            Status = SurveyStatus.Draft
        };

        _mockRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(existingSurvey);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.UpdateSurveyAsync(updateSurvey));
        Assert.Equal("User IDs do not match.", exception.Message);
    }

    [Fact]
    public async Task UpdateSurveyAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var survey = new Survey
        {
            Id = -1,
            Title = "Test",
            UserId = "user1"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.UpdateSurveyAsync(survey));
        Assert.Equal("Invalid survey ID.", exception.Message);
    }

    #endregion

    #region DeleteSurveyAsync Tests

    [Fact]
    public async Task DeleteSurveyAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteSurveyAsync(1))
            .ReturnsAsync(1);

        // Act
        var result = await _surveyService.DeleteSurveyAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteSurveyAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteSurveyAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.DeleteSurveyAsync(0));
        Assert.Equal("Invalid survey id.", exception.Message);
    }

    [Fact]
    public async Task DeleteSurveyAsync_WithNegativeId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _surveyService.DeleteSurveyAsync(-1));
        Assert.Equal("Invalid survey id.", exception.Message);
    }

    #endregion

    #region GetQuestionsForSurveyAsync Tests

    [Fact]
    public async Task GetQuestionsForSurveyAsync_WithValidSurveyId_ReturnsQuestionsAndChoices()
    {
        // Arrange
        var questions = new List<Question>
        {
            new() { Id = 1, SurveyId = 1, QuestionText = "Q1", QuestionType = QuestionType.Text }
        };
        var choices = new List<Choice>
        {
            new() { Id = 1, QuestionId = 1, ChoiceText = "Choice 1" }
        };

        _mockRepository.Setup(r => r.GetQuestionsForSurveyAsync(1))
            .ReturnsAsync((questions, choices));

        // Act
        var (resultQuestions, resultChoices) = await _surveyService.GetQuestionsForSurveyAsync(1);

        // Assert
        Assert.Single(resultQuestions);
        Assert.Single(resultChoices);
        _mockRepository.Verify(r => r.GetQuestionsForSurveyAsync(1), Times.Once);
    }

    #endregion
}