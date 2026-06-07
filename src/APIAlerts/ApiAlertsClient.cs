using APIAlerts.network;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace APIAlerts;

/// <summary>
/// Instance-based client. Construct directly or register as
/// <see cref="IApiAlertsClient"/> in a DI container. <see cref="ApiAlerts"/> is
/// the static singleton facade over a default instance.
/// </summary>
public class ApiAlertsClient : IApiAlertsClient
{
    private readonly string _apiKey;
    private readonly bool _debug;
    private readonly ILogger _logger;
    private readonly Network _network;

    /// <param name="apiKey">Workspace API key (Bearer token on every request).</param>
    /// <param name="debug">Log success / warning / non-critical errors via <paramref name="logger"/>.</param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    /// <param name="httpClient">Optional HttpClient; the SDK creates one with a 30s timeout if null.</param>
    public ApiAlertsClient(string apiKey, bool debug = false, ILogger? logger = null, HttpClient? httpClient = null)
    {
        _apiKey = apiKey;
        _debug = debug;
        _logger = logger ?? NullLogger.Instance;
        _network = new Network(httpClient);
    }

    public void SetOverrides(string integration, string version, string baseUrl) =>
        _network.SetOverrides(integration, version, baseUrl);

    public void Send(Event evt, string? apiKey = null)
    {
        var key = !string.IsNullOrEmpty(apiKey) ? apiKey : _apiKey;

        if (string.IsNullOrEmpty(key))
        {
            _logger.LogError("x (apialerts.com) Error: api key is missing");
            return;
        }
        if (string.IsNullOrEmpty(evt.Message))
        {
            _logger.LogError("x (apialerts.com) Error: message is required");
            return;
        }

        _ = Task.Run(async () =>
        {
            var result = await _network.PostEvent(key!, evt).ConfigureAwait(false);
            if (_debug) LogResult(result);
        });
    }

    public async Task<SendResult> SendAsync(Event evt, string? apiKey = null)
    {
        var key = !string.IsNullOrEmpty(apiKey) ? apiKey : _apiKey;

        if (string.IsNullOrEmpty(key))
            return new SendResult { Success = false, Error = "api key is missing" };
        if (string.IsNullOrEmpty(evt.Message))
            return new SendResult { Success = false, Error = "message is required" };

        var result = await _network.PostEvent(key!, evt).ConfigureAwait(false);
        if (_debug) LogResult(result);
        return result;
    }

    private void LogResult(SendResult result)
    {
        if (result.Success)
        {
            _logger.LogInformation("✓ (apialerts.com) Alert sent to {Workspace} ({Channel})", result.Workspace, result.Channel);
            foreach (var warning in result.Warnings)
                _logger.LogWarning("! (apialerts.com) Warning: {Warning}", warning);
        }
        else
        {
            _logger.LogError("x (apialerts.com) Error: {Error}", result.Error);
        }
    }
}
