using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.UnitTests.Controllers;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using yAppLambda.Controllers;
using yAppLambda.Models;
using yAppLambda.Common;
using Amazon.DynamoDBv2.DataModel;
using yAppLambda.DynamoDB;

public class VoteControllerTests
{
    private readonly Mock<IAppSettings> _mockAppSettings;
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock;
    private readonly Mock<ICognitoActions> _mockCognitoActions;
    private readonly VoteController _voteController;
    private readonly Mock<IVoteActions> _mockVoteActions;

    public VoteControllerTests()
    {
        _mockAppSettings = new Mock<IAppSettings>();
        _dynamoDbContextMock = new Mock<IDynamoDBContext>();
        _mockCognitoActions = new Mock<ICognitoActions>();
        _mockVoteActions = new Mock<IVoteActions>();
        _voteController = new VoteController(_mockAppSettings.Object, _mockCognitoActions.Object,
            _dynamoDbContextMock.Object, _mockVoteActions.Object);
    }

    #region AddVote Tests

    [Fact]
    public async Task AddVote_ShouldReturnBadResult_WhenRequestIsNull()
    {
        // Arrange
        Vote request = null;

        // Act
        var result = await _voteController.AddVote(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Request body is required and must contain the post/comment's id and the user's id.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddVote_ShouldReturnNotFound_WhenUidIsNotFound()
    {
        // Arrange
        var request = new Vote { PID = "20", IsPost = true, Type = true, UID = "userDoesNotExist" };
        _mockCognitoActions.Setup(u => u.GetUserById(request.UID)).ReturnsAsync((User)null);

        // Act
        var result = await _voteController.AddVote(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found", notFoundResult.Value);
    }

    [Fact]
    public async Task AddVote_ShouldReturnOK_WhenVoteIsCreatedSuccessfully()
    {
        // Arrange 
        var request = new Vote { PID = "1", IsPost = true, Type = true, UID = "uid" };
        var voter = new User {Id = "uid" };
        var vote = new Vote 
        { 
            PID = "1", 
            IsPost = true, 
            Type = true, 
            UID = "uid" 
        };

        // Mock GetUserById to return the voter
        _mockCognitoActions.Setup(u => u.GetUserById(request.UID)).ReturnsAsync(voter);

        // Mock AddVote to return a new Vote object
        _mockVoteActions.Setup(c => c.AddVote(It.IsAny<Vote>())).ReturnsAsync(new OkObjectResult(vote));

        // Act
        var result = await _voteController.AddVote(request);

        // Assert
        var returnedVote = result.Value;
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);
    }
    
    #endregion

    #region GetVote Tests

    [Fact]
    public async Task GetVote_ShouldReturnVote_WhenSuccessful()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "1",
            IsPost = true,
            Type = true,
            UID = "uid"
        };

        _mockVoteActions.Setup(v => v.GetVote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(vote);

        // Act
        var result = await _voteController.GetVote(vote.UID, vote.PID, vote.Type);

        // Assert
        var returnedVote = Assert.IsType<Vote>(result.Value);
        Assert.Equal(vote.PID, returnedVote.PID);
        Assert.Equal(vote.IsPost, returnedVote.IsPost);
        Assert.Equal(vote.Type, returnedVote.Type);
        Assert.Equal(vote.UID, returnedVote.UID);
    }

    [Fact]
    public async Task GetVote_ShouldReturnNotFound_WhenVoteNotFound()
    {
        // Arrange
        _mockVoteActions.Setup(v => v.GetVote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync((Vote)null);

        // Act
        var result = await _voteController.GetVote("!", "!", true);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Vote does not exist", notFoundResult.Value);
    }

    [Fact]
    public async Task GetVote_ShouldReturnBadRequest_WithNullId()
    {
        // Act
        var result = await _voteController.GetVote(null, null, true);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("UID and Comment/Post ID are both required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetVote_ShouldReturnBadRequest_WithEmptyPid()
    {
        // Act
        var result = await _voteController.GetVote("", "hi", true);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("UID and Comment/Post ID are both required", badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetVote_ShouldReturnBadRequest_WithEmptyUid()
    {
        // Act
        var result = await _voteController.GetVote("hi", "", true);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("UID and Comment/Post ID are both required", badRequestResult.Value);
    }

    #endregion

    #region GetVotesByPid Tests

    [Fact]
    public async Task GetVotesByPid_ShouldReturnVotes_WhenSuccessful()
    {
        // Arrange
        var vote = new Vote
        {
            PID = "1",
            IsPost = true,
            Type = true,
            UID = "uid"
        };

        var list = new List<Vote>();
        list.Add(vote);

        _mockVoteActions.Setup(v => v.GetVotesByPid(It.IsAny<string>())).ReturnsAsync(list);

        // Act
        var result = await _voteController.GetVotesByPid(vote.PID);

        // Assert
        var returnedList = Assert.IsType<List<Vote>>(result.Value);
        Assert.Equal(1, returnedList.Count);
        Assert.Equal(vote.PID, returnedList.First().PID);
        Assert.Equal(vote.IsPost, returnedList.First().IsPost);
        Assert.Equal(vote.Type, returnedList.First().Type);
        Assert.Equal(vote.UID, returnedList.First().UID);
    }

    [Fact]
    public async Task GetVotesByPid_ShouldReturnBadRequest_WithNullPid()
    {
        // Act
        var result = await _voteController.GetVotesByPid(null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Pid is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetVotesByPid_ShouldReturnBadRequest_WithEmptyPid()
    {
        // Act
        var result = await _voteController.GetVotesByPid("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Pid is required", badRequestResult.Value);
    }

    #endregion
    
    #region RemoveVote Tests

    [Fact]
    public async Task RemoveVote_ShouldReturnTrue_WhenVoteIsDeletedSuccessfully()
    {
        // Arrange
        _mockVoteActions.Setup(v => v.RemoveVote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);

        // Act
        var response = await _voteController.RemoveVote("1", "1", true);

        // Assert
        Assert.True(response.Value);
    }

    [Fact]
    public async Task RemoveVote_ShouldReturnBadRequest_WhenPidIsNull()
    {
        // Act
        var result = await _voteController.RemoveVote(null, null, true);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User id and post/comment id is required",
            badRequestResult.Value);
    }
    
    [Fact]
    public async Task RemoveVote_ShouldReturnFalse_WhenDeleteFails()
    {
        // Arrange
        _mockVoteActions.Setup(v => v.RemoveVote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(false);

        // Act
        var response = await _voteController.RemoveVote("1", "1", true);
        
        // Assert
        Assert.False(response.Value);
    }

    #endregion

}
