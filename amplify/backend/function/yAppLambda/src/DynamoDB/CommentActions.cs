using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class CommentActions : ICommentActions
{
    // Default table name for comment table
    private const string CommentTableName = "Comment-test";
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly string _commentTable;
    private readonly DynamoDBOperationConfig _config;

    public CommentActions(IAppSettings appSettings, IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;

        _commentTable = string.IsNullOrEmpty(_appSettings.CommentTableName)
            ? CommentTableName
            : _appSettings.CommentTableName;
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _commentTable
        };
    }
    
    /// <summary>
    /// Creates a comment
    /// </summary>
    /// <param name="comment">The comment object that contains information on the comment.</param>
    /// <returns>An ActionResult containing the created Comment object or an error status.</returns>
    public async Task<ActionResult<Comment>> CreateComment(Comment comment)
    {
        try
        {
            // Set the current time
            comment.CreatedAt = DateTime.Now;
            // Sets a unique ID for the comment
            comment.CID = Guid.NewGuid().ToString();

            await _dynamoDbContext.SaveAsync(comment, _config);

            return new OkObjectResult(comment);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create comment: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets the post given comment ID
    /// </summary>
    /// <param name="cid">The id to find a post given a comment id.</param>
    /// <returns>The post associated to the comment.</returns>
    public async Task<Post> GetPostByCid(string cid)
    {
        try
        {
            var post = await _dynamoDbContext.LoadAsync<Post>(cid, _config);

            if(post.Anonymous)
            {
                post.UserName = "Anonymous";
            }
            
            return post;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get post: " + e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Gets the comment given comment ID
    /// </summary>
    /// <param name="cid">The id to find a comment given a comment id.</param>
    /// <returns>The comment associated to the cid.</returns>
    public async Task<Comment> GetCommentById(string cid)
    {
        try
        {
            var comment = await _dynamoDbContext.LoadAsync<Comment>(cid, _config);
            comment.UserName = "Anonymous"; // Setting user name to anonymous
            
            return comment;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get comment: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets all comments with given username
    /// </summary>
    /// <param name="userName">The username to find all comments a user has made.</param>
    /// <returns>A list of comments made by a user.</returns>
    public async Task<List<Comment>> GetCommentsByUser(string userName)
    {
        try
        {
            List<ScanCondition> scanConditions = new List<ScanCondition>
            {
                new ScanCondition("UserName", ScanOperator.Equal, userName)
            };

            // Query comments where the commenter's username is 'userName' and is equal to the input
            var comments = await _dynamoDbContext.ScanAsync<Comment>(scanConditions, _config).GetRemainingAsync();

            return comments;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get comments: " + e.Message);
            return new List<Comment>();
        }
    }
    
    /// <summary>
    /// Gets all comments with given post ID
    /// </summary>
    /// <param name="pid">The id to find a post and comment thread.</param>
    /// <returns>A list of comments made under a post.</returns>
    public async Task<List<Comment>> GetCommentsByPid(string pid)
    {
        try
        {
            List<ScanCondition> scanCondition = new List<ScanCondition>
            {
                new ScanCondition("pid", ScanOperator.Equal, pid)
            };

            // Query comments where their pid is 'pid'
            var comments = await _dynamoDbContext.ScanAsync<Comment>(scanCondition, _config).GetRemainingAsync();

            return comments;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get comments: " + e.Message);
            return new List<Comment>();
        }
    }
    
    /// <summary>
    /// Deletes a comment from the database by a comment id
    /// </summary>
    /// <param name="cid">The id of the comment to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteComment(string cid)
    {
        bool result = true;
        try
        {
            // Load the comment record to check if it exists
            var comment = GetCommentById(cid);

            if (comment.Result == null)
            {
                Console.WriteLine("Failed to retrieve comment");
                result = false;
            }
            else
            {
                // Delete the comment from the database
                await _dynamoDbContext.DeleteAsync(comment.Result, _config);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete comment: " + e.Message);
            result = false;
        }
        return result;
    }
    
    /// <summary>
    /// Edits an already existing comment
    /// </summary>
    /// <param name="updatedComment">The new version of the comment after editing.</param>
    /// <returns>An ActionResult containing the edited Comment object if successful, or an error message if it fails.</returns>
    public async Task<ActionResult<Comment>> UpdateComment(Comment updatedComment)
    {
        try
        {
            await _dynamoDbContext.SaveAsync(updatedComment, _config);
            return new OkObjectResult(updatedComment);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to update comment: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}