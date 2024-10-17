using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a comment created by a user
/// </summary>
public class Comment
{
    [JsonPropertyName("cid")]
    public string CID { get; set; } // Partition key

    [JsonPropertyName("pid")]
    public string PID { get; set; } // Sort key 1

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } // Sort key 2

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } // Sort key 2

    [JsonPropertyName("userName")]
    public string UserName { get; set; } // The username of the user who created the comment

    [JsonPropertyName("commentBody")]
    public string CommentBody {get; set; } // The contents of the comment

    [JsonPropertyName("upvotes")]
    public int Upvotes { get; set; } // The number of upvotes the comment has

    [JsonPropertyName("downvotes")]
    public int Downvotes { get; set; } // The number of downvotes the comment has
}