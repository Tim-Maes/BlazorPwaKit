namespace BlazorPwaKit;

public enum CacheStrategy
{
    CacheFirst,
    NetworkFirst,
    StaleWhileRevalidate,
    NetworkOnly,
    CacheOnly
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CachePolicyAttribute : Attribute
{
    public CacheStrategy Strategy { get; }
    public string? CacheKey { get; }
    public int? MaxAgeSeconds { get; }

    public CachePolicyAttribute(CacheStrategy strategy, string? cacheKey = null, int? maxAgeSeconds = null)
    {
        Strategy = strategy;
        CacheKey = cacheKey;
        MaxAgeSeconds = maxAgeSeconds;
    }
}
