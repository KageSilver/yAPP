using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;
using yAppLambda.Enum;

namespace yAppLambda.Controllers;

/// <summary>
/// The `FriendController` class is an API controller in the `yAppLambda` project.
/// It is responsible for handling HTTP requests related to friend operations.
/// The api gateway's entry point is /api, therefore we need to ensure "api" in the route.
/// </summary>
[ApiController]
[Route("api/friends")]
public class FriendController : ControllerBase
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;
    private readonly IFriendshipActions _friendshipActions;
    private readonly IFriendshipStatusActions _friendshipStatusActions;

    public FriendController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext,
                            IFriendshipActions friendshipActions, IFriendshipStatusActions friendshipStatusActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _friendshipActions = friendshipActions;
        _friendshipStatusActions = friendshipStatusActions;
    }

    // POST: api/friends/friendRequest with body { "fromUserName": "fromUsername", "toUserName": "toUsername" }
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

        if (request == null || string.IsNullOrEmpty(request.FromUserName) || string.IsNullOrEmpty(request.ToUserName))
        {
            result = BadRequest("Request body is required and must contain the sender AND receiver's usernames");
        }
        else
        {
            Console.WriteLine("Friend request from: " + request.FromUserName + " to: " + request.ToUserName);
            var friend = await _cognitoActions.GetUser(request.ToUserName);
            if (friend == null)
            {
                result = NotFound("Friend not found");
            }
            else
            {
                // Get friendships AB and BA
                var existingFriendship = await _friendshipActions.GetFriendship(request.FromUserName, request.ToUserName);
                var existingReversedFriendship = await _friendshipActions.GetFriendship(request.ToUserName, request.FromUserName);

                bool noAB = existingFriendship == null || existingFriendship.Value == null;
                bool noBA = existingReversedFriendship == null || existingReversedFriendship.Value == null;

                // If both friendships AB or BA does not exist, create a new one
                if (noAB && noBA)
                {
                    // Create new friendship AB
                    var friendship = new Friendship
                    {
                        FromUserName = request.FromUserName,
                        ToUserName = request.ToUserName,
                        Status = FriendshipStatus.Pending
                    };

                    var createResult = await _friendshipActions
                        .CreateFriendship(friendship);

                    result = createResult.Result is OkObjectResult
                        ? (ActionResult<Friendship>)friendship
                        : BadRequest("Failed to create friendship");
                }
                else
                {
                    result = BadRequest("Friendship already exists");
                }
            }
        }
        return result;
    }

    // PUT: api/friends/acceptFriendship with body { "fromUserName": "username", "toUserName": "username", "status": 1 }
    /// <summary>
    /// Updates the status of the sent friendship.
    /// </summary>
    /// <param name="request">The friend request object containing the details of the request.</param>
    /// <returns>An ActionResult containing the updated Friendship object if the update is successful, or an error message if it fails.</returns>
    [HttpPut("updateFriendRequest")]
    [ProducesResponseType(typeof(Friendship), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Friendship>> UpdateFriendRequest([FromBody] FriendRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.FromUserName) || string.IsNullOrEmpty(request.ToUserName))
        {
            return BadRequest("Request body is required and must contain username and friend's username");
        }

        ActionResult<Friendship> result;

        // Get the request's status
        var status = _friendshipStatusActions.GetFriendshipStatus(request.Status);

        // Retrieve the friendship object described in the request
        var friendship = await _friendshipActions.GetFriendship(request.FromUserName, request.ToUserName);

        // If the friendship actually exists...
        if (friendship != null)
        {
            // Update the status of the friendship. There are 2 statuses: Pending and Accepted.
            if (friendship.Value != null && status != FriendshipStatus.All)
            {
                friendship.Value.Status = _friendshipStatusActions.GetFriendshipStatus(request.Status);

                var updateResult = await _friendshipActions.UpdateFriendshipStatus(friendship.Value);

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

    // GET: api/friends/getFriendsByStatus?userName={username}&status={status}
    /// <summary>
    /// Retrieves all friends of a user filtered by a specified status.
    /// </summary>
    /// <param name="userName">The username of the user whose friends are to be retrieved.</param>
    /// <param name="status">The status of the friendships to filter by (-1: All requests, 0: Pending, 1: Accepted).</param>
    /// <returns>An ActionResult containing a list of Friendship objects if the retrieval is successful, or an error message if it fails.</returns>
    [HttpGet("getFriendsByStatus")]
    [ProducesResponseType(typeof(List<Friendship>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Friendship>>> GetFriends(string userName, int status = -1)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return BadRequest("Username is required");
        }

        // Get the friendship status
        var friendshipStatus = _friendshipStatusActions.GetFriendshipStatus(status);

        // Get all the friendships of the status
        var friendships = await _friendshipActions.GetAllFriends(userName, friendshipStatus);

        return friendships;
    }

    // GET: api/friends/getFriendship?fromUsername={username1}&toUsername={username2}
    /// <summary>
    /// Retrieves the friendship between `fromUsername` and `toUsername`.
    /// </summary>
    /// <param name="fromUserName">The username of the friendship's sender.</param>
    /// <param name="toUserName">The username of the friendship's recipient.</param>
    /// <returns>An ActionResult containing the frinedship if the retrieval is successful, or null if it fails.</returns>
    [HttpGet("getFriendship")]
    [ProducesResponseType(typeof(Friendship), StatusCodes.Status200OK)]
    public async Task<ActionResult<Friendship>> GetFriendship(string fromUserName, string toUserName)
    {
        if (string.IsNullOrEmpty(fromUserName) || string.IsNullOrEmpty(toUserName))
        {
            return BadRequest("Usernames are required");
        }

        // Get friendship object between sender and receiver
        var friend = await _friendshipActions.GetFriendship(fromUserName, toUserName);

        return friend;
    }

    // DELETE: api/friends/deleteFriendship?fromUsername={username1}&toUsername={username2}
    /// <summary>
    /// Deletes a friendship from the database.
    /// </summary>
    /// <param name="fromUserName">The username of the friendship's sender.</param>
    /// <param name="toUserName">The username of the friendship's recipient.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    [HttpDelete("deleteFriendship")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteFriendship(string fromUserName, string toUserName)
    {
        if(string.IsNullOrEmpty(fromUserName) || string.IsNullOrEmpty(toUserName))
        {
            return BadRequest("Usernames are required");
        }

        var friendship = await _friendshipActions.GetFriendship(fromUserName, toUserName);
        if(friendship == null)
        {
            return NotFound("The friendship doesn't exist!");
        }

        return await _friendshipActions.DeleteFriendship(fromUserName, toUserName);
    }
}