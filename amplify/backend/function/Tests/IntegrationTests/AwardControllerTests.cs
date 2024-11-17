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
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Controllers;
using yAppLambda.Enum;

namespace Tests.IntegrationTests;

public class AwardControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;
    private readonly List<AwardType> awardTypes;

    //we must use simulator email to test the user without using email quota
    private const string TestUserEmail1 = "bounce6@simulator.amazonses.com";
    private const string TestUserEmail2 = "bounce2@simulator.amazonses.com";
    private static string _testUserId = ""; // this will be updated in the first test when the test user is created
    // this will be updated in GetNewAwardsByUser_ShouldCreateTotalPostsAward_WhenSuccessful() when the award is created and will be deleted later
    private static string _postsAwardId = ""; 
    
    private ICognitoActions _cognitoActions;
    private IAwardActions _awardActions;
    private IFriendshipActions _friendshipActions;
    
    public AwardControllerIntegrationTests()
    {
        var webHostBuilder = new WebHostBuilder().UseStartup<Startup>();

        var server = new TestServer(webHostBuilder);

        _client = server.CreateClient();

        _appSettings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(@"appSettings.json"),
            new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
        
        awardTypes = JsonConvert.DeserializeObject<List<AwardType>>(File.ReadAllText(@"awards.json"));

        IAmazonCognitoIdentityProvider cognitoClient =
            new AmazonCognitoIdentityProviderClient(_appSettings.AwsRegionEndpoint);

        _cognitoActions = new CognitoActions(cognitoClient, _appSettings);
        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = _appSettings.AwsRegionEndpoint
        };
        var client = new AmazonDynamoDBClient(config);
        IDynamoDBContext dynamoDbContext = new DynamoDBContext(client);

        _awardActions = new AwardActions(_appSettings, dynamoDbContext);
        _friendshipActions = new FriendshipActions(_appSettings, dynamoDbContext);
    }
    
    #region GetAwardById Tests

    [Fact, Order(1)]
    public async Task GetAwardById_ShouldReturnAward_WhenSuccessful()
    {
        //setup the user for testing
        await _cognitoActions.CreateUser(TestUserEmail1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail1}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);
        _testUserId = user.Id;
        
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = _testUserId,
            CreatedAt = DateTime.Now,
            Name = "GetAwardById_ShouldReturnAward_WhenSuccessful()"
        };

        var createAward = await _awardActions.CreateAward(request);
        var actionResult = Assert.IsType<ActionResult<Award>>(createAward);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedAward = Assert.IsType<Award>(okResult.Value);
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardById?aid={returnedAward.AID}");
        var responseString = await response.Content.ReadAsStringAsync();
        var award = JsonConvert.DeserializeObject<Award>(responseString);
        
        // Assert
        Assert.NotNull(award);
        Assert.Equal(award.PID, request.PID);
        Assert.Equal(award.UID, request.UID);
        Assert.Equal(award.AID, request.AID);
        Assert.Equal(award.Name, request.Name);

        // Clean up
        await _awardActions.DeleteAward(returnedAward.AID);
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }
    
    [Fact, Order(2)]
    public async Task GetAwardById_ShouldReturnNotFound_WhenAwardNotFound()
    {
        // Act
        var aid = "1";
        var response = await _client.GetAsync($"/api/awards/getAwardById?aid={aid}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact, Order(3)]
    public async Task GetAwardById_ShouldReturnBadRequest_WithInvalidAID()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardById?aid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion
    
    #region GetAwardsByUser Tests

    [Fact, Order(4)]
    public async Task GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = _testUserId,
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()"
        };

        await _awardActions.CreateAward(request);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByUser?uid={_testUserId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var award = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(award);
        Assert.Equal(1, award.Count);
        Assert.Equal(award.First().PID, request.PID);
        Assert.Equal(award.First().UID, request.UID);
        Assert.Equal(award.First().AID, request.AID);
        Assert.Equal(award.First().Name, request.Name);

        // Clean up
        await _awardActions.DeleteAward(request.AID);
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }

    [Fact, Order(5)]
    public async Task GetAwardsByUser_ShouldReturnBadRequest_WithInvalidUID()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByUser?uid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion

    #region GetAwardsByPost Tests

    [Fact, Order(6)]
    public async Task GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = _testUserId,
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByPost_ShouldReturnAwards_WhenSuccessful()"
        };

        await _awardActions.CreateAward(request);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByPost?pid={request.PID}");
        var responseString = await response.Content.ReadAsStringAsync();
        var award = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(award);
        Assert.Equal(1, award.Count);
        Assert.Equal(award.First().PID, request.PID);
        Assert.Equal(award.First().UID, request.UID);
        Assert.Equal(award.First().AID, request.AID);
        Assert.Equal(award.First().Name, request.Name);

        // Clean up
        await _awardActions.DeleteAward(request.AID);
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }
    
    [Fact, Order(7)]
    public async Task GetAwardsByPosts_ShouldReturnBadRequest_WithInvalidPID()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getAwardsByPost?pid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion
    
    #region GetNewAwardsByUser Tests
    
    [Fact, Order(8)]
    public async Task GetNewAwardsByUser_ShouldCreateTotalPostsAward_WhenSuccessful()
    {
        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetNewAwardsByUser_ShouldCreateTotalPostsAward_WhenSuccessful()",
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
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={_testUserId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var awards = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(awards);
        Assert.Equal(1, awards.Count);
        Assert.Equal(1, awards.First().Tier);
        Assert.Equal("posts", awards.First().Type);
        Assert.NotNull(awards.First().Name);
        Assert.NotNull(awards.First().PID);
        Assert.NotNull(awards.First().AID);
        Assert.NotNull(awards.First().UID);
        
        // Clean up
        await _client.DeleteAsync($"/api/posts/deletePost?pid={newPost.PID}");
        _postsAwardId = awards.First().AID; // will be deleted later
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }

    [Fact, Order(9)]
    public async Task GetNewAwardsByUser_ShouldCreateUpvoteAward_WhenSuccessful()
    {
        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetNewAwardsByUser_ShouldCreateUpvoteAward_WhenSuccessful()",
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
        
        // Update post upvotes
        newPost.Upvotes = awardTypes.Where(a => a.Type.Equals("upvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum;
        var content2 = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");
        await _client.PutAsync($"/api/posts/updatePost", content2);
        await Task.Delay(TimeSpan.FromSeconds(10)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={_testUserId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var awards = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(awards);
        Assert.Equal(1, awards.Count);
        Assert.Equal(1, awards.First().Tier);
        Assert.Equal("upvote", awards.First().Type);
        Assert.NotNull(awards.First().Name);
        Assert.NotNull(awards.First().PID);
        Assert.NotNull(awards.First().AID);
        Assert.NotNull(awards.First().UID);
        
        // Clean up
        await _client.DeleteAsync($"/api/posts/deletePost?pid={newPost.PID}");
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }
    
    [Fact, Order(10)]
    public async Task GetNewAwardsByUser_ShouldCreateDownvoteAward_WhenSuccessful()
    {
        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetNewAwardsByUser_ShouldCreateDownvoteAward_WhenSuccessful()",
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
        
        // Update post upvotes
        newPost.Downvotes = awardTypes.Where(a => a.Type.Equals("downvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum;
        var content2 = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");
        await _client.PutAsync($"/api/posts/updatePost", content2);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={_testUserId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var awards = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(awards);
        Assert.Equal(1, awards.Count);
        Assert.Equal(1, awards.First().Tier);
        Assert.Equal("downvote", awards.First().Type);
        Assert.NotNull(awards.First().Name);
        Assert.NotNull(awards.First().PID);
        Assert.NotNull(awards.First().AID);
        Assert.NotNull(awards.First().UID);
        
        // Clean up
        await _client.DeleteAsync($"/api/posts/deletePost?pid={newPost.PID}");
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }

    [Fact, Order(11)]
    public async Task GetNewAwardsByUser_ShouldCreateCommentAward_WhenSuccessful()
    {
        // Arrange
        var request = new NewPost
        {
            UID = _testUserId,
            PostTitle = "GetNewAwardsByUser_ShouldCreateCommentAward_WhenSuccessful()",
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

        var minComments = awardTypes.Where(a => a.Type.Equals("comment")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum;
        
        var newComment = new NewComment
        {
            PID = newPost.PID,
            UID = _testUserId
        };

        for (var i = 0; i < minComments; i++)
        {
            newComment.CommentBody = "GetNewAwardsByUser_ShouldCreateCommentAward_WhenSuccessful(): " + i;
            var commentContent = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
                "application/json");

            await _client.PostAsync("/api/comments/createComment", commentContent);
        }
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed

        // Act
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={_testUserId}");
        var responseString = await response.Content.ReadAsStringAsync();
        var awards = JsonConvert.DeserializeObject<List<Award>>(responseString);
        
        // Assert
        Assert.NotNull(awards);
        Assert.Equal(1, awards.Count);
        Assert.Equal(1, awards.First().Tier);
        Assert.Equal("comment", awards.First().Type);
        Assert.NotNull(awards.First().Name);
        Assert.NotNull(awards.First().PID);
        Assert.NotNull(awards.First().AID);
        Assert.NotNull(awards.First().UID);
        
        // Clean up
        await _client.DeleteAsync($"/api/posts/deletePost?pid={newPost.PID}");
        // Test user is deleted in GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    }
    
    [Fact, Order(12)]
    public async Task GetNewAwardsByUser_ShouldCreateFriendsAward_WhenSuccessful()
    {
        // Arrange
        var friendRequest = new FriendRequest
        {
            FromUserName = TestUserEmail2,
            ToUserId = _testUserId
        };

        var content = new StringContent(JsonConvert.SerializeObject(friendRequest), System.Text.Encoding.UTF8,
            "application/json");
        var response1 = await _client.PostAsync("/api/friends/friendRequest", content);
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newFriendship = JsonConvert.DeserializeObject<Friendship>(responseString1);

        newFriendship.Status = FriendshipStatus.Accepted;
        content = new StringContent(JsonConvert.SerializeObject(newFriendship), System.Text.Encoding.UTF8, "application/json");
        await _client.PutAsync("/api/friends/updateFriendRequest", content);
        await Task.Delay(TimeSpan.FromSeconds(2)); // Adjust the delay duration as needed
        
        // Act
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={_testUserId}");
        var responseString2 = await response.Content.ReadAsStringAsync();
        var awards = JsonConvert.DeserializeObject<List<Award>>(responseString2);
        
        // Assert
        Assert.NotNull(awards);
        Assert.Equal(1, awards.Count);
        Assert.Equal(1, awards.First().Tier);
        Assert.Equal("friends", awards.First().Type);
        Assert.NotNull(awards.First().Name);
        Assert.NotNull(awards.First().PID);
        Assert.NotNull(awards.First().AID);
        Assert.NotNull(awards.First().UID);
        
        // Clean up
        await _awardActions.DeleteAward(awards.First().AID);
        await _friendshipActions.DeleteFriendship(TestUserEmail2, TestUserEmail1);
    }
    
    [Fact, Order(13)]
    public async Task GetNewAwardsByUser_ShouldReturnBadRequest_WithInvalidUserId()
    {
        // Act
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(14)]
    public async Task GetNewAwardsByUser_ShouldReturnNotFound_WithUserDoesNotExist()
    {
        // Act
        var uid = "1";
        var response = await _client.GetAsync($"/api/awards/getNewAwardsByUser?uid={uid}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        // clean up
        await _cognitoActions.DeleteUser(TestUserEmail1);
        await _awardActions.DeleteAward(_postsAwardId);
    }

    #endregion
}