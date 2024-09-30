using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public class FriendshipActions: IFriendshipActions
{
    //This is the default table name for the friendship table, but we can try to use whatever one in the app settings first,
    // and if it doesn't exist, we can use this one.
    private const string FriendshipTableName = "Friendship-test";
    private readonly IAppSettings _appSettings;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly string _friendshipTable;
    private readonly DynamoDBOperationConfig _config;
    public FriendshipActions(IAppSettings appSettings,IDynamoDBContext dynamoDbContext)
    {
        _appSettings = appSettings;
        _dynamoDbContext = dynamoDbContext;

        _friendshipTable = string.IsNullOrEmpty(_appSettings.FriendshipTableName)
            ? FriendshipTableName
            : _appSettings.FriendshipTableName;
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _friendshipTable
        };
    }

    /// <summary>
    /// Creates a new friendship between two users.
    /// </summary>
    /// <param name="friendship">The friendship object containing details of the friendship.</param>
    /// <returns>An ActionResult containing the created Friendship object or an error status.</returns>
    public  async Task<ActionResult<Friendship>> CreateFriendship(Friendship friendship)
    {
        try
        {
            // update the current time
            friendship.CreatedAt = DateTime.Now;
            await _dynamoDbContext.SaveAsync(friendship, _config);

            return new OkObjectResult(friendship);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to create friendship: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// To accept/decline a friendship, we need to update the status of the friendship to "Accepted"/ "Declined"
    /// </summary>
    /// <param name="friendship">The friendship object containing updated details.</param>
    /// <returns>An ActionResult containing the updated Friendship object or an error status.</returns>
    public async Task<ActionResult<Friendship>> UpdateFriendshipStatus(Friendship friendship)
    {
        try
        {
            // if the friendship is accepted or declined, we need to update the updated time
            if (friendship.Status == FriendshipStatus.Accepted || friendship.Status == FriendshipStatus.Declined)
            {
                friendship.UpdatedAt = DateTime.Now;
                
            }else if (friendship.Status == FriendshipStatus.Pending)
            {
                // if the friendship is pending,
                // we need to update the created time since the request user would send a new request
                friendship.CreatedAt = DateTime.Now;
            }
          
            // update the friendship status
            await _dynamoDbContext.SaveAsync(friendship, _config);
            return new OkObjectResult(friendship);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to update friendship status: " + e.Message);
            return new StatusCodeResult(statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Retrieves all friendships of a user with a specified status.
    /// </summary>
    /// <param name="userName">The username of the user whose friendships are to be retrieved.</param>
    /// <param name="friendshipStatus">The status of the friendships to filter by.</param>
    /// <returns>A list of friendships matching the specified criteria.</returns>
    public async Task<List<Friendship>> GetAllFriends(string userName, FriendshipStatus friendshipStatus)
    {
        try
        {
            // Query friendships where the user is the `FromUserName` with the specified status
            var friendshipsFrom = await _dynamoDbContext.QueryAsync<Friendship>(userName, _config).GetRemainingAsync();

            var filteredFriendshipsFrom = friendshipsFrom.ToList();
            List<ScanCondition> scanConditions;

            if (friendshipStatus == FriendshipStatus.Accepted || friendshipStatus == FriendshipStatus.Pending ||
                friendshipStatus == FriendshipStatus.Declined)
            {
                filteredFriendshipsFrom = friendshipsFrom.Where(f => f.Status == friendshipStatus).ToList();
                // Use ScanCondition to scan friendships where the user is the `ToUserName` and the status is 1
                scanConditions = new List<ScanCondition>
                {
                    new ScanCondition("ToUserName", ScanOperator.Equal, userName),
                    new ScanCondition("Status", ScanOperator.Equal, friendshipStatus)
                };
            }
            else
            {
                // get all the friends of a user regardless of the status
                scanConditions = new List<ScanCondition>();
                scanConditions.Add(new ScanCondition("ToUserName", ScanOperator.Equal, userName));
            }

            var friendshipsTo = await _dynamoDbContext.ScanAsync<Friendship>(scanConditions, _config).GetRemainingAsync();

            // Combine the filtered results
            var allFilteredFriendships = filteredFriendshipsFrom.Concat(friendshipsTo).ToList();

            return allFilteredFriendships;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get friendships: " + e.Message);
            return new List<Friendship>();
        }
    }


    /// <summary>
    /// Checks if a friendship exists between two users.
    /// </summary>
    /// <param name="fromUserName">The username of the user who sent the friend request.</param>
    /// <param name="toUserName">The username of the user who received the friend request.</param>
    /// <returns>An ActionResult containing the Friendship object if it exists, or null if it does not.</returns>
    public async Task<ActionResult<Friendship>> GetFriendship(string fromUserName, string toUserName)
    {
        try
        {
            var friendship = await _dynamoDbContext.LoadAsync<Friendship>(fromUserName, toUserName, _config);
            return friendship;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get friendship: " + e.Message);
            return null;
        }
    }
}