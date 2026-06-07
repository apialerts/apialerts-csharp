using System.Text.Json.Serialization;

namespace APIAlerts;

/// <summary>
/// A single notification dispatched to API Alerts.
///
/// Only <see cref="Message"/> is required. All other fields are optional
/// and omitted from the JSON payload when null.
/// </summary>
public class Event
{
    /// <summary>
    /// Human-readable notification text. Required. This is what appears on
    /// the push notification lock screen.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Workspace channel the push notification fires on. Defaults to the
    /// workspace default channel when omitted.
    /// </summary>
    public string? Channel { get; init; }

    /// <summary>
    /// Identifies what kind of thing happened. Optional but recommended.
    /// Use dotted notation (e.g. <c>ci.deploy.success</c>, <c>payment.failed</c>,
    /// <c>user.signup</c>) so routing rules can match glob patterns like
    /// <c>ci.*</c> or <c>*.failed</c>.
    /// </summary>
    /// <remarks>
    /// Serialised as <c>event</c> over the wire. The property is named
    /// <c>EventKey</c> in C# because <c>event</c> is a reserved keyword.
    /// </remarks>
    [JsonPropertyName("event")]
    public string? EventKey { get; init; }

    /// <summary>Short headline some destinations render separately from the message body.</summary>
    public string? Title { get; init; }

    /// <summary>Categorisation tags for filtering and search.</summary>
    public string[]? Tags { get; init; }

    /// <summary>
    /// URL associated with the event. Available as a deeplink for push
    /// notifications and as a call-to-action for routed destinations.
    /// </summary>
    public string? Link { get; init; }

    /// <summary>
    /// Arbitrary key-value metadata. Available to non-push destinations for
    /// templating (Slack message bodies, email templates, webhook payloads).
    /// </summary>
    public object? Data { get; init; }
}
