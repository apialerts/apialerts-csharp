namespace Script.GitHub;

using APIAlerts;

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

        var apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Error: APIALERTS_API_KEY environment variable is not set");
            return;
        }

        Alerts.Configure(apiKey);
        
        var alert = CreateAlert(build, release, publish);
        // Using async here to ensure GitHub Actions will wait for the response before terminating the script early
        await Alerts.SendAsync(alert);
    }

    private static string? GetApiKey()
    {
        return Environment.GetEnvironmentVariable("APIALERTS_API_KEY");
    }

    private static (bool build, bool release, bool publish) ParseFlags(string[] args)
    {
        var flags = new HashSet<string>(args);
        var build = flags.Contains("build");
        var release = flags.Contains("release");
        var publish = flags.Contains("publish");
        return (build, release, publish);
    }

    private static Alert CreateAlert(bool build, bool release, bool publish)
    {
        var eventChannel = "developer";
        var eventMessage = "apialerts-csharp";
        var eventTags = Array.Empty<string>();
        const string eventLink = "https://github.com/apialerts/apialerts-csharp/actions";

        if (build)
        {
            eventMessage = "C# - PR build success";
            eventTags = new[] { "CI/CD", "C#", "Build" };
        }
        else if (release)
        {
            eventMessage = "C# - Build for publish success";
            eventTags = new[] { "CI/CD", "C#", "Build" };
        }
        else if (publish)
        {
            eventChannel = "releases";
            eventMessage = "C# - NuGet publish success";
            eventTags = new[] { "CI/CD", "C#", "Deploy" };
        }

        return new Alert
        {
            Message = eventMessage,
            Channel = eventChannel,
            Tags = eventTags,
            Link = eventLink
        };
    }
}