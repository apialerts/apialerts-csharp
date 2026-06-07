using APIAlerts;

var apiKey = Environment.GetEnvironmentVariable("APIALERTS_API_KEY") ?? "";
if (string.IsNullOrEmpty(apiKey))
{
    Console.Error.WriteLine("Error: APIALERTS_API_KEY environment variable is not set");
    Environment.Exit(1);
}

var isBuild            = args.Contains("--build");
var isRelease          = args.Contains("--release");
var isPublish          = args.Contains("--publish");
var isIntegrationTests = args.Contains("--integration-tests");

var channelIdx = Array.IndexOf(args, "--channel");
var channel    = channelIdx >= 0 && channelIdx + 1 < args.Length ? args[channelIdx + 1] : "testing";

Client.Configure(apiKey);

var link = "https://github.com/apialerts/apialerts-csharp/actions";

if (isBuild)
{
    var result = await Client.SendAsync(new Event
    {
        Message  = "C# SDK - PR build success",
        Channel  = "developer",
        EventKey = "ci.sdk.build.csharp",
        Title    = "Build Passed",
        Tags     = ["CI/CD", "C#", "Build"],
        Link     = link,
    });
    if (result.Success)
        Console.WriteLine($"✓ Sent to {result.Workspace} ({result.Channel})");
    else
    {
        Console.Error.WriteLine($"Error: {result.Error}");
        Environment.Exit(1);
    }
}
else if (isRelease)
{
    var result = await Client.SendAsync(new Event
    {
        Message  = "C# SDK - Build for publish success",
        Channel  = "developer",
        EventKey = "ci.sdk.release.csharp",
        Title    = "Release Build Passed",
        Tags     = ["CI/CD", "C#", "Build"],
        Link     = link,
    });
    if (result.Success)
        Console.WriteLine($"✓ Sent to {result.Workspace} ({result.Channel})");
    else
    {
        Console.Error.WriteLine($"Error: {result.Error}");
        Environment.Exit(1);
    }
}
else if (isPublish)
{
    var result = await Client.SendAsync(new Event
    {
        Message  = "C# SDK - NuGet publish success",
        Channel  = "releases",
        EventKey = "ci.sdk.publish.csharp",
        Title    = "Published",
        Tags     = ["CI/CD", "C#", "Deploy"],
        Link     = link,
    });
    if (result.Success)
        Console.WriteLine($"✓ Sent to {result.Workspace} ({result.Channel})");
    else
    {
        Console.Error.WriteLine($"Error: {result.Error}");
        Environment.Exit(1);
    }
}
else if (isIntegrationTests)
{
    var minimalResult = await Client.SendAsync(new Event { Message = "C# SDK - minimal", Channel = channel });
    if (minimalResult.Success)
        Console.WriteLine($"✓ sent to {minimalResult.Workspace} ({minimalResult.Channel})");
    else
    {
        Console.Error.WriteLine($"Error (minimal): {minimalResult.Error}");
        Environment.Exit(1);
    }

    var fullResult = await Client.SendAsync(new Event
    {
        Message  = "C# SDK - full",
        Channel  = channel,
        EventKey = "sdk.test",
        Title    = "Integration Test",
        Tags     = ["CI/CD", "C#"],
        Link     = link,
        Data     = new { version = "2.0.0" },
    });
    if (fullResult.Success)
    {
        Console.WriteLine($"✓ sent to {fullResult.Workspace} ({fullResult.Channel})");
        foreach (var warning in fullResult.Warnings)
            Console.WriteLine($"! Warning: {warning}");
    }
    else
    {
        Console.Error.WriteLine($"Error (full): {fullResult.Error}");
        Environment.Exit(1);
    }
}
else
{
    Console.Error.WriteLine("Error: pass --build, --release, --publish, or --integration-tests");
    Environment.Exit(1);
}
