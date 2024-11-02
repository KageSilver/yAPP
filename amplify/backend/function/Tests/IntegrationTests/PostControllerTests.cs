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
using System.Net;
using Newtonsoft.Json;
using yAppLambda.Models;
using System.Text;

namespace Tests.IntegrationTests;

public class PostControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce4@simulator.amazonses.com";
    private const string TestUserEmail2 = "bounce3@simulator.amazonses.com";
    private static string _testUserId = ""; // this will be updated in the first test when the test user is created

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

    #region CreatePost Tests

    [Fact, Order(1)]
    public async Task CreatePost_ValidRequest_ReturnsPost()
    {
        //setup the user for testing
        await _cognitoActions.CreateUser(TestUserEmail);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);
        _testUserId = user.Id;

        // Arrange
        var newPost = new NewPost
        {
            UID = _testUserId,
            PostTitle = "CreatePost_ValidRequest_ReturnsPost()",
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
        Assert.Equal(_testUserId, post.UID);
        Assert.Equal(newPost.PostTitle, post.PostTitle);
        Assert.Equal(newPost.PostBody, post.PostBody);
        Assert.Equal(newPost.DiaryEntry, post.DiaryEntry);
        Assert.Equal(newPost.Anonymous, post.Anonymous);

        // Clean up
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }

    [Fact, Order(2)]
    public async Task CreatePost_PosterNotFound_ReturnsNotFound()
    {
        // Arrange
        var newPost = new NewPost
        {
            UID = "userDoesNotExist",
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

    [Fact, Order(3)]
    public async Task CreatePost_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var newPost = new NewPost
        {
            UID = "",
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

    [Fact, Order(4)]
    public async Task GetRecentPosts_ShouldReturnPosts_WhenRequestIsSuccessful()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()
        // Arrange
        var newPost = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetRecentPosts_ShouldReturnPosts_WhenRequestIsSuccessful()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new post for testing
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        var responseString = await response1.Content.ReadAsStringAsync();
        var responsePost = JsonConvert.DeserializeObject<Post>(responseString);
        
        var list = new List<Post>();
        list.Add(responsePost);

        // Act
        var time = new TimeSpan(0, 0, 0, 1);
        var date = DateTime.Now.Add(time).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        var response2 = await _client.GetAsync($"/api/posts/getRecentPosts?since={date}&maxResults={1}");

        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        Assert.NotNull(responseString2);
        
        // Clean up
        await _postActions.DeletePost(responsePost.PID);
        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }
    
    [Fact, Order(5)]
    public async Task GetRecentPosts_ShouldReturnBadRequest_WithInvalidRequest()
    {
        // Act
        var response = await _client.GetAsync($"/api/posts/getRecentPosts?since={DateTime.Now}&maxResults={-1}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region DeletePost Tests
    
    [Fact, Order(6)]
    public async Task DeletePost_ShouldReturnTrue_WhenPostIsDeletedSuccessfully()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()
        // Arrange
        var newPost = new NewPost
        {
            UID = _testUserId,
            PostTitle = "DeletePost_ShouldReturnTrue_WhenPostIsDeletedSuccessfully()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new post for testing
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        var responseString = await response1.Content.ReadAsStringAsync();
        var responsePost = JsonConvert.DeserializeObject<Post>(responseString);

        // Act
        var response2 = await _client.DeleteAsync($"/api/posts/deletePost?pid={responsePost.PID}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.True(result);

        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }
    
    [Fact, Order(7)]
    public async Task DeletePost_ShouldReturnBadRequest_WhenPostIdIsNull()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/posts/deletePost?pid={null}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(8)]
    public async Task DeletePost_ShouldReturnFalse_WhenDeleteFails()
    {
        // Act
        var response2 = await _client.DeleteAsync($"/api/posts/deletePost?pid={"1"}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region UpdatePost Tests

    [Fact, Order(9)]
    public async Task UpdatePost_ShouldReturnOk_WhenPostIsUpdatedSuccessfully()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()

        // Arrange
        var newPost = new NewPost
        {
            UID = _testUserId,
            PostTitle = "title",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new post for testing
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        var responseString = await response1.Content.ReadAsStringAsync();
        var responsePost = JsonConvert.DeserializeObject<Post>(responseString);
        
        // make updates to the post
        responsePost.PostTitle = "edited post";
        responsePost.PostBody = "this post has been edited";
        var content2 = new StringContent(JsonConvert.SerializeObject(responsePost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response2 = await _client.PutAsync($"/api/posts/updatePost", content2);
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var updatedPost = JsonConvert.DeserializeObject<Post>(responseString2);

        // Assert
        Assert.NotNull(updatedPost);
        Assert.Equal(responsePost.PID, updatedPost.PID);
        Assert.Equal(responsePost.UID, updatedPost.UID);
        Assert.Equal(responsePost.PostTitle, updatedPost.PostTitle);
        Assert.Equal(responsePost.PostBody, updatedPost.PostBody);
        Assert.Equal(responsePost.Upvotes, updatedPost.Upvotes);
        Assert.Equal(responsePost.Downvotes, updatedPost.Downvotes);
        Assert.Equal(responsePost.DiaryEntry, updatedPost.DiaryEntry);
        Assert.Equal(responsePost.Anonymous, updatedPost.Anonymous);

        // Clean up
        await _postActions.DeletePost(responsePost.PID);
        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }
    
    [Fact, Order(10)]
    public async Task UpdatePost_ShouldReturnBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var content = new StringContent(JsonConvert.SerializeObject(null), System.Text.Encoding.UTF8,
            "application/json");
        
        // Act
        var response = await _client.PutAsync($"/api/posts/updatePost", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
    
    #region GetPostById Tests
    
    [Fact, Order(11)]
    public async Task GetPostById_ShouldReturnPost_WhenSuccessful()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()

        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "title",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new post to query
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newPost = JsonConvert.DeserializeObject<Post>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/posts/getPostById?pid={newPost.PID}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString2);

        // Assert
        Assert.NotNull(post);
        Assert.Equal(newPost.PID, post.PID);
        Assert.Equal(newPost.UID, post.UID);
        Assert.Equal(newPost.PostTitle, post.PostTitle);
        Assert.Equal(newPost.PostBody, post.PostBody);
        Assert.Equal(newPost.Upvotes, post.Upvotes);
        Assert.Equal(newPost.Downvotes, post.Downvotes);
        Assert.Equal(newPost.DiaryEntry, post.DiaryEntry);
        Assert.Equal(newPost.Anonymous, post.Anonymous);

        // Clean up
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }

    [Fact, Order(12)]
    public async Task GetPostById_ShouldReturnNotFound_WhenPostNotFound()
    {
        // Act
        var pid = "1";
        var response = await _client.GetAsync($"/api/posts/getPostById?pid={pid}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(13)]
    public async Task GetPostById_ShouldReturnBadRequest_WithInvalidPostId()
    {
        // Act
        var response = await _client.GetAsync($"/api/posts/getPostById?pid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion

    #region GetPostsByUser Tests

    [Fact, Order(14)]
    public async Task GetPostsByUser_ShouldReturnPosts_WhenSuccessful()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()

        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetPostsByUser_ShouldReturnPosts_WhenSuccessful()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new post to query
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newPost = JsonConvert.DeserializeObject<Post>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/posts/getPostsByUser?uid={_testUserId}&diaryEntry={false}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var postList = JsonConvert.DeserializeObject<List<Post>>(responseString2);

        // Assert
        Assert.Equal(1, postList.Count);
        Assert.Equal(newPost.PID, postList.First().PID);
        Assert.Equal(newPost.UID, postList.First().UID);
        Assert.Equal(newPost.PostTitle, postList.First().PostTitle);
        Assert.Equal(newPost.PostBody, postList.First().PostBody);
        Assert.Equal(newPost.Upvotes, postList.First().Upvotes);
        Assert.Equal(newPost.Downvotes, postList.First().Downvotes);
        Assert.Equal(newPost.DiaryEntry, postList.First().DiaryEntry);
        Assert.Equal(newPost.Anonymous, postList.First().Anonymous);

        // Clean up
        await _postActions.DeletePost(newPost.PID);
        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }

    [Fact, Order(15)]
    public async Task GetPostsByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var response = await _client.GetAsync($"/api/posts/getPostsByUser?uid={null}&diaryEntry={false}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GetDiariesByUser Tests

    [Fact, Order(16)]
    public async Task GetDiariesByUser_ShouldReturnPosts_WhenSuccessful()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()

        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetDiariesByUser_ShouldReturnPosts_WhenSuccessful()",
            PostBody = "body",
            DiaryEntry = true,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new post to query
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newPost = JsonConvert.DeserializeObject<Post>(responseString1);

        // Act
        var date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        var response2 = await _client.GetAsync($"/api/posts/getDiariesByUser?uid={_testUserId}&current={date}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var postList = JsonConvert.DeserializeObject<List<Post>>(responseString2);

        // Assert
        Assert.Equal(1, postList.Count);
        Assert.Equal(newPost.PID, postList.First().PID);
        Assert.Equal(newPost.UID, postList.First().UID);
        Assert.Equal(newPost.PostTitle, postList.First().PostTitle);
        Assert.Equal(newPost.PostBody, postList.First().PostBody);
        Assert.Equal(newPost.Upvotes, postList.First().Upvotes);
        Assert.Equal(newPost.Downvotes, postList.First().Downvotes);
        Assert.Equal(newPost.DiaryEntry, postList.First().DiaryEntry);
        Assert.Equal(newPost.Anonymous, postList.First().Anonymous);

        // Clean up
        await _postActions.DeletePost(newPost.PID);
        // Test user is deleted in GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    }

    [Fact, Order(17)]
    public async Task GetDiariesByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var response = await _client.GetAsync($"/api/posts/getDiariesByUser?uid={null}&current={DateTime.Now}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region GetDiariesByFriends Tests

    [Fact, Order(18)]
    public async Task GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()
    {
        // Uses the test user set up in CreatePost_ValidRequest_ReturnsPost()

        // Creating test user for friendship setup
        await _cognitoActions.CreateUser(TestUserEmail2);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail2}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);
        string _testUserId2 = user.Id;

        // Arrange
        var friendRequest = new FriendRequest
        {
            FromUserName = TestUserEmail2,
            ToUserId = _testUserId
        };

        var content = new StringContent(JsonConvert.SerializeObject(friendRequest), System.Text.Encoding.UTF8,
            "application/json");
        var friendshipResponse = await _client.PostAsync("/api/friends/friendRequest", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        await friendshipResponse.Content.ReadAsStringAsync();

        var friendRequest2 = new FriendRequest
        {
            FromUserName = TestUserEmail2,
            ToUserName = TestUserEmail,
            Status = 1 // Accepted
        };

        var content2 = new StringContent(JsonConvert.SerializeObject(friendRequest2), Encoding.UTF8, "application/json");

        // Act
        var updateRequest = await _client.PutAsync("/api/friends/updateFriendRequest", content2);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        await updateRequest.Content.ReadAsStringAsync();

        // Arrange
        var request = new NewPost
        {
            UID = _testUserId2,
            PostTitle = "GetDiariesByFriends_ShouldReturnPosts_WhenSuccessful()",
            PostBody = "body",
            DiaryEntry = true,
            Anonymous = true
        };
        
        content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new post to query
        var response1 = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newPost = JsonConvert.DeserializeObject<Post>(responseString1);

        // Act
        var date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
        var response2 = await _client.GetAsync($"/api/posts/getDiariesByFriends?uid={_testUserId}&current={date}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var postList = JsonConvert.DeserializeObject<List<Post>>(responseString2);

        // Assert
        Assert.Equal(1, postList.Count);
        Assert.Equal(newPost.PID, postList.First().PID);
        Assert.Equal(newPost.UID, postList.First().UID);
        Assert.Equal(newPost.PostTitle, postList.First().PostTitle);
        Assert.Equal(newPost.PostBody, postList.First().PostBody);
        Assert.Equal(newPost.Upvotes, postList.First().Upvotes);
        Assert.Equal(newPost.Downvotes, postList.First().Downvotes);
        Assert.Equal(newPost.DiaryEntry, postList.First().DiaryEntry);
        Assert.Equal(newPost.Anonymous, postList.First().Anonymous);

        // Clean up
        await _cognitoActions.DeleteUser(TestUserEmail);
        await _cognitoActions.DeleteUser(TestUserEmail2);
        await _postActions.DeletePost(newPost.PID);
    }

    [Fact, Order(19)]
    public async Task GetDiariesByFriends_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var response = await _client.GetAsync($"/api/posts/getDiariesByFriends?uid={null}&current={DateTime.Now}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}