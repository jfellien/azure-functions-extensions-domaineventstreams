using System.Text.Json.Serialization;

namespace devCrowd.CustomBindings.SampleFunctions.Models;

public class SampleEntity
{
    [JsonPropertyName("entityId")]
    public string EntityId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}