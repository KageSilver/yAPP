using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Extensions.Ordering;
using yAppLambda;
using yAppLambda.Models;

public class UserControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    private readonly AmazonCognitoIdentityProviderClient _cognitoIdentityProvider;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce@simulator.amazonses.com";

    public UserControllerIntegrationTests()
    {
        var webHostBuilder = new WebHostBuilder()
            .UseStartup<Startup>();

        var server = new TestServer(webHostBuilder);

        _client = server.CreateClient();

        _appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(@"appSettings.json"),
            new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });

        var awsCognitoIdentityProviderConfig = new AmazonCognitoIdentityProviderConfig
        {
            RegionEndpoint = _appSettings.AwsRegionEndpoint
        };

        _cognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(awsCognitoIdentityProviderConfig);

        // setup the user for testing
        CreateUserAsync(TestUserEmail).Wait();
    }

    [Fact, Order(1)]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.GetAsync("/api/users/getUserByName?username=nonexistentUser");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(1)]
    public async Task GetUser_ShouldReturnFound_WhenUserExist()
    {
        var response = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact, Order(2)]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var user = new User(); // Empty or null user object to trigger BadRequest

        // Act
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact, Order(2)]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserNameIsNull()
    {
        var user = new User { UserName = null, Name = "", NickName = "" };
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(2)]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var user = new User { UserName = "nonexistentUser", Name = "Test User", NickName = "TestNick" };
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(2)]
    public async Task UpdateUser_ShouldReturnOk_WhenUserExist()
    {
        var user = new User { UserName = TestUserEmail, Name = "Test User", NickName = "TestNick" };
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //delete the user after the test
        DeleteUserAsync(TestUserEmail).Wait();
        //verify the user is deleted
        var response2 = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
    }


    /// <summary>
    /// Creates a new user in the Cognito user pool with the specified email.
    /// Purpose to create a user for testing
    /// </summary>
    /// <param name="email">The email address of the user to create.</param>
    private async Task CreateUserAsync(string email)
    {
        var request = new AdminCreateUserRequest
        {
            UserPoolId = _appSettings.UserPoolId,
            Username = email,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType
                {
                    Name = "email",
                    Value = email
                },
                new AttributeType
                {
                    Name = "email_verified",
                    Value = "true"
                }
            },
            TemporaryPassword = "TemporaryPassword123!",
            MessageAction = "SUPPRESS" // To suppress sending a welcome email
        };

        try
        {
            var response = await _cognitoIdentityProvider.AdminCreateUserAsync(request);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating user: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes a user from the Cognito user pool with the specified username.
    /// </summary>
    /// <param name="username">The username of the user to delete.</param>
    private async Task DeleteUserAsync(string username)
    {
        var request = new AdminDeleteUserRequest
        {
            UserPoolId = _appSettings.UserPoolId,
            Username = username
        };

        try
        {
            await _cognitoIdentityProvider.AdminDeleteUserAsync(request);
            Console.WriteLine($"User {username} deleted successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting user: {e.Message}");
        }
    }
}