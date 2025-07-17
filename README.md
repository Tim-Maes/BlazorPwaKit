# BlazorPwaKit

A toolkit for building Progressive Web Apps (PWAs) with Blazor WebAssembly.

Still in early development.

## Features

- **ServiceWorkerManager**: Register, update, and unregister service workers with full lifecycle event support.
- **CachePolicyProvider**: Attribute/service for per-resource caching strategies (CacheFirst, NetworkFirst, StaleWhileRevalidate, etc.).
- **OfflineFallback**: Configurable component to render a “you’re offline” page or placeholder when a requested route or asset isn’t cached.
- **UpdatePrompt**: UI component that detects a new service worker version and prompts the user to refresh/reload.
- **ConnectivityStatus**: UI component and service for real-time online/offline state and change events.
- **Easy DI integration**: One-liner setup for all services and components.
- **Example project**: See `BlazorPwaKit.Example` for real-world usage.

## Getting Started

### 1. Install the NuGet Package

```bash
dotnet add package BlazorPwaKit
```

### 2. Register Services in `Program.cs`

```csharp
using BlazorPwaKit;

builder.Services.AddBlazorPwaKit();

// Optional: customize fallback route
builder.Services.AddBlazorPwaKit(options =>
{
    options.OfflineFallbackPath = "/offline"; 
});
```

### 3. Add Usings

Add to your `_Imports.razor` (or at the top of your components):

```csharp
@using BlazorPwaKit
@using BlazorPwaKit.Components
```

### 4. Use the Components

- **Service Worker Status:**

```razor
<ServiceWorkerStatus />
```

- **Update Prompt:**
 
```razor
<UpdatePrompt />
<!-- Or customize: -->
<UpdatePrompt PromptMessage="New version!" ButtonText="Update Now" PromptClass="my-update-prompt" />
```

- **Connectivity Status:**

```razor
<ConnectivityStatus />
<!-- Or customize: -->
<ConnectivityStatus OnlineText="Connected" OfflineText="Disconnected" Class="my-conn-status" />
```

- **Offline Fallback:**

```razor
@* Default fallback *@
<OfflineFallback />

@* Or custom content *@
<OfflineFallback>
    <div>Your custom offline message</div>
</OfflineFallback>
```

### 5. Advanced: Cache Policies

Set up cache strategies for resources in `Program.cs`:

```csharp
var cachePolicyProvider = host.Services.GetRequiredService<ICachePolicyProvider>();
cachePolicyProvider.SetPolicyForResource(".png", new CachePolicy { Strategy = CacheStrategy.CacheFirst });
cachePolicyProvider.SetPolicyForResource("api/", new CachePolicy { Strategy = CacheStrategy.NetworkFirst });
// ...
```

## Customization

- All components accept parameters for text and CSS class overrides.
- You can override styles by targeting the default class names in your own CSS.
- For full control, use the provided services/events directly in your own components.

## Example Project

See `BlazorPwaKit.Example` for a working demo of all features.

## Requirements

- .NET 8
- Blazor WebAssembly

## License

MIT
