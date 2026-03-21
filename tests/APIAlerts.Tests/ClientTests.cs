using System.Net;
using System.Text.Json;
using APIAlerts.Tests.mocks;
using Xunit;

namespace APIAlerts.Tests;

public class ClientTests : IDisposable
{
    public void Dispose() => Client.Reset();

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_Returns_Failure_WhenKeyIsEmpty()
    {
        var client = new ApiAlertsClient("", httpClient: MockHttp.Success());
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("api key is missing", result.Error);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_WhenMessageIsEmpty()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Success());
        var result = await client.SendAsync(new Event { Message = "" });
        Assert.False(result.Success);
        Assert.Equal("message is required", result.Error);
    }

    // ── HTTP status codes ─────────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_Returns_SendResult_On200()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Success("W", "C"));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.True(result.Success);
        Assert.Equal("W", result.Workspace);
        Assert.Equal("C", result.Channel);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task SendAsync_Returns_Warnings_On200()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Success(warnings: ["deprecated"]));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.True(result.Success);
        Assert.Single(result.Warnings);
        Assert.Equal("deprecated", result.Warnings[0]);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_On400()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.BadRequest));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("bad request", result.Error);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_On401()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.Unauthorized));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("unauthorized — check your api key", result.Error);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_On403()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.Forbidden));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("forbidden", result.Error);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_On429()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.TooManyRequests));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("rate limit exceeded", result.Error);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_On500()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.InternalServerError));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("unexpected status: 500", result.Error);
    }

    [Fact]
    public async Task SendAsync_Returns_Failure_OnBadJson()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.OK, "not json"));
        var result = await client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("invalid response from server", result.Error);
    }

    // ── Request headers ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_Sends_AuthorizationHeader()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            """{"workspace":"W","channel":"C","warnings":[]}""");
        var client = new ApiAlertsClient("my-api-key", httpClient: new HttpClient(handler));
        await client.SendAsync(new Event { Message = "test" });
        Assert.Equal("Bearer my-api-key", handler.LastRequest!.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task SendAsync_Sends_IntegrationHeaders()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            """{"workspace":"W","channel":"C","warnings":[]}""");
        var client = new ApiAlertsClient("key", httpClient: new HttpClient(handler));
        await client.SendAsync(new Event { Message = "test" });
        Assert.Equal("csharp", handler.LastRequest!.Headers.GetValues("X-Integration").First());
        Assert.Equal("2.0.0", handler.LastRequest!.Headers.GetValues("X-Version").First());
    }

    [Fact]
    public async Task SetOverrides_Changes_IntegrationHeaders()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            """{"workspace":"W","channel":"C","warnings":[]}""");
        var client = new ApiAlertsClient("key", httpClient: new HttpClient(handler));
        client.SetOverrides("github-actions", "1.0.0", "http://localhost");
        await client.SendAsync(new Event { Message = "test" });
        Assert.Equal("github-actions", handler.LastRequest!.Headers.GetValues("X-Integration").First());
        Assert.Equal("1.0.0", handler.LastRequest!.Headers.GetValues("X-Version").First());
    }

    [Fact]
    public async Task SendWithKey_Uses_ProvidedApiKey()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            """{"workspace":"W","channel":"C","warnings":[]}""");
        var client = new ApiAlertsClient("original-key", httpClient: new HttpClient(handler));
        await client.SendWithKey("override-key", new Event { Message = "test" });
        Assert.Equal("Bearer override-key", handler.LastRequest!.Headers.Authorization?.ToString());
    }

    // ── Payload serialization ─────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_Serializes_AllFields()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            """{"workspace":"W","channel":"developer","warnings":[]}""");
        var client = new ApiAlertsClient("key", httpClient: new HttpClient(handler));
        await client.SendAsync(new Event
        {
            Message  = "Full payload",
            Channel  = "developer",
            EventKey = "ci.deploy",
            Title    = "Deployed",
            Tags     = ["CI/CD", "C#"],
            Link     = "https://github.com",
            Data     = new { version = "2.0.0" },
        });
        var body = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal("Full payload", body.RootElement.GetProperty("message").GetString());
        Assert.Equal("developer",    body.RootElement.GetProperty("channel").GetString());
        Assert.Equal("ci.deploy",    body.RootElement.GetProperty("event").GetString());
        Assert.Equal("Deployed",     body.RootElement.GetProperty("title").GetString());
        Assert.Equal("https://github.com", body.RootElement.GetProperty("link").GetString());
    }

    [Fact]
    public async Task SendAsync_Omits_NullFields()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            """{"workspace":"W","channel":"C","warnings":[]}""");
        var client = new ApiAlertsClient("key", httpClient: new HttpClient(handler));
        await client.SendAsync(new Event { Message = "minimal" });
        var body = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.False(body.RootElement.TryGetProperty("channel",  out _));
        Assert.False(body.RootElement.TryGetProperty("event",    out _));
        Assert.False(body.RootElement.TryGetProperty("title",    out _));
        Assert.False(body.RootElement.TryGetProperty("tags",     out _));
        Assert.False(body.RootElement.TryGetProperty("link",     out _));
        Assert.False(body.RootElement.TryGetProperty("data",     out _));
    }

    // ── Fire-and-forget ───────────────────────────────────────────────────────

    [Fact]
    public async Task Send_DoesNotThrow_OnError()
    {
        var client = new ApiAlertsClient("key", httpClient: MockHttp.Client(HttpStatusCode.Unauthorized));
        // Should complete without throwing
        await client.Send(new Event { Message = "test" });
    }

    // ── Global singleton ──────────────────────────────────────────────────────

    [Fact]
    public async Task Client_SendAsync_Returns_Failure_BeforeConfigure()
    {
        var result = await Client.SendAsync(new Event { Message = "test" });
        Assert.False(result.Success);
        Assert.Equal("client not configured", result.Error);
    }

    [Fact]
    public async Task Client_Configure_InitializesClient()
    {
        Client.Configure("key", httpClient: MockHttp.Success("W", "C"));
        var result = await Client.SendAsync(new Event { Message = "test" });
        Assert.True(result.Success);
        Assert.Equal("W", result.Workspace);
    }

    [Fact]
    public async Task Client_Configure_IsIdempotent()
    {
        Client.Configure("first-key", httpClient: MockHttp.Success());
        Client.Configure("second-key"); // should be ignored
        var result = await Client.SendAsync(new Event { Message = "test" });
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Client_Send_ReturnsFailure_BeforeConfigure()
    {
        // Should complete without throwing; logs error to Console.Error
        await Client.Send(new Event { Message = "test" });
    }
}
