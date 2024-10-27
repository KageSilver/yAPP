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
    private static string _testUid = "";

    private ICognitoActions _cognitoActions;
    private ICommentActions _commentActions;

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
        _testUid = user.Id;

        // Arrange
        var newComment = new NewComment
        {
            PID = "createCommentValidRequest",
            UID = _testUid,
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
        Assert.Equal(_testUid, comment.UID);
        Assert.Equal(newComment.CommentBody, comment.CommentBody);
        Assert.Equal(newComment.PID, comment.PID);

        // Clean up (need to be uncommented when deletion is implemented)
        await _commentActions.DeleteComment(comment.CID);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(2)]
    public async Task CreateComment_CommenterNotFound_ReturnsNotFound()
    {
        // Arrange
        var newComment = new NewComment
        {
            PID = "createCommentCommenterNotFound",
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
            UID = _testUid,
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
        await _commentActions.DeleteComment(comment.CID);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
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
            UID = _testUid,
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
        var response2 = await _client.GetAsync($"/api/comments/getCommentsByUid?uid={request.UID}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var commentList = JsonConvert.DeserializeObject<List<Comment>>(responseString2);

        // Assert
        Assert.Equal(1, commentList.Count);
        Assert.Equal(newComment.CID, commentList.First().CID);
        Assert.Equal(newComment.PID, commentList.First().PID);
        Assert.Equal(newComment.UID, commentList.First().UID);
        Assert.Equal(newComment.CommentBody, commentList.First().CommentBody);
        Assert.Equal(newComment.Upvotes, commentList.First().Upvotes);
        Assert.Equal(newComment.Downvotes, commentList.First().Downvotes);

        // Clean up
        await _commentActions.DeleteComment(newComment.CID);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
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
            UID = _testUid,
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
        await _commentActions.DeleteComment(newComment.CID);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
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

    #region UpdateComment Tests

    [Fact, Order(14)]
    public async Task UpdateComment_ShouldReturnOk_WhenCommentIsUpdatedSuccessfully()
    {
        // Uses the test user set up in CreateComment_ValidRequest_ReturnsComment()

        // Arrange
        var newComment = new NewComment
        {
            PID = "UpdateCommentShouldReturnOk",
            UID = _testUid,
            CommentBody = "UpdateComment_ShouldReturnOk_WhenCommentIsUpdatedSuccessfully()",
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new comment to test
        var response1 = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseComment = JsonConvert.DeserializeObject<Comment>(responseString1);
        
        // Make updates to the comment
        responseComment.CommentBody = "this comment has been edited";
        var content2 = new StringContent(JsonConvert.SerializeObject(responseComment), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response2 = await _client.PutAsync($"/api/comments/updateComment", content2);
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var updatedComment = JsonConvert.DeserializeObject<Comment>(responseString2);

        // Assert
        Assert.NotNull(updatedComment);
        Assert.Equal(responseComment.CID, updatedComment.CID);
        Assert.Equal(responseComment.PID, updatedComment.PID);
        Assert.Equal(responseComment.UID, updatedComment.UID);
        Assert.Equal(responseComment.CommentBody, updatedComment.CommentBody);
        Assert.Equal(responseComment.Upvotes, updatedComment.Upvotes);
        Assert.Equal(responseComment.Downvotes, updatedComment.Downvotes);

        // Clean up
        await _commentActions.DeleteComment(responseComment.CID);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
    }
    
    [Fact, Order(15)]
    public async Task UpdateComment_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var content = new StringContent(JsonConvert.SerializeObject(null), System.Text.Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PutAsync($"/api/comments/updateComment", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
    
    #region DeleteComments Tests
    
    [Fact, Order(16)]
    public async Task DeleteComments_ShouldReturnTrue_WhenCommentsAreDeletedSuccessfully()
    {
        // Uses the test user set up in CreateComment_ValidRequest_ReturnsComment()

        // Setup the post to delete
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "DeleteComments_ShouldReturnTrue_WhenCommentsAreDeletedSuccessfully()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };

        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new post for testing
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var responsePost = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange comments now
        var newComment1 = new NewComment
        {
            PID = responsePost.PID,
            UID = _testUid,
            CommentBody = "DeleteComments_ShouldReturnTrue_WhenCommentsAreDeletedSuccessfully1()",
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(newComment1), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new comment for testing
        var response1 = await _client.PostAsync("/api/comments/createComment", content1);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseComment1 = JsonConvert.DeserializeObject<Comment>(responseString1);
        
        // Arrange
        var newComment2 = new NewComment
        {
            PID = responsePost.PID,
            UID = _testUid,
            CommentBody = "DeleteComments_ShouldReturnTrue_WhenCommentsAreDeletedSuccessfully2()",
        };

        var content2 = new StringContent(JsonConvert.SerializeObject(newComment2), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new comment for testing
        var response2 = await _client.PostAsync("/api/comments/createComment", content2);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        var responseString2 = await response2.Content.ReadAsStringAsync();
        var responseComment2 = JsonConvert.DeserializeObject<Comment>(responseString2);

        // Act (delete the post and hopefully the comments with it...)
        var postResponse = await _client.DeleteAsync($"/api/posts/deletePost?pid={responsePost.PID}");
        var postResponseString = postResponse.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(postResponseString);

        // Assert
        Assert.True(result);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
    }
    
    [Fact, Order(17)]
    public async Task DeleteComments_ShouldReturnBadRequest_WhenPostIdIsNull()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/posts/deletePost?pid={null}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(18)]
    public async Task DeleteComments_ShouldReturnFalse_WhenDeleteFails()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/posts/deletePost?pid={"!"}");
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region DeleteComment Tests
    
    [Fact, Order(19)]
    public async Task DeleteComment_ShouldReturnTrue_WhenCommentIsDeletedSuccessfully()
    {
        // Uses the test user set up in CreateComment_ValidRequest_ReturnsComment()
        // Arrange
        var newComment = new NewComment
        {
            UID = _testUid,
            CommentBody = "DeleteComment_ShouldReturnTrue_WhenCommentIsDeletedSuccessfully()",
            PID = "DeleteCommentShouldReturnTrue"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new comment for testing
        var response1 = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response1.Content.ReadAsStringAsync();
        var responseComment = JsonConvert.DeserializeObject<Comment>(responseString);

        // Act
        var response2 = await _client.DeleteAsync($"/api/comments/deleteComment?cid={responseComment.CID}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.True(result);
        // Test user is deleted in DeleteComment_ShouldReturnFalse_WhenDeleteFails()
    }
    
    [Fact, Order(20)]
    public async Task DeleteComment_ShouldReturnBadRequest_WhenCommentIdIsNull()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/comments/deleteComment?cid={null}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(21)]
    public async Task DeleteComment_ShouldReturnFalse_WhenDeleteFails()
    {
        // Act
        // This comment id will need to be changed
        var response2 = await _client.DeleteAsync($"/api/comments/deleteComment?cid={"DeleteCommentShouldReturnTrue"}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.False(result);
        
        // Clean up
        await _cognitoActions.DeleteUser(TestUserEmail);
    }

    #endregion

}