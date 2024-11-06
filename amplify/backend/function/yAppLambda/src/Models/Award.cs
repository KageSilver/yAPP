using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents an award earned by a user
/// </summary>
public class Award
{
    [JsonPropertyName("aid")]
    public string AID { get; set; }

    [JsonPropertyName("pid")]
    public string PID { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}