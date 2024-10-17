using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a new comment being created in UI
/// </summary>
public class NewComment
{
    [JsonPropertyName("userName")]
    public string UserName { get; set ; }

    [JsonPropertyName("commentBody")]
    public string CommentBody { get; set; }
    
    [JsonPropertyName("pid")]
    public string PID { get; set; }
}