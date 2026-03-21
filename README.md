# API Alerts • C# Client

[![NuGet](https://img.shields.io/nuget/v/apialerts)](https://www.nuget.org/packages/apialerts)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

[NuGet](https://www.nuget.org/packages/apialerts) • [GitHub](https://github.com/apialerts/apialerts-csharp) • [API Alerts](https://apialerts.com)

Effortless project notifications. Send once, deliver everywhere.

## Installation

```bash
dotnet add package apialerts
```

## Quick Start

```csharp
using APIAlerts;

Client.Configure("your-api-key");
await Client.Send(new Event { Message = "Deploy complete" });
```

## Usage

### Global singleton (recommended)

Call `Configure` once at startup, then use `Send` / `SendAsync` anywhere.

```csharp
using APIAlerts;

Client.Configure("your-api-key");

// Fire-and-forget — never throws
await Client.Send(new Event { Message = "Deploy complete" });

// Or get the result back — never throws
var result = await Client.SendAsync(new Event { Message = "Deploy complete" });
if (result.Success)
    Console.WriteLine($"Sent to {result.Workspace} ({result.Channel})");
else
    Console.Error.WriteLine($"Error: {result.Error}");
```

### Event fields

Only `Message` is required. All other fields are optional.

```csharp
var evt = new Event
{
    Message  = "Deploy complete",
    Channel  = "releases",
    EventKey = "ci.deploy",
    Title    = "Deployed",
    Tags     = ["CI/CD", "C#"],
    Link     = "https://github.com/apialerts/apialerts-csharp/actions",
    Data     = new { version = "2.0.0" },
};
```

| Field      | Type       | Required | Description                      |
|------------|------------|----------|----------------------------------|
| `Message`  | `string`   | Yes      | Main notification message        |
| `Channel`  | `string`   | No       | Target channel name              |
| `EventKey` | `string`   | No       | Event key (e.g. `ci.deploy`)     |
| `Title`    | `string`   | No       | Short title                      |
| `Tags`     | `string[]` | No       | Categorisation tags              |
| `Link`     | `string`   | No       | URL attached to the notification |
| `Data`     | `object`   | No       | Arbitrary key-value metadata     |

### Instance-based client

Use `ApiAlertsClient` directly when you need multiple clients or full
lifecycle control.

```csharp
var client = new ApiAlertsClient("your-api-key", debug: true);
var result = await client.SendAsync(new Event { Message = "Deploy complete" });
if (result.Success)
    Console.WriteLine($"Sent to {result.Workspace} ({result.Channel})");
```

## Links

- [Documentation](https://apialerts.com/docs)
- [Sign up](https://apialerts.com)
- [GitHub Issues](https://github.com/apialerts/apialerts-csharp/issues)
- [NuGet Package](https://www.nuget.org/packages/apialerts)
