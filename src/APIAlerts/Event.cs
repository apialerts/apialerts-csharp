namespace APIAlerts;

/// <summary>
/// An event to send to the API Alerts platform.
/// Only <see cref="Message"/> is required. All other fields are optional
/// and omitted from the JSON payload when null.
/// </summary>
public class Event
{
    /// <summary>Required. The notification message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional. The channel to deliver to. Uses the default channel if null.</summary>
    public string? Channel { get; init; }

    /// <summary>Optional. An event key for routing rules (e.g. "ci.deploy").</summary>
    public string? EventKey { get; init; }

    /// <summary>Optional. A short title displayed above the message.</summary>
    public string? Title { get; init; }

    /// <summary>Optional. Tags to attach to the event.</summary>
    public string[]? Tags { get; init; }

    /// <summary>Optional. A URL to include with the event.</summary>
    public string? Link { get; init; }

    /// <summary>Optional. Arbitrary structured data attached to the event.</summary>
    public object? Data { get; init; }
}
