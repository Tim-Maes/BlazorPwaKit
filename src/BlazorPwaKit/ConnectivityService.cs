using Microsoft.JSInterop;

namespace BlazorPwaKit;

public class ConnectivityService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ConnectivityService>? _dotNetRef;
    private IJSObjectReference? _jsModule;

    public event EventHandler<bool>? ConnectivityChanged;
    public bool IsOnline { get; private set; } = true;

    public ConnectivityService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _dotNetRef = DotNetObjectReference.Create(this);
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorPwaKit/connectivity.js");
        IsOnline = await _jsModule.InvokeAsync<bool>("getIsOnline");
        await _jsModule.InvokeVoidAsync("registerConnectivityHandler", _dotNetRef);
    }

    [JSInvokable]
    public void OnConnectivityChanged(bool isOnline)
    {
        IsOnline = isOnline;
        ConnectivityChanged?.Invoke(this, isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("disposeConnectivityHandler");
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }
}
