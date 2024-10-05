using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.DynamoDBv2.DocumentModel;

namespace Tests.UnitTests.Controllers;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using yAppLambda.Controllers;
using yAppLambda.Models;
using yAppLambda.Common;
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.Models;
using yAppLambda.DynamoDB;

public class PostControllerTests
{
    private readonly Mock<IAppSettings> _mockAppSettings;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly PostController _postController;
    private readonly Mock<IPostActions> _mockPostActions;

    public PostControllerTests()
    {
        _mockAppSettings = new Mock<IAppSettings>();
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockPostActions = new Mock<IPostActions>();
        _postController = new PostController(_mockAppSettings.Object, _mockCognitoActions.Object,
            _dynamoDbContextMock.Object, _mockPostActions.Object);
    }

    #region CreatePost Tests

    [Fact]
    public async Task CreatePost_ShouldReturnBadResult_WhenRequestIsNull()
    {
        // Arrange
        NewPost request = null;

        // Act
        var result = await _postController.CreatePost(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("request body is required and must contain poster's username, post title and post body", badRequestResult.Value);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnNotFound_WhenUsernameIsNotFound()
    {
        // Arrange
        var request = new NewPost { UserName = "userDoesNotExist", PostTitle = "title", PostBody = "body", DiaryEntry = false, Anonymous = true };
        _mockCognitoActions.Setup(c => c.GetUser(request.UserName)).ReturnsAsync((User)null);

        // Act
        var result = await _postController.CreatePost(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Post creator not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnOK_WhenPostIsCreatedSuccessfully()
    {
        // Arrange 
        var request = new NewPost { UserName = "user1@example.com", PostTitle = "title", PostBody = "body", DiaryEntry = false, Anonymous = true };
        var poster = new User {UserName = "user1@example.com" };
        var post = new Post 
        { 
            UserName = request.UserName, 
            PostTitle = request.PostTitle, 
            PostBody = request.PostBody, 
            Upvotes = 0, 
            Downvotes = 0, 
            DiaryEntry = request.DiaryEntry, 
            Anonymous = request.Anonymous 
        };

        // Mock GetUser to return the poster
        _mockCognitoActions.Setup(c => c.GetUser(request.UserName)).ReturnsAsync(poster);

        // Mock CreatePost to return a new Post object
        _mockPostActions.Setup(p => p.CreatePost(It.IsAny<Post>())).ReturnsAsync(new OkObjectResult(post));

        // Act
        var result = await _postController.CreatePost(request);

        // Assert
        var returnedPost = result.Value;
        // cannot assert PID being equal since PID is randomly generated
        Assert.Equal(post.UserName, returnedPost.UserName);
        Assert.Equal(post.PostTitle, returnedPost.PostTitle);
        Assert.Equal(post.PostBody, returnedPost.PostBody);
        Assert.Equal(post.Upvotes, returnedPost.Upvotes);
        Assert.Equal(post.Downvotes, returnedPost.Downvotes);
        Assert.Equal(post.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(post.Anonymous, returnedPost.Anonymous);
    }
    
    #endregion

    #region DeletePost Tests

    [Fact]
    public async Task DeletePost_ShouldReturnTrue_WhenPostIsDeletedSuccessfully()
    {

    }

    [Fact]
    public async Task DeletePost_ShouldReturnBadRequest_WhenPostIdIsNull()
    {

    }
    
    [Fact]
    public async Task DeletePost_ShouldReturnFalse_WhenDeleteFails()
    {

    }

    [Fact]
    public async Task DeletePost_ShouldReturnNotFound_WhenPostIsNotFound()
    {

    }

    #endregion

    #region UpdatePost Tests

    [Fact]
    public async Task UpdatePost_ShouldReturnOk_WhenPostIsUpdatedSuccessfully()
    {

    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenRequestIsNull()
    {

    }
    
    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenUsernameIsMissing()
    {

    }
    
    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenPostBodyIsMissing()
    {

    }
    
    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenPostTitleIsMissing()
    {

    }
    
    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenUpdateFails()
    {

    }

    [Fact]
    public async Task UpdatePost_ShouldReturnNotFound_WhenPostIsNotFound()
    {

    }

    #endregion

    #region GetRecentPosts Tests

    [Fact]
    public async Task GetRecentPosts_ShouldReturnPosts_WhenRequestIsSuccessful()
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

        var list = new List<Post>();
        list.Add(post);
        
        _mockPostActions.Setup(p => p.GetRecentPosts(It.IsAny<DateTime>(),It.IsAny<int>())).ReturnsAsync(list);

        // Act
        var result = await _postController.GetRecentPosts(DateTime.Now, 1);
        var resultPosts = result.Value;
        
        // Assert
        Assert.IsType<List<Post>>(resultPosts);
        Assert.Equal(1, resultPosts.Count);
        Assert.Equal(post.PID, resultPosts.First().PID);
        Assert.Equal(post.UserName, resultPosts.First().UserName);
        Assert.Equal(post.PostTitle, resultPosts.First().PostTitle);
        Assert.Equal(post.PostBody, resultPosts.First().PostBody);
        Assert.Equal(post.Upvotes, resultPosts.First().Upvotes);
        Assert.Equal(post.Downvotes, resultPosts.First().Downvotes);
        Assert.Equal(post.DiaryEntry, resultPosts.First().DiaryEntry);
        Assert.Equal(post.Anonymous, resultPosts.First().Anonymous);
    }

    [Fact]
    public async Task GetRecentPosts_ShouldReturnBadRequest_WithInvalidRequest()
    {
        // Act
        var result = await _postController.GetRecentPosts(DateTime.Now, -1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("requires valid max result number and valid time", badRequestResult.Value);
    }

    #endregion
}
