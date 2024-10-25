using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a new post being created
/// </summary>
public class NewPost
{
    [JsonPropertyName("uid")]
    public string UID { get; set ; }

    [JsonPropertyName("postTitle")]
    public string PostTitle { get; set; }

    [JsonPropertyName("postBody")]
    public string PostBody { get; set; }

    [JsonPropertyName("diaryEntry")]
    public bool DiaryEntry { get; set; }

    [JsonPropertyName("anonymous")]
    public bool Anonymous { get; set; }
}