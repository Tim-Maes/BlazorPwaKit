using Microsoft.Extensions.DependencyInjection;

namespace BlazorPwaKit;

/// <summary>
/// Extension methods for registering BlazorPwaKit services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds BlazorPwaKit services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBlazorPwaKit(this IServiceCollection services)
    {
        services.AddScoped<ServiceWorkerManager>();
        services.AddSingleton<ICachePolicyProvider, CachePolicyProvider>();
        services.AddScoped<ConnectivityService>();
        return services;
    }
    
    /// <summary>
    /// Adds the ServiceWorkerManager to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddServiceWorkerManager(this IServiceCollection services)
    {
        services.AddScoped<ServiceWorkerManager>();
        return services;
    }
}