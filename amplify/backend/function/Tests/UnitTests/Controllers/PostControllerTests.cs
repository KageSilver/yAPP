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
using yAppLambda.Models;
using yAppLambda.DynamoDB;

public class PostControllerTests
{
    private readonly Mock<IAppSettings> _mockAppSettings;
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly PostController _postController;
    private readonly Mock<IPostActions> _mockPostActions;

    public PostControllerTests()
    {
        _mockAppSettings = new Mock<IAppSettings>();
        _mockDbContext = new Mock<IDynamoDBContext>();
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockPostActions = new Mock<IPostActions>();
        _postController = new PostController(_mockAppSettings.Object, _mockCognitoActions.Object,
            _mockDbContext.Object, _mockPostActions.Object);
    }

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

    #region GetPostById Tests

    [Fact]
    public async Task GetPostById_ShouldReturnPost_WhenSuccessful()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UserName = "username",
            CreatedAt = DateTime.Now,
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _mockPostActions.Setup(p => p.GetPostById(It.IsAny<string>())).ReturnsAsync(post);

        // Act
        var result = await _postController.GetPostById(post.PID);

        // Assert
        var returnedPost = Assert.IsType<Post>(result.Value);
        Assert.Equal(post.PID, returnedPost.PID);
        Assert.Equal(post.CreatedAt, returnedPost.CreatedAt);
        Assert.Equal(post.UserName, returnedPost.UserName);
        Assert.Equal(post.PostTitle, returnedPost.PostTitle);
        Assert.Equal(post.PostBody, returnedPost.PostBody);
        Assert.Equal(post.Upvotes, returnedPost.Upvotes);
        Assert.Equal(post.Downvotes, returnedPost.Downvotes);
        Assert.Equal(post.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(post.Anonymous, returnedPost.Anonymous);
    }

    [Fact]
    public async Task GetPostById_ShouldReturnNotFound_WhenPostNotFound()
    {
        // Arrange
        _mockPostActions.Setup(p => p.GetPostById(It.IsAny<string>())).ReturnsAsync((Post)null);

        // Act
        var result = await _postController.GetPostById("1");
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Post does not exist", notFoundResult.Value);
    }

    [Fact]
    public async Task GetPostById_ShouldReturnBadRequest_WithInvalidPostId()
    {
        // Act
        var result = await _postController.GetPostById(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Post ID is required", badRequestResult.Value);
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
            UserName = "username",
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

        _mockPostActions.Setup(p => p.GetPostsByUser(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(list);

        // Act
        var result = await _postController.GetPostsByUser(post.UserName, false);

        // Assert
        var returnedList = Assert.IsType<List<Post>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(post.PID, returnedList.First().PID);
        Assert.Equal(post.CreatedAt, returnedList.First().CreatedAt);
        Assert.Equal(post.UserName, returnedList.First().UserName);
        Assert.Equal(post.PostTitle, returnedList.First().PostTitle);
        Assert.Equal(post.PostBody, returnedList.First().PostBody);
        Assert.Equal(post.Upvotes, returnedList.First().Upvotes);
        Assert.Equal(post.Downvotes, returnedList.First().Downvotes);
        Assert.Equal(post.DiaryEntry, returnedList.First().DiaryEntry);
        Assert.Equal(post.Anonymous, returnedList.First().Anonymous);

    }

    [Fact]
    public async Task GetPostsByUser_ShouldReturnBadRequest_WithInvalidUsername()
    {
        // Act
        var result = await _postController.GetPostsByUser(null, false);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("username is required", badRequestResult.Value);
    }

    #endregion
}
