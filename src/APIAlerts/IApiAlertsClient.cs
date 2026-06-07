namespace APIAlerts;

/// <summary>
/// Instance-based client. Register it as a singleton in your DI container when
/// you want lifecycle control, test mocking, or multiple keys. <see cref="ApiAlerts"/>
/// is a static facade over a default instance.
/// </summary>
public interface IApiAlertsClient
{
    /// <summary>Override the X-Integration / X-Version headers and base URL. Internal use.</summary>
    void SetOverrides(string integration, string version, string baseUrl);

    /// <summary>Fire-and-forget send. Never throws. <paramref name="apiKey"/> overrides the configured key for this call.</summary>
    void Send(Event evt, string? apiKey = null);

    /// <summary>Awaitable send. Returns a <see cref="SendResult"/>; never throws.</summary>
    Task<SendResult> SendAsync(Event evt, string? apiKey = null);
}
