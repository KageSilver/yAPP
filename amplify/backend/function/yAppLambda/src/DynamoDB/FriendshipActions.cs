using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public static class FriendshipActions
{
    //This is the default table name for the friendship table, but we can try to use whatever one in the app settings first,
    // and if it doesn't exist, we can use this one.
    private const string FriendshipTableName = "Friendship-test";

    /// <summary>
    /// Creates a new friendship between two users.
    /// </summary>
    /// <param name="friendship">The friendship object containing details of the friendship.</param>
    /// <param name="dynamoDbContext">The DynamoDB context to interact with the database.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>An ActionResult containing the created Friendship object or an error status.</returns>
    public static async Task<ActionResult<Friendship>> CreateFriendship(Friendship friendship,
        IDynamoDBContext dynamoDbContext, IAppSettings appSettings)
    {
        try
        {
            var friendshipTable = string.IsNullOrEmpty(appSettings.FriendshipTableName)
                ? FriendshipTableName
                : appSettings.FriendshipTableName;
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = friendshipTable
            };

            // update the current time
            friendship.CreatedAt = DateTime.Now;
            await dynamoDbContext.SaveAsync(friendship, config);

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
    /// <param name="dynamoDbContext">The DynamoDB context to interact with the database.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>An ActionResult containing the updated Friendship object or an error status.</returns>
    public static async Task<ActionResult<Friendship>> UpdateFriendshipStatus(Friendship friendship,
        IDynamoDBContext dynamoDbContext, IAppSettings appSettings)
    {
        try
        {
            var friendshipTable = string.IsNullOrEmpty(appSettings.FriendshipTableName)
                ? FriendshipTableName
                : appSettings.FriendshipTableName;
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = friendshipTable
            };
            friendship.UpdatedAt = DateTime.Now;
            await dynamoDbContext.SaveAsync(friendship, config);
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
    /// <param name="dynamoDbContext">The DynamoDB context to interact with the database.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>A list of friendships matching the specified criteria.</returns>
    public static async Task<List<Friendship>> GetAllFriends(string userName, FriendshipStatus friendshipStatus,
        IDynamoDBContext dynamoDbContext, IAppSettings appSettings)
    {
        try
        {
            var friendshipTable = string.IsNullOrEmpty(appSettings.FriendshipTableName)
                ? FriendshipTableName
                : appSettings.FriendshipTableName;
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = friendshipTable
            };

            // Query friendships where the user is the `FromUserName` with the specified status
            var friendshipsFrom = await dynamoDbContext.QueryAsync<Friendship>(userName, config).GetRemainingAsync();

            var filteredFriendshipsFrom = new List<Friendship>();
            List<ScanCondition> scanConditions;

            if (friendshipStatus == FriendshipStatus.Accepted || friendshipStatus == FriendshipStatus.Pending ||
                friendshipStatus != FriendshipStatus.Declined)
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

            var friendshipsTo = await dynamoDbContext.ScanAsync<Friendship>(scanConditions, config).GetRemainingAsync();

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
    /// <param name="dynamoDbContext">The DynamoDB context to interact with the database.</param>
    /// <param name="appSettings">The application settings containing configuration values.</param>
    /// <returns>An ActionResult containing the Friendship object if it exists, or null if it does not.</returns>
    public static async Task<ActionResult<Friendship>> GetFriendship(string fromUserName, string toUserName,
        IDynamoDBContext dynamoDbContext, IAppSettings appSettings)
    {
        try
        {
            var friendshipTable = string.IsNullOrEmpty(appSettings.FriendshipTableName)
                ? FriendshipTableName
                : appSettings.FriendshipTableName;
            var config = new DynamoDBOperationConfig
            {
                OverrideTableName = friendshipTable
            };

            var friendship = await dynamoDbContext.LoadAsync<Friendship>(fromUserName, toUserName, config);
            return friendship;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to get friendship: " + e.Message);
            return null;
        }
    }
}