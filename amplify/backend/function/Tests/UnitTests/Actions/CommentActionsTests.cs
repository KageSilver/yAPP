using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.Models;
using yAppLambda.DynamoDB;

namespace Tests.UnitTests.Actions;

public class CommentActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly ICommentActions _commentActionsMock;
    private const string CommentTableName = "Comment-test";

    public CommentActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(CommentTableName);

        // Initialize the dynamoDbContextMock
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        
        // Initialize the CommentActions with the mocks
        _commentActionsMock = new CommentActions(_appSettingsMock.Object, _dynamoDbContextMock.Object);
    }
    
    #region CreateComment Tests

    [Fact]
    public async Task CreateComment_ShouldReturnOK_WhenCommentIsCreatedSuccessfully()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "1",
            CreatedAt = now,
            UpdatedAt = now,
            UserName = "user1@example.com",
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        _appSettingsMock.Setup(a => a.CommentTableName).Returns(CommentTableName);

        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _commentActionsMock.CreateComment(comment);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Comment>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedComment = Assert.IsType<Comment>(okResult.Value);
        Assert.Equal(comment.CID, returnedComment.CID);
        Assert.Equal(comment.PID, returnedComment.PID);
        Assert.Equal(comment.CreatedAt, returnedComment.CreatedAt);
        Assert.Equal(comment.UpdatedAt, returnedComment.UpdatedAt);
        Assert.Equal(comment.UserName, returnedComment.UserName);
        Assert.Equal(comment.CommentBody, returnedComment.CommentBody);
        Assert.Equal(comment.Upvotes, returnedComment.Upvotes);
        Assert.Equal(comment.Downvotes, returnedComment.Downvotes);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "1",
            CreatedAt = now,
            UpdatedAt = now,
            UserName = "user1@example.com",
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        _appSettingsMock.Setup(a => a.CommentTableName).Returns(CommentTableName);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error saving to DynamoDB"));
            
        // Act
        var result = await _commentActionsMock.CreateComment(comment);

        // Assert
        // Assert that the result is ActionResult<Comment>
        var actionResult = Assert.IsType<ActionResult<Comment>>(result); 
        // Access the Result property to get the actual StatusCodeResult
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    #endregion

    #region GetCommentById Tests

    [Fact]
    public async Task GetCommentById_ShouldReturnComment_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "11111",
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UserName = "Anonymous",
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        // Sets up LoadAsync to return the request comment
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _commentActionsMock.GetCommentById(request.CID);

        // Assert
        var returnedComment = Assert.IsType<Comment>(result);
        Assert.Equal(request.CID, returnedComment.CID);
        Assert.Equal(request.PID, returnedComment.PID);
        Assert.Equal(request.CreatedAt, returnedComment.CreatedAt);
        Assert.Equal(request.UpdatedAt, returnedComment.UpdatedAt);
        Assert.Equal("Anonymous", returnedComment.UserName);
        Assert.Equal(request.CommentBody, returnedComment.CommentBody);
        Assert.Equal(request.Upvotes, returnedComment.Upvotes);
        Assert.Equal(request.Downvotes, returnedComment.Downvotes);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Comment>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCommentById_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error loading comment"));
            
        // Act
        var result = await _commentActionsMock.GetCommentById("111");
        
        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Comment>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetCommentsByUser Tests
    
    [Fact]
    public async Task GetCommentsByUser_ShouldReturnComments_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "1",
            UserName = "username",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        var list = new List<Comment>();
        list.Add(comment);

        // Mock the AsyncSearch<Comment> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Comment>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Comment>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _commentActionsMock.GetCommentsByUser(comment.UserName);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(comment.CID, result.First().CID);
        Assert.Equal(comment.PID, result.First().PID);
        Assert.Equal(comment.CreatedAt, result.First().CreatedAt);
        Assert.Equal(comment.UpdatedAt, result.First().UpdatedAt);
        Assert.Equal(comment.UserName, result.First().UserName);
        Assert.Equal(comment.CommentBody, result.First().CommentBody);
        Assert.Equal(comment.Upvotes, result.First().Upvotes);
        Assert.Equal(comment.Downvotes, result.First().Downvotes);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Comment>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task GetCommentsByUser_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Comment>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error querying comments"));

        // Act
        var result = await _commentActionsMock.GetCommentsByUser("username");

        // Assert
        Assert.Empty(result);
    }
    
    #endregion

    #region GetCommentsByPid Tests
    
    [Fact]
    public async Task GetCommentsByPid_ShouldReturnComments_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "1",
            UserName = "username",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        var list = new List<Comment>();
        list.Add(comment);

        // Mock the AsyncSearch<Comment> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Comment>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Comment>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _commentActionsMock.GetCommentsByPid(comment.PID);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(comment.CID, result.First().CID);
        Assert.Equal(comment.PID, result.First().PID);
        Assert.Equal(comment.CreatedAt, result.First().CreatedAt);
        Assert.Equal(comment.UpdatedAt, result.First().UpdatedAt);
        Assert.Equal(comment.UserName, result.First().UserName);
        Assert.Equal(comment.CommentBody, result.First().CommentBody);
        Assert.Equal(comment.Upvotes, result.First().Upvotes);
        Assert.Equal(comment.Downvotes, result.First().Downvotes);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Comment>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task GetCommentsByPid_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Comment>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error querying comments"));

        // Act
        var result = await _commentActionsMock.GetCommentsByPid("!");

        // Assert
        Assert.Empty(result);
    }
    
    #endregion

}
