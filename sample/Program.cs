using APIAlerts;

var apiKey = Environment.GetEnvironmentVariable("APIALERTS_API_KEY")
    ?? throw new Exception("APIALERTS_API_KEY not set");

// Parse flags: --build, --release, --publish (informational, no-args runs both tests)
var isBuild   = args.Contains("--build");
var isRelease = args.Contains("--release");
var isPublish = args.Contains("--publish");

if (isBuild || isRelease || isPublish)
{
    var flag = isBuild ? "--build" : isRelease ? "--release" : "--publish";
    Console.WriteLine($"Running in {flag} mode");
}

Client.Configure(apiKey);

// Minimal send — message only
var minimalResult = await Client.SendAsync(new Event { Message = "C# SDK - minimal" });
if (minimalResult.Success)
{
    Console.WriteLine($"✓ sent to {minimalResult.Workspace} ({minimalResult.Channel})");
}
else
{
    Console.Error.WriteLine($"x Error (minimal): {minimalResult.Error}");
    return;
}

// Full send — all fields
var fullResult = await Client.SendAsync(new Event
{
    Message  = "C# SDK - full",
    Channel  = "developer",
    EventKey = "sdk.test",
    Title    = "Integration Test",
    Tags     = ["CI/CD", "C#"],
    Link     = "https://github.com/apialerts/apialerts-csharp/actions",
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
    Console.Error.WriteLine($"x Error (full): {fullResult.Error}");
}
