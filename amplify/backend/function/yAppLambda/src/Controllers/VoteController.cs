using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// The 'VoteController" class is an API controller in the 'yAppLamba project. 
/// It is responsible for handling HTTP requests related to vote operations.
/// </summary>
[ApiController]
[Route("api/votes")]
public class VoteController : ControllerBase 
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;
    private readonly IVoteActions _voteActions;

    public VoteController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext, IVoteActions voteActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _voteActions = voteActions;
    }

    // GET: api/votes/getVoteStatus?uid={uid}&pid={pid}&type={type}
    /// <summary>
    /// Get the given vote by uid, pid, and type
    /// </summary>
    /// <param name="uid">The uid of the current user.</param>
    /// <param name="pid">The pid of the post or comment.</param>
    /// <param name="type">Whether it's checking for an upvote/downvote.</param>
    /// <returns>A boolean result showing if the vote exists.</returns>
    [HttpGet("getVote")]
    [ProducesResponseType(typeof(Vote), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Vote>> GetVote(string uid, string pid, bool type)
    {
        if(string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pid) )
        {
            return BadRequest("UID and Comment/Post ID are both required");
        }

        var vote = await _voteActions.GetVote(uid, pid, type);

        if(vote == null)
        {
            return NotFound("Vote does not exist");
        }

        return vote;
    }

    // GET: api/votes/getVotesByPid?pid={pid}
    /// <summary>
    /// Gets all votes with given PID
    /// </summary>
    /// <param name="pid">The pid to find a vote under.</param>
    /// <returns>A list of votes made under a post/comment.</returns>
    [HttpGet("getVotesByPid")]
    [ProducesResponseType(typeof(List<Vote>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Vote>>> GetVotesByPid(string pid)
    {
        if(string.IsNullOrEmpty(pid))
        {
            return BadRequest("Pid is required");
        }

        var votes = await _voteActions.GetVotesByPid(pid);

        return votes;
    }

    // POST: api/votes/addVote with body {"pid": "pid", "isPost": "isPost", "type": "type", "uid": "uid"}
    /// <summary>
    /// Creates a new vote
    /// </summary>
    /// <param name="vote">The vote object to be created.</param>
    /// <returns>An ActionResult containing the created vote object if successful, or an error message if it fails.</returns>
    [HttpPost("addVote")]
    [ProducesResponseType(typeof(Vote), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Vote>> AddVote([FromBody] Vote request)
    {
        ActionResult<Vote> result;

        if(request == null || string.IsNullOrEmpty(request.PID) || string.IsNullOrEmpty(request.UID))
        {
            result = BadRequest("Request body is required and must contain the post/comment's id and the user's id.");
        }
        else
        {
            Console.WriteLine("Post request from: " + request.UID + " with id: " + request.PID);

            var user = await _cognitoActions.GetUserById(request.UID);

            if(user == null)
            {
                result = NotFound("User not found");
            }
            else
            {
                var vote = new Vote
                {
                    PID = request.PID,
                    IsPost = request.IsPost,
                    Type = request.Type,
                    UID = request.UID
                };

                var createResult = await _voteActions.AddVote(vote);
                result = createResult.Result is OkObjectResult
                    ? (ActionResult<Vote>)vote
                    : BadRequest("Failed to create vote object");
            }
        }

        return result;
    }

    // DELETE: api/votes/removeVote?uid={uid}&pid={pid}&type={type}
    /// <summary>
    /// Remove the corresponding vote by pid and uid
    /// </summary>
    /// <param name="uid">The uid of the current user.</param>
    /// <param name="pid">The pid of the post or comment.</param>
    /// <param name="type">Whether it's removing an upvote/downvote.</param>
    /// <returns>A boolean result determining if the deletion failed.</returns>
    [HttpDelete("removeVote")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> RemoveVote(string uid, string pid, bool type)
    {
        if(string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pid))
        {
            return BadRequest("User id and post/comment id is required");
        }

        var deleted = await _voteActions.RemoveVote(uid, pid, type);

        return deleted;
    }

}