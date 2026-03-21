namespace APIAlerts;

/// <summary>
/// The result of a <see cref="ApiAlertsClient.SendAsync"/> call.
/// Check <see cref="Success"/> before reading other fields.
/// </summary>
public class SendResult
{
    public bool Success { get; init; }
    public string? Workspace { get; init; }
    public string? Channel { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public string? Error { get; init; }
}
