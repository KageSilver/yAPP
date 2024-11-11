using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a vote for a user on a post/comment
/// </summary>
public class Vote
{
    [JsonPropertyName("id")]
    public string ID { get; set; } // Partition key (id of the post/comment)

    [JsonPropertyName("isPost")]
    public bool IsPost { get; set; } // Is it a post (true) or a comment (false)

    [JsonPropertyName("type")]
    public bool Type { get; set; } // Is it an upvote (true) or a downvote (false)

    [JsonPropertyName("uid")]
    public string UID { get; set; } // The UID of the user who voted
}