using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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
            // Set the current time (to see if the comment has been recently updated, check if createdAt != updatedAt)
            comment.CreatedAt = DateTime.Now;
            comment.UpdatedAt = comment.CreatedAt;
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
    /// Gets the comment given comment ID
    /// </summary>
    /// <param name="cid">The id to find a comment given a comment id.</param>
    /// <returns>The comment associated to the cid.</returns>
    public async Task<Comment> GetCommentById(string cid)
    {
        try
        {
            var comment = await _dynamoDbContext.LoadAsync<Comment>(cid, _config);
            
            return comment;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get comment: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets all comments with given UID
    /// </summary>
    /// <param name="uid">The UID to find all comments a user has made.</param>
    /// <returns>A list of comments made by a user.</returns>
    public async Task<List<Comment>> GetCommentsByUid(string uid)
    {
        try
        {
            var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
            expressionAttributeValues.Add(":uid", uid);
            // Query comments where their uid is 'uid' and sorted by date created
            var query = new QueryOperationConfig()
            {
                IndexName = "CreatedAtIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "UID = :uid",
                    ExpressionAttributeValues = expressionAttributeValues
                },
                AttributesToGet = new List<string>
                {
                    "CID"
                },
                Select = SelectValues.SpecificAttributes,
                BackwardSearch = true
            };

            var result = await _dynamoDbContext.FromQueryAsync<Comment>(query, _config).GetNextSetAsync();
            
            var comments = new List<Comment>();
            
            foreach(Comment comment in result)
            {
                var thisComment = GetCommentById(comment.CID).Result;
                comments.Add(thisComment);
            }

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
            var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
            expressionAttributeValues.Add(":pid", pid);
            // Query comments where their pid is 'pid' and sorted by date created
            var query = new QueryOperationConfig()
            {
                IndexName = "CreatedAtIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PID = :pid",
                    ExpressionAttributeValues = expressionAttributeValues
                },
                AttributesToGet = new List<string>
                {
                    "CID"
                },
                Select = SelectValues.SpecificAttributes,
                BackwardSearch = true
            };

            var result = await _dynamoDbContext.FromQueryAsync<Comment>(query, _config).GetNextSetAsync();
            
            var comments = new List<Comment>();
            
            foreach(Comment comment in result)
            {
                var thisComment = GetCommentById(comment.CID).Result;
                comments.Add(thisComment);
            }

            return comments;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get comments: " + e.Message);
            return new List<Comment>();
        }
    }
    
}