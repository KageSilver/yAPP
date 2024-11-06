using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
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

    public CommentController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext, IAwardActions awardActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _awardActions = awardActions;
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

        var award = await _awardActions.GetAwardById(pid);

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
}