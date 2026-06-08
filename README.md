# API Alerts • C# / .NET / Unity / Godot Client

[![NuGet](https://img.shields.io/nuget/v/apialerts)](https://www.nuget.org/packages/apialerts)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

[NuGet](https://www.nuget.org/packages/apialerts) • [GitHub](https://github.com/apialerts/apialerts-csharp) • [API Alerts](https://apialerts.com)

Effortless project notifications. Send once, deliver everywhere.

Targets `netstandard2.0` and `net10.0`. Works in:

- **.NET 10 / .NET 8** (ASP.NET Core, Blazor, Azure Functions, console apps)
- **.NET Framework 4.6.1+** and **Mono**
- **Unity** (2018+, both Mono and IL2CPP backends)
- **Godot** C# projects

## Installation

```bash
dotnet add package apialerts
```

That covers .NET, ASP.NET Core, Blazor, Azure Functions, and console apps. Unity and Godot need a couple of extra steps - see [Game engines](#game-engines-unity--godot) below.

## Quick Start

```csharp
using APIAlerts;

ApiAlerts.Configure("your-api-key");
await ApiAlerts.SendAsync(new Event { Message = "Deploy complete" });
```

## Usage

### Global singleton

Call `Configure` once at startup, then use `Send` / `SendAsync` anywhere.

```csharp
using APIAlerts;

ApiAlerts.Configure("your-api-key");

// Fire-and-forget. Never throws, drops errors silently unless debug is on.
ApiAlerts.Send(new Event { Message = "Deploy complete" });

// Awaitable. Never throws, check result.Success.
var result = await ApiAlerts.SendAsync(new Event { Message = "Deploy complete" });
if (result.Success)
    Console.WriteLine($"Sent to {result.Workspace} ({result.Channel})");
else
    Console.Error.WriteLine($"Error: {result.Error}");
```

### Dependency injection

Register `IApiAlertsClient` and inject it:

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddApiAlerts("your-api-key");
```

```csharp
public class DeployNotifier(IApiAlertsClient alerts)
{
    public Task Notify() => alerts.SendAsync(new Event { Message = "Deploy complete" });
}
```

Or construct one directly, no container:

```csharp
IApiAlertsClient alerts = new ApiAlertsClient("your-api-key");
```

`AddApiAlerts` routes SDK logs through the app's `ILoggerFactory`. Use the instance client for DI, mocking, or multiple keys; the `ApiAlerts` singleton stays for quick one-off use.

### Enable debug logging

```csharp
ApiAlerts.Configure("your-api-key", debug: true);
```

The SDK uses `Microsoft.Extensions.Logging.ILogger`. By default it logs to `NullLogger.Instance` (silent). Inject your application's logger to surface SDK output:

```csharp
ApiAlerts.Configure(
    "your-api-key",
    debug: true,
    logger: serviceProvider.GetRequiredService<ILogger<Program>>()
);
```

Critical errors (missing API key, not configured) always log regardless of the `debug` flag.

### Custom HttpClient

Inject your own `HttpClient` to share connection pools, plug in handlers (Polly, retry, proxy), or route through `IHttpClientFactory`:

```csharp
ApiAlerts.Configure(
    "your-api-key",
    httpClient: httpClientFactory.CreateClient("apialerts")
);
```

If you don't provide one, the SDK creates an `HttpClient` with a 30-second timeout.

### Event fields

Only `Message` is required. All other fields are optional and omitted from the JSON payload when null.

```csharp
var evt = new Event
{
    Message  = "Deploy complete",
    Channel  = "releases",
    EventKey = "ci.deploy.success",
    Title    = "Deployed",
    Tags     = new[] { "CI/CD", "C#" },
    Link     = "https://github.com/apialerts/apialerts-csharp/actions",
    Data     = new { version = "1.0.0" },
};

await ApiAlerts.SendAsync(evt);
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Message` | `string` | Yes | Human-readable notification text. This is what appears on the push notification lock screen. |
| `Channel` | `string` | No | Workspace channel the push notification fires on. Defaults to the workspace default channel when omitted. |
| `EventKey` | `string` | No | Identifies what kind of thing happened. Optional but recommended. Use dotted notation (e.g. `ci.deploy.success`, `payment.failed`, `user.signup`) so routing rules can match glob patterns like `ci.*` or `*.failed`. Serialised as `event` over the wire. |
| `Title` | `string` | No | Short headline some destinations render separately from the message body. |
| `Tags` | `string[]` | No | Categorisation tags for filtering and search. |
| `Link` | `string` | No | URL associated with the event. Available as a deeplink for push notifications and as a call-to-action for routed destinations. |
| `Data` | `object` | No | Arbitrary key-value metadata. Available to non-push destinations for templating (Slack message bodies, email templates, webhook payloads). |

### Send to multiple workspaces

Pass an `apiKey` as the optional last argument to override the configured key for a single call.

```csharp
ApiAlerts.Send(new Event { Message = "Deploy complete" }, apiKey: "other-workspace-api-key");

var result = await ApiAlerts.SendAsync(
    new Event { Message = "Deploy complete" },
    apiKey: "other-workspace-api-key"
);
```

## API

| Method | Description |
|---|---|
| `ApiAlerts.Configure(apiKey, debug, logger, httpClient)` | Initialise the singleton. First call wins; subsequent calls are no-ops. |
| `ApiAlerts.Send(evt, apiKey)` | Fire-and-forget. Never throws, drops errors silently unless `debug` is on. |
| `ApiAlerts.SendAsync(evt, apiKey)` | Awaitable, returns `SendResult`. Never throws. |
| `new ApiAlertsClient(apiKey, debug, logger, httpClient)` | Construct an instance (implements `IApiAlertsClient`) for DI, mocking, or multiple keys. |
| `services.AddApiAlerts(apiKey, debug)` | Register `IApiAlertsClient` as a singleton in a DI container. |

### SendResult fields

| Field | Type | Description |
|---|---|---|
| `Success` | `bool` | `true` if delivered |
| `Workspace` | `string?` | Workspace name (present on success) |
| `Channel` | `string?` | Channel name (present on success) |
| `Warnings` | `IReadOnlyList<string>` | Non-fatal server warnings |
| `Error` | `string?` | Error message (present on failure) |

## Game engines (Unity & Godot)

Get a push on your phone the moment something happens in your game or live-ops, without building your own backend:

- a new high score lands on the leaderboard
- a player submits feedback or a bug report
- a server-side error threshold trips during a live event
- an in-app purchase completes (or fails)
- a Unity Cloud Build or Godot export finishes in CI

```csharp
ApiAlerts.Send(new Event
{
    Message  = "New #1 on the leaderboard: 184,200",
    Channel  = "liveops",
    EventKey = "game.leaderboard.record",
    Tags     = new[] { "leaderboard" },
});
```

### Unity

NuGet packages don't import into Unity natively, and `APIAlerts.dll` needs assemblies Unity doesn't ship (System.Text.Json and friends), so it won't run on its own.

- **[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)** (recommended) - install it, search `apialerts`, click install. It resolves the whole dependency tree for you.
- **Manual DLL drop** (advanced) - extract `lib/netstandard2.0/APIAlerts.dll` from the [`.nupkg`](https://www.nuget.org/packages/apialerts) into `Assets/Plugins/`, along with *every* DLL in its dependency tree (System.Text.Json, System.Net.Http.Json, the Microsoft.Extensions.* abstractions, and their transitive deps). Fiddly by hand, which is why NuGetForUnity is recommended.

Unity also has its own `UnityEngine.Event`, so `new Event { }` is ambiguous under `using UnityEngine;`. Fully-qualify the SDK type as `APIAlerts.Event`, or alias it with `using AlertEvent = APIAlerts.Event;`.

### Godot (C#)

```bash
dotnet add package apialerts
```

Standard .NET tooling works here, so NuGet restores the dependencies for you - no manual DLL juggling like Unity. This requires the **.NET edition of Godot** (the Mono/.NET build), not the standard GDScript-only build.

## Links

- [Documentation](https://apialerts.com/docs/sdks/csharp)
- [Sign up](https://apialerts.com)
- [GitHub Issues](https://github.com/apialerts/apialerts-csharp/issues)
- [NuGet Package](https://www.nuget.org/packages/apialerts)
