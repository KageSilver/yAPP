using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Threading.Tasks;
using Xunit;
using yAppLambda;
using Newtonsoft.Json;
using System.Net;
using yAppLambda.Models;

namespace Tests.IntegrationTests;

public class CommentControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    // We must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce4@simulator.amazonses.com";
    private static string testUid = "";

    private ICognitoActions _cognitoActions;
    private ICommentActions _commentActions;
    private static int numComments = 0;

    public CommentControllerIntegrationTests()
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

        _commentActions = new CommentActions(_appSettings, dynamoDbContext);
    }

    #region CreateComment Tests

    [Fact, Order(1)]
    public async Task CreateComment_ValidRequest_ReturnsComment()
    {
        //Setup the user for testing in first test
        await _cognitoActions.CreateUser(TestUserEmail);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);
        testUid = user.Id;

        // Arrange
        var newComment = new NewComment
        {
            PID = "1",
            UID = testUid,
            CommentBody = "CreateComment_ValidRequest_ReturnsComment()"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        // Assert
        var responseString = await response.Content.ReadAsStringAsync();

        var comment = JsonConvert.DeserializeObject<Comment>(responseString);

        Assert.NotNull(comment);
        Assert.Equal(testUid, comment.UID);
        Assert.Equal(newComment.CommentBody, comment.CommentBody);
        Assert.Equal(newComment.PID, comment.PID);

        // Clean up (need to be uncommented when deletion is implemented)
        // await _commentActions.DeleteComment(comment.CID);
        // Test user is deleted in GetCommentsByPid_ShouldReturnComments_WhenSuccessful()
        numComments+=1;
    }

    [Fact, Order(2)]
    public async Task CreateComment_CommenterNotFound_ReturnsNotFound()
    {
        // Arrange
        var newComment = new NewComment
        {
            PID = "1",
            UID = "userDoesNotExist",
            CommentBody = "body"
        };

        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/comments/createComment", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(3)]
    public async Task CreateComment_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var newComment = new NewComment
        {
            PID = "",
            UID = "",
            CommentBody = ""
        };

        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/comments/createComment", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
    
    #region GetCommentById Tests
    
    [Fact, Order(4)]
    public async Task GetCommentById_ShouldReturnComment_WhenSuccessful()
    {
        // Uses the test user set up in CreateComment_ValidRequest_ReturnsComment()

        // Arrange
        var request = new NewComment
        {
            PID = "getCommentIdShouldReturnComment",
            UID = testUid,
            CommentBody = "GetCommentById_ShouldReturnComment_WhenSuccessful()"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new comment to query
        var response1 = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newComment = JsonConvert.DeserializeObject<Comment>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/comments/getCommentById?cid={newComment.CID}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var comment = JsonConvert.DeserializeObject<Comment>(responseString2);

        // Assert
        Assert.NotNull(comment);
        Assert.Equal(newComment.CID, comment.CID);
        Assert.Equal(newComment.PID, comment.PID);
        Assert.Equal(newComment.UID, comment.UID);
        Assert.Equal(newComment.CommentBody, comment.CommentBody);
        Assert.Equal(newComment.Upvotes, comment.Upvotes);
        Assert.Equal(newComment.Downvotes, comment.Downvotes);

        // Clean up
        // await _commentActions.DeleteComment(comment.CID);
        // Test user is deleted in GetCommentsByPid_ShouldReturnComments_WhenSuccessful()
        numComments+=1;
    }

    [Fact, Order(5)]
    public async Task GetCommentById_ShouldReturnNotFound_WhenCommentNotFound()
    {
        // Act
        var cid = "1";
        var response = await _client.GetAsync($"/api/comments/getCommentById?cid={cid}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(6)]
    public async Task GetCommentById_ShouldReturnBadRequest_WithNullCommentId()
    {
        // Act
        var response = await _client.GetAsync($"/api/comments/getCommentById?cid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(7)]
    public async Task GetCommentById_ShouldReturnBadRequest_WithEmptyCommentId()
    {
        // Act
        var response = await _client.GetAsync($"/api/comments/getCommentById?cid=");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion

    #region GetCommentsByUid Tests

    [Fact, Order(8)]
    public async Task GetCommentsByUid_ShouldReturnComments_WhenSuccessful()
    {
        // Uses the test user set up in CreateComment_ValidRequest_ReturnsComment()

        // Arrange
        var request = new NewComment
        {
            PID = "getCommentsByUidShouldReturnComments",
            UID = testUid,
            CommentBody = "GetCommentsByUid_ShouldReturnComments_WhenSuccessful()",
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new comment to query
        var response1 = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newComment = JsonConvert.DeserializeObject<Comment>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/comments/getCommentsByUid?uid={testUid}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var commentList = JsonConvert.DeserializeObject<List<Comment>>(responseString2);

        numComments+=1;
        // Assert
        Assert.Equal(numComments, commentList.Count);
        Assert.Equal(newComment.CID, commentList.Last().CID);
        Assert.Equal(newComment.PID, commentList.Last().PID);
        Assert.Equal(newComment.UID, commentList.Last().UID);
        Assert.Equal(newComment.CommentBody, commentList.Last().CommentBody);
        Assert.Equal(newComment.Upvotes, commentList.Last().Upvotes);
        Assert.Equal(newComment.Downvotes, commentList.Last().Downvotes);

        // Clean up
        //await _commentActions.DeleteComment(newComment.CID);
    }

    [Fact, Order(9)]
    public async Task GetCommentsByUid_ShouldReturnBadRequest_WithNullUid()
    {
        // Act
        var response = await _client.GetAsync($"/api/comments/getCommentsByUid?uid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(10)]
    public async Task GetCommentsByUid_ShouldReturnBadRequest_WithEmptyUid()
    {
        // Act
        var response = await _client.GetAsync($"/api/comments/getCommentsByUid?uid=");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GetCommentsByPid Tests

    [Fact, Order(11)]
    public async Task GetCommentsByPid_ShouldReturnComments_WhenSuccessful()
    {
        // Uses the test user set up in CreateComment_ValidRequest_ReturnsComment()

        // Arrange
        var request = new NewComment
        {
            PID = "getCommentsByPidShouldReturnComments",
            UID = testUid,
            CommentBody = "GetCommentsByPid_ShouldReturnComments_WhenSuccessful()",
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new comment to query
        var response1 = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newComment = JsonConvert.DeserializeObject<Comment>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/comments/getCommentsByPid?pid={request.PID}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var commentList = JsonConvert.DeserializeObject<List<Comment>>(responseString2);

        // Assert
        Assert.Equal(1, commentList.Count);
        Assert.Equal(newComment.CID, commentList.Last().CID);
        Assert.Equal(newComment.PID, commentList.Last().PID);
        Assert.Equal(newComment.UID, commentList.Last().UID);
        Assert.Equal(newComment.CommentBody, commentList.Last().CommentBody);
        Assert.Equal(newComment.Upvotes, commentList.Last().Upvotes);
        Assert.Equal(newComment.Downvotes, commentList.Last().Downvotes);

        // Clean up
        await _cognitoActions.DeleteUser(TestUserEmail);
        //await _commentActions.DeleteComment(newComment.CID);
    }

    [Fact, Order(12)]
    public async Task GetCommentsByPid_ShouldReturnBadRequest_WithNullPid()
    {
        // Act
        var response = await _client.GetAsync($"/api/comments/getCommentsByPid?pid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(13)]
    public async Task GetCommentsByPid_ShouldReturnBadRequest_WithEmptyPid()
    {
        // Act
        var response = await _client.GetAsync($"/api/comments/getCommentsByPid?pid=");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}