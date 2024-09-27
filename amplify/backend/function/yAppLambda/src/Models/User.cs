using System.Text.Json.Serialization;

namespace yAppLambda.Models;

public class User
{
    [JsonPropertyName("userName")]
    public string UserName { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    /// <summary>
    /// The attributes to edit/add
    /// </summary>
    public Dictionary<string,string> Attributes { get; set; }
}