using System.Text.RegularExpressions;
using Xunit;

namespace APIAlerts.Tests;

/// <summary>
/// Pins the literal values of SDK constants. Catches accidental changes to
/// the integration name, version shape, base URL, or timeout - matches the
/// canonical pattern documented in SDK Design.
/// </summary>
public class ConstantsTests
{
    [Fact]
    public void IntegrationName_IsCsharp()
    {
        Assert.Equal("csharp", Constants.IntegrationName);
    }

    [Fact]
    public void BaseUrl_IsApiAlertsEventEndpoint()
    {
        Assert.Equal("https://api.apialerts.com/event", Constants.ApiUrl);
    }

    [Fact]
    public void Timeout_Is30SecondsPerSpec()
    {
        Assert.Equal(30, Constants.TimeoutSeconds);
    }

    [Fact]
    public void Version_IsOneXSemver()
    {
        // Major version pin: bumping to v2.x requires updating this test
        // deliberately. Catches accidental major bumps.
        Assert.Matches(@"^1\.\d+\.\d+([-+][\w.]+)?$", Constants.Version);
    }
}
