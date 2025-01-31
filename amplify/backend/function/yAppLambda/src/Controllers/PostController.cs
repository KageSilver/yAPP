using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// The "PostController" class is an API controller in the yAppLambda project. 
/// It is responsible for handling HTTP requests related to post operations.
/// </summary>
[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase 
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;
    private readonly IPostActions _postActions;
    private readonly IVoteActions _voteActions;

    public PostController(IAppSettings appSettings, ICognitoActions cognitoActions, 
                          IDynamoDBContext dbContext, IPostActions postActions, IVoteActions voteActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _postActions = postActions;
        _voteActions = voteActions;
    }
    
    // POST: api/posts/createPost with body { "uid": "uid", "postTitle": "title", "postBody": "body", "diaryEntry": false, "anonymous": false }
    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="post">The post object containing the details of the post</param>
    /// <returns>An ActionResult containing the Post object if the request is successful, or an error message if it fails</returns>
    [HttpPost("createPost")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> CreatePost([FromBody] NewPost request)
    {
        ActionResult<Post> result;

        if(request == null || string.IsNullOrEmpty(request.PostBody) || string.IsNullOrEmpty(request.PostTitle) || string.IsNullOrEmpty(request.UID))
        {
            return BadRequest("request body is required and must contain poster's uid, post title and post body");
        }

        var poster = await _cognitoActions.GetUserById(request.UID);

        if(poster == null)
        {
            return NotFound("Post creator not found");
        }

        if (request.DiaryEntry && GetDiariesByUser(request.UID, DateTime.UtcNow.Date).Result.Value.Count > 0)
        {
            return BadRequest("Cannot make more than one diary entry a day");
        }

        var post = new Post
        {
            UID = request.UID,
            PostTitle = request.PostTitle,
            PostBody = request.PostBody,
            DiaryEntry = request.DiaryEntry,
            Anonymous = request.Anonymous,
            Upvotes = 0,
            Downvotes = 0
        };

        var createResult = await _postActions.CreatePost(post);
        result = createResult.Result is OkObjectResult
            ? (ActionResult<Post>)post
            : BadRequest("Failed to create post");
        
        return result;
    }

    // GET: api/posts/getPostById?pid={pid}
    /// <summary>
    /// Retrieves a post by a unique identifier.
    /// </summary>
    /// <param name="pid">The unique identifier for a post.</param>
    /// <returns>An ActionResult containing the Post object if found, or a NotFound result otherwise</returns>
    [HttpGet("getPostById")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> GetPostById(string pid)
    {
        if(string.IsNullOrEmpty(pid))
        {
            return BadRequest("Post ID is required");
        }

        var post = await _postActions.GetPostById(pid);

        if(post == null)
        {
            return NotFound("Post does not exist");
        }

        return post;
    }

    // GET: api/posts/getPostsByUser?uid={uid}
    /// <summary>
    /// Gets all public posts from a user
    /// </summary>
    /// <param name="uid">The uid used to find all posts created by a user.</param>
    /// <returns>A list of public posts created by a user.</returns>
    [HttpGet("getPostsByUser")]
    [ProducesResponseType(typeof(List<Post>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Post>>> GetPostsByUser(string uid)
    {
        if(string.IsNullOrEmpty(uid))
        {
            return BadRequest("uid is required");
        }

        var posts = await _postActions.GetPostsByUser(uid);

        return posts;
    }
    
    // GET: api/posts/getDiariesByUser?uid={uid}&current={current}
    /// <summary>
    /// Gets the diary entries made by a user within a specific day
    /// </summary>
    /// <param name="uid">The author of the diary entry.</param>
    /// <param name="current">12am of a selected date to query.</param>
    /// <returns>The diary entry made by a user on the specified day.</returns>
    [HttpGet("getDiariesByUser")]
    [ProducesResponseType(typeof(List<Post>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Post>>> GetDiariesByUser (string uid, DateTime current)
    {
        if (string.IsNullOrEmpty(uid) || !DateTime.TryParse(current.ToString(), out current))
        {
            return BadRequest("uid and valid datetime is required");
        }
        
        var posts = await _postActions.GetDiariesByUser(uid, current);
        return posts;
    }
    
    // GET: api/posts/getDiariesByFriends?uid={uid}&current={current}
    /// <summary>
    /// Gets the diary entries made by the user's friends within a specific day
    /// </summary>
    /// <param name="uid">The user whose friends will be searched for.</param>
    /// <param name="current">12am of a selected date to query.</param>
    /// <returns>A list of diary entries made by the user's friends on the specified day</returns>
    [HttpGet("getDiariesByFriends")]
    [ProducesResponseType(typeof(List<Post>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Post>>> GetDiariesByFriends(string uid, DateTime current)
    {
        if (string.IsNullOrEmpty(uid) || !DateTime.TryParse(current.ToString(), out current))
        {
            return BadRequest("uid and valid datetime is required");
        }
        
        var posts = await _postActions.GetDiariesByFriends(_cognitoActions, uid, current);
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

        await _voteActions.DeleteVotes(pid);
        var deleted = await _postActions.DeletePost(pid);

        return deleted;
    }

    // PUT: api/posts/updatePost with body { "pid": "pid", "createdAt": "createdAt", "updatedAt": "updatedAt", "uid": "uid", "postTitle": "title", "postBody": "body", "upvotes": "upvotes", "downvotes": "downvotes", "diaryEntry": false, "anonymous": false }    
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
        if(request == null || string.IsNullOrEmpty(request.UID) || string.IsNullOrEmpty(request.PostBody) || string.IsNullOrEmpty(request.PostTitle))
        {
            return BadRequest("request body is required and must contain uid, post title, post body");
        }

        var post = await _postActions.UpdatePost(request);

        return post;
    }
}