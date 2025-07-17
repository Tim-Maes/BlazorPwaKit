using BlazorPwaKit.Example;
using BlazorPwaKit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add BlazorPwaKit services
builder.Services.AddBlazorPwaKit();

var host = builder.Build();

// Set up example cache policies and push to service worker
var cachePolicyProvider = host.Services.GetRequiredService<ICachePolicyProvider>();
// Example: CacheFirst for images, NetworkFirst for API, StaleWhileRevalidate for CSS
cachePolicyProvider.SetPolicyForResource(".png", new CachePolicy { Strategy = CacheStrategy.CacheFirst });
cachePolicyProvider.SetPolicyForResource(".jpg", new CachePolicy { Strategy = CacheStrategy.CacheFirst });
cachePolicyProvider.SetPolicyForResource("api/", new CachePolicy { Strategy = CacheStrategy.NetworkFirst });
cachePolicyProvider.SetPolicyForResource(".css", new CachePolicy { Strategy = CacheStrategy.StaleWhileRevalidate });

// Push policies to service worker after registration
var swManager = host.Services.GetRequiredService<ServiceWorkerManager>();
swManager.SetCachePolicyProvider(cachePolicyProvider);
_ = swManager.PushCachePoliciesAsync();

await host.RunAsync();
