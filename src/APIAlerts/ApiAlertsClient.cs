using APIAlerts.network;

namespace APIAlerts;

/// <summary>
/// Instance-based API Alerts client.
/// Use <see cref="Client"/> for a convenient global singleton, or construct
/// this class directly when you need multiple clients or full lifecycle control.
/// </summary>
public class ApiAlertsClient
{
    private readonly string _apiKey;
    private readonly bool _debug;
    private readonly Network _network;

    public ApiAlertsClient(string apiKey, bool debug = false, HttpClient? httpClient = null)
    {
        _apiKey  = apiKey;
        _debug   = debug;
        _network = new Network(httpClient);
    }

    /// <summary>
    /// Override the integration name, version, and base URL.
    /// Used by official integrations and in tests to redirect requests to a mock server.
    /// </summary>
    public void SetOverrides(string integration, string version, string baseUrl) =>
        _network.SetOverrides(integration, version, baseUrl);

    /// <summary>
    /// Send an event — fire-and-forget (async Task).
    /// Critical errors are always logged to Console.Error regardless of debug setting.
    /// HTTP errors and success are only logged when debug is enabled.
    /// Never throws.
    /// </summary>
    public async Task Send(Event evt)
    {
        // Critical errors — always log
        if (string.IsNullOrEmpty(_apiKey))
        {
            await Console.Error.WriteLineAsync("x (apialerts.com) Error: api key is missing");
            return;
        }
        if (string.IsNullOrEmpty(evt.Message))
        {
            await Console.Error.WriteLineAsync("x (apialerts.com) Error: message is required");
            return;
        }

        var result = await SendAsync(evt);

        if (!_debug) return;

        if (!result.Success)
        {
            await Console.Error.WriteLineAsync($"x (apialerts.com) Error: {result.Error}");
        }
        else
        {
            await Console.Error.WriteLineAsync($"✓ (apialerts.com) Alert sent to {result.Workspace} ({result.Channel})");
            foreach (var warning in result.Warnings)
                await Console.Error.WriteLineAsync($"! (apialerts.com) Warning: {warning}");
        }
    }

    /// <summary>
    /// Send an event and return a <see cref="SendResult"/>.
    /// Never throws — check <see cref="SendResult.Success"/> instead.
    /// </summary>
    public async Task<SendResult> SendAsync(Event evt)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return new SendResult { Success = false, Error = "api key is missing" };
        if (string.IsNullOrEmpty(evt.Message))
            return new SendResult { Success = false, Error = "message is required" };

        return await _network.PostEvent(_apiKey, evt);
    }

    /// <summary>Send an event using an explicit API key, bypassing the configured one.</summary>
    public async Task<SendResult> SendWithKey(string apiKey, Event evt)
    {
        if (string.IsNullOrEmpty(apiKey))
            return new SendResult { Success = false, Error = "api key is missing" };
        if (string.IsNullOrEmpty(evt.Message))
            return new SendResult { Success = false, Error = "message is required" };

        return await _network.PostEvent(apiKey, evt);
    }
}
