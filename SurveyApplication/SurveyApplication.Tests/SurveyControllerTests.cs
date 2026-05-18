using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SurveyApplication.Controllers;
using SurveyApplication.Mapper;
using SurveyBusinessLayer.Interface;
using Xunit;

namespace SurveyApplication.Tests;

public class SurveyControllerTests
{
    private readonly Mock<ISurveyService> _mockSurveyService;
    private readonly SurveyController _controller;

    public SurveyControllerTests()
    {
        _mockSurveyService = new Mock<ISurveyService>();
        _controller = new SurveyController(_mockSurveyService.Object);
    }

    #region GetAllSurveys Tests

    [Fact]
    public async Task GetAllSurveys_WithValidSurveys_ReturnsOkWithSurveyList()
    {
        // Arrange
        var surveys = new List<Survey>
        {
            new() { Id = 1, Title = "Survey 1", Status = SurveyStatus.Draft, CreatedDate = DateTime.UtcNow },
            new() { Id = 2, Title = "Survey 2", Status = SurveyStatus.Published, CreatedDate = DateTime.UtcNow }
        };
        _mockSurveyService.Setup(s => s.GetAllSurveysAsync())
            .ReturnsAsync(surveys);

        // Act
        var result = await _controller.GetAllSurveys();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedSurveys = Assert.IsType<List<SurveyTableDto>>(okResult.Value);
        Assert.Equal(2, returnedSurveys.Count);
    }

    [Fact]
    public async Task GetAllSurveys_WithNoSurveys_ReturnsNotFound()
    {
        // Arrange
        _mockSurveyService.Setup(s => s.GetAllSurveysAsync())
            .ThrowsAsync(new KeyNotFoundException("Survey not found."));

        // Act
        var result = await _controller.GetAllSurveys();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region GetSurveyById Tests

    [Fact]
    public async Task GetSurveyById_WithValidId_ReturnsOkWithSurveyDetails()
    {
        // Arrange
        var survey = new Survey
        {
            Id = 1,
            Title = "Test Survey",
            Status = SurveyStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            UserId = "user1"
        };
        var questions = new List<Question>();
        var choices = new List<Choice>();

        _mockSurveyService.Setup(s => s.GetSurveyByIdAsync(1))
            .ReturnsAsync(survey);
        _mockSurveyService.Setup(s => s.GetQuestionsForSurveyAsync(1))
            .ReturnsAsync((questions, choices));

        // Act
        var result = await _controller.GetSurveyByIdAsync(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetSurveyById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockSurveyService.Setup(s => s.GetSurveyByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Survey not found."));

        // Act
        var result = await _controller.GetSurveyByIdAsync(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetSurveyById_WithException_ReturnsInternalServerError()
    {
        // Arrange
        _mockSurveyService.Setup(s => s.GetSurveyByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetSurveyByIdAsync(1);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, errorResult.StatusCode);
    }

    #endregion

    #region CreateSurvey Tests

    [Fact]
    public async Task CreateSurvey_WithValidDto_ReturnsCreatedAtRoute()
    {
        // Arrange
        var surveyDto = new SurveyDto
        {
            Title = "New Survey",
            Description = "Test",
            IsActive = true,
            IsAnonymous = false,
            UserId = "user1",
            Status = "Draft"
        };
        _mockSurveyService.Setup(s => s.CreateSurveyAsync(It.IsAny<Survey>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.CreateSurvey(surveyDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("GetAllSurveys", createdResult.RouteName);
    }

    [Fact]
    public async Task CreateSurvey_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var surveyDto = new SurveyDto
        {
            Title = "",
            UserId = "user1"
        };
        _mockSurveyService.Setup(s => s.CreateSurveyAsync(It.IsAny<Survey>()))
            .ThrowsAsync(new ArgumentException("Survey title is required."));

        // Act
        var result = await _controller.CreateSurvey(surveyDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    #endregion

    #region UpdateSurvey Tests

    [Fact]
    public async Task UpdateSurvey_WithValidData_ReturnsOkWithUpdatedSurvey()
    {
        // Arrange
        var surveyDto = new SurveyDto
        {
            Id = 1,
            Title = "Updated Survey",
            UserId = "user1",
            Status = "Draft",
            IsActive = true,
            IsAnonymous = false
        };
        _mockSurveyService.Setup(s => s.UpdateSurveyAsync(It.IsAny<Survey>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateSurvey(1, surveyDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateSurvey_WithPublishedSurvey_ReturnsConflict()
    {
        // Arrange
        var surveyDto = new SurveyDto
        {
            Id = 1,
            Title = "Survey",
            UserId = "user1",
            Status = "Published"
        };
        _mockSurveyService.Setup(s => s.UpdateSurveyAsync(It.IsAny<Survey>()))
            .ThrowsAsync(new InvalidOperationException("Cannot update a published survey."));

        // Act
        var result = await _controller.UpdateSurvey(1, surveyDto);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(409, conflictResult.StatusCode);
    }
    [Fact]
    public async Task UpdateSurvey_WithNonExistentSurvey_ReturnsNotFound()
    {
        // Arrange
        var surveyDto = new SurveyDto
        {
            Id = 999,
            Title = "Survey",
            UserId = "user1"
        };
        _mockSurveyService.Setup(s => s.UpdateSurveyAsync(It.IsAny<Survey>()))
            .ThrowsAsync(new KeyNotFoundException("Survey not found."));

        // Act
        var result = await _controller.UpdateSurvey(999, surveyDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region DeleteSurvey Tests

    [Fact]
    public async Task DeleteSurvey_WithValidId_ReturnsOk()
    {
        // Arrange
        _mockSurveyService.Setup(s => s.DeleteSurveyAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSurvey(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task DeleteSurvey_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockSurveyService.Setup(s => s.DeleteSurveyAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteSurvey(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task DeleteSurvey_WithException_ReturnsInternalServerError()
    {
        // Arrange
        _mockSurveyService.Setup(s => s.DeleteSurveyAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteSurvey(1);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);
    }

    #endregion
}