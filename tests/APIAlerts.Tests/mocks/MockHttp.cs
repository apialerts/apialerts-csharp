using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace APIAlerts.Tests.mocks;

internal static class MockHttp
{
    internal static HttpClient Client(HttpStatusCode statusCode, string body = "")
    {
        return new HttpClient(new MockHttpMessageHandler(statusCode, body));
    }

    internal static HttpClient Success(string workspace = "My Workspace", string channel = "general", IEnumerable<string>? warnings = null)
    {
        var warningsList = warnings is null ? "[]" : $"[{string.Join(",", warnings.Select(w => $"\"{w}\""))}]";
        var body = $$"""{"workspace":"{{workspace}}","channel":"{{channel}}","warnings":{{warningsList}}}""";
        return Client(HttpStatusCode.OK, body);
    }
}

internal class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    public MockHttpMessageHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        // Read body before the request is disposed by the caller's `using` block
        LastRequestBody = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : null;
        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        };
    }
}
