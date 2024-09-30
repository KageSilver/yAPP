using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using yAppLambda.Common;
using yAppLambda.Models;

namespace Tests.UnitTests;

public class CognitoActionsTests
{
    private readonly Mock<IAmazonCognitoIdentityProvider> _cognitoClientMock;
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly CognitoActions _cognitoActions;

    public CognitoActionsTests()
    {
        _cognitoClientMock = new Mock<IAmazonCognitoIdentityProvider>();
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);

        _cognitoActions = new CognitoActions(_cognitoClientMock.Object, _appSettingsMock.Object);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnUpdatedUser_WhenUpdateIsSuccessful()
    {
        // Arrange
        var user = new User { UserName = "user1", Name = "User One", NickName = "User1Nick" };

        var updateResponse = new AdminUpdateUserAttributesResponse
        {
            HttpStatusCode = HttpStatusCode.OK
        };

        var getUserResponse = new AdminGetUserResponse
        {
            Username = "user1",
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = "user1@example.com" },
                new AttributeType { Name = "name", Value = "User One" },
                new AttributeType { Name = "nickname", Value = "User1Nick" },
                new AttributeType { Name = "sub", Value = "12345" }
            }
        };

        _cognitoClientMock.Setup(c =>
                c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateResponse);

        _cognitoClientMock.Setup(c =>
                c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getUserResponse);

        // Act
        var result = await _cognitoActions.UpdateUser(user);

        // Assert
        var actionResult = Assert.IsType<ActionResult<User>>(result);
        var returnedUser = Assert.IsType<User>(actionResult.Value);

        Assert.Equal("user1", returnedUser.UserName);
        Assert.Equal("User One", returnedUser.Name);
        Assert.Equal("User1Nick", returnedUser.NickName);
        _cognitoClientMock.Verify(
            c => c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNull_WhenUpdateFails()
    {
        // Arrange
        var user = new User { UserName = "user1", Name = "User One", NickName = "User1Nick" };

        _cognitoClientMock.Setup(c =>
                c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _cognitoActions.UpdateUser(user);

        // Assert
        Assert.Null(result);
        _cognitoClientMock.Verify(
            c => c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userName = "user1";

        var getUserResponse = new AdminGetUserResponse
        {
            Username = userName,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = "user1@example.com" },
                new AttributeType { Name = "name", Value = "User One" },
                new AttributeType { Name = "nickname", Value = "User1Nick" },
                new AttributeType { Name = "sub", Value = "12345" }
            }
        };

        _cognitoClientMock.Setup(c =>
                c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getUserResponse);

        // Act
        var result = await _cognitoActions.GetUser(userName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.UserName);
        Assert.Equal("User One", result.Name);
        Assert.Equal("user1@example.com", result.Email);
        Assert.Equal("12345", result.Id);
    }

    [Fact]
    public async Task GetUser_ShouldReturnNull_WhenAmazonServiceExceptionIsThrown()
    {
        // Arrange
        const string userName = "user1";

        _cognitoClientMock.Setup(c =>
                c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonServiceException("Service error"));

        // Act
        var result = await _cognitoActions.GetUser(userName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        const string userId = "12345";

        var listUsersResponse = new ListUsersResponse
        {
            Users = new List<UserType>
            {
                new UserType
                {
                    Username = "user1",
                    Attributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = "user1@example.com" },
                        new AttributeType { Name = "name", Value = "User One" },
                        new AttributeType { Name = "sub", Value = "12345" },
                        new AttributeType { Name = "nickname", Value = "User1Nick" }
                    }
                }
            }
        };

        _cognitoClientMock.Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listUsersResponse);

        // Act
        var result = await _cognitoActions.GetUserById(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.UserName);
        Assert.Equal("User One", result.Name);
        Assert.Equal("user1@example.com", result.Email);
        Assert.Equal("12345", result.Id);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var userId = "12345";

        var listUsersResponse = new ListUsersResponse
        {
            Users = new List<UserType>()
        };

        _cognitoClientMock.Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(listUsersResponse);

        // Act
        var result = await _cognitoActions.GetUserById(userId);

        // Assert
        Assert.Null(result);
    }
}