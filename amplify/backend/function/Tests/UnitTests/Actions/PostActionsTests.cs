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
    
    #region CreatPost Tests

    [Fact]
    public async Task CreatePost_ShouldReturnOK_WhenPostIsCreatedSuccessfully()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UID = "user1@example.com",
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
        Assert.Equal(post.UID, returnedPost.UID);
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
            UID = "user1@example.com",
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

    #endregion

    #region DeletePost Tests

    [Fact]
    public async Task DeletePost_ShouldCallDeleteAsync()
    {
        // Arrange
        var request = new Post
        {
            PID = "11111",
            CreatedAt = DateTime.Now,
            UID = "uid",
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
        var result = await _postActionsMock.DeletePost(request.PID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeletePost_ShouldHandleException_WhenPostDoesNotExist()
    {
        // Arrange
        var request = new Post
        {
            PID = "11111",
            CreatedAt = DateTime.Now,
            UID = "uid",
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
        var result = await _postActionsMock.DeletePost(request.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeletePost_ShouldHandleException_WhenDeletePostFails()
    {
        // Arrange
        var request = new Post
        {
            PID = "11111",
            CreatedAt = DateTime.Now,
            UID = "uid",
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
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion

    #region UpdatePost Tests

    [Fact]
    public async Task UpdatePost_ShouldReturnOk_WhenPostIsUpdatedSuccessfully()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _dynamoDbContextMock.Setup(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _postActionsMock.UpdatePost(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPost = Assert.IsType<Post>(okResult.Value);

        Assert.Equal(request.PID, returnedPost.PID);
        Assert.Equal(request.UID, returnedPost.UID);
        Assert.Equal(request.PostTitle, returnedPost.PostTitle);
        Assert.Equal(request.PostBody, returnedPost.PostBody);
        Assert.Equal(request.Upvotes, returnedPost.Upvotes);
        Assert.Equal(request.Downvotes, returnedPost.Downvotes);
        Assert.Equal(request.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(request.Anonymous, returnedPost.Anonymous);

        _dynamoDbContextMock.Verify(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _dynamoDbContextMock.Setup(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Error updatiing post in database"));

        // Act
        var result = await _postActionsMock.UpdatePost(request);
        
        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
    
    #endregion

    #region GetPostById Tests

    [Fact]
    public async Task GetPostById_ShouldReturnPost_WhenSuccessful()
    {
        // Arrange
        var request = new Post
        {
            PID = "11111",
            CreatedAt = DateTime.Now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _postActionsMock.GetPostById(request.PID);

        // Assert
        var returnedPost = Assert.IsType<Post>(result);
        Assert.Equal(request.PID, returnedPost.PID);
        Assert.Equal(request.CreatedAt, returnedPost.CreatedAt);
        Assert.Equal(request.UID, returnedPost.UID);
        Assert.Equal(request.PostTitle, returnedPost.PostTitle);
        Assert.Equal(request.PostBody, returnedPost.PostBody);
        Assert.Equal(request.Upvotes, returnedPost.Upvotes);
        Assert.Equal(request.Downvotes, returnedPost.Downvotes);
        Assert.Equal(request.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(request.Anonymous, returnedPost.Anonymous);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPostById_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error loading post"));
            
        // Act
        var result = await _postActionsMock.GetPostById("111");
        
        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetPostsByUser Tests
    
    [Fact]
    public async Task GetPostsByUser_ShouldReturnPosts_WhenSuccessful()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var list = new List<Post>();
        list.Add(post);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Post>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Post>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _postActionsMock.GetPostsByUser(post.UID, false);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(post.PID, result.First().PID);
        Assert.Equal(post.CreatedAt, result.First().CreatedAt);
        Assert.Equal(post.UID, result.First().UID);
        Assert.Equal(post.PostTitle, result.First().PostTitle);
        Assert.Equal(post.PostBody, result.First().PostBody);
        Assert.Equal(post.Upvotes, result.First().Upvotes);
        Assert.Equal(post.Downvotes, result.First().Downvotes);
        Assert.Equal(post.DiaryEntry, result.First().DiaryEntry);
        Assert.Equal(post.Anonymous, result.First().Anonymous);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Post>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task GetPostsByUser_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Post>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error querying posts"));

        // Act
        var result = await _postActionsMock.GetPostsByUser("uid", false);

        // Assert
        Assert.Empty(result);
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
            UID = "uid",
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
        Assert.Equal(post.UID, result.First().UID);
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
        // Sets up FromQueryAsync to fail
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Throws(new Exception("Could not load post"));
            
        // Act
        var result = await _postActionsMock.GetRecentPosts(DateTime.Now, 1);

        // Assert
        Assert.Empty(result);
    }
    
    #endregion
}
