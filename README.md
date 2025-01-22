# apialerts-dotnet

C# client for the [apialerts.com](https://apialerts.com/) platform

[Docs](https://apialerts.com/docs/dotnet) • [GitHub](https://github.com/apialerts/apialerts-dotnet) • [Nuget](https://www.nuget.org/packages/APIAlerts)

### Overview

The APIAlerts NuGet package simplifies the process of setting up and managing alerts within your API projects. It provides functionalities to activate the package with an API key and offers methods for publishing alerts asynchronously and synchronously.

### Installation

APIAlerts is available as a NuGet package. You can install it using the following command:

````bash
PM> Install-Package APIAlerts
````

### Initialize the client

The client is implemented as a singleton, ensuring that only one instance is created and used throughout the application.


````csharp
// Initialise the ApiAlerts client with your API key
APIAlerts.Client.Configure(yourApiKey);

// You can enable debug mode to see logs messages using the additional debug parameter
APIAlerts.Client.Configure(yourApiKey, true);
````

### Send Events

You can send alerts by constructing the AlertEvent class and passing it to the Send() function.

```csharp
var alert = new APIAlerts.Alert
{
    Message = "My alert message",               // required message
    Channel = "my-channel-identifier",          // optional, uses the default channel if not provided
    Tags = new List<string> { "tag1", "tag2" }, // optional
    Link = "https://example.com"                // optional
};

APIAlerts.Client.Send(alert);
```

The APIAlerts.Client.SendAsync() methods are also available if you need to wait for a successful execution. However, the Send() functions are generally always preferred.

### Send with API Key functions

You may have the need to talk to different API Alerts workspaces in your application. You can use the SendWithApiKey() functions to send alerts to override the default API key for that single send call.

```csharp
APIAlerts.Client.SendWithApiKey("other_api_key", alert);
```

### Feedback & Support

If you have any questions or feedback, please create an issue on our GitHub repository. We are always looking to improve our service and would love to hear from you. Thanks for using API Alerts!
