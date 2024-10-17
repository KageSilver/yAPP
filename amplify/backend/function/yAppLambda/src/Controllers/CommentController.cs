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

    public CommentController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext, ICommentActions commentActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _commentActions = commentActions;
    }

    // POST: api/comments/createComment with body { "userName": "username", "commentBody": "body", "pid": "pid" }
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

        if(request == null || string.IsNullOrEmpty(request.CommentBody) || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.PID))
        {
            result = BadRequest("request body is required and must contain commenter's username, comment body, and the original post's id.");
        }
        else
        {
            Console.WriteLine("Post request from: " + request.UserName + " with pid: " + request.PID);

            var poster = await _cognitoActions.GetUser(request.UserName);

            if(poster == null)
            {
                result = NotFound("Post creator not found");
            }
            else
            {
                var comment = new Comment
                {
                    UserName = request.UserName,
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

    // GET: api/comments/getPostByCid?cid={cid}
    /// <summary>
    /// Gets the post given comment ID
    /// </summary>
    /// <param name="cid">The id to find a post given a comment id.</param>
    /// <returns>The post associated to the comment.</returns>
    [HttpGet("getPostByCid")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> GetPostByCid(string cid)
    {
        if(string.IsNullOrEmpty(cid))
        {
            return BadRequest("Comment ID is required");
        }

        var post = await _commentActions.GetPostByCid(cid);

        if(post == null)
        {
            return NotFound("Post does not exist");
        }

        return post;
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
    public async Task<ActionResult<Post>> GetCommentById(string cid)
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

    // GET: api/comments/getCommentsByPid?pid={pid}
    /// <summary>
    /// Gets all comments with given post ID
    /// </summary>
    /// <param name="pid">The id to find a post and comment thread.</param>
    /// <returns>A list of comments made under a post.</returns>
    [HttpGet("getCommentsByPid")]
    [ProducesResponseType(typeof(List<Comment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Comment>>> GetPostsByUser(string userName, bool diaryEntry)
    {
        if(string.IsNullOrEmpty(userName))
        {
            return BadRequest("username is required");
        }

        var posts = await _postActions.GetPostsByUser(userName, diaryEntry);

        return posts;
    }

    // GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
    /// <summary>
    /// Gets recent posts from before a specified time.
    /// </summary>
    /// <param name="since">Returns posts made after this time.</param>
    /// <param name="maxResults">The maximum number of results to retrieve.</param>
    /// <returns>A list of recent posts.</returns>
    [HttpGet("getRecentPosts")]
    [ProducesResponseType(typeof(List<Post>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Post>>> GetRecentPosts(DateTime since, int maxResults)
    {
        if(maxResults < 0 || !DateTime.TryParse(since.ToString(), out since))
        {
            return BadRequest("requires valid max result number and valid time");
        }

        var posts = await _postActions.GetRecentPosts(since, maxResults);

        return posts;
    }
    
    // DELETE: api/posts/deletePost?pid={pid}
    /// <summary>
    /// Deletes a post from the database by a post id.
    /// </summary>
    /// <param name="pid">The id of the post to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    [HttpDelete("deletePost")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeletePost(string pid)
    {
        if(string.IsNullOrEmpty(pid))
        {
            return BadRequest("Post id is required");
        }

        var deleted = await _postActions.DeletePost(pid);

        return deleted;
    }

    // PUT: api/posts/updatePost with body { "pid": "pid", "createdAt": "createdAt", "userName": "username", "postTitle": "title", "postBody": "body", "upvotes": "upvotes", "downvotes": "downvotes", "diaryEntry": false, "anonymous": false }
    /// <summary>
    /// Edits an already existing post.
    /// </summary>
    /// <param name="request">The new version of the post after editing.</param>
    /// <returns>An ActionResult containing the edited Post object if successful, or an error message if it fails.</returns>
    [HttpPut("updatePost")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> UpdatePost([FromBody] Post request)
    {
        if(request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.PostBody) || string.IsNullOrEmpty(request.PostTitle))
        {
            return BadRequest("request body is required and must contain username, post title, post body");
        }

        var post = await _postActions.UpdatePost(request);

        return post;
    }
}