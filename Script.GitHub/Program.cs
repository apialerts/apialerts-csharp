namespace Script.GitHub;

class Program
{
    static async Task Main(string[] args)
    {
        var (build, release, publish) = ParseFlags(args);
        if (!build && !release && !publish)
        {
            Console.WriteLine("Usage: Script.GitHub build|release|publish");
            return;
        }

        var apiKey = Environment.GetEnvironmentVariable("APIALERTS_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.Error.WriteLine("Error: APIALERTS_API_KEY environment variable is not set");
            return;
        }

        APIAlerts.ApiAlerts.Configure(apiKey);

        // Minimal send
        var minimal = await APIAlerts.ApiAlerts.SendAsync(new APIAlerts.Event { Message = "C# SDK sample - minimal" });
        Console.WriteLine($"Minimal alert sent to {minimal.Workspace} ({minimal.Channel})");

        // Full send
        var evt = CreateEvent(build, release, publish);
        var result = await APIAlerts.ApiAlerts.SendAsync(evt);
        Console.WriteLine($"Alert sent to {result.Workspace} ({result.Channel})");
    }

    private static (bool build, bool release, bool publish) ParseFlags(string[] args)
    {
        var flags = new HashSet<string>(args);
        return (flags.Contains("build"), flags.Contains("release"), flags.Contains("publish"));
    }

    private static APIAlerts.Event CreateEvent(bool build, bool release, bool publish)
    {
        const string link = "https://github.com/apialerts/apialerts-csharp/actions";

        if (build) return new APIAlerts.Event
        {
            Channel = "developer",
            EventKey = "ci.sdk.build.csharp",
            Title = "Build Passed",
            Message = "C# - PR build success",
            Tags = ["CI/CD", "C#", "Build"],
            Link = link,
        };

        if (release) return new APIAlerts.Event
        {
            Channel = "developer",
            EventKey = "ci.sdk.release.csharp",
            Title = "Release Built",
            Message = "C# - Build for publish success",
            Tags = ["CI/CD", "C#", "Build"],
            Link = link,
        };

        return new APIAlerts.Event
        {
            Channel = "releases",
            EventKey = "ci.sdk.publish.csharp",
            Title = "Published",
            Message = "C# - NuGet publish success",
            Tags = ["CI/CD", "C#", "Deploy"],
            Link = link,
        };
    }
}
