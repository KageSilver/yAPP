using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a tier of an award that can be earned by a user
/// </summary>
public class AwardTier
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("minimum")]
    public int Minimum { get; set; }

    [JsonPropertyName("tierNum")]
    public int TierNum { get; set; }
}