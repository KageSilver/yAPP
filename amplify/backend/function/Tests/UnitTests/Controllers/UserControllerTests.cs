namespace Tests.UnitTests.Controllers;

using yAppLambda.Common;
using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Controllers; 
using yAppLambda.Models;     

public class UserControllerTests
{
    private readonly UserController _controller;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly Mock<IAppSettings> _mockAppSettings;

    public UserControllerTests()
    {
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockAppSettings = new Mock<IAppSettings>();

        _mockAppSettings.Setup(a => a.UserPoolId).Returns("test_pool_id");

        _controller = new UserController(_mockAppSettings.Object, _mockCognitoActions.Object);
    }

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _controller.UpdateUser(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("request body is required and must contain username and name", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUsernameOrNameIsNull()
    {
        // Arrange
        var user = new User { UserName = null, Name = "John" }; // Username is null

        // Act
        var result = await _controller.UpdateUser(user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("request body is required and must contain username and name", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var user = new User { UserName = "johndoe", Name = "John" };
        _mockCognitoActions.Setup(m => m.UpdateUser(It.IsAny<User>())).ReturnsAsync((User)null);

        // Act
        var result = await _controller.UpdateUser(user);

        // Assert
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnUser_WhenSuccessful()
    {
        // Arrange
        var user = new User { UserName = "johndoe", Name = "John" };
        _mockCognitoActions.Setup(m => m.UpdateUser(It.IsAny<User>())).ReturnsAsync(user);

        // Act
        var result = await _controller.UpdateUser(user);

        // Assert
        var returnedUser = Assert.IsType<User>(result.Value);
        Assert.Equal(user, returnedUser);
    }

    #endregion

    #region GetUserByName Tests

    [Fact]
    public async Task GetUser_ShouldReturnBadRequest_WhenUsernameIsNull()
    {
        // Act
        var result = await _controller.GetUser(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("username is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUser_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockCognitoActions.Setup(m => m.GetUser(It.IsAny<string>())).ReturnsAsync((User)null);

        // Act
        var result = await _controller.GetUser("johndoe");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetUser_ShouldReturnUser_WhenSuccessful()
    {
        var user = new User { UserName = "johndoe", Name = "John" };
        _mockCognitoActions.Setup(m => m.GetUser(It.IsAny<string>())).ReturnsAsync(new User { UserName = "johndoe", Name = "John" });
        
        // Act
        var result = await _controller.GetUser("johndoe");

        // Assert
        var returnedUser = Assert.IsType<User>(result.Value); 
        Assert.Equal(user.UserName, returnedUser.UserName); 
        Assert.Equal(user.Name, returnedUser.Name); 
    }

    #endregion

    #region GetUserById Tests

    [Fact]
    public async Task GetUserById_ShouldReturnBadRequest_WhenIdIsNull()
    {
        // Act
        var result = await _controller.GetUserById(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Id is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        _mockCognitoActions.Setup(m => m.GetUserById(It.IsAny<string>())).ReturnsAsync((User)null);

        // Act
        var result = await _controller.GetUserById("123");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenSuccessful()
    {
        // Arrange
        var user = new User { UserName = "johndoe", Name = "John" };
        _mockCognitoActions.Setup(m => m.GetUserById(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById("123");

        // Assert
        var returnedUser = Assert.IsType<User>(result.Value);
        Assert.Equal(user, returnedUser);
    }

    #endregion
}
