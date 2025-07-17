# BlazorPwaKit

A toolkit for building Progressive Web Apps (PWAs) with Blazor WebAssembly.

## Features
- ServiceWorkerManager: Register, update, and unregister service workers with lifecycle event support
- Easy integration with Blazor DI
- Example project included

## Usage
1. Add the library to your Blazor WebAssembly project.
2. Register services in `Program.cs`:

```
builder.Services.AddBlazorPwaKit();
```

3. Use the `ServiceWorkerManager` in your components or use the `<ServiceWorkerStatus />` demo component.

## Example
See the `Example/BlazorPwaKit.Example` project for a working demo.

## Requirements
- .NET 8
- Blazor WebAssembly

## License
MIT
