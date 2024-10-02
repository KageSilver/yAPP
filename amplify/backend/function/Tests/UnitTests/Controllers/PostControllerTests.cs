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
}
