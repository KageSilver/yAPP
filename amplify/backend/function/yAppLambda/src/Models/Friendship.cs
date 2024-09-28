using yAppLambda.Enum;

namespace yAppLambda.Models;

/// <summary>
/// Represents a friendship between two users.
/// </summary>
public class Friendship
{
    public string FromUserName { get; set; } // Partition Key

    public string ToUserName { get; set; } // Sort Key
    
    public FriendshipStatus Status { get; set; } // e.g. "Pending", "Accepted", "Declined"
    
    public DateTime CreatedAt { get; set; }
}