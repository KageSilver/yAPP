using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class AwardActions : IAwardActions
{
    // This is the default table name for the award table
    private const string AwardTableName = "Award-test"; 
    private readonly string _awardTable;
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDBOperationConfig _config;
    private readonly ICommentActions _commentActions;
    private readonly IPostActions _postActions;
    private readonly IFriendshipActions _friendshipActions;
    private readonly List<AwardType> awardTypes;

    public AwardActions(IAppSettings appSettings, IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;
        
        _awardTable = string.IsNullOrEmpty(_appSettings.AwardTableName)
            ? AwardTableName
            : _appSettings.AwardTableName;
        
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _awardTable
        };

        awardTypes = JsonConvert.DeserializeObject<List<AwardType>>(File.ReadAllText(@"awards.json"));

        _commentActions = new CommentActions(appSettings, dynamoDbContext);
        _postActions = new PostActions(appSettings, dynamoDbContext);
        _friendshipActions = new FriendshipActions(appSettings, dynamoDbContext);
    }

    /// <summary>
    /// Creates a new award
    /// </summary>
    /// <param name="award">The award object to be created.</param>
    /// <returns>An ActionResult containing the created award object if successful, or an error message if it fails.</returns>
    public async Task<ActionResult<Award>> CreateAward(Award award)
    {
        try
        {
            award.CreatedAt = DateTime.Now;
            
            // Gets a unique ID for the award
            award.AID = Guid.NewGuid().ToString();

            await _dynamoDbContext.SaveAsync(award, _config);
            return new OkObjectResult(award);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create award: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }
    
    /// <summary>
    /// Gets an award by the award ID
    /// </summary>
    /// <param name="aid">The id to find an award.</param>
    /// <returns>An ActionResult containing the Award object if found, or a NotFound result otherwise.</returns>
    public async Task<Award> GetAwardById(string aid)
    {
        try
        {
            var award = await _dynamoDbContext.LoadAsync<Award>(aid, _config);
            return award;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get award: " + e.Message);
            return null;
        }
    }
    
    /// <summary>
    /// Gets all awards from a user
    /// </summary>
    /// <param name="uid">The user who earned the awards being fetched.</param>
    /// <returns>A list of awards earned by the user.</returns>
    public async Task<List<Award>> GetAwardsByUser(string uid)
    {
        try
        {
            // Scan for awards where the award earner's uid is 'uid'
            List<ScanCondition> scanConditions = new List<ScanCondition>
            {
                new ScanCondition("UID", ScanOperator.Equal, uid)
            };
            
            var awards = await _dynamoDbContext.ScanAsync<Award>(scanConditions, _config).GetRemainingAsync();
            return awards;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get awards: " + e.Message);
            return new List<Award>();
        }
    }
    
    /// <summary>
    /// Gets all awards from a post
    /// </summary>
    /// <param name="pid">The post on which the awards were earned.</param>
    /// <returns>A list of awards earned on the post.</returns>
    public async Task<List<Award>> GetAwardsByPost(string pid)
    {
        try
        {
            var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
            expressionAttributeValues.Add(":pid", pid);

            var query = new QueryOperationConfig()
            {
                IndexName = "PIDIndex",
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PID = :pid",
                    ExpressionAttributeValues = expressionAttributeValues
                },
                AttributesToGet = new List<string>
                {
                    "AID"
                },
                Select = SelectValues.SpecificAttributes,
                BackwardSearch = true
            };

            var result = await _dynamoDbContext.FromQueryAsync<Award>(query, _config).GetNextSetAsync();
            
            var awards = new List<Award>();
            
            foreach(Award award in result)
            {
                var thisAward = GetAwardById(award.AID).Result;
                awards.Add(thisAward);
            }

            return awards;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get awards: " + e.Message);
            return new List<Award>();
        }
    }
    
    /// <summary>
    /// Deletes all awards earned on a specific post
    /// </summary>
    /// <param name="pid">The post on which the awards were earned to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteAwardsByPost(string pid)
    {
        try
        {
            var awards = await GetAwardsByPost(pid);

            if(awards.Count > 0)
            {
                foreach(Award award in awards)
                {
                    if(!await DeleteAward(award.AID))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete awards: " + e.Message);
            return false;
        }
    }
        
    /// <summary>
    /// Deletes an award from the database by an award id
    /// </summary>
    /// <param name="aid">The id of the award to be deleted.</param>
    /// <returns>A boolean indicating whether the deletion was successful.</returns>
    public async Task<bool> DeleteAward(string aid)
    {
        try
        {
            // Load the award record to check if it exists
            var award = GetAwardById(aid);

            if (award.Result == null)
            {
                Console.WriteLine("Failed to retrieve award to be deleted");
                return false;
            }

            // Delete the award from the database
            await _dynamoDbContext.DeleteAsync(award.Result, _config);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to delete award: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets new awards a user has earned
    /// </summary>
    /// <param name="uid">The user who earned the awards being fetched.</param>
    /// <returns>A list of new awards earned by the user.</returns>
    public async Task<List<Award>> GetNewAwardsByUser(string uid)
    {
        var newAwards = new List<Award>();
        var userPosts = await _postActions.GetPostsByUser(uid);
        
        newAwards.AddRange(await CheckNewAwardsPerPost(userPosts));
        newAwards.AddRange(await CheckNewAwardsTotalPosts(userPosts, uid));
        
        return newAwards;
    }

    /// <summary>
    /// Checks a list of posts for awards
    /// </summary>
    /// <param name="posts">The list of posts to check for awards.</param>
    /// <returns>A list of awards earned on the list of posts.</returns>
    public async Task<List<Award>> CheckNewAwardsPerPost(List<Post> posts)
    {
        var list = new List<Award>();
        
        foreach(Post post in posts)
        {
            try
            {
                var awards = await GetAwardsByPost(post.PID);

                var comments = await _commentActions.GetCommentsByPid(post.PID);
                list.AddRange((await CheckAwardType(post, awards, awardTypes.Where(a => a.Type.Equals("comment")).First(), comments.Count)));
                
                list.AddRange((await CheckAwardType(post, awards, awardTypes.Where(a => a.Type.Equals("upvote")).First(), post.Upvotes)));
                
                list.AddRange((await CheckAwardType(post, awards, awardTypes.Where(a => a.Type.Equals("downvote")).First(), post.Downvotes)));
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to check post for awards: " + e.Message);
            }
        }

        return list;
    }

    /// <summary>
    /// Checks number of posts a user has made for awards
    /// </summary>
    /// <param name="posts">The list of posts to check for awards.</param>
    /// <returns>A list of awards earned on the number of posts made.</returns>
    public async Task<List<Award>> CheckNewAwardsTotalPosts(List<Post> posts, string uid)
    {
        var post = new Post();
        post.PID = "NA";
        post.UID = uid;
        var awards = await GetAwardsByPost(post.PID);
        
        return await CheckAwardType(post, awards, awardTypes.Where(a => a.Type.Equals("posts")).First(), posts.Count());
    }

    /// <summary>
    /// Checks a post for a type of award
    /// </summary>
    /// <param name="post">The post to be checked for awards.</param>
    /// <param name="awards">The existing awards on the post.</param>
    /// <param name="type">The type award to check for.</param>
    /// <param name="count">The amount of an award type to compare against the tiers.</param>
    /// <returns>A list of awards earned on a post.</returns>
    private async Task<List<Award>> CheckAwardType(Post post, List<Award> awards, AwardType type, int count)
    {
        var list = new List<Award>();
        var theseAwards = awards.Where(a => a.Type.Equals(type.Type));
        
        foreach (AwardTier tier in type.Tiers)
        {
            // check if user has already received this tier of the award on this post
            bool doesntExist = theseAwards.Count() == 0 || theseAwards.Where(c => c.Tier == tier.TierNum) == null;

            if (doesntExist && count >= tier.Minimum)
            {
                var award = new Award
                {
                    PID = post.PID,
                    UID = post.UID,
                    Name = tier.Name,
                    Type = type.Type,
                    Tier = tier.TierNum
                };

                list.Add((await CreateAward(award)).Value);
            }
        }

        return list;
    }
}