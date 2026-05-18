using Entities;
using Moq;
using SurveyBusinessLayer;
using SurveyDataAccessLayer.Interface;
using Xunit;

namespace SurveyBusinessLayer.Tests;

public class ChoiceServiceTests
{
    private readonly Mock<IChoiceRepository> _choiceRepositoryMock;
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly ChoiceService _choiceService;

    public ChoiceServiceTests()
    {
        _choiceRepositoryMock = new Mock<IChoiceRepository>();
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _choiceService = new ChoiceService(
            _choiceRepositoryMock.Object,
            _questionRepositoryMock.Object);
    }

    [Fact]
    public async Task GetChoicesByQuestionIdAsync_WithValidQuestionId_ReturnsChoices()
    {
        var choices = new List<Choice>
        {
            new() { Id = 1, QuestionId = 10, ChoiceText = "Yes", OrderIndex = 1 },
            new() { Id = 2, QuestionId = 10, ChoiceText = "No", OrderIndex = 2 }
        };

        _choiceRepositoryMock
            .Setup(r => r.GetChoicesByQuestionIdAsync(10))
            .ReturnsAsync(choices);

        var result = await _choiceService.GetChoicesByQuestionIdAsync(10);

        Assert.Equal(2, result.Count);
        Assert.Equal("Yes", result[0].ChoiceText);
        _choiceRepositoryMock.Verify(r => r.GetChoicesByQuestionIdAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetChoicesByQuestionIdAsync_WithInvalidId_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _choiceService.GetChoicesByQuestionIdAsync(0));

        Assert.Equal("Invalid question id", ex.Message);
    }

    [Fact]
    public async Task GetChoicesByQuestionIdAsync_WithNoChoices_ThrowsKeyNotFoundException()
    {
        _choiceRepositoryMock
            .Setup(r => r.GetChoicesByQuestionIdAsync(10))
            .ReturnsAsync(new List<Choice>());

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _choiceService.GetChoicesByQuestionIdAsync(10));

        Assert.Equal("No choices found", ex.Message);
    }

    [Fact]
    public async Task CreateChoiceAsync_WithValidChoice_ReturnsTrue()
    {
        var choice = new Choice
        {
            QuestionId = 5,
            ChoiceText = "Option 1",
            OrderIndex = 1
        };

        _questionRepositoryMock
            .Setup(r => r.GetQuestionByIdAsync(5))
            .ReturnsAsync(new Question { Id = 5, QuestionText = "Sample Question" });

        _choiceRepositoryMock
            .Setup(r => r.CreateChoiceAsync(choice))
            .ReturnsAsync(true);

        var result = await _choiceService.CreateChoiceAsync(choice);

        Assert.True(result);
        _choiceRepositoryMock.Verify(r => r.CreateChoiceAsync(choice), Times.Once);
    }

    [Fact]
    public async Task CreateChoiceAsync_WithNullChoice_ThrowsArgumentNullException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _choiceService.CreateChoiceAsync(null!));

        Assert.Equal("choice", ex.ParamName);
    }

    [Fact]
    public async Task CreateChoiceAsync_WithInvalidQuestionId_ThrowsArgumentException()
    {
        var choice = new Choice
        {
            QuestionId = 0,
            ChoiceText = "Option 1",
            OrderIndex = 1
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _choiceService.CreateChoiceAsync(choice));

        Assert.Equal("Invalid question id", ex.Message);
    }

    [Fact]
    public async Task CreateChoiceAsync_WithMissingQuestion_ThrowsKeyNotFoundException()
    {
        var choice = new Choice
        {
            QuestionId = 5,
            ChoiceText = "Option 1",
            OrderIndex = 1
        };

        _questionRepositoryMock
            .Setup(r => r.GetQuestionByIdAsync(5))
            .ReturnsAsync((Question?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _choiceService.CreateChoiceAsync(choice));

        Assert.Equal("No question found", ex.Message);
    }

    [Fact]
    public async Task UpdateChoiceAsync_WithValidChoice_ReturnsTrue()
    {
        var choice = new Choice
        {
            Id = 1,
            QuestionId = 5,
            ChoiceText = "Updated Option",
            OrderIndex = 1
        };

        _questionRepositoryMock
            .Setup(r => r.GetQuestionByIdAsync(5))
            .ReturnsAsync(new Question { Id = 5,  QuestionText = "Sample Question" });

        _choiceRepositoryMock
            .Setup(r => r.UpdateChoiceAsync(choice))
            .ReturnsAsync(true);

        var result = await _choiceService.UpdateChoiceAsync(choice);

        Assert.True(result);
        _choiceRepositoryMock.Verify(r => r.UpdateChoiceAsync(choice), Times.Once);
    }

    [Fact]
    public async Task UpdateChoiceAsync_WithNullChoice_ThrowsArgumentNullException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _choiceService.UpdateChoiceAsync(null!));

        Assert.Equal("choice", ex.ParamName);
    }

    [Fact]
    public async Task UpdateChoiceAsync_WithInvalidIds_ThrowsArgumentException()
    {
        var choice = new Choice
        {
            Id = 0,
            QuestionId = 5,
            ChoiceText = "Updated Option"
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _choiceService.UpdateChoiceAsync(choice));

        Assert.Equal("Invalid choice id or question id", ex.Message);
    }

    [Fact]
    public async Task UpdateChoiceAsync_WithMissingQuestion_ThrowsKeyNotFoundException()
    {
        var choice = new Choice
        {
            Id = 1,
            QuestionId = 5,
            ChoiceText = "Updated Option"
        };

        _questionRepositoryMock
            .Setup(r => r.GetQuestionByIdAsync(5))
            .ReturnsAsync((Question?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _choiceService.UpdateChoiceAsync(choice));

        Assert.Equal("No question found", ex.Message);
    }

    [Fact]
    public async Task DeleteChoiceAsync_WithValidId_ReturnsTrue()
    {
        _choiceRepositoryMock
            .Setup(r => r.DeleteChoiceAsync(1))
            .ReturnsAsync(true);

        var result = await _choiceService.DeleteChoiceAsync(1);

        Assert.True(result);
        _choiceRepositoryMock.Verify(r => r.DeleteChoiceAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteChoiceAsync_WithInvalidId_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _choiceService.DeleteChoiceAsync(0));

        Assert.Equal("Invalid choice id", ex.Message);
    }
}