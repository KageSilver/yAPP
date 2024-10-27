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

    public PostController(IAppSettings appSettings, ICognitoActions cognitoActions, 
                          IDynamoDBContext dbContext, IPostActions postActions)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
        _postActions = postActions;
    }

    /// <code>
    /// POST: api/posts/createPost with body
    /// {
    ///     "uid": "uid",
    ///     "postBody": "body",
    ///     "postTitle": "title",
    ///     "diaryEntry": false,
    ///     "anonymous": false
    /// }
    /// </code>
    /// ----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="post">The post object containing the details of the post</param>
    /// <returns>An ActionResult containing the Post object if the request is successful, or an error message if it fails</returns>
    /// ----------------------------------------------------------------------------------------------------------------
    [HttpPost("createPost")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> CreatePost([FromBody] NewPost request)
    {
        ActionResult<Post> result;

        if(request == null || string.IsNullOrEmpty(request.PostBody) || 
           string.IsNullOrEmpty(request.PostTitle) || string.IsNullOrEmpty(request.UID))
        {
            result = BadRequest("request body is required and must contain poster's uid, post title and post body");
        }
        else
        {
            Console.WriteLine("Post request from: " + request.UID + " and title: " + request.PostTitle);

            var poster = await _cognitoActions.GetUserById(request.UID);

            if(poster == null)
            {
                result = NotFound("Post creator not found");
            }
            else
            {
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
            }
        }
        
        return result;
    }

    // GET: api/posts/getPostById?pid={pid}
    /// ----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Retrieves a post by a unique identifier.
    /// </summary>
    /// <param name="pid">The unique identifier for a post.</param>
    /// <returns>An ActionResult containing the Post object if found, or a NotFound result otherwise</returns>
    /// ----------------------------------------------------------------------------------------------------------------
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

    // GET: api/posts/getPostsByUser?uid={uid}&diaryEntry={diaryEntry}
    /// ----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Retrieves all posts from a user, either all public posts or all diary entries.
    /// </summary>
    /// <param name="uid">The uid used to find all posts created by a user.</param>
    /// <param name="diaryEntry">If the query is for public posts or diary entries.</param>
    /// <returns>A list of posts created by a user, either public posts or diary entries.</returns>
    /// ----------------------------------------------------------------------------------------------------------------
    [HttpGet("getPostsByUser")]
    [ProducesResponseType(typeof(List<Post>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Post>>> GetPostsByUser(string uid, bool diaryEntry)
    {
        if(string.IsNullOrEmpty(uid))
        {
            return BadRequest("uid is required");
        }

        var posts = await _postActions.GetPostsByUser(uid, diaryEntry);

        return posts;
    }
    
    // GET: api/posts/getRecentPosts?since={since}&maxResults={maxResults}
    /// ----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Gets recent posts from before a specified time.
    /// </summary>
    /// <param name="since">Returns posts made after this time.</param>
    /// <param name="maxResults">The maximum number of results to retrieve.</param>
    /// <returns>A list of recent posts.</returns>
    /// ----------------------------------------------------------------------------------------------------------------
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
    /// ----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Deletes a post from the database by a post id.
    /// </summary>
    /// <param name="pid">The id of the post to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    /// ----------------------------------------------------------------------------------------------------------------
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

    /// <code>
    /// PUT: api/posts/updatePost with body
    /// {
    ///     "pid": "pid",
    ///     "createdAt": "createdAt",
    ///     "updatedAt": "updatedAt",
    ///     "uid": "uid",
    ///     "postTitle": "title",
    ///     "postBody": "body",
    ///     "upvotes": "upvotes",
    ///     "downvotes": "downvotes",
    ///     "diaryEntry": false,
    ///     "anonymous": false
    /// }
    /// </code>
    /// ----------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Edits an already existing post.
    /// </summary>
    /// <param name="request">The new version of the post after editing.</param>
    /// <returns>An ActionResult containing the edited Post object if successful, or an error message if it fails.</returns>
    /// ----------------------------------------------------------------------------------------------------------------
    [HttpPut("updatePost")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> UpdatePost([FromBody] Post request)
    {
        if(request == null || string.IsNullOrEmpty(request.UID) || 
           string.IsNullOrEmpty(request.PostBody) || string.IsNullOrEmpty(request.PostTitle))
        {
            return BadRequest("request body is required and must contain uid, post title, post body");
        }

        var post = await _postActions.UpdatePost(request);

        return post;
    }
}