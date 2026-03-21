using System.Text.Json.Serialization;

namespace APIAlerts.network.contract;

internal class EventResponse
{
    [JsonPropertyName("workspace")]
    public string Workspace { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("warnings")]
    public List<string>? Warnings { get; set; }
}
