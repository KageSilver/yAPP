using Microsoft.AspNetCore.Mvc;
using yAppLambda.Enum;
using yAppLambda.Models;

namespace yAppLambda.DynamoDB;

public interface IFriendshipActions
{
    /// <summary>
    /// Creates a new friendship.
    /// </summary>
    /// <param name="friendship">The friendship object to be created.</param>
    /// <returns>An ActionResult containing the created Friendship object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Friendship>> CreateFriendship(Friendship friendship);


    /// <summary>
    /// Updates the status of an existing friendship.
    /// </summary>
    /// <param name="friendship">The friendship object with the updated status.</param>
    /// <returns>An ActionResult containing the updated Friendship object if successful, or an error message if it fails.</returns>
    Task<ActionResult<Friendship>> UpdateFriendshipStatus(Friendship friendship);

    /// <summary>
    /// Retrieves a friendship between two users.
    /// </summary>
    /// <param name="fromUserName">The username of the user who sent the friend request.</param>
    /// <param name="toUserName">The username of the user who received the friend request.</param>
    /// <returns>An ActionResult containing the Friendship object if found, or an error message if it fails.</returns>
    Task<ActionResult<Friendship>> GetFriendship(string fromUserName, string toUserName);

    /// <summary>
    /// Retrieves all friends of a user with a specific friendship status.
    /// </summary>
    /// <param name="userName">The username of the user whose friends are to be retrieved.</param>
    /// <param name="friendshipStatus">The status of the friendships to be retrieved.</param>
    /// <returns>A Task containing a list of Friendship objects.</returns>
    Task<List<Friendship>> GetAllFriends(string userName, FriendshipStatus friendshipStatus);
}