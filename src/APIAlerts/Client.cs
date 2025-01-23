using APIAlerts.network;
using APIAlerts.util;

namespace APIAlerts;

internal interface IClient
{
    void Configure(string apiKey, bool logging = true);
    Task SendAsync(string? apiKey, Alert model);
}

internal class Client : IClient
{
    private readonly Endpoints _endpoints;
    private string? _defaultApiKey;
    private readonly Logger _logger = new();

    internal Client(HttpClient? httpClient = null)
    {
        _endpoints = new Endpoints(httpClient);
    }

    public void Configure(string apiKey, bool logging)
    {
        _defaultApiKey = apiKey;
        _logger.Configure(logging);
    }

    public async Task SendAsync(string? apiKey, Alert model)
    {
        var useKey = apiKey ?? _defaultApiKey;

        if (string.IsNullOrEmpty(useKey))
        {
            _logger.Error("API Key not provided. Use Configure() to set a default key, or pass the key as a parameter to the SendWithKey/SendWithKeySendAsync function.");
            return;
        }

        if (string.IsNullOrWhiteSpace(model.Message))
        {
            _logger.Error("Message is required");
            return;
        }

        var result = await _endpoints.SendEvent(useKey, model);
        if (result.IsSuccess)
        {
            _logger.Success($"Alert sent to {result.Data?.Workspace ?? "?"} ({result.Data?.Channel ?? "?"}) successfully.");
            var errors = result.Data?.Errors ?? new List<string>();
            foreach (var error in errors)
            {
                _logger.Warning(error);
            }
            return;
        }
        
        _logger.Error(result.Error?.Message ?? "Unknown error");
    }
}