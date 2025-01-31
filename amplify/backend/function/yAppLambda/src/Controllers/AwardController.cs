using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// The 'AwardController" class is an API controller in the 'yAppLamba project. 
/// It is responsible for handling HTTP requests related to award operations.
/// </summary>
[ApiController]
[Route("api/awards")]
public class AwardController : ControllerBase 
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;
    private readonly IAwardActions _awardActions;
    private readonly IPostActions _postActions;
    private readonly IFriendshipActions _friendshipActions;

    public AwardController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext, 
        IAwardActions awardActions, IPostActions postActions, IFriendshipActions friendshipActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _awardActions = awardActions;
        _postActions = postActions;
        _friendshipActions = friendshipActions;
    }

    // GET: api/awards/getAwardById?aid={aid}
    /// <summary>
    /// Retrieves an award by a unique identifier.
    /// </summary>
    /// <param name="aid">The unique identifier for an award.</param>
    /// <returns>An ActionResult containing the Award object if found, or a NotFound result otherwise</returns>
    [HttpGet("getAwardById")]
    [ProducesResponseType(typeof(Award), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Award>> GetAwardById(string aid)
    {
        if(string.IsNullOrEmpty(aid))
        {
            return BadRequest("Award ID is required");
        }

        var award = await _awardActions.GetAwardById(aid);

        if(award == null)
        {
            return NotFound("Award does not exist");
        }

        return award;
    }

    // GET: api/awards/getAwardsByUser?uid={uid}
    /// <summary>
    /// Gets all awards from a user
    /// </summary>
    /// <param name="uid">The user who earned the awards being fetched.</param>
    /// <returns>A list of awards earned by the user.</returns>
    [HttpGet("getAwardsByUser")]
    [ProducesResponseType(typeof(List<Award>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Award>>> GetAwardsByUser(string uid)
    {
        if(string.IsNullOrEmpty(uid))
        {
            return BadRequest("uid is required");
        }

        var awards = await _awardActions.GetAwardsByUser(uid);

        return awards;
    }

    // GET: api/awards/getAwardsByPost?pid={pid}
    /// <summary>
    /// Gets all awards from a post
    /// </summary>
    /// <param name="pid">The post on which the awards were earned.</param>
    /// <returns>A list of awards earned on the post.</returns>
    [HttpGet("getAwardsByPost")]
    [ProducesResponseType(typeof(List<Award>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Award>>> GetAwardsByPost(string pid)
    {
        if(string.IsNullOrEmpty(pid))
        {
            return BadRequest("pid is required");
        }

        var awards = await _awardActions.GetAwardsByPost(pid);

        return awards;
    }

    // GET: api/awards/getNewAwardsByUser?uid={uid}
    /// <summary>
    /// Gets new awards a user has earned
    /// </summary>
    /// <param name="uid">The user who earned the awards being fetched.</param>
    /// <returns>A list of new awards earned by the user.</returns>
    [HttpGet("getNewAwardsByUser")]
    [ProducesResponseType(typeof(List<Award>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Award>>> GetNewAwardsByUser(string uid)
    {
        if(string.IsNullOrEmpty(uid))
        {
            return BadRequest("uid is required");
        }

        var user = await _cognitoActions.GetUserById(uid);

        if (user == null)
        {
            return NotFound("User does not exist");
        }

        var awards = new List<Award>();
        
        var posts = await _postActions.GetPostsByUser(uid);
        awards.AddRange(await _awardActions.CheckNewAwardsPerPost(posts));
        awards.AddRange(await _awardActions.CheckNewAwardsTotalPosts(posts, uid));

        var friends = await _friendshipActions.GetAllFriends(user.UserName, FriendshipStatus.Accepted);
        awards.AddRange(await _awardActions.CheckNewAwardsFriends(friends, uid));

        return awards;
    }
}