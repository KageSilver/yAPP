using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.DynamoDBv2.DocumentModel;

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
        Assert.Equal("request body is required and must contain poster's uid, post title and post body", badRequestResult.Value);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnNotFound_WhenUIDIsNotFound()
    {
        // Arrange
        var request = new NewPost { 
            UID = "userDoesNotExist", 
            PostTitle = "title", 
            PostBody = "body", 
            DiaryEntry = false, 
            Anonymous = true 
        };
        _mockCognitoActions.Setup(c => c.GetUserById(request.UID)).ReturnsAsync((User)null);

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
        var request = new NewPost 
        { 
            UID = "user1@example.com", 
            PostTitle = "title", 
            PostBody = "body", 
            DiaryEntry = false, 
            Anonymous = true 
        };
        var poster = new User {UserName = "user1@example.com" };
        var post = new Post 
        { 
            UID = request.UID, 
            PostTitle = request.PostTitle, 
            PostBody = request.PostBody, 
            Upvotes = 0, 
            Downvotes = 0, 
            DiaryEntry = request.DiaryEntry, 
            Anonymous = request.Anonymous 
        };

        // Mock GetUser to return the poster
        _mockCognitoActions.Setup(c => c.GetUserById(request.UID)).ReturnsAsync(poster);

        // Mock CreatePost to return a new Post object
        _mockPostActions.Setup(p => p.CreatePost(It.IsAny<Post>())).ReturnsAsync(new OkObjectResult(post));

        // Act
        var result = await _postController.CreatePost(request);

        // Assert
        var returnedPost = result.Value;
        // cannot assert PID being equal since PID is randomly generated
        Assert.Equal(post.UID, returnedPost.UID);
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
        // Arrange
        _mockPostActions.Setup(p => p.DeletePost(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var response = await _postController.DeletePost("1");

        // Assert
        Assert.True(response.Value);
    }

    [Fact]
    public async Task DeletePost_ShouldReturnBadRequest_WhenPostIdIsNull()
    {
        // Act
        var result = await _postController.DeletePost(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Post id is required",
            badRequestResult.Value);
    }
    
    [Fact]
    public async Task DeletePost_ShouldReturnFalse_WhenDeleteFails()
    {
        // Arrange
        _mockPostActions.Setup(p => p.DeletePost(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var response = await _postController.DeletePost("1");
        
        // Assert
        Assert.False(response.Value);
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
            UID = "username",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _mockPostActions.Setup(p => p.UpdatePost(It.IsAny<Post>())).ReturnsAsync(new OkObjectResult(request));

        // Act
        var response = await _postController.UpdatePost(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(response);
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
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Act
        var response = await _postController.UpdatePost(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("request body is required and must contain uid, post title, post body",
            badRequestResult.Value);
    }
    
    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenUIDIsMissing()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UID = "",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Act
        var response = await _postController.UpdatePost(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("request body is required and must contain uid, post title, post body",
            badRequestResult.Value);

    }
    
    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenPostBodyIsMissing()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Act
        var response = await _postController.UpdatePost(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("request body is required and must contain uid, post title, post body",
            badRequestResult.Value);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenPostTitleIsMissing()
    {
        // Arrange
        var request = new Post
        {
            PID = "1",
            CreatedAt = DateTime.Now,
            UID = "uid",
            PostTitle = "",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Act
        var response = await _postController.UpdatePost(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Equal("request body is required and must contain uid, post title, post body",
            badRequestResult.Value);
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
            UID = "uid",
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
        Assert.Equal(post.UID, resultPosts.First().UID);
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

    #region GetPostById Tests

    [Fact]
    public async Task GetPostById_ShouldReturnPost_WhenSuccessful()
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

        _mockPostActions.Setup(p => p.GetPostById(It.IsAny<string>())).ReturnsAsync(post);

        // Act
        var result = await _postController.GetPostById(post.PID);

        // Assert
        var returnedPost = Assert.IsType<Post>(result.Value);
        Assert.Equal(post.PID, returnedPost.PID);
        Assert.Equal(post.CreatedAt, returnedPost.CreatedAt);
        Assert.Equal(post.UID, returnedPost.UID);
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

        _mockPostActions.Setup(p => p.GetPostsByUser(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(list);

        // Act
        var result = await _postController.GetPostsByUser(post.UID, false);

        // Assert
        var returnedList = Assert.IsType<List<Post>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(post.PID, returnedList.First().PID);
        Assert.Equal(post.CreatedAt, returnedList.First().CreatedAt);
        Assert.Equal(post.UID, returnedList.First().UID);
        Assert.Equal(post.PostTitle, returnedList.First().PostTitle);
        Assert.Equal(post.PostBody, returnedList.First().PostBody);
        Assert.Equal(post.Upvotes, returnedList.First().Upvotes);
        Assert.Equal(post.Downvotes, returnedList.First().Downvotes);
        Assert.Equal(post.DiaryEntry, returnedList.First().DiaryEntry);
        Assert.Equal(post.Anonymous, returnedList.First().Anonymous);

    }

    [Fact]
    public async Task GetPostsByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var result = await _postController.GetPostsByUser(null, false);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("uid is required", badRequestResult.Value);
    }

    #endregion
}
