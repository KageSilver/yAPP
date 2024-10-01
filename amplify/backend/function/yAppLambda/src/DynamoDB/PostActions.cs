using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class PostActions : IPostActions
{
    //This is the default table name for the post table
    private const string PostTableName = "Post-test";
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly string _postTable;
    private readonly DynamoDBOperationConfig _config;

    public PostActions(IAppSettings appSettings, IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;

        _postTable = string.IsNullOrEmpty(_appSettings.PostTableName)
            ? PostTableName
            : _appSettings.PostTableName;
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _postTable
        };
    }
    
    /// <summary>
    /// Creates a post
    /// </summary>
    /// <param name="post">The post object that contains information on the post.</param>
    /// <returns>An ActionResult containing the created Post object or an error status.</returns>
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        try
        {
            // update the current time
            post.CreatedAt = DateTime.Now;
            // gets a unique ID for the post
            post.PID = Guid.NewGuid().ToString();

            await _dynamoDbContext.SaveAsync(post, _config);

            return new OkObjectResult(post);

        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create post: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a post by the post ID
    /// </summary>
    /// <param name="pid">The id to find a post.</param>
    /// <returns>An ActionResult containing the Post object if found, or a NotFound result otherwise.</returns>
    public async Task<ActionResult<Post>> GetPostById(string pid)
    {
        try
        {
            var post = await _dynamoDbContext.LoadAsync<Post>(pid);
            return post;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get post: " + e.Message);
            return null;
        }
    }
}