using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace APIAlerts;

/// <summary>
/// Global singleton facade. Call <see cref="Configure"/> once, then
/// <see cref="Send"/> / <see cref="SendAsync"/> anywhere. For DI, mocking, or
/// multiple keys, use <see cref="IApiAlertsClient"/> / <see cref="ApiAlertsClient"/>.
/// </summary>
public static class ApiAlerts
{
    private static IApiAlertsClient? _instance;

    /// <summary>Configure the singleton. First call wins; configure once at startup.</summary>
    /// <param name="apiKey">Workspace API key (Bearer token on every request).</param>
    /// <param name="debug">Log success / warning / non-critical errors via <paramref name="logger"/>.</param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    /// <param name="httpClient">Optional HttpClient; the SDK creates one with a 30s timeout if null.</param>
    public static void Configure(string apiKey, bool debug = false, ILogger? logger = null, HttpClient? httpClient = null) =>
        _instance ??= new ApiAlertsClient(apiKey, debug, logger, httpClient);

    /// <summary>Override the X-Integration / X-Version headers and base URL. For wrapper libraries to identify themselves. Call after <see cref="Configure"/>.</summary>
    public static void SetOverrides(string integration, string version, string baseUrl) =>
        _instance?.SetOverrides(integration, version, baseUrl);

    /// <summary>Fire-and-forget send. Never throws. <paramref name="apiKey"/> overrides the configured key for this call.</summary>
    /// <param name="evt">The event to deliver. Only <see cref="Event.Message"/> is required.</param>
    /// <param name="apiKey">Optional one-shot key override.</param>
    public static void Send(Event evt, string? apiKey = null)
    {
        if (_instance is null)
        {
            NullLogger.Instance.LogError("x (apialerts.com) Error: client not configured");
            return;
        }
        _instance.Send(evt, apiKey);
    }

    /// <summary>Awaitable send. Returns a <see cref="SendResult"/>; never throws (check <see cref="SendResult.Success"/>).</summary>
    /// <param name="evt">The event to deliver. Only <see cref="Event.Message"/> is required.</param>
    /// <param name="apiKey">Optional one-shot key override.</param>
    public static async Task<SendResult> SendAsync(Event evt, string? apiKey = null)
    {
        if (_instance is null)
            return new SendResult { Success = false, Error = "client not configured" };
        return await _instance.SendAsync(evt, apiKey).ConfigureAwait(false);
    }

    /// <summary>Resets the global client. For use in tests only.</summary>
    internal static void Reset() => _instance = null;
}
