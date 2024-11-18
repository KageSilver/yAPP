using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using yAppLambda.Controllers;
using yAppLambda.Models;
using yAppLambda.Common;
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.DynamoDB;
using Amazon.CognitoIdentityProvider.Model;
using System.Net;
using System.Runtime.CompilerServices;
using yAppLambda.Enum;
using Amazon.CognitoIdentityProvider;

namespace Tests.UnitTests.Controllers;

public class AwardControllerTests
{
    private readonly Mock<IAppSettings> _mockAppSettings;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly AwardController _awardController;
    private readonly Mock<IAwardActions> _mockAwardActions;
    private readonly Mock<IPostActions> _mockPostActions;
    private readonly Mock<IFriendshipActions> _mockFriendshipActions;
    private readonly Mock<IAmazonCognitoIdentityProvider> _cognitoClientMock;
    private readonly CognitoActions _cognitoActions;
    
    public AwardControllerTests()
    {
        _mockAppSettings = new Mock<IAppSettings>();
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockAwardActions = new Mock<IAwardActions>();
        _mockPostActions = new Mock<IPostActions>();
        _mockFriendshipActions = new Mock<IFriendshipActions>();
        _awardController = new AwardController(_mockAppSettings.Object, _mockCognitoActions.Object,
            _dynamoDbContextMock.Object, _mockAwardActions.Object, _mockPostActions.Object, _mockFriendshipActions.Object);
    }
    
    #region GetAwardById Tests

    [Fact]
    public async Task GetAwardById_ShouldReturnAward_WhenSuccessful()
    {
        // Arrange
        var award = new Award
        {
            AID = "1",
            PID = "1",
            UID = "1",
            CreatedAt = DateTime.Now,
            Name = "GetAwardById_ShouldReturnAward_WhenSuccessful()"
        };
        
        _mockAwardActions.Setup(a => a.GetAwardById(It.IsAny<string>())).ReturnsAsync(award);
        
        // Act
        var result = await _awardController.GetAwardById(award.AID);

        // Assert
        var returnedAward = Assert.IsType<Award>(result.Value);
        Assert.Equal(award.PID, returnedAward.PID);
        Assert.Equal(award.UID, returnedAward.UID);
        Assert.Equal(award.AID, returnedAward.AID);
        Assert.Equal(award.Name, returnedAward.Name);
    }
    
    [Fact]
    public async Task GetAwardById_ShouldReturnNotFound_WhenAwardNotFound()
    {
        // Arrange
        _mockAwardActions.Setup(p => p.GetAwardById(It.IsAny<string>())).ReturnsAsync((Award)null);

        // Act
        var result = await _awardController.GetAwardById("1");
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Award does not exist", notFoundResult.Value);
    }
    
    [Fact]
    public async Task GetAwardById_ShouldReturnBadRequest_WithInvalidAID()
    {
        // Act
        var result = await _awardController.GetAwardById(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Award ID is required", badRequestResult.Value);
    }
    
    #endregion
    
    #region GetAwardsByUser Tests

    [Fact]
    public async Task GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var award = new Award
        {
            AID = "1",
            PID = "1",
            UID = "1",
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()"
        };

        var list = new List<Award>();
        list.Add(award);
        
        _mockAwardActions.Setup(a => a.GetAwardsByUser(It.IsAny<string>())).ReturnsAsync(list);
        
        // Act
        var result = await _awardController.GetAwardsByUser(award.UID);

        // Assert
        var returnedList = Assert.IsType<List<Award>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(award.PID, returnedList.First().PID);
        Assert.Equal(award.UID, returnedList.First().UID);
        Assert.Equal(award.AID, returnedList.First().AID);
        Assert.Equal(award.Name, returnedList.First().Name);
    }

    [Fact]
    public async Task GetAwardsByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var result = await _awardController.GetAwardsByUser(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("uid is required", badRequestResult.Value);
    }
    
    #endregion

    #region GetAwardsByPost Tests

    [Fact]
    public async Task GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var award = new Award
        {
            AID = "1",
            PID = "1",
            UID = "1",
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()"
        };

        var list = new List<Award>();
        list.Add(award);
        
