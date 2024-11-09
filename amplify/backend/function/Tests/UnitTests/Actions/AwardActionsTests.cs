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
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.Models;
using yAppLambda.DynamoDB;

namespace Tests.UnitTests.Actions;

public class AwardActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly IAwardActions _awardActionsMock;
    private const string AwardTableName = "Award-test";

    public AwardActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.PostTableName).Returns(AwardTableName);

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
    
    #region CheckForPostAwards Tests

    [Fact]
    public async Task CheckForPostAwards_ShouldCallCreateAward_WhenNewAwardIsAchieved()
    {
        // Arrange
        var now = DateTime.Now;
        var post = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "uid",
            PostTitle = "CheckForPostAwards_ShouldCallCreateAward_WhenNewAwardIsAchieved()",
            PostBody = "body",
            Upvotes = 100,
            Downvotes = 100,
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
        var result = await _awardActionsMock.CheckForPostAwards(postList);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Award>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }
    
    // todo add individual tests for: new comment award, new upvote award, new downvote award, no award, multiple of the same award, different awards together
    
    #endregion
}