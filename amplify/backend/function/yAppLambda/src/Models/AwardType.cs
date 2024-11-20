using System.Text.Json.Serialization;

namespace yAppLambda.Models;

/// <summary>
/// Represents a type of award that can be earned by a user
/// </summary>
public class AwardType
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("tiers")]
    public List<AwardTier> Tiers { get; set; }
}