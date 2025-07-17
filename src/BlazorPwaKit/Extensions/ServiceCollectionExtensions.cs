using Microsoft.Extensions.DependencyInjection;

namespace BlazorPwaKit;

/// <summary>
/// Options for configuring BlazorPwaKit services
/// </summary>
public class BlazorPwaKitOptions
{
    /// <summary>
    /// Path to the offline fallback page
    /// </summary>
    public string OfflineFallbackPath { get; set; } = "/offline";
}

/// <summary>
/// Extension methods for registering BlazorPwaKit services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds BlazorPwaKit services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration for BlazorPwaKit</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBlazorPwaKit(this IServiceCollection services, Action<BlazorPwaKitOptions>? configure = null)
    {
        var options = new BlazorPwaKitOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
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