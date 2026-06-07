using APIAlerts;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>DI registration for <see cref="IApiAlertsClient"/>.</summary>
public static class ApiAlertsServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="IApiAlertsClient"/> as a singleton. SDK logs route
    /// through the app's <see cref="ILoggerFactory"/> when one is registered.
    /// </summary>
    public static IServiceCollection AddApiAlerts(this IServiceCollection services, string apiKey, bool debug = false) =>
        services.AddSingleton<IApiAlertsClient>(sp =>
            new ApiAlertsClient(apiKey, debug, sp.GetService<ILoggerFactory>()?.CreateLogger("APIAlerts")));
}
