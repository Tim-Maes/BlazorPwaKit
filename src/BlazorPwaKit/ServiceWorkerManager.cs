using Microsoft.JSInterop;
using System.Text.Json;

namespace BlazorPwaKit;

/// <summary>
/// Service worker lifecycle events
/// </summary>
public record ServiceWorkerEvent(string Type, string? Message = null);

/// <summary>
/// Service worker registration state
/// </summary>
public enum ServiceWorkerState
{
    NotRegistered,
    Registering,
    Registered,
    Updating,
    Updated,
    Error
}

/// <summary>
/// Service worker registration information
/// </summary>
public record ServiceWorkerRegistration(
    string Scope,
    ServiceWorkerState State,
    string? ScriptUrl = null,
    string? Error = null
);

/// <summary>
/// Manages service worker lifecycle: registration, updates, and unregistration
/// </summary>
public class ServiceWorkerManager : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly Dictionary<string, ServiceWorkerRegistration> _registrations = new();
    private ICachePolicyProvider? _cachePolicyProvider;

    public event EventHandler<ServiceWorkerEvent>? ServiceWorkerInstalled;
    public event EventHandler<ServiceWorkerEvent>? ServiceWorkerActivated;
    public event EventHandler<ServiceWorkerEvent>? ServiceWorkerFetch;
    public event EventHandler<ServiceWorkerEvent>? ServiceWorkerMessage;
    public event EventHandler<ServiceWorkerEvent>? ServiceWorkerError;
    public event EventHandler<ServiceWorkerEvent>? ServiceWorkerUpdated;

    public ServiceWorkerManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(async () =>
        {
            var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorPwaKit/serviceWorkerManager.js");
            await module.InvokeVoidAsync("initialize", DotNetObjectReference.Create(this));
            return module;
        });
    }

    /// <summary>
    /// Registers a service worker with the specified script URL and scope
    /// </summary>
    /// <param name="scriptUrl">The URL of the service worker script</param>
    /// <param name="scope">The scope of the service worker (optional)</param>
    /// <returns>True if registration was successful</returns>
    public async Task<bool> RegisterAsync(string scriptUrl, string? scope = null)
    {
        try
        {
            var key = GetRegistrationKey(scriptUrl, scope);
            _registrations[key] = new ServiceWorkerRegistration(scope ?? "/", ServiceWorkerState.Registering, scriptUrl);
            
            var module = await _moduleTask.Value;
            var result = await module.InvokeAsync<bool>("registerServiceWorker", scriptUrl, scope);
            
            if (result)
            {
                _registrations[key] = _registrations[key] with { State = ServiceWorkerState.Registered };
                return true;
            }
            else
            {
                _registrations[key] = _registrations[key] with { State = ServiceWorkerState.Error, Error = "Registration failed" };
                return false;
            }
        }
        catch (Exception ex)
        {
            var key = GetRegistrationKey(scriptUrl, scope);
            _registrations[key] = new ServiceWorkerRegistration(scope ?? "/", ServiceWorkerState.Error, scriptUrl, ex.Message);
            ServiceWorkerError?.Invoke(this, new ServiceWorkerEvent("error", ex.Message));
            return false;
        }
    }

    /// <summary>
    /// Updates the service worker registration
    /// </summary>
    /// <param name="scriptUrl">The URL of the service worker script</param>
    /// <param name="scope">The scope of the service worker (optional)</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateAsync(string scriptUrl, string? scope = null)
    {
        try
        {
            var key = GetRegistrationKey(scriptUrl, scope);
            if (_registrations.TryGetValue(key, out var registration))
            {
                _registrations[key] = registration with { State = ServiceWorkerState.Updating };
            }
            
            var module = await _moduleTask.Value;
            var result = await module.InvokeAsync<bool>("updateServiceWorker", scriptUrl, scope);
            
            if (result && _registrations.TryGetValue(key, out registration))
            {
                _registrations[key] = registration with { State = ServiceWorkerState.Updated };
                ServiceWorkerUpdated?.Invoke(this, new ServiceWorkerEvent("updated"));
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            ServiceWorkerError?.Invoke(this, new ServiceWorkerEvent("error", ex.Message));
            return false;
        }
    }

    /// <summary>
    /// Unregisters a service worker
    /// </summary>
    /// <param name="scriptUrl">The URL of the service worker script</param>
    /// <param name="scope">The scope of the service worker (optional)</param>
    /// <returns>True if unregistration was successful</returns>
    public async Task<bool> UnregisterAsync(string scriptUrl, string? scope = null)
    {
        try
        {
            var module = await _moduleTask.Value;
            var result = await module.InvokeAsync<bool>("unregisterServiceWorker", scriptUrl, scope);
            
            if (result)
            {
                var key = GetRegistrationKey(scriptUrl, scope);
                _registrations.Remove(key);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            ServiceWorkerError?.Invoke(this, new ServiceWorkerEvent("error", ex.Message));
            return false;
        }
    }

    /// <summary>
    /// Gets the current registration information for a service worker
    /// </summary>
    /// <param name="scriptUrl">The URL of the service worker script</param>
    /// <param name="scope">The scope of the service worker (optional)</param>
    /// <returns>Service worker registration information</returns>
    public ServiceWorkerRegistration? GetRegistration(string scriptUrl, string? scope = null)
    {
        var key = GetRegistrationKey(scriptUrl, scope);
        return _registrations.TryGetValue(key, out var registration) ? registration : null;
    }

    /// <summary>
    /// Gets all current service worker registrations
    /// </summary>
    /// <returns>Dictionary of all registrations</returns>
    public IReadOnlyDictionary<string, ServiceWorkerRegistration> GetAllRegistrations()
    {
        return _registrations.AsReadOnly();
    }

    /// <summary>
    /// Checks if service workers are supported in the current browser
    /// </summary>
    /// <returns>True if service workers are supported</returns>
    public async Task<bool> IsServiceWorkerSupportedAsync()
    {
        try
        {
            var module = await _moduleTask.Value;
            return await module.InvokeAsync<bool>("isServiceWorkerSupported");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current service worker state
    /// </summary>
    /// <returns>Current service worker state</returns>
    public async Task<string?> GetServiceWorkerStateAsync()
    {
        try
        {
            var module = await _moduleTask.Value;
            return await module.InvokeAsync<string?>("getServiceWorkerState");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Pushes all cache policies to the service worker
    /// </summary>
    public async Task PushCachePoliciesAsync()
    {
        if (_cachePolicyProvider == null) return;
        var policies = _cachePolicyProvider.ExportPoliciesForJs();
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setCachePolicies", policies);
    }

    /// <summary>
    /// Sets the cache policy provider
    /// </summary>
    /// <param name="provider">The cache policy provider</param>
    public void SetCachePolicyProvider(ICachePolicyProvider provider)
    {
        _cachePolicyProvider = provider;
    }

    /// <summary>
    /// Callback method invoked from JavaScript when service worker events occur
    /// </summary>
    [JSInvokable]
    public void OnServiceWorkerEvent(string eventType, string? message = null)
    {
        var serviceWorkerEvent = new ServiceWorkerEvent(eventType, message);
        
        switch (eventType.ToLowerInvariant())
        {
            case "install":
                ServiceWorkerInstalled?.Invoke(this, serviceWorkerEvent);
                break;
            case "activate":
                ServiceWorkerActivated?.Invoke(this, serviceWorkerEvent);
                break;
            case "fetch":
                ServiceWorkerFetch?.Invoke(this, serviceWorkerEvent);
                break;
            case "message":
                ServiceWorkerMessage?.Invoke(this, serviceWorkerEvent);
                break;
            case "error":
                ServiceWorkerError?.Invoke(this, serviceWorkerEvent);
                break;
            case "updated":
                ServiceWorkerUpdated?.Invoke(this, serviceWorkerEvent);
                break;
        }
    }

    private static string GetRegistrationKey(string scriptUrl, string? scope)
    {
        return $"{scriptUrl}:{scope ?? "/"}";
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("dispose");
            await module.DisposeAsync();
        }
    }
}