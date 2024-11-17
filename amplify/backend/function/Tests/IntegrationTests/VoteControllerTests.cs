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
using System.IO.Pipes;

namespace Tests.IntegrationTests;

public class VoteControllerIntegrationTests
{
    private readonly HttpClient _client;

    private readonly IAppSettings _appSettings;

    // We must use simulator email to test the user without using email quota
    private const string TestUserEmail = "bounce7@simulator.amazonses.com";
    private static string _testUid = "";

    private ICognitoActions _cognitoActions;
    private IVoteActions _voteActions;
    private IPostActions _postActions;
    private ICommentActions _commentActions;

    public VoteControllerIntegrationTests()
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

        _voteActions = new VoteActions(_appSettings, dynamoDbContext);
        _postActions = new PostActions(_appSettings, dynamoDbContext);
        _commentActions = new CommentActions(_appSettings, dynamoDbContext);
    }

    #region AddVote Tests

    [Fact, Order(1)]
    public async Task AddVote_ValidRequest_ReturnsVote()
    {
        //Setup the user for testing in first test
        await _cognitoActions.CreateUser(TestUserEmail);
        await Task.Delay(TimeSpan.FromSeconds(5)); // make sure the user is created

        var responseId = await _client.GetAsync($"/api/users/getUserByName?username={TestUserEmail}");
        Assert.Equal(HttpStatusCode.OK, responseId.StatusCode);
        var responseIdString = await responseId.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(responseIdString);
        _testUid = user.Id;

        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "AddVote_ValidRequest_ReturnsVote()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange
        var vote = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(vote), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        // Assert
        var responseString1 = await response1.Content.ReadAsStringAsync();

        var returnedVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        Assert.NotNull(returnedVote);
        Assert.Equal(_testUid, returnedVote.UID);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);

        // Clean up
        await _voteActions.RemoveVote(vote.UID, vote.PID, true);
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(2)]
    public async Task AddVote_ValidRequest_ReturnsVote_ForDownvote()
    {
        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "AddVote_ValidRequest_ReturnsVote_ForDownvote()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange
        var vote = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(vote), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        // Assert
        var responseString1 = await response1.Content.ReadAsStringAsync();

        var returnedVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        Assert.NotNull(returnedVote);
        Assert.Equal(_testUid, returnedVote.UID);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);

        // Clean up
        await _voteActions.RemoveVote(vote.UID, vote.PID, true);
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(3)]
    public async Task AddVote_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "addVoteUserNotFound",
            IsPost = true,
            Type = true,
            UID = "hiiiiii"
        };

        var content = new StringContent(JsonConvert.SerializeObject(vote), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/votes/addVote", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
    
    #region GetVote Tests
    
    [Fact, Order(4)]
    public async Task GetVote_ShouldReturnVote_WhenSuccessful()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()
        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "GetVote_ShouldReturnVote_WhenSuccessful()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange votes now
        var vote = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(vote), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new vote to query
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/votes/getVote?uid={newVote.UID}&pid={newVote.PID}&type={newVote.Type}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var returnedVote = JsonConvert.DeserializeObject<Vote>(responseString2);

        // Assert
        Assert.NotNull(returnedVote);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);

        // Clean up
        await _voteActions.RemoveVote(vote.UID, vote.PID, vote.Type);
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(5)]
    public async Task GetVote_ShouldReturnNotFound_WhenVoteNotFound()
    {
        // Act
        var pid = "1";
        var response = await _client.GetAsync($"/api/votes/getVote?uid={_testUid}&pid={pid}&type={true}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Order(6)]
    public async Task GetVote_ShouldReturnBadRequest_WithNullPid()
    {
        // Act
        var response = await _client.GetAsync($"/api/votes/getVote?uid={_testUid}&pid={null}&type={true}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(7)]
    public async Task GetVote_ShouldReturnBadRequest_WithEmptyPid()
    {
        // Act
        var response = await _client.GetAsync($"/api/votes/getVote?uid={_testUid}&pid=&type={true}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(8)]
    public async Task GetVote_ShouldReturnBadRequest_WithNullUid()
    {
        // Act
        var pid = "1";
        var response = await _client.GetAsync($"/api/votes/getVote?uid={null}&pid={pid}&type={true}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(9)]
    public async Task GetVote_ShouldReturnBadRequest_WithEmptyUid()
    {
        // Act
        var pid = "1";
        var response = await _client.GetAsync($"/api/votes/getVote?uid=&pid={pid}&type={true}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    #endregion

    #region GetVotesByPid Tests

    [Fact, Order(10)]
    public async Task GetVotesByPid_ShouldReturnVotes_WhenSuccessful()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()
        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "GetVotesByPid_ShouldReturnVotes_WhenSuccessful()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange vote now
        var request = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8,
            "application/json");

        // Creates a new vote to query
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed
        var responseString1 = await response1.Content.ReadAsStringAsync();
        var newVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        // Act
        var response2 = await _client.GetAsync($"/api/votes/getVotesByPid?pid={request.PID}");
        var responseString2 = await response2.Content.ReadAsStringAsync();
        var voteList = JsonConvert.DeserializeObject<List<Vote>>(responseString2);

        // Assert
        Assert.Equal(1, voteList.Count);
        Assert.Equal(newVote.PID, voteList.First().PID);
        Assert.Equal(newVote.IsPost, voteList.First().IsPost);
        Assert.Equal(newVote.Type, voteList.First().Type);
        Assert.Equal(newVote.UID, voteList.First().UID);

        // Clean up
        await _voteActions.RemoveVote(newVote.UID, newVote.PID, newVote.Type);
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(11)]
    public async Task GetVotesByPid_ShouldReturnBadRequest_WithNullPid()
    {
        // Act
        var response = await _client.GetAsync($"/api/votes/getVotesByPid?pid={null}");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(12)]
    public async Task GetVotesByPid_ShouldReturnBadRequest_WithEmptyPid()
    {
        // Act
        var response = await _client.GetAsync($"/api/votes/getVotesByPid?pid=");
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region DeleteVotes Tests
    
    [Fact, Order(13)]
    public async Task DeleteVotes_ShouldReturnTrue_WhenVotesAreDeletedSuccessfully()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()

        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "DeleteVotes_ShouldReturnTrue_WhenVotesAreDeletedSuccessfully()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);
        
        // Arrange votes now
        var newVote1 = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(newVote1), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseVote1 = JsonConvert.DeserializeObject<Vote>(responseString1);
        
        var newVote2 = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };

        var content2 = new StringContent(JsonConvert.SerializeObject(newVote2), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response2 = await _client.PostAsync("/api/votes/addVote", content2);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString2 = await response2.Content.ReadAsStringAsync();
        var responseVote2 = JsonConvert.DeserializeObject<Vote>(responseString2);

        // Act (delete the post and hopefully the votes with it...)
        var postResponse = await _client.DeleteAsync($"/api/posts/deletePost?pid={post.PID}");
        var postResponseString = postResponse.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(postResponseString);

        // Assert
        Assert.True(result);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(14)]
    public async Task DeleteVotes_ShouldReturnTrue_WhenVotesAreDeletedSuccessfully_FromComment()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()

        // Setup the comment to delete
        var newComment = new NewComment
        {
            PID = "1",
            UID = _testUid,
            CommentBody = "DeleteVotes_ShouldReturnTrue_WhenVotesAreDeletedSuccessfully_FromComment()"
        };

        var content1 = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new comment for testing
        var response1 = await _client.PostAsync("/api/comments/createComment", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        Console.WriteLine(responseString1);
        var responseComment = JsonConvert.DeserializeObject<Comment>(responseString1);
        Console.WriteLine(responseComment);
        
        // Arrange votes now
        var newVote1 = new Vote
        {
            PID = responseComment.CID,
            IsPost = false,
            Type = true,
            UID = _testUid
        };
        
        var content2 = new StringContent(JsonConvert.SerializeObject(newVote1), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response2 = await _client.PostAsync("/api/votes/addVote", content2);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString2 = await response2.Content.ReadAsStringAsync();
        var responseVote1 = JsonConvert.DeserializeObject<Vote>(responseString2);
        
        var newVote2 = new Vote
        {
            PID = responseComment.CID,
            IsPost = false,
            Type = true,
            UID = _testUid
        };

        var content3 = new StringContent(JsonConvert.SerializeObject(newVote2), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response3 = await _client.PostAsync("/api/votes/addVote", content3);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString3 = await response3.Content.ReadAsStringAsync();
        var responseVote2 = JsonConvert.DeserializeObject<Vote>(responseString3);

        // Act (delete the comment and hopefully the votes with it...)
        var commentResponse = await _client.DeleteAsync($"/api/comments/deleteComment?cid={responseComment.CID}");
        var commentResponseString = commentResponse.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(commentResponseString);

        // Assert
        Assert.True(result);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }
    
    [Fact, Order(15)]
    public async Task DeleteVotes_ShouldReturnBadRequest_WhenPostIdIsNull()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/posts/deletePost?pid={null}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(16)]
    public async Task DeleteVotes_ShouldReturnFalse_WhenDeleteFails()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/posts/deletePost?pid={"!"}");
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region RemoveVote Tests
    
    [Fact, Order(17)]
    public async Task RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()
        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "RemoveVote_ShouldReturnTrue()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange vote now
        var newVote = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(newVote), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        // Act
        var response2 = await _client.DeleteAsync($"/api/votes/removeVote?uid={newVote.UID}&pid={newVote.PID}&type={newVote.Type}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.True(result);
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(18)]
    public async Task RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully_ForDownvote()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()
        // Setup post to create a vote under
        var newPost = new NewPost
        {
            UID = _testUid,
            PostTitle = "RemoveVote_ShouldReturnTrue_ForDownvote()",
            PostBody = "body",
            DiaryEntry = false,
            Anonymous = true
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newPost), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/posts/createPost", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var post = JsonConvert.DeserializeObject<Post>(responseString);

        // Arrange
        var newVote = new Vote
        {
            PID = post.PID,
            IsPost = true,
            Type = false,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(newVote), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        // Act
        var response2 = await _client.DeleteAsync($"/api/votes/removeVote?uid={newVote.UID}&pid={newVote.PID}&type={newVote.Type}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.True(result);
        await _postActions.DeletePost(post.PID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(19)]
    public async Task RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully_ForComment()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()
        // Setup comment to create a vote under
        var newComment = new NewComment
        {
            UID = _testUid,
            CommentBody = "RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully_ForComments()",
            PID = "1"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var comment = JsonConvert.DeserializeObject<Comment>(responseString);

        // Arrange vote now
        var newVote = new Vote
        {
            PID = comment.CID,
            IsPost = false,
            Type = true,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(newVote), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        // Act
        var response2 = await _client.DeleteAsync($"/api/votes/removeVote?uid={newVote.UID}&pid={newVote.PID}&type={newVote.Type}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.True(result);
        await _commentActions.DeleteComment(comment.CID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }

    [Fact, Order(20)]
    public async Task RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully_ForComment_Downvote()
    {
        // Uses the test user set up in AddVote_ValidRequest_ReturnsVote()
        // Setup comment to create a vote under
        var newComment = new NewComment
        {
            UID = _testUid,
            CommentBody = "RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully_ForComment_Downvote()",
            PID = "1"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(newComment), System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/comments/createComment", content);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString = await response.Content.ReadAsStringAsync();
        var comment = JsonConvert.DeserializeObject<Comment>(responseString);

        // Arrange
        var newVote = new Vote
        {
            PID = comment.CID,
            IsPost = false,
            Type = false,
            UID = _testUid
        };
        
        var content1 = new StringContent(JsonConvert.SerializeObject(newVote), System.Text.Encoding.UTF8,
            "application/json");

        // Create a new vote for testing
        var response1 = await _client.PostAsync("/api/votes/addVote", content1);
        await Task.Delay(TimeSpan.FromSeconds(5)); // Adjust the delay duration as needed

        var responseString1 = await response1.Content.ReadAsStringAsync();
        var responseVote = JsonConvert.DeserializeObject<Vote>(responseString1);

        // Act
        var response2 = await _client.DeleteAsync($"/api/votes/removeVote?uid={newVote.UID}&pid={newVote.PID}&type={newVote.Type}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.True(result);
        await _commentActions.DeleteComment(comment.CID);
        // Test user is deleted in RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    }
    
    [Fact, Order(21)]
    public async Task RemoveVote_ShouldReturnBadRequest_WhenPidIsNull()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/votes/removeVote?uid={_testUid}&pid={null}&type={true}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(22)]
    public async Task RemoveVote_ShouldReturnBadRequest_WhenUidIsNull()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/votes/removeVote?uid={null}&pid=1&type={true}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(23)]
    public async Task RemoveVote_ShouldReturnBadRequest_WhenPidIsEmpty()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/votes/removeVote?uid={_testUid}&pid=&type={true}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact, Order(24)]
    public async Task RemoveVote_ShouldReturnBadRequest_WhenUidIsEmpty()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/votes/removeVote?uid=&pid=1&type={true}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact, Order(25)]
    public async Task RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    {
        // Act
        var response2 = await _client.DeleteAsync($"/api/votes/removeVote?uid=1&pid=1&type={true}");
        var responseString2 = response2.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<bool>(responseString2);

        // Assert
        Assert.False(result);
        
        // Clean up
        await _cognitoActions.DeleteUser(TestUserEmail);
    }

    #endregion

}