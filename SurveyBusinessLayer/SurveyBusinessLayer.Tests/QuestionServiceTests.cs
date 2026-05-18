using Entities;
using Moq;
using SurveyBusinessLayer;
using SurveyDataAccessLayer.Interface;
using Xunit;

namespace SurveyBusinessLayer.Tests;

public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<ISurveyRepository> _mockSurveyRepository;
    private readonly QuestionService _questionService;

    public QuestionServiceTests()
    {
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockSurveyRepository = new Mock<ISurveyRepository>();
        _questionService = new QuestionService(_mockQuestionRepository.Object, _mockSurveyRepository.Object);
    }

    #region GetAllQuestionsAsync Tests

    [Fact]
    public async Task GetAllQuestionsAsync_WithValidSurveyId_ReturnsQuestions()
    {
        // Arrange
        var questions = new List<Question>
        {
            new() { Id = 1, SurveyId = 1, QuestionText = "Q1", QuestionType = QuestionType.Text },
            new() { Id = 2, SurveyId = 1, QuestionText = "Q2", QuestionType = QuestionType.Radio }
        };
        _mockQuestionRepository.Setup(r => r.GetAllQuestionsAsync(1))
            .ReturnsAsync(questions);

        // Act
        var result = await _questionService.GetAllQuestionsAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Q1", result[0].QuestionText);
    }

    [Fact]
    public async Task GetAllQuestionsAsync_WithInvalidSurveyId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _questionService.GetAllQuestionsAsync(0));
        Assert.Equal("Invalid survey id", exception.Message);
    }

    [Fact]
    public async Task GetAllQuestionsAsync_WithNoQuestions_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockQuestionRepository.Setup(r => r.GetAllQuestionsAsync(1))
            .ReturnsAsync((List<Question>?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _questionService.GetAllQuestionsAsync(1));
        Assert.Equal("No questions found", exception.Message);
    }

    #endregion

    #region GetQuestionByIdAsync Tests

    [Fact]
    public async Task GetQuestionByIdAsync_WithValidId_ReturnsQuestion()
    {
        // Arrange
        var question = new Question
        {
            Id = 1,
            SurveyId = 1,
            QuestionText = "Test Question",
            QuestionType = QuestionType.Text
        };
        _mockQuestionRepository.Setup(r => r.GetQuestionByIdAsync(1))
            .ReturnsAsync(question);

        // Act
        var result = await _questionService.GetQuestionByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Question", result.QuestionText);
    }

    [Fact]
    public async Task GetQuestionByIdAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _questionService.GetQuestionByIdAsync(0));
        Assert.Equal("Invalid question id or survey id", exception.Message);
    }

    [Fact]
    public async Task GetQuestionByIdAsync_WithNotFoundQuestion_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockQuestionRepository.Setup(r => r.GetQuestionByIdAsync(999))
            .ReturnsAsync((Question?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _questionService.GetQuestionByIdAsync(999));
        Assert.Equal("No question found", exception.Message);
    }

    #endregion

    #region CreateQuestionAsync Tests

    [Fact]
    public async Task CreateQuestionAsync_WithValidTextQuestion_ReturnsQuestionId()
    {
        // Arrange
        var survey = new Survey { Id = 1, Status = SurveyStatus.Draft, UserId = "user1" };
        var question = new Question
        {
            QuestionText = "What is your name?",
            QuestionType = QuestionType.Text,
            IsRequired = true
        };

        _mockSurveyRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);
        _mockQuestionRepository.Setup(r => r.CreateQuestionAsync(1, question, null))
            .ReturnsAsync(1);

        // Act
        var result = await _questionService.CreateQuestionAsync(1, question, null!);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, question.SurveyId);
    }

    [Fact]
    public async Task CreateQuestionAsync_RadioQuestionWithoutChoices_ThrowsArgumentException()
    {
        // Arrange
        var survey = new Survey { Id = 1, Status = SurveyStatus.Draft, UserId = "user1" };
        var question = new Question
        {
            QuestionText = "Choose one",
            QuestionType = QuestionType.Radio
        };

        _mockSurveyRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _questionService.CreateQuestionAsync(1, question, new List<Choice>()));
        Assert.Equal("No choice found", exception.Message);
    }

    [Fact]
    public async Task CreateQuestionAsync_TextQuestionWithChoices_ThrowsArgumentException()
    {
        // Arrange
        var survey = new Survey { Id = 1, Status = SurveyStatus.Draft, UserId = "user1" };
        var question = new Question
        {
            QuestionText = "Your answer?",
            QuestionType = QuestionType.Text
        };
        var choices = new List<Choice>
        {
            new() { ChoiceText = "Choice 1" }
        };

        _mockSurveyRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _questionService.CreateQuestionAsync(1, question, choices));
        Assert.Equal("this type of question does not support choices", exception.Message);
    }

    [Fact]
    public async Task CreateQuestionAsync_EmptyQuestionText_ThrowsArgumentException()
    {
        // Arrange
        var question = new Question
        {
            QuestionText = "",
            QuestionType = QuestionType.Text
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _questionService.CreateQuestionAsync(1, question, null!));
        Assert.Equal("No question found", exception.Message);
    }

    [Fact]
    public async Task CreateQuestionAsync_PublishedSurvey_ThrowsInvalidOperationException()
    {
        // Arrange
        var survey = new Survey { Id = 1, Status = SurveyStatus.Published, UserId = "user1" };
        var question = new Question
        {
            QuestionText = "Question?",
            QuestionType = QuestionType.Text
        };

        _mockSurveyRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _questionService.CreateQuestionAsync(1, question, null!));
        Assert.Equal("Cannot modify questions in a published survey.", exception.Message);
    }

    #endregion

    #region UpdateQuestionAsync Tests

    [Fact]
    public async Task UpdateQuestionAsync_WithValidQuestion_ReturnsRowsAffected()
    {
        // Arrange
        var existingQuestion = new Question
        {
            Id = 1,
            SurveyId = 1,
            QuestionText = "Old Text",
            QuestionType = QuestionType.Text
        };
        var survey = new Survey { Id = 1, Status = SurveyStatus.Draft, UserId = "user1" };
        var updatedQuestion = new Question
        {
            Id = 1,
            SurveyId = 1,
            QuestionText = "New Text",
            QuestionType = QuestionType.Text
        };

        _mockQuestionRepository.Setup(r => r.GetQuestionByIdAsync(1))
            .ReturnsAsync(existingQuestion);
        _mockSurveyRepository.Setup(r => r.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);
        _mockQuestionRepository.Setup(r => r.UpdateQuestionAsync(updatedQuestion))
            .ReturnsAsync(1);

        // Act
        var result = await _questionService.UpdateQuestionAsync(updatedQuestion);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task UpdateQuestionAsync_NonExistentQuestion_ThrowsKeyNotFoundException()
    {
        // Arrange
        var question = new Question
        {
            Id = 999,
            QuestionText = "Test",
            QuestionType = QuestionType.Text
        };

        _mockQuestionRepository.Setup(r => r.GetQuestionByIdAsync(999))
            .ReturnsAsync((Question?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _questionService.UpdateQuestionAsync(question));
        Assert.Equal("No question found", exception.Message);
    }

    #endregion

    #region DeleteQuestionAsync Tests

    [Fact]
    public async Task DeleteQuestionAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        _mockQuestionRepository.Setup(r => r.DeleteQuestionAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _questionService.DeleteQuestionAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteQuestionAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _questionService.DeleteQuestionAsync(0));
        Assert.Equal("Invalid question id or survey id", exception.Message);
    }

    #endregion
}