namespace APIAlerts;

/// <summary>
/// The outcome of an <see cref="ApiAlerts.SendAsync"/> call.
/// <see cref="SendAsync"/> never throws - inspect <see cref="Success"/> to
/// branch on delivery, then read <see cref="Workspace"/> / <see cref="Channel"/>
/// on success or <see cref="Error"/> on failure.
/// </summary>
public class SendResult
{
    /// <summary><c>true</c> if the event was delivered to API Alerts.</summary>
    public bool Success { get; init; }

    /// <summary>Workspace name returned by the server. Populated on success.</summary>
    public string? Workspace { get; init; }

    /// <summary>Channel the event landed on. Populated on success.</summary>
    public string? Channel { get; init; }

    /// <summary>Non-fatal warnings returned by the server (e.g. deprecated field usage). Empty when there are none.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>Human-readable error message. Populated on failure.</summary>
    public string? Error { get; init; }
}
