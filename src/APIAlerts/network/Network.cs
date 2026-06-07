using System.Net;
using System.Net.Http.Json;
using APIAlerts.network.contract;

namespace APIAlerts.network;

internal class Network
{
    private readonly HttpClient _httpClient;
    private string _integration = Constants.IntegrationName;
    private string _version = Constants.Version;
    private string _baseUrl = Constants.ApiUrl;

    internal Network(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(Constants.TimeoutSeconds) };
    }

    internal void SetOverrides(string integration, string version, string baseUrl)
    {
        _integration = integration;
        _version = version;
        _baseUrl = baseUrl;
    }

    internal async Task<SendResult> PostEvent(string apiKey, Event evt)
    {
        try
        {
            var payload = new EventRequest
            {
                Message = evt.Message,
                Channel = evt.Channel,
                Event = evt.EventKey,
                Title = evt.Title,
                Tags = evt.Tags,
                Link = evt.Link,
                Data = evt.Data,
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("X-Integration", _integration);
            request.Headers.Add("X-Version", _version);
            request.Content = JsonContent.Create(payload, options: Json.JsonOptions);

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                200 => await ParseSuccess(response).ConfigureAwait(false),
                400 => new SendResult { Success = false, Error = "bad request" },
                401 => new SendResult { Success = false, Error = "unauthorized, check your api key" },
                403 => new SendResult { Success = false, Error = "forbidden" },
                429 => new SendResult { Success = false, Error = "rate limit exceeded" },
                _ => new SendResult { Success = false, Error = $"unexpected status: {(int)response.StatusCode}" },
            };
        }
        catch (Exception)
        {
            return new SendResult { Success = false, Error = "invalid response from server" };
        }
    }

    private static async Task<SendResult> ParseSuccess(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<EventResponse>(Json.JsonOptions).ConfigureAwait(false);
            if (body is null)
                return new SendResult { Success = false, Error = "invalid response from server" };

            return new SendResult
            {
                Success = true,
                Workspace = body.Workspace,
                Channel = body.Channel,
                Warnings = body.Warnings ?? new List<string>(),
            };
        }
        catch (Exception)
        {
            return new SendResult { Success = false, Error = "invalid response from server" };
        }
    }
}
