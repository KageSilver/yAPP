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

public class PostControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce4@simulator.amazonses.com";

    private ICognitoActions _cognitoActions;
    private IPostActions _postActions;

    public PostControllerIntegrationTests()
    {
        var webHostBuilder = new WebHostBuilder().UseStartup<Startup>();

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

        _postActions = new PostActions(_appSettings, dynamoDbContext);
    }

    [Fact]
    public async Task CreatePost_ValidRequest_ReturnsPost()
    {
        //setup the user for testing
        await _cognitoActions.CreateUser(TestUserEmail);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);

        // Arrange
        var newPost = new NewPost
        {
            UserName = TestUserEmail,
            PostTitle = "title",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();

        var post = JsonConvert.DeserializeObject<Post>(responseString);

        Assert.NotNull(post);
        Assert.Equal(TestUserEmail, post.UserName);
        Assert.Equal(newPost.PostTitle, post.PostTitle);
        Assert.Equal(newPost.PostBody, post.PostBody);
        Assert.Equal(newPost.DiaryEntry, post.DiaryEntry);
        Assert.Equal(newPost.Anonymous, post.Anonymous);

        // Clean up
        await _cognitoActions.DeleteUser(TestUserEmail);
        await _postActions.DeletePost(post.PID);
    }

    [Fact]
    public async Task CreatePost_PosterNotFound_ReturnsNotFound()
    {
        // Arrange
        var newPost = new NewPost
        {
            UserName = "userDoesNotExist",
            PostTitle = "title",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };

        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePost_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var newPost = new NewPost
        {
            UserName = "",
            PostTitle = "",
            PostBody = "",
            DiaryEntry = false,
            Anonymous = true
        };

        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}