using APIAlerts.Tests.mocks;
using Xunit;

namespace APIAlerts.Tests;

public class AlertsTests
{
    [Fact]
    public void Configure_SetsDefaultApiKeyAndDebugMode()
    {
        var mockClient = new MockClient();
        Alerts.SetClient(mockClient);

        Alerts.Configure("test-api-key", true);

        Assert.Equal("test-api-key", mockClient.ApiKey);
        Assert.True(mockClient.Debug);
    }

    [Fact]
    public void Send_ValidRequest_LogsSuccess()
    {
        var mockClient = new MockClient();
        Alerts.SetClient(mockClient);

        var alert = new Alert
        {
            Message = "test message"
        };
        Alerts.Send(alert);
        Assert.Null(mockClient.SentApiKey);
        
        Alerts.SendWithApiKey("test-api-key", alert);
        
        Assert.Equal("test-api-key", mockClient.SentApiKey);
        Assert.Null(mockClient.SentChannel);
        Assert.Equal("test message", mockClient.SentMessage);
        Assert.Null(mockClient.SentTags);
        Assert.Null(mockClient.SentLink);
    }

    [Fact]
    public async Task SendAsync_ValidRequest_LogsSuccess()
    {
        var mockClient = new MockClient();
        Alerts.SetClient(mockClient);

        var alert = new Alert
        {
            Message = "test message"
        };
        await Alerts.SendAsync(alert);
        Assert.Null(mockClient.SentApiKey);

        await Alerts.SendWithApiKeyAsync("test-api-key", alert);
        
        Assert.Equal("test-api-key", mockClient.SentApiKey);
        Assert.Null(mockClient.SentChannel);
        Assert.Equal("test message", mockClient.SentMessage);
        Assert.Null(mockClient.SentTags);
        Assert.Null(mockClient.SentLink);
    }
}