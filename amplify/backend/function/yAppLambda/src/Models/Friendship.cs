namespace yAppLambda.Models;

public class Friendship
{
    public string FromUserName { get; set; } // Partition Key

    public string ToUserName { get; set; } // Sort Key
    
    public Friendship Status { get; set; } // e.g. "Pending", "Accepted", "Declined"
    
    public DateTime CreatedAt { get; set; }
}