using yAppLambda.DynamoDB;
using yAppLambda.Enum;

namespace Tests.UnitTests.Controllers;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using yAppLambda.Controllers;
using yAppLambda.Models;
using yAppLambda.Common;
using Amazon.DynamoDBv2.DataModel;

public class FriendControllerTests
{
    private readonly Mock<IAppSettings> _mockAppSettings;
    private readonly Mock<IDynamoDBContext> _mockDbContext;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly FriendController _friendController;
    private readonly Mock<IFriendshipActions> _mockFriendshipActions;
    private readonly Mock<IFriendshipStatusActions> _mockFriendshipStatusActions;

    public FriendControllerTests()
    {
        _mockAppSettings = new Mock<IAppSettings>();
        _mockDbContext = new Mock<IDynamoDBContext>();
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockFriendshipActions = new Mock<IFriendshipActions>();
        _mockFriendshipStatusActions = new Mock<IFriendshipStatusActions>();
        _friendController = new FriendController(_mockAppSettings.Object, _mockCognitoActions.Object,
            _mockDbContext.Object, _mockFriendshipActions.Object,_mockFriendshipStatusActions.Object);
    }

    [Fact]
    public async Task SendFriendRequest_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        FriendRequest request = null;

        // Act
        var result = await _friendController.SendFriendRequest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("request body is required and must contain username and friend's id", badRequestResult.Value);
    }

    [Fact]
    public async Task SendFriendRequest_ShouldReturnNotFound_WhenFriendIsNotFound()
    {
        // Arrange
        var request = new FriendRequest { FromUserName = "user1@example.com", ToUserId = "nonexistentId" };
        _mockCognitoActions.Setup(c => c.GetUserById(request.ToUserId)).ReturnsAsync((User)null);

        // Act
        var result = await _friendController.SendFriendRequest(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Friend not found", notFoundResult.Value);
    }

    [Fact]
    public async Task SendFriendRequest_ShouldReturnOk_WhenFriendRequestIsCreatedSuccessfully()
    {
        // Arrange
        var request = new FriendRequest { FromUserName = "user1@example.com", ToUserId = "user2Id" };
        var friend = new User { UserName = "user2@example.com" };
        var friendship = new Friendship
            { FromUserName = request.FromUserName, ToUserName = friend.UserName, Status = FriendshipStatus.Pending };
        // Mock GetUserById to return a valid user
        _mockCognitoActions.Setup(c => c.GetUserById(request.ToUserId)).ReturnsAsync(friend);

        // Mock GetFriendship to return null (no existing friendship)
        _mockFriendshipActions.Setup(f => f.GetFriendship(request.FromUserName, friend.UserName))
            .ReturnsAsync((ActionResult<Friendship>)null);

        // Mock CreateFriendship to return the newly created friendship
        _mockFriendshipActions.Setup(f => f.CreateFriendship(It.IsAny<Friendship>()))
            .ReturnsAsync(new OkObjectResult(friendship));


        // Act
        var result = await _friendController.SendFriendRequest(request);

        // Assert
        var returnedFriendship = result.Value;
        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);
        Assert.Equal(friendship.Status, returnedFriendship.Status);
    }

    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        FriendRequest request = null;

        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("request body is required and must contain username and friend's username",
            badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnNotFound_WhenFriendshipIsNotFound()
    {
        // Arrange
        var request = new FriendRequest
            { FromUserName = "user1@example.com", ToUserName = "user2@example.com", Status = 1 };
        _mockFriendshipActions.Setup(f => f.GetFriendship(request.FromUserName, request.ToUserName))
            .ReturnsAsync((ActionResult<Friendship>)null);

        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Friendship not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnOk_WhenFriendshipStatusIsUpdatedSuccessfully()
    {
        // Arrange
        var request = new FriendRequest
            { FromUserName = "user1@example.com", ToUserName = "user2@example.com", Status = 1 };
        var existingFriendship = new Friendship
            { FromUserName = request.FromUserName, ToUserName = request.ToUserName, Status = FriendshipStatus.Pending };

        _mockFriendshipActions.Setup(f => f.GetFriendship(request.FromUserName, request.ToUserName))
            .ReturnsAsync(new OkObjectResult(existingFriendship));
        _mockFriendshipActions.Setup(f => f.UpdateFriendshipStatus(existingFriendship))
            .ReturnsAsync(new OkObjectResult(existingFriendship));
        _mockFriendshipActions.Setup(a => a.GetFriendship(request.FromUserName, request.ToUserName))
            .ReturnsAsync(new ActionResult<Friendship>(existingFriendship));
        _mockFriendshipStatusActions.Setup(s => s.GetFriendshipStatus(It.IsAny<int>())).Returns(FriendshipStatus.Accepted);
    
        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert

        var updatedFriendship = result.Value;
        Assert.Equal(FriendshipStatus.Accepted, updatedFriendship.Status);
    }
    
    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnBadRequest_WhenUserNameIsMissing()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = null, // FromUserName is missing
            ToUserName = "user2@example.com",
            Status = 1
        };

        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("request body is required and must contain username and friend's username",
            badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnBadRequest_WhenToUserNameIsMissing()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = "user1@example.com",
            ToUserName = null, // ToUserName is missing
            Status = 1
        };

        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert
        var badRequestResult = result.Result;
        Assert.IsType<NotFoundObjectResult>(badRequestResult);
   
    }
    
    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnBadRequest_WhenUpdateFails()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = 1
        };
        var existingFriendship = new Friendship
        {
            FromUserName = request.FromUserName,
            ToUserName = request.ToUserName,
            Status = FriendshipStatus.Pending
        };

        // Mock the friendship retrieval
        _mockFriendshipActions.Setup(f => f.GetFriendship(request.FromUserName, request.ToUserName))
            .ReturnsAsync(new OkObjectResult(existingFriendship));
    
        // Mock a failure when updating the friendship status
        _mockFriendshipActions.Setup(f => f.UpdateFriendshipStatus(existingFriendship))
            .ReturnsAsync(new BadRequestObjectResult("Failed to update friendship status"));

        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to update friendship status", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateFriendRequest_ShouldReturnBadRequest_WhenStatusIsAll()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = (int)FriendshipStatus.All // Status is set to All
        };
        var existingFriendship = new Friendship
        {
            FromUserName = request.FromUserName,
            ToUserName = request.ToUserName,
            Status = FriendshipStatus.Pending
        };

        _mockFriendshipActions.Setup(f => f.GetFriendship(request.FromUserName, request.ToUserName))
            .ReturnsAsync(new OkObjectResult(existingFriendship));

        // Mock status return for All
        _mockFriendshipStatusActions.Setup(s => s.GetFriendshipStatus(It.IsAny<int>()))
            .Returns(FriendshipStatus.All);

        // Act
        var result = await _friendController.UpdateFriendRequest(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to update friendship status", badRequestResult.Value);
    }


}