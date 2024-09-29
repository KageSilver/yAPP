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
using yAppLambda.Enum;

namespace UnitTests;

public class FriendshipActionTests
{
    private readonly Mock<IAppSettings> _appSettingsMock = new();
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock = new();
    private const string FriendshipTableName = "Friendship-test";
    

    [Fact]
    public async Task CreateFriendship_ShouldReturnOk_WhenFriendshipIsCreatedSuccessfully()
    {
        // Arrange
        var friendship = new Friendship
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = 0
        };
        
        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(FriendshipTableName);

        var dynamoDbContext = new Mock<IDynamoDBContext>();
        dynamoDbContext.Setup(d => d.SaveAsync(friendship, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await FriendshipActions.CreateFriendship(friendship, dynamoDbContext.Object, _appSettingsMock.Object);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedFriendship = Assert.IsType<Friendship>(okResult.Value);
        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);

        dynamoDbContext.Verify(
            d => d.SaveAsync(friendship, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None), Times.Once);
    }


    [Fact]
    public async Task CreateFriendship_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var friendship = new Friendship
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = 0
        };
        
        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(FriendshipTableName);

        var dynamoDbContext = new Mock<IDynamoDBContext>();
        dynamoDbContext.Setup(d => d.SaveAsync(friendship, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Error saving to DynamoDB"));

        // Act
        var result = await FriendshipActions.CreateFriendship(friendship, dynamoDbContext.Object, _appSettingsMock.Object);

        // Assert
        var actionResult =
            Assert.IsType<ActionResult<Friendship>>(result); // Assert that the result is ActionResult<Friendship>
        var statusCodeResult =
            Assert.IsType<StatusCodeResult>(actionResult
                .Result); // Access the Result property to get the actual StatusCodeResult
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
    
    [Fact]
    public async Task UpdateFriendshipStatus_ShouldReturnOk_WhenFriendshipIsUpdatedSuccessfully()
    {
        // Arrange
        var friendship = new Friendship
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = FriendshipStatus.Accepted // Or Declined
        };

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(string.Empty);
        _dynamoDbContextMock.Setup(d => d.SaveAsync(friendship, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await FriendshipActions.UpdateFriendshipStatus(friendship, _dynamoDbContextMock.Object, _appSettingsMock.Object);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedFriendship = Assert.IsType<Friendship>(okResult.Value);

        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);
        Assert.Equal(friendship.Status, returnedFriendship.Status);
        Assert.NotNull(returnedFriendship.UpdatedAt); // Ensure UpdatedAt is set

        _dynamoDbContextMock.Verify(d => d.SaveAsync(friendship, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateFriendshipStatus_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var friendship = new Friendship
        {
            FromUserName = "user1@example.com",
            ToUserName = "user2@example.com",
            Status = FriendshipStatus.Accepted
        };

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(FriendshipTableName);
        _dynamoDbContextMock.Setup(d => d.SaveAsync(friendship, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Error updating friendship in DynamoDB"));

        // Act
        var result = await FriendshipActions.UpdateFriendshipStatus(friendship, _dynamoDbContextMock.Object, _appSettingsMock.Object);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
    
     [Fact]
    public async Task GetAllFriends_ShouldReturnFriendships_WhenStatusIsAccepted()
    {
        // Arrange
        const string userName = "user1@example.com";
        const FriendshipStatus friendshipStatus = FriendshipStatus.Accepted;

        var friendshipsFrom = new List<Friendship>
        {
            new Friendship { FromUserName = userName, ToUserName = "user2@example.com", Status = friendshipStatus }
        };

        var friendshipsTo = new List<Friendship>
        {
            new Friendship { FromUserName = "user2@example.com", ToUserName = userName, Status = friendshipStatus }
        };

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(string.Empty);
        
        var queryFromSearchMock = new Mock<AsyncSearch<Friendship>>();
        queryFromSearchMock.Setup(q => q.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendshipsFrom);

        var scanToSearchMock = new Mock<AsyncSearch<Friendship>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendshipsTo);

        _dynamoDbContextMock.Setup(d => d.QueryAsync<Friendship>(userName, It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);

        _dynamoDbContextMock.Setup(d => d.ScanAsync<Friendship>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await FriendshipActions.GetAllFriends(userName, friendshipStatus, _dynamoDbContextMock.Object, _appSettingsMock.Object);

        // Assert
        Assert.Equal(2, result.Count); // One from `FromUserName` and one from `ToUserName`
        Assert.Contains(result, f => f.FromUserName == userName);
        Assert.Contains(result, f => f.ToUserName == userName);

        _dynamoDbContextMock.Verify(d => d.QueryAsync<Friendship>(userName, It.IsAny<DynamoDBOperationConfig>()), Times.Once);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Friendship>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetAllFriends_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        const string userName = "user1@example.com";
        const FriendshipStatus friendshipStatus = FriendshipStatus.Accepted;

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(FriendshipTableName);

        // Mock the AsyncSearch<Friendship> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Friendship>>();
        queryFromSearchMock.Setup(q => q.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error querying friendships"));

        // Setup QueryAsync to return the mocked AsyncSearch<Friendship>
        _dynamoDbContextMock.Setup(d => d.QueryAsync<Friendship>(userName, It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);
        
        var result = await FriendshipActions.GetAllFriends(userName, friendshipStatus, _dynamoDbContextMock.Object, _appSettingsMock.Object);

        // Assert
        Assert.Empty(result); // Should return an empty list on exception
    }
    
    [Fact]
    public async Task GetFriendship_ShouldReturnFriendship_WhenExists()
    {
        // Arrange
        const string fromUserName = "user1@example.com";
        const string toUserName = "user2@example.com";
        var friendship = new Friendship
        {
            FromUserName = fromUserName,
            ToUserName = toUserName,
            Status = FriendshipStatus.Accepted
        };

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(string.Empty);
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(friendship);

        // Act
        var result = await FriendshipActions.GetFriendship(fromUserName, toUserName, _dynamoDbContextMock.Object, _appSettingsMock.Object);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var returnedFriendship = Assert.IsType<Friendship>(actionResult.Value);

        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);

        _dynamoDbContextMock.Verify(d => d.LoadAsync<Friendship>(fromUserName, toUserName, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFriendship_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        const string fromUserName = "user1@example.com";
        const string toUserName = "user2@example.com";

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(string.Empty);
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error loading friendship from DynamoDB"));

        // Act
        var result = await FriendshipActions.GetFriendship(fromUserName, toUserName, _dynamoDbContextMock.Object, _appSettingsMock.Object);

        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Friendship>(fromUserName, toUserName, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
}
