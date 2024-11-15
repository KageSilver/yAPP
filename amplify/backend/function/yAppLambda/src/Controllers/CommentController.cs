using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// The 'CommentController" class is an API controller in the 'yAppLamba project. 
/// It is responsible for handling HTTP requests related to comment operations.
/// </summary>
[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase 
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;
    private readonly ICommentActions _commentActions;
    private readonly IVoteActions _voteActions;

    public CommentController(IAppSettings appSettings, ICognitoActions cognitoActions, 
                            IDynamoDBContext dbContext, ICommentActions commentActions, IVoteActions voteActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _commentActions = commentActions;
        _voteActions = voteActions;
    }

    // POST: api/comments/createComment with body { "uid": "uid", "commentBody": "body", "pid": "pid" }
    /// <summary>
    /// Creates a comment
    /// </summary>
    /// <param name="comment">The comment object that contains information on the comment.</param>
    /// <returns>An ActionResult containing the created Comment object or an error status.</returns>
    [HttpPost("createComment")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Comment>> CreateComment([FromBody] NewComment request)
    {
        ActionResult<Comment> result;

        if(request == null || string.IsNullOrEmpty(request.CommentBody) || string.IsNullOrEmpty(request.UID) || string.IsNullOrEmpty(request.PID))
        {
            result = BadRequest("Request body is required and must contain commenter's uid, comment body, and the original post's id.");
        }
        else
        {
            Console.WriteLine("Post request from: " + request.UID + " with pid: " + request.PID);

            var commenter = await _cognitoActions.GetUserById(request.UID);

            if(commenter == null)
            {
                result = NotFound("Comment creator not found");
            }
            else
            {
                var comment = new Comment
                {
                    PID = request.PID,
                    UID = request.UID,
                    CommentBody = request.CommentBody,
                    Upvotes = 0,
                    Downvotes = 0
                };

                var createResult = await _commentActions.CreateComment(comment);
                result = createResult.Result is OkObjectResult
                    ? (ActionResult<Comment>)comment
                    : BadRequest("Failed to create comment");
            }
        }

        return result;
    }

    // GET: api/comments/getCommentById?cid={cid}
    /// <summary>
    /// Gets the comment given comment ID
    /// </summary>
    /// <param name="cid">The id to find a comment given a comment id.</param>
    /// <returns>The comment associated to the cid.</returns>
    [HttpGet("getCommentById")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Comment>> GetCommentById(string cid)
    {
        if(string.IsNullOrEmpty(cid))
        {
            return BadRequest("Comment ID is required");
        }

        var comment = await _commentActions.GetCommentById(cid);

        if(comment == null)
        {
            return NotFound("Comment does not exist");
        }

        return comment;
    }

    // GET: api/comments/getCommentsByUid?uid={uid}
    /// Gets all comments with given uid
    /// </summary>
    /// <param name="uid">The uid to find all comments a user has made.</param>
    /// <returns>A list of comments made by a user.</returns>
    [HttpGet("getCommentsByUid")]
    [ProducesResponseType(typeof(List<Comment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Comment>>> GetCommentsByUid(string uid)
    {
        if(string.IsNullOrEmpty(uid))
        {
            return BadRequest("Uid is required");
        }

        var comments = await _commentActions.GetCommentsByUid(uid);

        return comments;
    }

    // GET: api/comments/getCommentsByPid?pid={pid}
    /// <summary>
    /// Gets all comments with given post ID
    /// </summary>
    /// <param name="pid">The id to find a post and comment thread.</param>
    /// <returns>A list of comments made under a post.</returns>
    [HttpGet("getCommentsByPid")]
    [ProducesResponseType(typeof(List<Comment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Comment>>> GetCommentsByPid(string pid)
    {
        if(string.IsNullOrEmpty(pid))
        {
            return BadRequest("Post ID is required");
        }

        var comments = await _commentActions.GetCommentsByPid(pid);

        return comments;
    }

    // DELETE: api/comments/deleteComment?cid={cid}
    /// <summary>
    /// Deletes a comment from the database by a comment id.
    /// </summary>
    /// <param name="cid">The id of the comment to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    [HttpDelete("deleteComment")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteComment(string cid)
    {
        if(string.IsNullOrEmpty(cid))
        {
            return BadRequest("Comment ID is required");
        }

        await _voteActions.DeleteVotes(cid);
        var deleted = await _commentActions.DeleteComment(cid);

        return deleted;
    }

    // PUT: api/comments/updateComment with body { "cid": "cid", "pid": "pid", "uid": "uid", "createdAt": "createdAt", "updatedAt": "updatedAt", "commentBody": "body", "upvotes": "upvotes", "downvotes": "downvotes" }
    /// <summary>
    /// Edits an already existing comment.
    /// </summary>
    /// <param name="request">The new version of the comment after editing.</param>
    /// <returns>An ActionResult containing the edited Comment object if successful, or an error message if it fails.</returns>
    [HttpPut("updateComment")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Comment>> UpdateComment([FromBody] Comment request)
    {
        if(request == null || string.IsNullOrEmpty(request.UID) || string.IsNullOrEmpty(request.CommentBody) || string.IsNullOrEmpty(request.PID))
        {
            return BadRequest("Request body is required and must contain pid, uid and comment body");
        }

        var comment = await _commentActions.UpdateComment(request);

        return comment;
    }
}