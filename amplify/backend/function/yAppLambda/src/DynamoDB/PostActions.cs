using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Common;
using yAppLambda.Controllers;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class PostActions : IPostActions
{
    private const string PostTableName = "Post-test"; // This is the default table name for the post table
    private readonly string _postTable;
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDBOperationConfig _config;
    private readonly ICommentActions _commentActions;
    private readonly ICognitoActions _cognitoActions;
    private readonly IFriendshipActions _friendshipActions;

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
        
        _commentActions = new CommentActions(appSettings, dynamoDbContext);
        _friendshipActions = new FriendshipActions(appSettings, dynamoDbContext);
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
            // Update the current time
            var now = DateTime.Now;
            post.CreatedAt = now;
            post.UpdatedAt = now;
            
            // Gets a unique ID for the post
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
    public async Task<Post> GetPostById(string pid)
    {
        try
        {
            var post = await _dynamoDbContext.LoadAsync<Post>(pid, _config);
            return post;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get post: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets the user's public posts/diary entries
    /// </summary>
    /// <param name="uid">The author of the public posts/diary entries to be fetched.</param>
    /// <param name="diaryEntry">If the query is for public posts or diary entries.</param>
    /// <returns>A list of posts created by a user, either public posts or diary entries.</returns>
    public async Task<List<Post>> GetPostsByUser(string uid, bool diaryEntry)
    {
        try
        {
            // Scan for posts where the poster's uid is 'uid' and 'diaryEntry' is equal to the input
            List<ScanCondition> scanConditions = new List<ScanCondition>
            {
                new ScanCondition("UID", ScanOperator.Equal, uid),
                new ScanCondition("DiaryEntry", ScanOperator.Equal, diaryEntry)
            };
            
            var posts = await _dynamoDbContext.ScanAsync<Post>(scanConditions, _config).GetRemainingAsync();
            return posts;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get posts: " + e.Message);
            return new List<Post>();
        }
    }

    /// <summary>
    /// Gets the diary entry made by a user within a specific date range
    /// </summary>
    /// <param name="uid">The author of the diary entry.</param>
    /// <param name="startDate">The starting point of the date range to query.</param>
    /// <param name="endDate">The ending point of the date range to query.</param>
    /// <returns>The diary entry made by a user on the specified date range.</returns>
    public async Task<Post> GetDiariesByUser(string uid, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Query for diary entries made within start and end dates to narrow down posts to filter out 
            var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>
            {
                { ":start", startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                { ":end", endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            };

            var query = new QueryOperationConfig
            {
                IndexName = "CreatedAtIndex",  
                KeyExpression = new Expression
                {
                    ExpressionStatement = "CreatedAt BETWEEN :start AND :end",
                    ExpressionAttributeValues = expressionAttributeValues
                },
                AttributesToGet = new List<string>
                {
                    "UID", "DiaryEntry"
                },
                Select = SelectValues.SpecificAttributes  
            };

            var results = await _dynamoDbContext.FromQueryAsync<Post>(query, _config).GetNextSetAsync();

            // Filter results in memory to match the specific UID and DiaryEntry values
            foreach (var post in results)
            {
                if (post.UID == uid && post.DiaryEntry)
                {
                    return post;
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to retrive diary entry: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets the diary entries made by the user's friends within a specific date range
    /// </summary>
    /// <param name="uid">The user whose friends will be searched for.</param>
    /// <param name="startDate">The starting point of the date range to query.</param>
    /// <param name="endDate">The ending point of the date range to query.</param>
    /// <returns>A list of diary entries made by the user's friends on the specified date range.</returns>
    public async Task<List<Post>> GetDiariesByFriends(string uid, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Get all the user's friends
            var friends = _friendshipActions.GetAllFriends(uid, FriendshipStatus.Accepted).Result;
            
            // Get the posts written by the user's friends
            var filteredPosts = new List<Post>();
            foreach (Friendship friendship in friends)
            {
                var thisFriendUid = _cognitoActions.GetUser(friendship.ToUserName).Result.Id;
                var thisPost = GetDiariesByUser(thisFriendUid, startDate, endDate).Result;
                filteredPosts.Add(thisPost);
            }
            return filteredPosts;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to retrive diary entry: " + e.Message);
            return new List<Post>();
        }
    }
        
    /// <summary>
    /// Gets all recent posts
    /// </summary>
    /// <param name="since">Returns posts made after this time.</param>
    /// <param name="maxResults">The maximum number of results to retrieve.</param>
    /// <returns>A list of recent posts.</returns>
    public async Task<List<Post>> GetRecentPosts(DateTime since, int maxResults)
    {
        try
        {
            var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
            expressionAttributeValues.Add(":diaryEntry", false);
            
            var time = new TimeSpan(0, 0, 0, 1);
            since.Subtract(time);
            expressionAttributeValues.Add(":since", since.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            
            var query = new QueryOperationConfig()
            {
                IndexName = "CreatedAtIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "DiaryEntry = :diaryEntry AND CreatedAt < :since",
                    ExpressionAttributeValues = expressionAttributeValues
                },
                Limit = maxResults,
                AttributesToGet = new List<string>
                {
                    "PID"
                },
                Select = SelectValues.SpecificAttributes,
                BackwardSearch = true
            };

            var result = await _dynamoDbContext.FromQueryAsync<Post>(query, _config).GetNextSetAsync();
            
            var posts = new List<Post>();
            
            foreach(Post post in result)
            {
                var thisPost = GetPostById(post.PID).Result;
                posts.Add(thisPost);
            }

            return posts;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get recent posts: " + e.Message);
            return new List<Post>();
        }
    }
    
    /// <summary>
    /// Deletes a post from the database by a post id
    /// </summary>
    /// <param name="pid">The id of the post to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeletePost(string pid)
    {
        try
        {
            // Load the post record to check if it exists
            var post = GetPostById(pid);

            if (post.Result == null)
            {
                Console.WriteLine("Failed to retrieve post to be deleted");
                return false;
            }

            // Delete the post from the database
            await _commentActions.DeleteComments(pid);
            await _dynamoDbContext.DeleteAsync(post.Result, _config);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete post: " + e.Message);
            return false;
        }
    }
    
    /// <summary>
    /// Updates an already existing post
    /// </summary>
    /// <param name="updatedPost">The new version of the post after editing.</param>
    /// <returns>An ActionResult containing the edited Post object or an error status.</returns>
    public async Task<ActionResult<Post>> UpdatePost(Post updatedPost)
    {
        try
        {
            updatedPost.UpdatedAt = DateTime.Now; // Set updated time to current time
            await _dynamoDbContext.SaveAsync(updatedPost, _config);
            return new OkObjectResult(updatedPost);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to update post: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}