using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a friend request with the sender's username and the recipient's user ID.
/// </summary>
public class FriendRequest
{
    [JsonPropertyName("fromUserName")]
    public string FromUserName { get; set; } 
    
    [JsonPropertyName("toUserId")]
    public string ToUserId { get; set; } 
    
    [JsonPropertyName("toUserName")]
    public string ToUserName { get; set; }
    
    [JsonPropertyName("status")]
    public int Status { get; set; }
}