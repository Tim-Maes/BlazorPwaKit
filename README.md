# BlazorPwaKit

A comprehensive toolkit for building Progressive Web Apps with Blazor.

## Features

### ServiceWorkerManager ? (Implemented)

The `ServiceWorkerManager` provides a C# interface to manage service workers in your Blazor application:

- **Register** service workers with custom scopes
- **Update** existing service worker registrations
- **Unregister** service workers
- **Surface lifecycle events** (install, activate, fetch, message, error, updated)
- **Check browser support** for service workers
- **Get current state** of service workers

#### Usage

1. **Register the service in your `Program.cs`:**
```csharp
builder.Services.AddBlazorPwaKit();
// or just the ServiceWorkerManager
builder.Services.AddServiceWorkerManager();
```

2. **Inject and use in your components:**
```razor
@inject ServiceWorkerManager ServiceWorkerManager

<button @onclick="RegisterServiceWorker">Register Service Worker</button>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Subscribe to events
        ServiceWorkerManager.ServiceWorkerInstalled += OnServiceWorkerInstalled;
        ServiceWorkerManager.ServiceWorkerActivated += OnServiceWorkerActivated;
        ServiceWorkerManager.ServiceWorkerUpdated += OnServiceWorkerUpdated;
        ServiceWorkerManager.ServiceWorkerError += OnServiceWorkerError;
    }

    private async Task RegisterServiceWorker()
    {
        var success = await ServiceWorkerManager.RegisterAsync("service-worker.js");
        // Handle success/failure
    }

    private void OnServiceWorkerInstalled(object sender, ServiceWorkerEvent e)
    {
        // Handle installation event
    }
}
```

3. **Use the ServiceWorkerStatus component:**
```razor
<ServiceWorkerStatus />
```

#### API Reference

**ServiceWorkerManager Methods:**
- `RegisterAsync(string scriptUrl, string? scope = null)` - Register a service worker
- `UpdateAsync(string scriptUrl, string? scope = null)` - Update a service worker
- `UnregisterAsync(string scriptUrl, string? scope = null)` - Unregister a service worker
- `GetRegistration(string scriptUrl, string? scope = null)` - Get registration info
- `GetAllRegistrations()` - Get all registrations
- `IsServiceWorkerSupportedAsync()` - Check browser support
- `GetServiceWorkerStateAsync()` - Get current state

**Events:**
- `ServiceWorkerInstalled` - Fired when service worker is installed
- `ServiceWorkerActivated` - Fired when service worker is activated
- `ServiceWorkerFetch` - Fired on fetch events
- `ServiceWorkerMessage` - Fired on message events
- `ServiceWorkerError` - Fired on errors
- `ServiceWorkerUpdated` - Fired when service worker is updated

## Upcoming Features

- **CachePolicyProvider** - Attribute or service to declare per-resource caching strategies
- **UpdatePrompt** - UI component that detects new service worker versions
- **OfflineFallback** - Configurable component for offline scenarios
- **PushNotificationManager** - Push notification management
- **BackgroundSyncService** - Background sync for failed requests

## Getting Started

1. Install the package (when published):
```bash
dotnet add package BlazorPwaKit
```

2. Add to your `Program.cs`:
```csharp
builder.Services.AddBlazorPwaKit();
```

3. Ensure your app has a service worker file (e.g., `service-worker.js`) in the `wwwroot` folder

## Requirements

- .NET 8.0 or later
- Blazor WebAssembly or Blazor Server
- Modern browser with service worker support

## License

This project is licensed under the MIT License.