using yAppLambda.Enum;

namespace yAppLambda.Common;

public class FriendshipStatusActions : IFriendshipStatusActions
{
    /// <summary>
    /// Converts an integer status code to the corresponding FriendshipStatus enum value.
    /// </summary>
    /// <param name="status">The integer status code (0: Pending, 1: Accepted, 2: Declined).</param>
    /// <returns>The corresponding FriendshipStatus enum value.</returns>
    public FriendshipStatus GetFriendshipStatus(int status)
    {
        return status switch
        {
            0 => FriendshipStatus.Pending,
            1 => FriendshipStatus.Accepted,
            2 => FriendshipStatus.Declined,
            _ => FriendshipStatus.All
        };
    }
}