namespace yAppLambda.Models;

/// <summary>
/// Represents a new post being created
/// </summary>
public class NewPost
{
    [JsonPropertyName("userName")]
    public string UserName { get; set ; }

    [JsonPropertyName("postTitle")]
    public string PostTitle { get; set; }

    [JsonPropertyName("postBody")]
    public string postBody { get; set; }

    [JsonPropertyName("diaryEntry")]
    public bool DiaryEntry { get; set; }

    [JsonPropertyName("anonymous")]
    public bool Anonymous { get; set; }
}