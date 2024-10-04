using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;
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

public class PostActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly IPostActions _postActionsMock;
    private const string PostTableName = "Post-test";

    public PostActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.PostTableName).Returns(PostTableName);

        // Initialize the dynamoDbContextMock
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        
        // Initialize the PostActions with the mocks
        _postActionsMock = new PostActions(_appSettingsMock.Object, _dynamoDbContextMock.Object);
    }

    #region CreatePost Tests

    [Fact]
    public async Task CreatePost_ShouldReturnOK_WhenPostIsCreatedSuccessfully()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UserName = "user1@example.com",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _appSettingsMock.Setup(a => a.PostTableName).Returns(PostTableName);

        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _postActionsMock.CreatePost(post);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedPost = Assert.IsType<Post>(okResult.Value);
        Assert.Equal(post.PID, returnedPost.PID);
        Assert.Equal(post.UserName, returnedPost.UserName);
        Assert.Equal(post.PostTitle, returnedPost.PostTitle);
        Assert.Equal(post.PostBody, returnedPost.PostBody);
        Assert.Equal(post.Upvotes, returnedPost.Upvotes);
        Assert.Equal(post.Downvotes, returnedPost.Downvotes);
        Assert.Equal(post.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(post.Anonymous, returnedPost.Anonymous);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UserName = "user1@example.com",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _appSettingsMock.Setup(a => a.PostTableName).Returns(PostTableName);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error saving to DynamoDB"));
            
        // Act
        var result = await _postActionsMock.CreatePost(post);

        // Assert
        // Assert that the result is ActionResult<Post>
        var actionResult = Assert.IsType<ActionResult<Post>>(result); 
        // Access the Result property to get the actual StatusCodeResult
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task DeletePost_ShouldCallDeleteAsync()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UserName = "username",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        await _postActionsMock.DeletePost(request.PID);

        // Assert
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    #endregion

    #region DeletePost Tests

    [Fact]
    public async Task DeletePost_ShouldHandleException_WhenPostDoesNotExist()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UserName = "username",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Post does not exist"));

        // Act
        var exception = await Record.ExceptionAsync(() => _postActionsMock.DeletePost(request.PID));

        // Assert
        Assert.Null(exception);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    
    [Fact]
    public async Task DeletePost_ShouldHandleException_WhenDeletePostFails()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UserName = "username",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Sets up DeleteAsync to throw an exception           
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Could not delete post"));

        // Act
        var result = await _postActionsMock.DeletePost(request.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetRecentPosts Tests

    [Fact]
    public async Task GetRecentPosts_ShouldReturnPosts_WithAValidQuery()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UserName = "username",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var response = new Post { PID = "1" };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(post.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        var list = new List<Post>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Post>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);

        // Act
        var result = await _postActionsMock.GetRecentPosts(DateTime.Now, 1);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(post.PID, result.First().PID);
        Assert.Equal(post.UserName, result.First().UserName);
        Assert.Equal(post.PostTitle, result.First().PostTitle);
        Assert.Equal(post.PostBody, result.First().PostBody);
        Assert.Equal(post.Upvotes, result.First().Upvotes);
        Assert.Equal(post.Downvotes, result.First().Downvotes);
        Assert.Equal(post.DiaryEntry, result.First().DiaryEntry);
        Assert.Equal(post.Anonymous, result.First().Anonymous);

        _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetRecentPosts_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UserName = "username",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(post.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Throws(new Exception("Could not load post"));
            
        // Act
        var result = await _postActionsMock.GetRecentPosts(DateTime.Now, 1);

        // Assert
        Assert.Empty(result);
    }
    
    #endregion
}
