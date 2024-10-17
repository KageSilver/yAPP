using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a new comment being created in UI
/// </summary>
public class NewComment
{
    [JsonPropertyName("userName")]
    public string UserName { get; set ; }

    [JsonPropertyName("postBody")]
    public string PostBody { get; set; }
}