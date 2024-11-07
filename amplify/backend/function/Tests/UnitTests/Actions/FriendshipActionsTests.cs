namespace Tests.UnitTests.Actions;

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

public class FriendshipActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly IFriendshipActions _friendshipActionsMock;
    private const string FriendshipTableName = "Friendship-test";

    public FriendshipActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(FriendshipTableName);

        // Initialize the dynamoDbContextMock
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();

        // Initialize the FriendshipActions with the mocks
        _friendshipActionsMock = new FriendshipActions(_appSettingsMock.Object, _dynamoDbContextMock.Object);
    }

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

        _appSettingsMock
        .Setup(a => a.FriendshipTableName)
        .Returns(FriendshipTableName);

        // Setup SaveAsync to succeed
        _dynamoDbContextMock
        .Setup(d => d.SaveAsync(It.IsAny<Friendship>(),
                                It.IsAny<DynamoDBOperationConfig>(),
                                It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        // Act
        var result = await _friendshipActionsMock.CreateFriendship(friendship);

        // Assert

        // Assert that the action returned a valid Friendship Object
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        // Assert that the Friendship Object contains the expected parameters
        var returnedFriendship = Assert.IsType<Friendship>(okResult.Value);
        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);

        // Verify that SaveAsync was called once with the correct parameters
        _dynamoDbContextMock
        .Verify(d => d.SaveAsync(It.IsAny<Friendship>(),
                                 It.IsAny<DynamoDBOperationConfig>(),
                                 It.IsAny<CancellationToken>()), Times.Once);
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

        _appSettingsMock
        .Setup(a => a.FriendshipTableName)
        .Returns(FriendshipTableName);

        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock
        .Setup(d => d.SaveAsync(It.IsAny<Friendship>(),
                                It.IsAny<DynamoDBOperationConfig>(),
                                It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Error saving to DynamoDB"));

        // Act
        var result = await _friendshipActionsMock.CreateFriendship(friendship);

        // Assert

        // Assert that the result is ActionResult<Friendship>
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);

        // Access the Result property to get the actual StatusCodeResult
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
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
            Status = FriendshipStatus.Accepted 
        };

        _appSettingsMock
        .Setup(a => a.FriendshipTableName)
        .Returns(string.Empty);

        // Setup SaveAsync to succeed
        _dynamoDbContextMock
        .Setup(d => d.SaveAsync(friendship,
                                It.IsAny<DynamoDBOperationConfig>(),
                                CancellationToken.None))
        .Returns(Task.CompletedTask);

        // Act
        var result = await _friendshipActionsMock.UpdateFriendshipStatus(friendship);

        // Assert

        // Assert that the action returned a valid Friendship Object
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        // Assert that the Friendship Object contains the expected parameters
        var returnedFriendship = Assert.IsType<Friendship>(okResult.Value);
        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);
        Assert.Equal(friendship.Status, returnedFriendship.Status);
        Assert.NotNull(returnedFriendship.UpdatedAt); // Ensure UpdatedAt is set

        // Verify that SaveAsync was called once with the correct parameters
        _dynamoDbContextMock
        .Verify(d => d.SaveAsync(friendship,
                                 It.IsAny<DynamoDBOperationConfig>(),
                                 CancellationToken.None), Times.Once);
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

        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock
        .Setup(d => d.SaveAsync(friendship,
                                It.IsAny<DynamoDBOperationConfig>(),
                                CancellationToken.None))
        .ThrowsAsync(new Exception("Error updating friendship in DynamoDB"));

        // Act
        var result = await _friendshipActionsMock.UpdateFriendshipStatus(friendship);

        // Assert

        // Assert that the result is ActionResult<Friendship>
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);

        // Access the Result property to get the actual StatusCodeResult
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
            new Friendship
            {
                FromUserName = userName,
                ToUserName = "user2@example.com",
                Status = friendshipStatus
            }
        };
        var friendshipsTo = new List<Friendship>
        {
            new Friendship
            {
                FromUserName = "user2@example.com",
                ToUserName = userName,
                Status = friendshipStatus
            }
        };

        _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Friendship> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Friendship>>();
        queryFromSearchMock
        .Setup(q => q.GetRemainingAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(friendshipsFrom);

        // Mock the AsyncSearch<Friendship> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Friendship>>();
        scanToSearchMock
        .Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(friendshipsTo);

        // Setup the IDynamoDBContext mock to return the mocked AsyncSearch objects
        _dynamoDbContextMock
        .Setup(d => d.QueryAsync<Friendship>(userName,
                                             It.IsAny<DynamoDBOperationConfig>()))
        .Returns(queryFromSearchMock.Object);

        _dynamoDbContextMock
        .Setup(d => d.ScanAsync<Friendship>(It.IsAny<List<ScanCondition>>(),
                                            It.IsAny<DynamoDBOperationConfig>()))
        .Returns(scanToSearchMock.Object);

        // Act
        var result = await _friendshipActionsMock.GetAllFriends(userName, friendshipStatus);

        // Assert
        Assert.Equal(2, result.Count); // One from `FromUserName` and one from `ToUserName`
        Assert.Contains(result, f => f.FromUserName == userName);
        Assert.Contains(result, f => f.ToUserName == userName);

        _dynamoDbContextMock
        .Verify(d => d.QueryAsync<Friendship>(userName,
                                              It.IsAny<DynamoDBOperationConfig>()), Times.Once);
        _dynamoDbContextMock
        .Verify(d => d.ScanAsync<Friendship>(It.IsAny<List<ScanCondition>>(),
                                             It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetAllFriends_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        const string userName = "user1@example.com";
        const FriendshipStatus friendshipStatus = FriendshipStatus.Accepted;

        _appSettingsMock
        .Setup(a => a.FriendshipTableName)
        .Returns(FriendshipTableName);

        // Mock the AsyncSearch<Friendship> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Friendship>>();
        queryFromSearchMock
        .Setup(q => q.GetRemainingAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Error querying friendships"));

        var result = await _friendshipActionsMock.GetAllFriends(userName, friendshipStatus);

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

        _appSettingsMock
        .Setup(a => a.FriendshipTableName)
        .Returns(string.Empty);

        // Setup DB mock to return a Friendship Object
        _dynamoDbContextMock
        .Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                            It.IsAny<DynamoDBOperationConfig>(),
                                            It.IsAny<CancellationToken>()))
        .ReturnsAsync(friendship);

        // Act
        var result = await _friendshipActionsMock.GetFriendship(fromUserName, toUserName);

        // Assert

        // Assert that the action returned a valid Friendship Object
        var actionResult = Assert.IsType<ActionResult<Friendship>>(result);
        var returnedFriendship = Assert.IsType<Friendship>(actionResult.Value);

        // Assert that the Friendship Object contains the expected parameters
        Assert.Equal(friendship.FromUserName, returnedFriendship.FromUserName);
        Assert.Equal(friendship.ToUserName, returnedFriendship.ToUserName);

        // Verify that LoadAsync was called once with the correct parameters
        _dynamoDbContextMock
        .Verify(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                             It.IsAny<DynamoDBOperationConfig>(),
                                             It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFriendship_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        const string fromUserName = "user1@example.com";
        const string toUserName = "user2@example.com";

        _appSettingsMock
        .Setup(a => a.FriendshipTableName)
        .Returns(string.Empty);

        // Set up LoadAsync to throw exception
        _dynamoDbContextMock
        .Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                            It.IsAny<DynamoDBOperationConfig>(),
                                            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Error loading friendship from DynamoDB"));

        // Act
        var result = await _friendshipActionsMock.GetFriendship(fromUserName, toUserName);

        // Assert
        Assert.Null(result);
        _dynamoDbContextMock
        .Verify(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                             It.IsAny<DynamoDBOperationConfig>(),
                                             It.IsAny<CancellationToken>()), Times.Once);
    }

    #region DeleteFriendShip Tests

    [Fact]
    public async Task DeleteFriendship_ShouldCallDeleteAsync()
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

        // Sets up LoadAsync to return the friendship (for GetFriendship)
        _dynamoDbContextMock
        .Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                            It.IsAny<DynamoDBOperationConfig>(),
                                            It.IsAny<CancellationToken>()))
        .ReturnsAsync(friendship);

        // Sets up DeleteAsync to succeed
        _dynamoDbContextMock
        .Setup(d => d.DeleteAsync(It.IsAny<Friendship>,
                                  It.IsAny<DynamoDBOperationConfig>(),
                                  It.IsAny<CancellationToken>()));

        // Act
        var result = await _friendshipActionsMock.DeleteFriendship(fromUserName, toUserName);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock
        .Verify(d => d.DeleteAsync(friendship,
                                   It.IsAny<DynamoDBOperationConfig>(),
                                   It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteFriendship_ShouldHandleException_WhenFriendshipDoesNotExist()
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

        // Sets up LoadAsync to return the friendship (for GetFriendship)
        _dynamoDbContextMock
        .Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                            It.IsAny<DynamoDBOperationConfig>(),
                                            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Friendship does not exist"));

        // Act
        var result = await _friendshipActionsMock.DeleteFriendship(fromUserName, toUserName);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock
        .Verify(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                             It.IsAny<DynamoDBOperationConfig>(),
                                             It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFriendship_ShouldHandleException_WhenDeleteFriendshipFails()
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

        // Sets up LoadAsync to return the friendship (for GetFriendship)
        _dynamoDbContextMock
        .Setup(d => d.LoadAsync<Friendship>(fromUserName, toUserName,
                                            It.IsAny<DynamoDBOperationConfig>(),
                                            It.IsAny<CancellationToken>()))
        .ReturnsAsync(friendship);

        // Sets up DeleteAsync to throw an exception
        _dynamoDbContextMock
        .Setup(d => d.DeleteAsync(It.IsAny<Friendship>(),
                                  It.IsAny<DynamoDBOperationConfig>(),
                                  It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Could not delete friendship"));

        // Act
        var result = await _friendshipActionsMock.DeleteFriendship(fromUserName, toUserName);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock
        .Verify(d => d.DeleteAsync(friendship,
                                   It.IsAny<DynamoDBOperationConfig>(),
                                   It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