        _mockAwardActions.Setup(a => a.GetAwardsByPost(It.IsAny<string>())).ReturnsAsync(list);
        
        // Act
        var result = await _awardController.GetAwardsByPost(award.PID);

        // Assert
        var returnedList = Assert.IsType<List<Award>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(award.PID, returnedList.First().PID);
        Assert.Equal(award.UID, returnedList.First().UID);
        Assert.Equal(award.AID, returnedList.First().AID);
        Assert.Equal(award.Name, returnedList.First().Name);
    }
    
    [Fact]
    public async Task GetAwardsByPosts_ShouldReturnBadRequest_WithInvalidPID()
    {
        // Act
        var result = await _awardController.GetAwardsByPost(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("pid is required", badRequestResult.Value);
        
    }
    
    #endregion

    #region GetNewAwardsByUser Tests

    [Fact]
    public async Task GetNewAwardsByUser_ShouldReturnAwardsList_WhenSuccessful()
    {
        // Arrange
        var user = new User {UserName = "user1@example.com", Id = "1" };
        
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "1",
            UID = user.Id,
            CreatedAt = now,
            UpdatedAt = now,
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };
        var award1 = new Award
        {
            AID = "1",
            PID = post.PID,
            UID = user.Id,
            CreatedAt = now,
            Name = "GetNewAwardsByUser_ShouldReturnAwardsList_WhenSuccessful() 1"
        };
        
        var award2 = new Award
        {
            AID = "2",
            PID = post.PID,
            UID = user.Id,
            CreatedAt = now,
            Name = "GetNewAwardsByUser_ShouldReturnAwardsList_WhenSuccessful() 2"
        };
        var existingFriendship = new Friendship
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = FriendshipStatus.Accepted
        };

        var friendList = new List<Friendship>();
        friendList.Add(existingFriendship);

        var list1 = new List<Award>();
        list1.Add(award1);
        
        var list2 = new List<Award>();
        list2.Add(award2);

        var list = new List<Post>();
        list.Add(post);

        // Mock GetPostsByUser
        _mockPostActions.Setup(p => p.GetPostsByUser(It.IsAny<string>())).ReturnsAsync(list);

        // Mock GetUser to return the poster
        _mockCognitoActions.Setup(c => c.GetUserById(It.IsAny<string>())).ReturnsAsync(user);
        
        // Mock GetAllFriends
        _mockFriendshipActions
            .Setup(s => s.GetAllFriends(existingFriendship.FromUserName, It.IsAny<FriendshipStatus>()))
            .ReturnsAsync(friendList);
        
        // Mock award checks from award actions
        _mockAwardActions.Setup(a => a.CheckNewAwardsPerPost(It.IsAny<List<Post>>())).ReturnsAsync(list1);
        _mockAwardActions.Setup(a => a.CheckNewAwardsTotalPosts(It.IsAny<List<Post>>(), It.IsAny<string>())).ReturnsAsync(list2);
        _mockAwardActions.Setup(a => a.CheckNewAwardsFriends(It.IsAny<List<Friendship>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Award>());
        
        // Act
        var result = await _awardController.GetNewAwardsByUser(award1.UID);

        // Assert
        var returnedList = Assert.IsType<List<Award>>(result.Value);
        Assert.Equal(2, returnedList.Count);
        Assert.Equal(award1.PID, returnedList.First().PID);
        Assert.Equal(award1.UID, returnedList.First().UID);
        Assert.Equal(award1.AID, returnedList.First().AID);
        Assert.Equal(award1.Name, returnedList.First().Name);
        Assert.Equal(award2.PID, returnedList[1].PID);
        Assert.Equal(award2.UID, returnedList[1].UID);
        Assert.Equal(award2.AID, returnedList[1].AID);
        Assert.Equal(award2.Name, returnedList[1].Name);
    }
    
    [Fact]
    public async Task GetNewAwardsByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var result = await _awardController.GetNewAwardsByUser(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("uid is required", badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    {
        // Arrange
        var uid = "1";
        _mockCognitoActions.Setup(c => c.GetUserById(uid)).ReturnsAsync((User)null);
        
        // Act
        var result = await _awardController.GetNewAwardsByUser(uid);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User does not exist", notFoundResult.Value);
    }

    #endregion
}