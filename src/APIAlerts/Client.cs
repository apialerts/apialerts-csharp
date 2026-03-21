namespace APIAlerts;

/// <summary>
/// Global singleton facade for the API Alerts client.
/// Call <see cref="Configure"/> once at startup, then use
/// <see cref="Send"/> or <see cref="SendAsync"/> anywhere in your app.
/// </summary>
public static class Client
{
    private static ApiAlertsClient? _instance;

    /// <summary>Initialise the global client. Subsequent calls are no-ops.</summary>
    public static void Configure(string apiKey, bool debug = false, HttpClient? httpClient = null) =>
        _instance ??= new ApiAlertsClient(apiKey, debug, httpClient);

    /// <summary>
    /// Override the integration name, version, and base URL on the global client.
    /// No-op if <see cref="Configure"/> has not been called yet.
    /// </summary>
    public static void SetOverrides(string integration, string version, string baseUrl) =>
        _instance?.SetOverrides(integration, version, baseUrl);

    /// <summary>
    /// Send an event — fire-and-forget. Logs to Console.Error and returns if not configured.
    /// Never throws.
    /// </summary>
    public static async Task Send(Event evt)
    {
        if (_instance is null)
        {
            await Console.Error.WriteLineAsync("x (apialerts.com) Error: client not configured");
            return;
        }
        await _instance.Send(evt);
    }

    /// <summary>
    /// Send an event and return a <see cref="SendResult"/>.
    /// Returns a failure result if not configured.
    /// Never throws.
    /// </summary>
    public static async Task<SendResult> SendAsync(Event evt)
    {
        if (_instance is null)
            return new SendResult { Success = false, Error = "client not configured" };
        return await _instance.SendAsync(evt);
    }

    /// <summary>
    /// Send an event using an explicit API key and return a <see cref="SendResult"/>.
    /// Returns a failure result if not configured.
    /// Never throws.
    /// </summary>
    public static async Task<SendResult> SendWithKey(string apiKey, Event evt)
    {
        if (_instance is null)
            return new SendResult { Success = false, Error = "client not configured" };
        return await _instance.SendWithKey(apiKey, evt);
    }

    /// <summary>Resets the global client. For use in tests only.</summary>
    internal static void Reset() => _instance = null;
}
