using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;
using yAppLambda.Enum;

namespace yAppLambda.Controllers;

/// <summary>
/// The `FriendController` class is an API controller in the `yAppLambda` project. It is responsible for handling HTTP requests related to friend operations.
/// The api gateway's entry point is /api, therefore we need to ensure "api" in the route
/// </summary>
[ApiController]
[Route("api/friends")]
public class FriendController : ControllerBase
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;

    public FriendController(IAppSettings appSettings, IDynamoDBContext dbContext)
    {
        _appSettings = appSettings;
        _dbContext = dbContext;
    }
    
    // POST: api/friends/friendRequest with body { "fromUserName": "username", "toUserId": "id" }
    /// <summary>
    /// Sends a friend request from one user to another.
    /// </summary>
    /// <param name="request">The friend request object containing the details of the request.</param>
    /// <returns>An ActionResult containing the Friendship object if the request is successful, or an error message if it fails.</returns>
    [HttpPost("friendRequest")]
    [ProducesResponseType(typeof(Friendship), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Friendship>> SendFriendRequest([FromBody] FriendRequest request)
    {
        ActionResult<Friendship> result;

        if (request == null || request.FromUserName == null || request.ToUserId == null)
        {
            result = BadRequest("request body is required and must contain username and friend's id");
        }
        else
        {
            Console.WriteLine("Friend request from: " + request.FromUserName + " to: " + request.ToUserId);
            var friend = await CognitoActions.GetUserById(request.ToUserId, _appSettings);
            if (friend == null)
            {
                result = NotFound("Friend not found");
            }
            else
            {
                var friendship = new Friendship
                {
                    FromUserName = request.FromUserName,
                    ToUserName = friend.UserName,
                    Status = FriendshipStatus.Pending
                };

                var existingFriendship = await FriendshipActions.GetFriendship(request.FromUserName, friend.UserName,
                    _dbContext, _appSettings);

                if (existingFriendship == null)
                {
                    var createResult = await FriendshipActions.CreateFriendship(friendship, _dbContext, _appSettings);
                    result = createResult.Result is OkObjectResult
                        ? (ActionResult<Friendship>)friendship
                        : BadRequest("Failed to create friendship");
                }
                else
                {
                    if (existingFriendship.Value == null)
                    {
                        result = BadRequest("Failed to create friendship");
                    }
                    else
                    {
                        existingFriendship.Value.Status = FriendshipStatus.Pending;
                        var updateResult =
                            await FriendshipActions.UpdateFriendshipStatus(existingFriendship.Value, _dbContext,
                                _appSettings);
                        result = updateResult.Result is OkObjectResult
                            ? (ActionResult<Friendship>)existingFriendship.Value
                            : BadRequest("Failed to create friendship");
                    }
                }
            }
        }

        return result;
    }

    // POST: api/friends/updateFriendRequest with body { "fromUserName": "username", "toUserName": "username", "status": 1 }
    /// <summary>
    /// Updates the status of an existing friend request.
    /// </summary>
    /// <param name="request">The friend request object containing the details of the request.</param>
    /// <returns>An ActionResult containing the updated Friendship object if the update is successful, or an error message if it fails.</returns>
    [HttpPost("updateFriendRequest")]
    [ProducesResponseType(typeof(Friendship), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Friendship>> UpdateFriendRequest([FromBody] FriendRequest request)
    {
        if (request == null || request.FromUserName == null || request.FromUserName == null)

        {
            return BadRequest("request body is required and must contain username and friend's username");
        }

        ActionResult<Friendship> result = null;
        // get the status
        var status = GetFriendshipStatus(request.Status);

        // to retrieve the friendship object
        var friendship = await FriendshipActions.GetFriendship(request.FromUserName, request.ToUserName, _dbContext,
            _appSettings);

        if (friendship != null)
        {
            //update the status of the friendship, there are 3 statuses: Pending, Accepted, Declined
            if (friendship.Value != null && status != FriendshipStatus.All)
            {
                // update the status of the friendship
                friendship.Value.Status = GetFriendshipStatus(request.Status);
                var updateResult =
                    await FriendshipActions.UpdateFriendshipStatus(friendship.Value, _dbContext, _appSettings);
                result = updateResult.Result is OkObjectResult
                    ? (ActionResult<Friendship>)friendship.Value
                    : BadRequest("Failed to update friendship status");
            }
            else
            {
                result = BadRequest("Failed to update friendship status");
            }
        }
        else
        {
            result = NotFound("Friendship not found");
        }

        return result;
    }


    // GET: api/friends/getFriendsByStatus?userName={username}?status={status}
    /// <summary>
    /// Retrieves all friends of a user filtered by a specified status.
    /// </summary>
    /// <param name="userName">The username of the user whose friends are to be retrieved.</param>
    /// <param name="status">The status of the friendships to filter by (-1:All requests, 0: Pending, 1: Accepted, 2: Declined).</param>
    /// <returns>An ActionResult containing a list of Friendship objects if the retrieval is successful, or an error message if it fails.</returns>
    [HttpGet("getFriendsByStatus")]
    [ProducesResponseType(typeof(List<Friendship>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Friendship>>> GetFriends(string userName, int status = -1)
    {
        if (userName == null)
        {
            return BadRequest("username is required");
        }

        // get the friendship status
        var friendshipStatus = GetFriendshipStatus(status);

        var friendships = await FriendshipActions.GetAllFriends(userName, friendshipStatus, _dbContext, _appSettings);

        return friendships;
    }

    /// <summary>
    /// Converts an integer status code to the corresponding FriendshipStatus enum value.
    /// </summary>
    /// <param name="status">The integer status code (0: Pending, 1: Accepted, 2: Declined).</param>
    /// <returns>The corresponding FriendshipStatus enum value.</returns>
    private static FriendshipStatus GetFriendshipStatus(int status)
    {
        var friendshipStatus = status switch
        {
            0 => FriendshipStatus.Pending,
            1 => FriendshipStatus.Accepted,
            2 => FriendshipStatus.Declined,
            _ => FriendshipStatus.All
        };
        return friendshipStatus;
    }
}