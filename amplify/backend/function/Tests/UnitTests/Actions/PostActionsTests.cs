using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.Models;
using yAppLambda.DynamoDB;
using yAppLambda.Common;
using System.Net;
using yAppLambda.Enum;

namespace Tests.UnitTests.Actions;

public class PostActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly IPostActions _postActionsMock;
    private readonly Mock<IAmazonCognitoIdentityProvider> _cognitoClientMock;
    private readonly CognitoActions _cognitoActions;
    private const string PostTableName = "Post-test";

    public PostActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.PostTableName).Returns(PostTableName);

        // Initialize the dynamoDbContextMock
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        
        // Initialize the PostActions with the mocks
        _postActionsMock = new PostActions(_appSettingsMock.Object, _dynamoDbContextMock.Object);

        // Initialize the CognitoActions with the mocks
        _cognitoClientMock = new Mock<IAmazonCognitoIdentityProvider>();
        _cognitoActions = new CognitoActions(_cognitoClientMock.Object, _appSettingsMock.Object);
    }
    
    #region CreatePost Tests

    [Fact]
    public async Task CreatePost_ShouldReturnOK_WhenPostIsCreatedSuccessfully()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UID = "user1@example.com",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _appSettingsMock.Setup(a => a.PostTableName).Returns(PostTableName);

        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _postActionsMock.CreatePost(post);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedPost = Assert.IsType<Post>(okResult.Value);
        Assert.Equal(post.PID, returnedPost.PID);
        Assert.Equal(post.UID, returnedPost.UID);
        Assert.Equal(post.PostTitle, returnedPost.PostTitle);
        Assert.Equal(post.PostBody, returnedPost.PostBody);
        Assert.Equal(post.Upvotes, returnedPost.Upvotes);
        Assert.Equal(post.Downvotes, returnedPost.Downvotes);
        Assert.Equal(post.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(post.Anonymous, returnedPost.Anonymous);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var post = new Post
        {
            PID = "1",
            UID = "user1@example.com",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _appSettingsMock.Setup(a => a.PostTableName).Returns(PostTableName);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error saving to DynamoDB"));
            
        // Act
        var result = await _postActionsMock.CreatePost(post);

        // Assert
        // Assert that the result is ActionResult<Post>
        var actionResult = Assert.IsType<ActionResult<Post>>(result); 
        // Access the Result property to get the actual StatusCodeResult
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    #endregion

    #region DeletePost Tests

    [Fact]
    public async Task DeletePost_ShouldCallDeleteAsync()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _postActionsMock.DeletePost(request.PID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeletePost_ShouldHandleException_WhenPostDoesNotExist()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Post does not exist"));

        // Act
        var result = await _postActionsMock.DeletePost(request.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task DeletePost_ShouldHandleException_WhenDeletePostFails()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(request.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Sets up DeleteAsync to throw an exception           
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Could not delete post"));

        // Act
        var result = await _postActionsMock.DeletePost(request.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(It.IsAny<Post>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion

    #region UpdatePost Tests

    [Fact]
    public async Task UpdatePost_ShouldReturnOk_WhenPostIsUpdatedSuccessfully()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "1",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _dynamoDbContextMock.Setup(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _postActionsMock.UpdatePost(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPost = Assert.IsType<Post>(okResult.Value);

        Assert.Equal(request.PID, returnedPost.PID);
        Assert.Equal(request.UID, returnedPost.UID);
        Assert.Equal(request.PostTitle, returnedPost.PostTitle);
        Assert.Equal(request.PostBody, returnedPost.PostBody);
        Assert.Equal(request.Upvotes, returnedPost.Upvotes);
        Assert.Equal(request.Downvotes, returnedPost.Downvotes);
        Assert.Equal(request.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(request.Anonymous, returnedPost.Anonymous);

        _dynamoDbContextMock.Verify(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdatePost_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "1",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        _dynamoDbContextMock.Setup(d => d.SaveAsync(request, It.IsAny<DynamoDBOperationConfig>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Error updatiing post in database"));

        // Act
        var result = await _postActionsMock.UpdatePost(request);
        
        // Assert
        var actionResult = Assert.IsType<ActionResult<Post>>(result);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }
    
    #endregion

    #region GetPostById Tests

    [Fact]
    public async Task GetPostById_ShouldReturnPost_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        // Sets up LoadAsync to return the request post
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Act
        var result = await _postActionsMock.GetPostById(request.PID);

        // Assert
        var returnedPost = Assert.IsType<Post>(result);
        Assert.Equal(request.PID, returnedPost.PID);
        Assert.Equal(request.CreatedAt, returnedPost.CreatedAt);
        Assert.Equal(request.UpdatedAt, returnedPost.UpdatedAt);
        Assert.Equal(request.UID, returnedPost.UID);
        Assert.Equal(request.PostTitle, returnedPost.PostTitle);
        Assert.Equal(request.PostBody, returnedPost.PostBody);
        Assert.Equal(request.Upvotes, returnedPost.Upvotes);
        Assert.Equal(request.Downvotes, returnedPost.Downvotes);
        Assert.Equal(request.DiaryEntry, returnedPost.DiaryEntry);
        Assert.Equal(request.Anonymous, returnedPost.Anonymous);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPostById_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error loading post"));
            
        // Act
        var result = await _postActionsMock.GetPostById("111");
        
        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Post>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetPostsByUser Tests
    
    [Fact]
    public async Task GetPostsByUser_ShouldReturnPosts_WhenSuccessful()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "1",
            UID = "uid",
            CreatedAt = now,
            UpdatedAt = now,
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var list = new List<Post>();
        list.Add(post);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Post>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Post>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _postActionsMock.GetPostsByUser(post.UID, false);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(post.PID, result.First().PID);
        Assert.Equal(post.CreatedAt, result.First().CreatedAt);
        Assert.Equal(post.UpdatedAt, result.First().UpdatedAt);
        Assert.Equal(post.UID, result.First().UID);
        Assert.Equal(post.PostTitle, result.First().PostTitle);
        Assert.Equal(post.PostBody, result.First().PostBody);
        Assert.Equal(post.Upvotes, result.First().Upvotes);
        Assert.Equal(post.Downvotes, result.First().Downvotes);
        Assert.Equal(post.DiaryEntry, result.First().DiaryEntry);
        Assert.Equal(post.Anonymous, result.First().Anonymous);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Post>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task GetPostsByUser_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Post>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error querying posts"));

        // Act
        var result = await _postActionsMock.GetPostsByUser("uid", false);

        // Assert
        Assert.Empty(result);
    }
    
    #endregion

    #region GetRecentPosts Tests

    [Fact]
    public async Task GetRecentPosts_ShouldReturnPosts_WithAValidQuery()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "1",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "title",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var response = new Post { PID = "1" };

        // Sets up LoadAsync to return the request post (for in GetPostById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(post.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        var list = new List<Post>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Post>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);

        // Act
        var result = await _postActionsMock.GetRecentPosts(DateTime.Now, 1);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(post.PID, result.First().PID);
        Assert.Equal(post.UID, result.First().UID);
        Assert.Equal(post.PostTitle, result.First().PostTitle);
        Assert.Equal(post.PostBody, result.First().PostBody);
        Assert.Equal(post.Upvotes, result.First().Upvotes);
        Assert.Equal(post.Downvotes, result.First().Downvotes);
        Assert.Equal(post.DiaryEntry, result.First().DiaryEntry);
        Assert.Equal(post.Anonymous, result.First().Anonymous);

        _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetRecentPosts_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        // Sets up FromQueryAsync to fail
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Throws(new Exception("Could not load post"));
            
        // Act
        var result = await _postActionsMock.GetRecentPosts(DateTime.Now, 1);

        // Assert
        Assert.Empty(result);
    }
    
    #endregion

    // #region GetDiariesByUser Tests

    // [Fact]
    // public async Task GetDiariesByUser_ShouldReturnDiaries_WithAValidQuery()
    // {
    //     // Arrange
    //     var now = DateTime.Now;
    //     var post = new Post
    //     {
    //         PID = "1",
    //         UID = "user1",
    //         CreatedAt = now,
    //         UpdatedAt = now,
    //         PostTitle = "title",
    //         PostBody = "body",
    //         Upvotes = 0,
    //         Downvotes = 0,
    //         DiaryEntry = true,
    //         Anonymous = true
    //     };
    //     var response = new Post { PID = "1" };

    //     // Sets up LoadAsync to return the request post (for in GetPostById)
    //     _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(post.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(post);

    //     var list = new List<Post>();
    //     list.Add(response);

    //     // Mock the AsyncSearch<Post> returned by QueryAsync
    //     var queryFromSearchMock = new Mock<AsyncSearch<Post>>();
    //     queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(list);

    //     // Sets up FromQueryAsync to succeed
    //     _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
    //         .Returns(queryFromSearchMock.Object);

    //     // Act
    //     var result = await _postActionsMock.GetDiariesByUser(post.UID, now);

    //     // Assert
    //     Assert.Equal(1, result.Count);
    //     Assert.Equal(post.PID, result.First().PID);
    //     Assert.Equal(post.CreatedAt, result.First().CreatedAt);
    //     Assert.Equal(post.UpdatedAt, result.First().UpdatedAt);
    //     Assert.Equal(post.UID, result.First().UID);
    //     Assert.Equal(post.PostTitle, result.First().PostTitle);
    //     Assert.Equal(post.PostBody, result.First().PostBody);
    //     Assert.Equal(post.Upvotes, result.First().Upvotes);
    //     Assert.Equal(post.Downvotes, result.First().Downvotes);
    //     Assert.Equal(post.DiaryEntry, result.First().DiaryEntry);
    //     Assert.Equal(post.Anonymous, result.First().Anonymous);

    //     _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    // }

    // [Fact]
    // public async Task GetDiariesByUser_ShouldReturnEmptyList_WhenExceptionIsThrown()
    // {
    //     // Arrange
    //     var scanToSearchMock = new Mock<AsyncSearch<Post>>();
    //     scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
    //         .ThrowsAsync(new Exception("error querying diaries"));

    //     // Act
    //     var result = await _postActionsMock.GetDiariesByUser("uid", DateTime.Now);

    //     // Assert
    //     Assert.Empty(result);
    // }

    // #endregion

    // #region GetDiariesByFriends Tests

    // [Fact]
    // public async Task GetDiariesByFriends_ShouldReturnDiaries_WithAValidQuery()
    // {
        
    //     // Setup user
    //     var user = new User { UserName = "user1", Id = "12345", Name = "User One", NickName = "User1Nick" };

    //     var updateResponse = new AdminUpdateUserAttributesResponse
    //     {
    //         HttpStatusCode = HttpStatusCode.OK
    //     };

    //     var getUserResponse = new AdminGetUserResponse
    //     {
    //         Username = "user1",
    //         UserAttributes = new List<AttributeType>
    //         {
    //             new AttributeType { Name = "email", Value = "user1@example.com" },
    //             new AttributeType { Name = "name", Value = "User One" },
    //             new AttributeType { Name = "nickname", Value = "User1Nick" },
    //             new AttributeType { Name = "sub", Value = "12345" }
    //         }
    //     };

    //     var listUsersResponse = new ListUsersResponse
    //     {
    //         Users = new List<UserType>
    //         {
    //             new UserType
    //             {
    //                 Username = "user1",
    //                 Attributes = new List<AttributeType>
    //                 {
    //                     new AttributeType { Name = "email", Value = "user1@example.com" },
    //                     new AttributeType { Name = "name", Value = "User One" },
    //                     new AttributeType { Name = "nickname", Value = "User1Nick" },
    //                     new AttributeType { Name = "sub", Value = "12345" }
    //                 }
    //             }
    //         }
    //     };

    //     _cognitoClientMock.Setup(c =>
    //             c.AdminUpdateUserAttributesAsync(It.IsAny<AdminUpdateUserAttributesRequest>(),
    //                 It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(updateResponse);

    //     _cognitoClientMock.Setup(c =>
    //             c.AdminGetUserAsync(It.IsAny<AdminGetUserRequest>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(getUserResponse);
            
    //     _cognitoClientMock.Setup(c => c.ListUsersAsync(It.IsAny<ListUsersRequest>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(listUsersResponse);

    //     await _cognitoActions.UpdateUser(user);

    //     // Setup friendship
    //     const FriendshipStatus friendshipStatus = FriendshipStatus.Accepted;
    //     var friendshipsFrom = new List<Friendship>
    //     {
    //         new Friendship 
    //         { 
    //             FromUserName = user.UserName, 
    //             ToUserName = "user2@example.com", 
    //             Status = friendshipStatus 
    //         }
    //     };
    //     var friendshipsTo = new List<Friendship>
    //     {
    //         new Friendship 
    //         { 
    //             FromUserName = "user2@example.com", 
    //             ToUserName = user.UserName, 
    //             Status = friendshipStatus 
    //         }
    //     };

    //     _appSettingsMock.Setup(a => a.FriendshipTableName).Returns(string.Empty);
        
    //     // Mock the AsyncSearch<Friendship> returned by QueryAsync
    //     var queryFromSearchMock = new Mock<AsyncSearch<Friendship>>();
    //     queryFromSearchMock
    //     .Setup(q => q.GetRemainingAsync(It.IsAny<CancellationToken>()))
    //     .ReturnsAsync(friendshipsFrom);

    //     // Mock the AsyncSearch<Friendship> returned by ScanAsync
    //     var scanToSearchMock = new Mock<AsyncSearch<Friendship>>();
    //     scanToSearchMock
    //     .Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
    //     .ReturnsAsync(friendshipsTo);

    //     // Setup the IDynamoDBContext mock to return the mocked AsyncSearch objects
    //     _dynamoDbContextMock
    //     .Setup(d => d.QueryAsync<Friendship>(user.UserName, 
    //                                          It.IsAny<DynamoDBOperationConfig>()))
    //     .Returns(queryFromSearchMock.Object);

    //     _dynamoDbContextMock
    //     .Setup(d => d.ScanAsync<Friendship>(It.IsAny<List<ScanCondition>>(), 
    //                                         It.IsAny<DynamoDBOperationConfig>()))
    //     .Returns(scanToSearchMock.Object);

    //     // Now setup the post
    //     var now = DateTime.Now;
    //     var post = new Post
    //     {
    //         PID = "1",
    //         UID = "12345",
    //         CreatedAt = now,
    //         UpdatedAt = now,
    //         PostTitle = "title",
    //         PostBody = "body",
    //         Upvotes = 0,
    //         Downvotes = 0,
    //         DiaryEntry = true,
    //         Anonymous = true
    //     };
    //     var response = new Post { PID = "1" };

    //     // Sets up LoadAsync to return the request post (for in GetPostById)
    //     _dynamoDbContextMock.Setup(d => d.LoadAsync<Post>(post.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(post);

    //     var list = new List<Post>();
    //     list.Add(response);

    //     // Mock the AsyncSearch<Post> returned by QueryAsync
    //     var queryFromSearchMockPost = new Mock<AsyncSearch<Post>>();
    //     queryFromSearchMockPost.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(list);

    //     // Sets up FromQueryAsync to succeed
    //     _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
    //         .Returns(queryFromSearchMockPost.Object);

    //     // Act
    //     var result = await _postActionsMock.GetDiariesByFriends(_cognitoActions, post.UID, now);

    //     // Assert
    //     Assert.Equal(2, result.Count); // Needs to be 2 since there's "technically" two friends and both their posts return
    //     Assert.Equal(post.PID, result.First().PID);
    //     Assert.Equal(post.CreatedAt, result.First().CreatedAt);
    //     Assert.Equal(post.UpdatedAt, result.First().UpdatedAt);
    //     Assert.Equal(post.UID, result.First().UID);
    //     Assert.Equal(post.PostTitle, result.First().PostTitle);
    //     Assert.Equal(post.PostBody, result.First().PostBody);
    //     Assert.Equal(post.Upvotes, result.First().Upvotes);
    //     Assert.Equal(post.Downvotes, result.First().Downvotes);
    //     Assert.Equal(post.DiaryEntry, result.First().DiaryEntry);
    //     Assert.Equal(post.Anonymous, result.First().Anonymous);
        
    //     _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.AtLeastOnce);
    // }

    // [Fact]
    // public async Task GetDiariesByFriends_ShouldReturnEmptyList_WhenExceptionIsThrown()
    // {
    //     // Arrange
    //     // Sets up FromQueryAsync to fail
    //     _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Post>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
    //         .Throws(new Exception("Could not load diary entries"));
            
    //     // Act
    //     var result = await _postActionsMock.GetDiariesByFriends(_cognitoActions, "uid", DateTime.Now);

    //     // Assert
    //     Assert.Empty(result);
    // }

    // #endregion
}
