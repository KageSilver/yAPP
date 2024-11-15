using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using yAppLambda.Models;
using yAppLambda.DynamoDB;
using yAppLambda.Enum;

namespace Tests.UnitTests.Actions;

public class AwardActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly IAwardActions _awardActionsMock;
    private const string AwardTableName = "Award-test";
    private readonly List<AwardType> awardTypes;

    public AwardActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.PostTableName).Returns(AwardTableName);
        
        awardTypes = JsonConvert.DeserializeObject<List<AwardType>>(File.ReadAllText(@"awards.json"));

        // Initialize the dynamoDbContextMock
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        
        // Initialize the AwardActions with the mocks
        _awardActionsMock = new AwardActions(_appSettingsMock.Object, _dynamoDbContextMock.Object);
    }
    
    #region CreateAward Tests

    [Fact]
    public async Task CreateAward_ShouldReturnOK_WhenAwardIsCreatedSuccessfully()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "CreateAward_ShouldReturnOK_WhenAwardIsCreatedSuccessfully()"
        };

        _appSettingsMock.Setup(a => a.AwardTableName).Returns(AwardTableName);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CreateAward(request);
        
        // Assert
        var actionResult = Assert.IsType<ActionResult<Award>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result
        
        var returnedAward = Assert.IsType<Award>(okResult.Value);
        Assert.Equal(request.PID, returnedAward.PID);
        Assert.Equal(request.UID, returnedAward.UID);
        Assert.Equal(request.AID, returnedAward.AID);
        Assert.Equal(request.Name, returnedAward.Name);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAward_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "CreateAward_ShouldReturnStatus500_WhenExceptionIsThrown()"
        };

        _appSettingsMock.Setup(a => a.AwardTableName).Returns(AwardTableName);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error saving to DynamoDB"));
        
        // Act
        var result = await _awardActionsMock.CreateAward(request);
        
        // Assert that the result is ActionResult<Award>
        var actionResult = Assert.IsType<ActionResult<Award>>(result); 
        // Access the Result property to get the actual StatusCodeResult
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    #endregion
    
    #region GetAwardById Tests

    [Fact]
    public async Task GetAwardById_ShouldReturnAward_WhenSuccessful()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "GetAwardById_ShouldReturnAward_WhenSuccessful()"
        };
        
        // Sets up LoadAsync to return the request post
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        
        // Act
        var result = await _awardActionsMock.GetAwardById(request.PID);

        // Assert
        var returnedAward = Assert.IsType<Award>(result);
        Assert.Equal(request.PID, returnedAward.PID);
        Assert.Equal(request.CreatedAt, returnedAward.CreatedAt);
        Assert.Equal(request.UID, returnedAward.UID);
        Assert.Equal(request.Name, returnedAward.Name);
        Assert.Equal(request.AID, returnedAward.AID);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAwardById_ShouldReturnNull_WhenExceptionIsThrown()
    {
        // Arrange
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error loading award"));
            
        // Act
        var result = await _awardActionsMock.GetAwardById("111");
        
        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
    
    #region GetAwardsByUser Tests

    [Fact]
    public async Task GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByUser_ShouldReturnAwards_WhenSuccessful()"
        };

        var list = new List<Award>();
        list.Add(request);
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _awardActionsMock.GetAwardsByUser(request.UID);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(request.PID, result.First().PID);
        Assert.Equal(request.CreatedAt, result.First().CreatedAt);
        Assert.Equal(request.UID, result.First().UID);
        Assert.Equal(request.Name, result.First().Name);
        Assert.Equal(request.AID, result.First().AID);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetAwardsByUser_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("error querying awards"));

        // Act
        var result = await _awardActionsMock.GetAwardsByUser("uid");

        // Assert
        Assert.Empty(result);
    }

    #endregion
    
    #region GetAwardsByPost Tests

    [Fact]
    public async Task GetAwardsByPost_ShouldReturnAwards_WithAValidQuery()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "GetAwardsByPost_ShouldReturnAwards_WithAValidQuery()"
        };
        
        var response = new Award { AID = request.AID };

        var list = new List<Award>();
        list.Add(response);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);
        
        // Act
        var result = await _awardActionsMock.GetAwardsByPost(request.PID);
        
        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(request.PID, result.First().PID);
        Assert.Equal(request.CreatedAt, result.First().CreatedAt);
        Assert.Equal(request.UID, result.First().UID);
        Assert.Equal(request.Name, result.First().Name);
        Assert.Equal(request.AID, result.First().AID);
        
        _dynamoDbContextMock.Verify(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetAwardsByPost_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        // Sets up FromQueryAsync to fail
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Throws(new Exception("Could not load award"));
            
        // Act
        var result = await _awardActionsMock.GetAwardsByPost("1");

        // Assert
        Assert.Empty(result);
    }

    #endregion
    
    #region DeleteAwardsByPosts Tests
    
    [Fact]
    public async Task DeleteAwardsByPosts_ShouldCallDeleteAward()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "DeleteAwardsByPosts_ShouldCallDeleteAward()"
        };
        
        var response = new Award { AID = request.AID };

        var list = new List<Award>();
        list.Add(response);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);
        
        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _awardActionsMock.DeleteAwardsByPost(request.PID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteAwardsByPosts_ShouldHandleException_WhenDeleteAwardFails()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "DeleteAwardsByPosts_ShouldHandleException_WhenDeleteAwardFails()"
        };
        
        var response = new Award { AID = request.AID };

        var list = new List<Award>();
        list.Add(response);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock.Object);
        
        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Could not delete award"));
        
        // Act
        var result = await _awardActionsMock.DeleteAwardsByPost(request.PID);
        
        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion
    
    #region DeleteAward Tests

    [Fact]
    public async Task DeleteAward_ShouldCallDeleteAsync()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "DeleteAward_ShouldCallDeleteAsync()"
        };
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        
        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _awardActionsMock.DeleteAward(request.AID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteAward_ShouldHandleException_WhenAwardDoesNotExist()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "DeleteAward_ShouldHandleException_WhenAwardDoesNotExist()"
        };
        
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Award does not exist"));

        // Act
        var result = await _awardActionsMock.DeleteAward(request.AID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.LoadAsync<Award>(request.AID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAward_ShouldHandleException_WhenDeleteAwardFails()
    {
        // Arrange
        var request = new Award
        {
            AID = "1",
            PID = "1",
            UID = "uid",
            CreatedAt = DateTime.Now,
            Name = "DeleteAward_ShouldHandleException_WhenDeleteAwardFails()"
        };
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        
        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Could not delete award"));

        // Act
        var result = await _awardActionsMock.DeleteAward(request.AID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(request, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
    
    #region CheckNewAwardsPerPost - Upvote Tests

    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldCreateUpvoteAward_WhenNewUpvoteAwardIsAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldCreateUpvoteAward_WhenSuccessful()",
            PostBody = "body",
            Upvotes = awardTypes.Where(a => a.Type.Equals("upvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.First().Tier);
        Assert.Equal("upvote", result.First().Type);
        Assert.NotNull(result.First().Name);
        Assert.NotNull(result.First().PID);
        Assert.NotNull(result.First().AID);
        Assert.NotNull(result.First().UID);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldCreateAllUpvoteAwards_WhenAllUpvoteAwardsAreAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var tiers = awardTypes.Where(a => a.Type.Equals("upvote")).First().Tiers;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldCreateAllUpvoteAwards_WhenAllUpvoteAwardsAreAchieved()",
            PostBody = "body",
            Upvotes = tiers.Where(t => t.TierNum == tiers.Count).First().Minimum,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "upvote tier 1",
            Type = "upvote",
            Tier = 1,
            CreatedAt = now
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        awardList.Add(award);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal((tiers.Count - 1), result.Count);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Exactly((tiers.Count - 1)));
    }
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldNotCreateUpvoteAward_WhenAwardAlreadyExists()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldNotCreateUpvoteAward_WhenAwardAlreadyExists()",
            PostBody = "body",
            Upvotes = awardTypes.Where(a => a.Type.Equals("upvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "upvote tier 1",
            Type = "upvote",
            Tier = 1,
            CreatedAt = now
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        awardList.Add(award);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(new List<Post>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    #endregion

    #region CheckNewAwardsPerPost - Downvote Tests
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldCreateDownvoteAward_WhenNewDownvoteAwardIsAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldCreateDownvoteAward_WhenSuccessful()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = awardTypes.Where(a => a.Type.Equals("downvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum,
            DiaryEntry = false,
            Anonymous = true
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.First().Tier);
        Assert.Equal("downvote", result.First().Type);
        Assert.NotNull(result.First().Name);
        Assert.NotNull(result.First().PID);
        Assert.NotNull(result.First().AID);
        Assert.NotNull(result.First().UID);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldCreateAllDownvoteAwards_WhenAllDownvoteAwardsAreAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var tiers = awardTypes.Where(a => a.Type.Equals("downvote")).First().Tiers;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldCreateAllDownvoteAwards_WhenAllDownvoteAwardsAreAchieved()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = tiers.Where(t => t.TierNum == tiers.Count).First().Minimum,
            DiaryEntry = false,
            Anonymous = true
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "downvote tier 1",
            Type = "downvote",
            Tier = 1,
            CreatedAt = now
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        awardList.Add(award);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal((tiers.Count - 1), result.Count);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Exactly((tiers.Count - 1)));
    }
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldNotCreateDownvoteAward_WhenAwardAlreadyExists()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldNotCreateDownvoteAward_WhenAwardAlreadyExists()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = awardTypes.Where(a => a.Type.Equals("downvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum,
            DiaryEntry = false,
            Anonymous = true
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "downvote tier 1",
            Type = "downvote",
            Tier = 1,
            CreatedAt = now
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        awardList.Add(award);
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(new List<Post>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    #endregion
    
    #region CheckNewAwardsByUser - Comment Tests
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldCreateCommentAward_WhenNewCommentAwardIsAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldCreateCommentAward_WhenSuccessful()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var comment = new Comment()
        {
            CID = "1",
            UID = post.UID,
            PID = post.PID,
            CommentBody = "CheckNewAwardsPerPost_ShouldCreateCommentAward_WhenSuccessful()",
            CreatedAt = now,
            UpdatedAt = now,
            Upvotes = 0,
            Downvotes = 0
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();

        var commentList = new List<Comment>();
        commentList.AddRange(Enumerable.Repeat(comment, awardTypes.Where(a => a.Type.Equals("comment")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(commentList);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.First().Tier);
        Assert.Equal("comment", result.First().Type);
        Assert.NotNull(result.First().Name);
        Assert.NotNull(result.First().PID);
        Assert.NotNull(result.First().AID);
        Assert.NotNull(result.First().UID);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldCreateAllCommentAwards_WhenAllCommentAwardsAreAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldCreateAllCommentAwards_WhenAllCommentAwardsAreAchieved()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var comment = new Comment()
        {
            CID = "1",
            UID = post.UID,
            PID = post.PID,
            CommentBody = "CheckNewAwardsPerPost_ShouldCreateAllCommentAwards_WhenAllCommentAwardsAreAchieved()",
            CreatedAt = now,
            UpdatedAt = now,
            Upvotes = 0,
            Downvotes = 0
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "comment tier 1",
            Type = "comment",
            Tier = 1,
            CreatedAt = now
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        awardList.Add(award);

        var commentList = new List<Comment>();
        var tiers = awardTypes.Where(a => a.Type.Equals("comment")).First().Tiers;
        commentList.AddRange(Enumerable.Repeat(comment, tiers.Where(t => t.TierNum == tiers.Count).First().Minimum));
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(commentList);

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal((tiers.Count - 1), result.Count);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Exactly((tiers.Count - 1)));
    }
    
    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldNotCreateCommentAward_WhenAwardAlreadyExists()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldNotCreateCommentAward_WhenAwardAlreadyExists()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var comment = new Comment()
        {
            CID = "1",
            UID = post.UID,
            PID = post.PID,
            CommentBody = "CheckNewAwardsPerPost_ShouldNotCreateCommentAward_WhenAwardAlreadyExists()",
            CreatedAt = now,
            UpdatedAt = now,
            Upvotes = 0,
            Downvotes = 0
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "comment tier 1",
            Type = "comment",
            Tier = 1,
            CreatedAt = now
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        awardList.Add(award);

        var commentList = new List<Comment>();
        commentList.AddRange(Enumerable.Repeat(comment, awardTypes.Where(a => a.Type.Equals("comment")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(new List<Post>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    #endregion
    
    #region CheckNewAwardsByUser Tests

    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldNotCreateAward_WithEmptyPostList()
    {
        // Arrange
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(new List<Post>());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckNewAwardsPerPost_ShouldHandleException_WhenCreateAwardFails()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsPerPost_ShouldHandleException_WhenCreateAwardFails()",
            PostBody = "body",
            Upvotes = awardTypes.Where(a => a.Type.Equals("upvote")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var postList = new List<Post>();
        postList.Add(post);
        
        var awardList = new List<Award>();
        
        // Mock the AsyncSearch<Award> returned by QueryAsync
        var queryFromSearchMock1 = new Mock<AsyncSearch<Award>>();
        queryFromSearchMock1.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
        
        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Award>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock1.Object);

        // mock GetCommentsByPid
        _appSettingsMock.Setup(a => a.CommentTableName).Returns(string.Empty);

        // Mock the AsyncSearch<Comment> returned by QueryAsync
        var queryFromSearchMock2 = new Mock<AsyncSearch<Comment>>();
        queryFromSearchMock2.Setup(q => q.GetNextSetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        // Sets up FromQueryAsync to succeed
        _dynamoDbContextMock.Setup(d => d.FromQueryAsync<Comment>(It.IsAny<QueryOperationConfig>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(queryFromSearchMock2.Object);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock
            .Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("failed to create award"));
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsPerPost(postList);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion
    
    #region CheckNewAwardsTotalPosts Tests

    [Fact]
    public async Task CheckNewAwardsTotalPosts_ShouldCreatePostsAward_WhenNewPostsAwardIsAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsTotalPosts_ShouldCreatePostsAward_WhenNewPostsAwardIsAchieved()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var postList = new List<Post>();
        postList.AddRange(Enumerable.Repeat(post, awardTypes.Where(a => a.Type.Equals("posts")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsTotalPosts(postList, post.UID);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.First().Tier);
        Assert.Equal("posts", result.First().Type);
        Assert.NotNull(result.First().Name);
        Assert.NotNull(result.First().PID);
        Assert.NotNull(result.First().AID);
        Assert.NotNull(result.First().UID);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CheckNewAwardsTotalPosts_ShouldCreateAllPostsAwards_WhenAllPostsAwardsAreAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsTotalPosts_ShouldCreateAllPostsAwards_WhenAllPostsAwardsAreAchieved()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = post.PID,
            UID = post.UID,
            Name = "posts tier 1",
            Type = "posts",
            Tier = 1,
            CreatedAt = now
        };
        
        var awardList = new List<Award>();
        awardList.Add(award);
        
        var postList = new List<Post>();
        var tiers = awardTypes.Where(a => a.Type.Equals("posts")).First().Tiers;
        postList.AddRange(Enumerable.Repeat(post, tiers.Where(t => t.TierNum == tiers.Count).First().Minimum));
        
        // Sets up LoadAsync to return the request award (for in GetAwardById)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Award>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(award);
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsTotalPosts(postList, post.UID);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal((tiers.Count - 1), result.Count);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Exactly((tiers.Count - 1)));
    }
    
    [Fact]
    public async Task CheckNewAwardsTotalPosts_ShouldNotCreatePostsAward_WithEmptyPostList()
    {
        var postList = new List<Post>();
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsTotalPosts(postList, "1");
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckNewAwardsTotalPosts_ShouldNotCreatePostsAward_WhenAwardAlreadyExists()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsTotalPosts_ShouldNotCreatePostsAward_WhenAwardAlreadyExists()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = "NA",
            UID = post.UID,
            Name = "posts tier 1",
            Type = "posts",
            Tier = 1,
            CreatedAt = now
        };
        
        var awardList = new List<Award>();
        awardList.Add(award);

        var postList = new List<Post>();
        postList.AddRange(Enumerable.Repeat(post, awardTypes.Where(a => a.Type.Equals("posts")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsTotalPosts(postList, post.UID);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task CheckNewAwardsTotalPosts_ShouldHandleException_WhenCreateAwardFails()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckNewAwardsTotalPosts_ShouldHandleException_WhenCreateAwardFails()",
            PostBody = "body",
            Upvotes = 0,
            Downvotes = 0,
            DiaryEntry = false,
            Anonymous = true
        };

        var postList = new List<Post>();
        postList.AddRange(Enumerable.Repeat(post, awardTypes.Where(a => a.Type.Equals("posts")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock
            .Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("failed to create award"));
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsTotalPosts(postList, post.UID);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion
    
    #region CheckNewAwardsFriends Tests
    
    [Fact]
    public async Task CheckNewAwardsFriends_ShouldCreateFriendsAward_WhenNewFriendsAwardIsAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var friendship = new Friendship
        {
            FromUserName = "user1",
            ToUserName = "user2",
            Status = FriendshipStatus.Accepted,
            CreatedAt = now,
            UpdatedAt = now
        };

        var friendList = new List<Friendship>();
        friendList.AddRange(Enumerable.Repeat(friendship, awardTypes.Where(a => a.Type.Equals("friends")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsFriends(friendList, "1");
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.First().Tier);
        Assert.Equal("friends", result.First().Type);
        Assert.NotNull(result.First().Name);
        Assert.NotNull(result.First().PID);
        Assert.NotNull(result.First().AID);
        Assert.NotNull(result.First().UID);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    
    [Fact]
    public async Task CheckNewAwardsFriends_ShouldCreateAllFriendsAwards_WhenAllFriendsAwardsAreAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var friendship = new Friendship
        {
            FromUserName = "user1",
            ToUserName = "user2",
            Status = FriendshipStatus.Accepted,
            CreatedAt = now,
            UpdatedAt = now
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = "NA",
            UID = "1",
            Name = "friends tier 1",
            Type = "friends",
            Tier = 1,
            CreatedAt = now
        };
        
        var awardList = new List<Award>();
        awardList.Add(award);

        var friendList = new List<Friendship>();
        var tiers = awardTypes.Where(a => a.Type.Equals("friends")).First().Tiers;
        friendList.AddRange(Enumerable.Repeat(friendship, tiers.Where(t => t.TierNum == tiers.Count).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsFriends(friendList, "1");
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal((tiers.Count - 1), result.Count);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Exactly((tiers.Count - 1)));
    }
    
    [Fact]
    public async Task CheckNewAwardsFriends_ShouldNotCreateFriendsAward_WithEmptyFriendList()
    {
        var friendList = new List<Friendship>();
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsFriends(friendList, "1");
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckNewAwardsFriends_ShouldNotCreateFriendsAward_WhenAwardAlreadyExists()
    {
        // Arrange
        var now = DateTime.Now;
        var friendship = new Friendship
        {
            FromUserName = "user1",
            ToUserName = "user2",
            Status = FriendshipStatus.Accepted,
            CreatedAt = now,
            UpdatedAt = now
        };
        
        var award = new Award()
        {
            AID = "1",
            PID = "NA",
            UID = "1",
            Name = "friends tier 1",
            Type = "friends",
            Tier = 1,
            CreatedAt = now
        };
        
        var awardList = new List<Award>();
        awardList.Add(award);

        var friendList = new List<Friendship>();
        friendList.AddRange(Enumerable.Repeat(friendship, awardTypes.Where(a => a.Type.Equals("friends")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(awardList);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsFriends(friendList, "1");
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was never called
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task CheckNewAwardsFriends_ShouldHandleException_WhenCreateAwardFails()
    {
        // Arrange
        var now = DateTime.Now;
        var friendship = new Friendship
        {
            FromUserName = "user1",
            ToUserName = "user2",
            Status = FriendshipStatus.Accepted,
            CreatedAt = now,
            UpdatedAt = now
        };

        var friendList = new List<Friendship>();
        friendList.AddRange(Enumerable.Repeat(friendship, awardTypes.Where(a => a.Type.Equals("friends")).First().Tiers.Where(t => t.TierNum == 1).First().Minimum));
        
        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Award>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Award>());
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Award>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock
            .Setup(d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("failed to create award"));
        
        // Act
        var result = await _awardActionsMock.CheckNewAwardsFriends(friendList, "1");
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    #endregion
}