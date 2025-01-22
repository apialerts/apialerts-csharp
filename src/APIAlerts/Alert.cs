namespace APIAlerts;

/// <summary>
/// Represents an event to be sent.
/// </summary>
public class Alert
{
    /// <summary>
    /// Message to send. Cannot be null or empty.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Channel to send the alert to. If null, the default channel is used.
    /// </summary>
    public string? Channel { get; init; }

    /// <summary>
    /// Link to include with the event. If null, no link is included.
    /// </summary>
    public string? Link { get; init; }

    /// <summary>
    /// Array of tags to include with the event. If null, no tags are included.
    /// </summary>
    public string[]? Tags { get; init; }
}
