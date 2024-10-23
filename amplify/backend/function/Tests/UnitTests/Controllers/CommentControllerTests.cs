using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.UnitTests.Controllers;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using yAppLambda.Controllers;
using yAppLambda.Models;
using yAppLambda.Common;
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.DynamoDB;

public class CommentControllerTests
{
    private readonly Mock<IAppSettings> _mockAppSettings;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly CommentController _commentController;
    private readonly Mock<ICommentActions> _mockCommentActions;

    public CommentControllerTests()
    {
        _mockAppSettings = new Mock<IAppSettings>();
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockCommentActions = new Mock<ICommentActions>();
        _commentController = new CommentController(_mockAppSettings.Object, _mockCognitoActions.Object,
            _dynamoDbContextMock.Object, _mockCommentActions.Object);
    }

    #region CreateComment Tests

    [Fact]
    public async Task CreateComment_ShouldReturnBadResult_WhenRequestIsNull()
    {
        // Arrange
        NewComment request = null;

        // Act
        var result = await _commentController.CreateComment(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Request body is required and must contain commenter's uid, comment body, and the original post's id.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnNotFound_WhenUidIsNotFound()
    {
        // Arrange
        var request = new NewComment { PID = "20", UID = "userDoesNotExist", CommentBody = "body" };
        _mockCognitoActions.Setup(u => u.GetUserById(request.UID)).ReturnsAsync((User)null);

        // Act
        var result = await _commentController.CreateComment(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Comment creator not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnOK_WhenCommentIsCreatedSuccessfully()
    {
        // Arrange 
        var request = new NewComment { PID = "1", UID = "uid", CommentBody = "body" };
        var commenter = new User {Id = "uid" };
        // Omitting 
        var comment = new Comment 
        { 
            PID = "1",
            UID = request.UID, 
            CommentBody = request.CommentBody, 
            Upvotes = 0, 
            Downvotes = 0
        };

        // Mock GetUserById to return the commenter
        _mockCognitoActions.Setup(u => u.GetUserById(request.UID)).ReturnsAsync(commenter);

        // Mock CreateComment to return a new Comment object
        _mockCommentActions.Setup(c => c.CreateComment(It.IsAny<Comment>())).ReturnsAsync(new OkObjectResult(comment));

        // Act
        var result = await _commentController.CreateComment(request);

        // Assert
        var returnedComment = result.Value;
        // cannot assert CID being equal since CID is randomly generated
        Assert.Equal(comment.PID, returnedComment.PID);
        Assert.Equal(comment.UID, returnedComment.UID);
        Assert.Equal(comment.CommentBody, returnedComment.CommentBody);
        Assert.Equal(comment.Upvotes, returnedComment.Upvotes);
        Assert.Equal(comment.Downvotes, returnedComment.Downvotes);
    }
    
    #endregion

    #region GetCommentById Tests

    [Fact]
    public async Task GetCommentById_ShouldReturnComment_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        _mockCommentActions.Setup(c => c.GetCommentById(It.IsAny<string>())).ReturnsAsync(comment);

        // Act
        var result = await _commentController.GetCommentById(comment.CID);

        // Assert
        var returnedComment = Assert.IsType<Comment>(result.Value);
        Assert.Equal(comment.CID, returnedComment.CID);
        Assert.Equal(comment.PID, returnedComment.PID);
        Assert.Equal(comment.CreatedAt, returnedComment.CreatedAt);
        Assert.Equal(comment.UpdatedAt, returnedComment.UpdatedAt);
        Assert.Equal(comment.UID, returnedComment.UID);
        Assert.Equal(comment.CommentBody, returnedComment.CommentBody);
        Assert.Equal(comment.Upvotes, returnedComment.Upvotes);
        Assert.Equal(comment.Downvotes, returnedComment.Downvotes);
    }

    [Fact]
    public async Task GetCommentById_ShouldReturnNotFound_WhenCommentNotFound()
    {
        // Arrange
        _mockCommentActions.Setup(p => p.GetCommentById(It.IsAny<string>())).ReturnsAsync((Comment)null);

        // Act
        var result = await _commentController.GetCommentById("!");
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Comment does not exist", notFoundResult.Value);
    }

    [Fact]
    public async Task GetCommentById_ShouldReturnBadRequest_WithNullCommentId()
    {
        // Act
        var result = await _commentController.GetCommentById(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Comment ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCommentById_ShouldReturnBadRequest_WithEmptyCommentId()
    {
        // Act
        var result = await _commentController.GetCommentById("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Comment ID is required", badRequestResult.Value);
    }

    #endregion

    #region GetCommentsByUid Tests

    [Fact]
    public async Task GetCommentsByUid_ShouldReturnComments_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        var list = new List<Comment>();
        list.Add(comment);

        _mockCommentActions.Setup(c => c.GetCommentsByUid(It.IsAny<string>())).ReturnsAsync(list);

        // Act
        var result = await _commentController.GetCommentsByUid(comment.UID);

        // Assert
        var returnedList = Assert.IsType<List<Comment>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(comment.CID, returnedList.First().CID);
        Assert.Equal(comment.PID, returnedList.First().PID);
        Assert.Equal(comment.CreatedAt, returnedList.First().CreatedAt);
        Assert.Equal(comment.UpdatedAt, returnedList.First().UpdatedAt);
        Assert.Equal(comment.UID, returnedList.First().UID);
        Assert.Equal(comment.CommentBody, returnedList.First().CommentBody);
        Assert.Equal(comment.Upvotes, returnedList.First().Upvotes);
        Assert.Equal(comment.Downvotes, returnedList.First().Downvotes);
    }

    [Fact]
    public async Task GetCommentsByUid_ShouldReturnBadRequest_WithInvalidUid()
    {
        // Act
        var result = await _commentController.GetCommentsByUid(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Uid is required", badRequestResult.Value);
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
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };

        var list = new List<Comment>();
        list.Add(comment);

        _mockCommentActions.Setup(c => c.GetCommentsByPid(It.IsAny<string>())).ReturnsAsync(list);

        // Act
        var result = await _commentController.GetCommentsByPid(comment.PID);

        // Assert
        var returnedList = Assert.IsType<List<Comment>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(comment.CID, returnedList.First().CID);
        Assert.Equal(comment.PID, returnedList.First().PID);
        Assert.Equal(comment.CreatedAt, returnedList.First().CreatedAt);
        Assert.Equal(comment.UpdatedAt, returnedList.First().UpdatedAt);
        Assert.Equal(comment.UID, returnedList.First().UID);
        Assert.Equal(comment.CommentBody, returnedList.First().CommentBody);
        Assert.Equal(comment.Upvotes, returnedList.First().Upvotes);
        Assert.Equal(comment.Downvotes, returnedList.First().Downvotes);
    }

    [Fact]
    public async Task GetCommentsByPid_ShouldReturnBadRequest_WithNullPid()
    {
        // Act
        var result = await _commentController.GetCommentsByPid(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Post ID is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCommentsByPid_ShouldReturnBadRequest_WithEmptyPid()
    {
        // Act
        var result = await _commentController.GetCommentsByPid("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Post ID is required", badRequestResult.Value);
    }

    #endregion
}
