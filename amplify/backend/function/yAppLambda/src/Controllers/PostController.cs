using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.DynamoDB;
using yAppLambda.Models;

namespace yAppLambda.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase 
{
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dbContext;
    private readonly ICognitoActions _cognitoActions;

    public PostController(IAppSettings appSettings, ICognitoActions cognitoActions, IDynamoDBContext dbContext)
    {
        _appSettings = appSettings;
        _cognitoActions = cognitoActions;
        _dbContext = dbContext;
    }

    // POST: api/posts/createPost with body { "PID": "postID", "UserName": "username", "PostTitle": "title", "PostBody": "body", "Upvotes": 0, "Downvotes": 0, "DiaryEntry": false, "Anonymous": false }
    /// <summary>
    /// Creates a new post
    /// </summary>
    /// <param name="post">The post object containing the details of the post</param>
    /// <returns>An ActionResult containing the Post object if the request is successful, or an error message if it fails</returns>
    [HttpPost("createPost")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> CreatePost([FromBody] NewPost request)
    {
        ActionResult<Post> result;

        if(request == null || string.IsNullOrEmpty(request.UserName))
        {
            result = BadRequest("request body is required and must contain poster's username");
        }
        else
        {
            Console.WriteLine("Post request from: " + request.UserName + " and title: " + request.PostTitle);

            var poster = await _cognitoActions.GetUser(request.UserName);

            if(poster == null)
            {
                result = NotFound("Post creator not found");
            }
            else
            {
                var post = new Post
                {
                    UserName = request.UserName,
                    PostTitle = request.PostTitle,
                    PostBody = request.PostBody,
                    DiaryEntry = request.DiaryEntry,
                    Anonymous = request.Anonymous,
                    Upvotes = 0,
                    Downvotes = 0
                };

                var createResult = await PostActions.CreatePost(post, _dbContext, _appSettings);
                result = createResult.Result is OkObjectResult
                    ? (ActionResult<Post>)post
                    : BadRequest("Failed to create post");
            }
        }

        return result;
    }
}