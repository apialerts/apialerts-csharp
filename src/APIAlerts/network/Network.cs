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
        _httpClient = httpClient ?? new HttpClient();
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
                Event   = evt.EventKey,
                Title   = evt.Title,
                Tags    = evt.Tags,
                Link    = evt.Link,
                Data    = evt.Data,
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("X-Integration", _integration);
            request.Headers.Add("X-Version", _version);
            request.Content = JsonContent.Create(payload, options: Json.JsonOptions);

            var response = await _httpClient.SendAsync(request);

            return response.StatusCode switch
            {
                HttpStatusCode.OK           => await ParseSuccess(response),
                HttpStatusCode.BadRequest   => new SendResult { Success = false, Error = "bad request" },
                HttpStatusCode.Unauthorized => new SendResult { Success = false, Error = "unauthorized — check your api key" },
                HttpStatusCode.Forbidden    => new SendResult { Success = false, Error = "forbidden" },
                HttpStatusCode.TooManyRequests => new SendResult { Success = false, Error = "rate limit exceeded" },
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
            var body = await response.Content.ReadFromJsonAsync<EventResponse>(Json.JsonOptions);
            if (body is null)
                return new SendResult { Success = false, Error = "invalid response from server" };

            return new SendResult
            {
                Success   = true,
                Workspace = body.Workspace,
                Channel   = body.Channel,
                Warnings  = body.Warnings ?? [],
            };
        }
        catch (Exception)
        {
            return new SendResult { Success = false, Error = "invalid response from server" };
        }
    }
}
