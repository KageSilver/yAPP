using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a post created by a user
/// </summary>
public class Post
{
    [JsonPropertyName("pid")]
    public string PID { get; set; } // Partition key

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } // Sort key

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } // The time the post was updated

    [JsonPropertyName("uid")]
    public string UID { get; set; } // The uid of the user who created the post

    [JsonPropertyName("postTitle")]
    public string PostTitle { get; set; } // The title of the post

    [JsonPropertyName("postBody")]
    public string PostBody {get; set; } // The contents of the post

    [JsonPropertyName("upvotes")]
    public int Upvotes { get; set; } // The number of upvotes the post has

    [JsonPropertyName("downvotes")]
    public int Downvotes { get; set; } // The number of downvotes the post has

    [JsonPropertyName("diaryEntry")]
    public bool DiaryEntry { get; set; } // Is the post a diary entry

    [JsonPropertyName("anonymous")]
    public bool Anonymous { get; set; } // If the post is a diary entry, is it posted anonymously
}