using System.Linq;
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
using Amazon.DynamoDBv2.DocumentModel;
using System.Diagnostics.CodeAnalysis;

namespace Tests.UnitTests.Actions;

public class VoteActionsTests
{
    private readonly Mock<IAppSettings> _appSettingsMock;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly IVoteActions _voteActionsMock;
    private const string VoteTableName = "Vote-test";

    public VoteActionsTests()
    {
        _appSettingsMock = new Mock<IAppSettings>();
        _appSettingsMock.Setup(a => a.UserPoolId).Returns("test_pool_id");
        _appSettingsMock.Setup(a => a.AwsRegionEndpoint).Returns(Amazon.RegionEndpoint.USEast2);
        _appSettingsMock.Setup(a => a.VoteTableName).Returns(VoteTableName);

        // Initialize the dynamoDbContextMock
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        
        // Initialize the VoteActions with the mocks
        _voteActionsMock = new VoteActions(_appSettingsMock.Object, _dynamoDbContextMock.Object);
    }
    
    #region AddVote Tests

    [Fact]
    public async Task AddVote_ShouldReturnOK_WhenVoteIsCreatedSuccessfully()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "addVoteShouldReturnOK",
            IsPost = true,
            Type = true,
            UID = "c1cb"
        };

        _appSettingsMock.Setup(a => a.VoteTableName).Returns(VoteTableName);

        var now = DateTime.Now;
        var request = new Post
        {
            PID = "addVoteShouldReturnOK",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "c1cb",
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

        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _voteActionsMock.AddVote(vote);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Vote>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedVote = Assert.IsType<Vote>(okResult.Value);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddVote_ShouldReturnOK_WhenVoteIsCreatedSuccessfully_ForComment()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "addVoteShouldReturnOKForComment",
            IsPost = false,
            Type = true,
            UID = "c1cb"
        };

        _appSettingsMock.Setup(a => a.VoteTableName).Returns(VoteTableName);

        var now = DateTime.Now;
        var request = new Comment
        {
            PID = "addVoteShouldReturnOKForComment",
            CID = "addVoteShouldReturnOKForComment",
            CommentBody = "addVoteShouldReturnOKForComment",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "c1cb",
            Upvotes = 0,
            Downvotes = 0,
        };

        // Sets up LoadAsync to return the request comment
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Comment>(It.IsAny<string>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _voteActionsMock.AddVote(vote);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Vote>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedVote = Assert.IsType<Vote>(okResult.Value);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddVote_ShouldReturnOK_WhenVoteIsCreatedSuccessfully_ForDownvotes()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "addVoteShouldReturnOK",
            IsPost = true,
            Type = false,
            UID = "c1cb"
        };

        _appSettingsMock.Setup(a => a.VoteTableName).Returns(VoteTableName);

        // Arrange
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "addVoteShouldReturnOK",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "c1cb",
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

        // Setup SaveAsync to succeed
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _voteActionsMock.AddVote(vote);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Vote>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Access the actual result

        var returnedVote = Assert.IsType<Vote>(okResult.Value);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);

        // Verify the SaveAsync was called once with the correct parameters
        _dynamoDbContextMock.Verify(
            d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddVote_ShouldReturnStatus500_WhenExceptionIsThrown()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "addVoteShouldReturnStatus500",
            IsPost = true,
            Type = true,
            UID = ""
        };

        _appSettingsMock.Setup(a => a.VoteTableName).Returns(VoteTableName);
        
        // Setup SaveAsync to throw an exception
        _dynamoDbContextMock.Setup(d => d.SaveAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error saving to DynamoDB"));
            
        // Act
        var result = await _voteActionsMock.AddVote(vote);

        // Assert that the result is ActionResult<Vote>
        var actionResult = Assert.IsType<ActionResult<Vote>>(result); 
        // Access the Result property to get the actual StatusCodeResult
        var statusCodeResult = Assert.IsType<StatusCodeResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
    }

    #endregion

    #region GetVote Tests

    [Fact]
    public async Task GetVote_ShouldReturnVote_WhenSuccessful()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "getVoteShouldReturnVote",
            IsPost = true,
            Type = true,
            UID = "1234"
        };
        var response = new Vote { PID = "getVoteShouldReturnVote", IsPost = true, Type = true, UID = "1234" };

        // Sets up LoadAsync to return the request vote (for in GetVote)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Vote>(vote.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vote);

        var list = new List<Vote>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _voteActionsMock.GetVote(vote.UID, vote.PID, vote.Type);

        // Assert
        var returnedVote = Assert.IsType<Vote>(result);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetVote_ShouldReturnNull_WhenExceptionIsThrown()
    {
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        // Arrange
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to get vote: "));
            
        // Act
        var result = await _voteActionsMock.GetVote("", "", false);
        
        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    [Fact]
    public async Task GetVote_ShouldReturnNull_WhenTooManyVotesReturned()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "getVoteShouldReturnTooManyVotes",
            IsPost = true,
            Type = true,
            UID = "1234"
        };
        var response = new Vote { PID = "getVoteShouldReturnTooManyVotes", IsPost = true, Type = true, UID = "1234" };

        // Sets up LoadAsync to return the request vote (for in GetVote)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Vote>(vote.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vote);

        var list = new List<Vote>();
        list.Add(response);
        list.Add(response); // Adding two to be returned

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _voteActionsMock.GetVote(vote.UID, vote.PID, vote.Type);
        
        // Assert
        Assert.Null(result);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }

    #endregion

    #region GetVotesByPid Tests
    
    [Fact]
    public async Task GetVotesByPid_ShouldReturnVotes_WithAValidQuery()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "getVotesByPidShouldReturnVotes",
            IsPost = true,
            Type = true,
            UID = "uid"
        };
        var response = new Vote { PID = "getVotesByPidShouldReturnVotes", IsPost = true, Type = true, UID = "uid" };

        // Sets up LoadAsync to return the request vote (for in GetVote)
        _dynamoDbContextMock.Setup(d => d.LoadAsync<Vote>(vote.PID, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vote);

        var list = new List<Vote>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Act
        var result = await _voteActionsMock.GetVotesByPid(vote.PID);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal(vote.PID, result.First().PID);
        Assert.Equal(vote.IsPost, result.First().IsPost);
        Assert.Equal(vote.Type, result.First().Type);
        Assert.Equal(vote.UID, result.First().UID);

        _dynamoDbContextMock.Verify(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task GetVotesByPid_ShouldReturnEmptyList_WhenExceptionIsThrown()
    {
        // Arrange
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to get votes: "));

        // Act
        var result = await _voteActionsMock.GetVotesByPid("!");

        // Assert
        Assert.Empty(result);
    }
    
    #endregion

    #region RemoveVote Tests

    [Fact]
    public async Task RemoveVote_ShouldCallDeleteAsync()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "11111",
            IsPost = true,
            Type = true,
            UID = "11111"
        };
        var response = new Vote { PID = "11111", IsPost = true, Type = true, UID = "11111" };
        
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "c1cb",
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

        var list = new List<Vote>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _voteActionsMock.RemoveVote(vote.UID, vote.PID, vote.Type);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(response, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task RemoveVote_ShouldHandleException_WhenVoteDoesNotExist()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "",
            IsPost = true,
            Type = true,
            UID = ""
        };
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to retrieve vote"));

        // Act
        var result = await _voteActionsMock.RemoveVote(vote.UID, vote.PID, vote.Type);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task RemoveVote_ShouldHandleException_WhenRemoveVoteFails()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "11111",
            IsPost = true,
            Type = true,
            UID = "11111"
        };
        var response = new Vote { PID = "11111", IsPost = true, Type = true, UID = "11111" };
        
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "11111",
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

        var list = new List<Vote>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Sets up DeleteAsync to throw an exception           
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to delete vote"));

        // Act
        var result = await _voteActionsMock.RemoveVote(vote.UID, vote.PID, vote.Type);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(response, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }
    
    #endregion

    #region DeleteVotes Tests

    [Fact]
    public async Task DeleteVotes_ShouldCallRemoveVotes()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "DeleteVotes_ShouldCallRemoveVote()",
            IsPost = true,
            Type = true,
            UID = "uid"
        };
        var response = new Vote { PID = "DeleteVotes_ShouldCallRemoveVote()", IsPost = true, Type = true, UID = "uid" };
        
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "DeleteVotes_ShouldCallRemoveVote()",
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

        var list = new List<Vote>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);

        // Sets up DeleteAsync to succeed            
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _voteActionsMock.DeleteVotes(vote.PID);

        // Assert
        Assert.True(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(response, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task DeleteVotes_ShouldHandleException_WhenPostDoesNotExist()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "!",
            IsPost = true,
            Type = true,
            UID = "11111"
        };
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to retrieve votes"));

        // Act
        var result = await _voteActionsMock.DeleteVotes(vote.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteVotes_ShouldHandleException_WhenDeleteVotesFails()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "",
            IsPost = true,
            Type = true,
            UID = "11111"
        };
        var response = new Vote { PID = "", IsPost = true, Type = true, UID = "11111" };
        
        var now = DateTime.Now;
        var request = new Post
        {
            PID = "11111",
            CreatedAt = now,
            UpdatedAt = now,
            UID = "11111",
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

        var list = new List<Vote>();
        list.Add(response);

        // Mock the AsyncSearch<Post> returned by ScanAsync
        var scanToSearchMock = new Mock<AsyncSearch<Vote>>();
        scanToSearchMock.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);
            
        _dynamoDbContextMock.Setup(d => d.ScanAsync<Vote>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
            .Returns(scanToSearchMock.Object);
        
        // Sets up DeleteAsync to throw an exception           
        _dynamoDbContextMock.Setup(d => d.DeleteAsync(It.IsAny<Vote>(), It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to delete votes"));

        // Act
        var result = await _voteActionsMock.DeleteVotes(vote.PID);

        // Assert
        Assert.False(result);
        _dynamoDbContextMock.Verify(d => d.DeleteAsync(response, It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()));
    }
    
    #endregion

}
