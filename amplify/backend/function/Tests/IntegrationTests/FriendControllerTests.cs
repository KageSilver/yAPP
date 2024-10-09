using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit.Extensions.Ordering;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Enum;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using yAppLambda;
using Newtonsoft.Json;
using System.Net;
using Moq;
using yAppLambda.Models;

namespace Tests.IntegrationTests;

public class FriendControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail1 = "bounce2@simulator.amazonses.com";
    private const string TestUserEmail2 = "bounce3@simulator.amazonses.com";

    private ICognitoActions _cognitoActions;
    private IFriendshipActions _friendshipActions;

    public FriendControllerIntegrationTests()
    {
        var webHostBuilder = new WebHostBuilder()
            .UseStartup<Startup>();

        var server = new TestServer(webHostBuilder);

        _client = server.CreateClient();

        _appSettings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(@"appSettings.json"),
            new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });

        IAmazonCognitoIdentityProvider cognitoClient =
            new AmazonCognitoIdentityProviderClient(_appSettings.AwsRegionEndpoint);

        _cognitoActions = new CognitoActions(cognitoClient, _appSettings);
        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = _appSettings.AwsRegionEndpoint
        };
        var client = new AmazonDynamoDBClient(config);
        IDynamoDBContext dynamoDbContext = new DynamoDBContext(client);

        _friendshipActions = new FriendshipActions(_appSettings, dynamoDbContext);

       
    }

    [Fact, Order(1)]
    public async Task SendFriendRequest_ValidRequest_ReturnsFriendship()
    {
        //setup the user for testing
        await _cognitoActions.CreateUser(TestUserEmail1);
        await _cognitoActions.CreateUser(TestUserEmail2);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created, and it take time
        
        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail2}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);

        // Arrange
        var friendRequest = new FriendRequest
        {
            FromUserName = TestUserEmail1,
            ToUserId = user.Id
        };

        var content = new StringContent(JsonConvert.SerializeObject(friendRequest), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/friends/friendRequest", content);
        
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        // Assert
        var responseString = await response.Content.ReadAsStringAsync();

        var friendship = JsonConvert.DeserializeObject<Friendship>(responseString);

        Assert.NotNull(friendship);
        Assert.Equal(TestUserEmail1, friendship.FromUserName);
        Assert.Equal(TestUserEmail2, friendship.ToUserName);
        Assert.Equal(FriendshipStatus.Pending, friendship.Status);
    }

    [Fact, Order(2)]
    public async Task SendFriendRequest_FriendNotFound_ReturnsNotFound()
    {
        // Arrange
        var friendRequest = new FriendRequest
        {
            FromUserName = "user1",
            ToUserId = "nonexistentUser"
        };

        var content = new StringContent(JsonConvert.SerializeObject(friendRequest), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/friends/friendRequest", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(3)]
    public async Task SendFriendRequest_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var friendRequest = new FriendRequest
        {
            FromUserName = "user1",
            ToUserId = null
        };

        var content = new StringContent(JsonConvert.SerializeObject(friendRequest), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/friends/friendRequest", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(4)]
    public async Task UpdateFriendRequest_ValidRequest_ReturnsUpdatedFriendship()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = TestUserEmail1,
            ToUserName = TestUserEmail2,
            Status = 1 // Accepted
        };

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/friends/updateFriendRequest", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();
        var updatedFriendship = JsonConvert.DeserializeObject<Friendship>(responseString);

        Assert.NotNull(updatedFriendship);
        Assert.Equal(TestUserEmail1, updatedFriendship.FromUserName);
        Assert.Equal(TestUserEmail2, updatedFriendship.ToUserName);
        Assert.Equal(FriendshipStatus.Accepted, updatedFriendship.Status);
    }

    [Fact, Order(5)]
    public async Task UpdateFriendRequest_FriendshipNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = "user1",
            ToUserName = "nonexistentUser",
            Status = 1 // Accepted
        };

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/friends/updateFriendRequest", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(6)]
    public async Task UpdateFriendRequest_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new FriendRequest
        {
            FromUserName = "", // Invalid username
            ToUserName = "user2",
            Status = 1 // Accepted
        };

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/friends/updateFriendRequest", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(7)]
    public async Task GetFriendsByStatus_ValidRequest_ReturnsFriendsList()
    {
        // Arrange
        var userName = TestUserEmail1;
        var status = 1; // Accepted

        // Act
        var response = await _client.GetAsync($"/api/friends/getFriendsByStatus?userName={userName}&status={status}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var friends = JsonConvert.DeserializeObject<List<Friendship>>(responseString);

        Assert.NotNull(friends);
        Assert.All(friends, friend => Assert.Equal(FriendshipStatus.Accepted, friend.Status));
        Assert.All(friends, friend => Assert.Equal(TestUserEmail1, friend.FromUserName));
    }

    [Fact, Order(8)]
    public async Task GetFriendsByStatus_FriendsNotFound_ReturnsNotFound()
    {
        // Arrange
        var userName = "nonexistentUser";
        var status = 1; // Accepted

        // Act
        var response = await _client.GetAsync($"/api/friends/getFriendsByStatus?userName={userName}&status={status}");
        var content = await response.Content.ReadAsStringAsync();
        var friends = JsonConvert.DeserializeObject<List<Friendship>>(content);
        Assert.Empty(friends);
    }

    [Fact, Order(9)]
    public async Task GetFriendsByStatus_InvalidRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/friends/getFriendsByStatus?userName=&status=1");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(10)]
    public async Task GetFriendsByStatus_AllStatuses_ReturnsAllFriends()
    {
        // Arrange
        var userName = TestUserEmail1;
        var status = -1; // All statuses

        // Act
        var response = await _client.GetAsync($"/api/friends/getFriendsByStatus?userName={userName}&status={status}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var friends = JsonConvert.DeserializeObject<List<Friendship>>(responseString);

        Assert.NotNull(friends);
        Assert.All(friends, friend => Assert.Equal(TestUserEmail1, friend.FromUserName));
    }

    [Fact, Order(11)]
    public async Task GetFriendsByStatus_DeclinedStatuses_ReturnsAllFriends()
    {
        // Arrange
        var userName = TestUserEmail1;
        var status = 2; // declined statuse

        // Act
        var response = await _client.GetAsync($"/api/friends/getFriendsByStatus?userName={userName}&status={status}");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var friends = JsonConvert.DeserializeObject<List<Friendship>>(responseString);

        Assert.Empty(friends);
        //clean up
        _cognitoActions.DeleteUser(TestUserEmail1).Wait();
        _cognitoActions.DeleteUser(TestUserEmail2).Wait();
        //clean up database
        var value = await _friendshipActions.DeleteFriendship(TestUserEmail1, TestUserEmail2);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        Assert.True(value);
    }
}