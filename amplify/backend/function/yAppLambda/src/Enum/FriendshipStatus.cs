namespace yAppLambda.Enum;

/// <summary>
/// Represents the status of a friendship.
/// </summary>
public enum FriendshipStatus
{
    All = -1, // used to get all the friends of a user regardless of the status
    Pending = 0, // friendship request is pending
    Accepted = 1, // friendship request is accepted
    Declined = 2 // friendship request is declined
}