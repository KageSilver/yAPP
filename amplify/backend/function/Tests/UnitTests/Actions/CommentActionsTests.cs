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
using Amazon.DynamoDBv2.DocumentModel;
using System.Diagnostics.CodeAnalysis;

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
            PID = "createCommentShouldReturnOK",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "c1cb",
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
        Assert.Equal(comment.UID, returnedComment.UID);
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
            UID = "c1cb",
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
            UID = "c1cb",
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
        Assert.Equal(request.UID, returnedComment.UID);
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

    #region GetCommentsByUid Tests
    
    [Fact]
    public async Task GetCommentsByUid_ShouldReturnComments_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "getCommentsByUidShouldReturnComments",
            UID = "c1cb",
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
        var result = await _commentActionsMock.GetCommentsByUid(comment.UID);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(comment.CID, result.First().CID);
        Assert.Equal(comment.PID, result.First().PID);
        Assert.Equal(comment.CreatedAt, result.First().CreatedAt);
        Assert.Equal(comment.UpdatedAt, result.First().UpdatedAt);
        Assert.Equal(comment.UID, result.First().UID);
        Assert.Equal(comment.CommentBody, result.First().CommentBody);
        Assert.Equal(comment.Upvotes, result.First().Upvotes);
        Assert.Equal(comment.Downvotes, result.First().Downvotes);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Comment>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task GetCommentsByUid_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Comment>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error querying comments"));

        // Act
        var result = await _commentActionsMock.GetCommentsByUid("c1cb");

        // Assert
        Assert.Empty(result);
    }
    
    #endregion

    #region GetCommentsByPid Tests
    
    [Fact]
    public async Task GetCommentsByPid_ShouldReturnComments_WithAValidQuery()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "getCommentsByPidShouldReturnComments",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };
        var response = new Comment { CID = "1" };

        // Sets up LoadAsync to return the request comment (for in GetCommentById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(comment.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var list = new List<Comment>();
        list.Add(response);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);

        // Act
        var result = await _commentActionsMock.GetCommentsByPid(comment.PID);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(comment.CID, result.First().CID);
        Assert.Equal(comment.PID, result.First().PID);
        Assert.Equal(comment.UID, result.First().UID);
        Assert.Equal(comment.CreatedAt, result.First().CreatedAt);
        Assert.Equal(comment.UpdatedAt, result.First().UpdatedAt);
        Assert.Equal(comment.Upvotes, result.First().Upvotes);
        Assert.Equal(comment.Downvotes, result.First().Downvotes);

        _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
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

    #region DeleteComment Tests

    [Fact]
    public async Task DeleteComment_ShouldCallDeleteAsync()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "11111",
            PID = "11111",
            UID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "DeleteComment_ShouldCallDeleteAsync()",
            Upvotes = 0,
            Downvotes = 0
        };

        // Sets up LoadAsync to return the request comment (for in GetCommentById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(request.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _commentActionsMock.DeleteComment(request.CID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteComment_ShouldHandleException_WhenCommentDoesNotExist()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "11111",
            PID = "11111",
            UID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "DeleteComment_ShouldHandleException_WhenCommentDoesNotExist()",
            Upvotes = 0,
            Downvotes = 0
        };

        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(request.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Comment does not exist"));

        // Act
        var result = await _commentActionsMock.DeleteComment(request.CID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Comment>(request.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteComment_ShouldHandleException_WhenDeleteCommentFails()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "11111",
            PID = "11111",
            UID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "DeleteComment_ShouldHandleException_WhenDeleteCommentFails()",
            Upvotes = 0,
            Downvotes = 0
        };

        // Sets up LoadAsync to return the request comment (for in GetCommentById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(request.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Sets up DeleteAsync to throw an exception           
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Could not delete comment"));

        // Act
        var result = await _commentActionsMock.DeleteComment(request.CID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion

    #region DeleteComments Tests

    [Fact]
    public async Task DeleteComments_ShouldCallDeleteComment()
    {
        // Arrange
        var now = DateTime.Now;
        var comment = new Comment
        {
            CID = "1",
            PID = "DeleteComments_ShouldCallDeleteComment()",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "body",
            Upvotes = 0,
            Downvotes = 0
        };
        var response = new Comment { CID = "1" };

        // Sets up LoadAsync to return the request comment (for in GetCommentById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(comment.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var list = new List<Comment>();
        list.Add(response);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);

        // Sets up LoadAsync to return the request comment (for in GetCommentById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(comment.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Comment>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _commentActionsMock.DeleteComments(comment.PID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(comment, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteComments_ShouldHandleException_WhenPostDoesNotExist()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "11111",
            PID = "!",
            UID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "DeleteComments_ShouldHandleException_WhenPostDoesNotExist()",
            Upvotes = 0,
            Downvotes = 0
        };

        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(request.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to retrieve comments"));

        // Act
        var result = await _commentActionsMock.DeleteComments(request.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteComments_ShouldHandleException_WhenDeleteCommentsFails()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "11111",
            PID = "11111",
            UID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "DeleteComment_ShouldHandleException_WhenDeleteCommentFails()",
            Upvotes = 0,
            Downvotes = 0
        };

        // Sets up LoadAsync to return the request comment (for in GetCommentById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(request.CID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _commentActionsMock.DeleteComments(request.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    #endregion

    #region UpdateComment Tests

    [Fact]
    public async Task UpdateComment_ShouldReturnOk_WhenCommentIsUpdatedSuccessfully()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "UpdateComment_ShouldReturnOk_WhenCommentIsUpdatedSuccessfully()",
            Upvotes = 0,
            Downvotes = 0
        };

        _dynamoDbContextMock.Setup(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _commentActionsMock.UpdateComment(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Comment>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedComment = Assert.IsType<Comment>(okResult.Value);

        Assert.Equal(request.CID, returnedComment.CID);
        Assert.Equal(request.PID, returnedComment.PID);
        Assert.Equal(request.PID, returnedComment.PID);
        Assert.Equal(request.CreatedAt, returnedComment.CreatedAt);
        Assert.Equal(request.UpdatedAt, returnedComment.UpdatedAt);
        Assert.Equal(request.CommentBody, returnedComment.CommentBody);
        Assert.Equal(request.Upvotes, returnedComment.Upvotes);
        Assert.Equal(request.Downvotes, returnedComment.Downvotes);

        _dynamoDbContextMock.Verify(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateComment_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Comment
        {
            CID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            CommentBody = "UpdateComment_ShouldReturnStatus500_WhenExceptionIsThrown()",
            Upvotes = 0,
            Downvotes = 0
        };

        _dynamoDbContextMock.Setup(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Error updatiing comment in database"));

        // Act
        var result = await _commentActionsMock.UpdateComment(request);
        
        // Assert
        var actionResult = Assert.IsType<ActionResult<Comment>>(result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
    
    #endregion

}
