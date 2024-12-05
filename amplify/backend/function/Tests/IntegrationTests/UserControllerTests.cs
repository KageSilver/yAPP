using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Extensions.Ordering;
using yAppLambda;
using yAppLambda.Common;
using yAppLambda.Models;

namespace Tests.IntegrationTests;


public class UserControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    private readonly AmazonCognitoIdentityProviderClient _cognitoIdentityProvider;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce1@simulator.amazonses.com";
    private static string _testUserId = ""; // this will be updated from a test and use for another test

    private  ICognitoActions _cognitoActions;
    public UserControllerIntegrationTests()
    {
        var webHostBuilder = new WebHostBuilder()
            .UseStartup<Startup>();

        var server = new TestServer(webHostBuilder);

        _client = server.CreateClient();

        _appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(@"appSettings.json"),
            new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
        
        IAmazonCognitoIdentityProvider cognitoClient = new AmazonCognitoIdentityProviderClient(_appSettings.AwsRegionEndpoint);
        _cognitoActions = new CognitoActions(cognitoClient, _appSettings);

        // setup the user for testing
        _cognitoActions.CreateUser(TestUserEmail).Wait();
    }

    [Fact, Order(1)]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.GetAsync("/api/users/getUserByName?username=nonexistentUser");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(2)]
    public async Task GetUser_ShouldReturnFound_WhenUserExist()
    {
        var response = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact, Order(3)]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.GetAsync("/api/users/getUserById?id=nonexistentUser");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(4)]
    public async Task GetUserById_ShouldReturnFound_WhenUserExist()
    {
        //update id 
        var response = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(user);
        _testUserId = user.Id;
        Assert.NotEmpty(_testUserId);
        
        var response2 = await _client.GetAsync($"/api/users/getUserById?id={_testUserId}");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact, Order(5)]
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


    [Fact, Order(6)]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserNameIsNull()
    {
        var user = new User { UserName = null, Name = "", NickName = "" };
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(7)]
    public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var user = new User { UserName = "nonexistentUser", Name = "Test User", NickName = "TestNick" };
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact, Order(8)]
    public async Task UpdateUser_ShouldReturnOk_WhenUserExist()
    {
        var user = new User { UserName = TestUserEmail, Name = "Test User", NickName = "TestNick" };
        var response = await _client.PostAsync("/api/users/updateUser",
            new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //delete the user after the test
        _cognitoActions.DeleteUser(TestUserEmail).Wait();
        //verify the user is deleted
        var response2 = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
    }
}