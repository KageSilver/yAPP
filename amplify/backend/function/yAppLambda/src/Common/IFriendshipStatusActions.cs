using yAppLambda.Enum;

namespace yAppLambda.Common;

public interface IFriendshipStatusActions
{
    /// <summary>
    /// Retrieves the friendship status based on the provided status code.
    /// </summary>
    /// <param name="status">The status code representing the friendship status (-1: All, 0: Pending, 1: Accepted, 2: Declined).</param>
    /// <returns>The corresponding <see cref="FriendshipStatus"/>.</returns>
    FriendshipStatus GetFriendshipStatus(int status);
}