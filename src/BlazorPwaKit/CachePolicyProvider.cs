using System.Reflection;

namespace BlazorPwaKit;

public interface ICachePolicyProvider
{
    CachePolicy? GetPolicyForResource(string resourceUrl);
    void SetPolicyForResource(string resourceUrl, CachePolicy policy);
    Dictionary<string, string> ExportPoliciesForJs();
}

public class CachePolicy
{
    public CacheStrategy Strategy { get; set; }
    public string? CacheKey { get; set; }
    public int? MaxAgeSeconds { get; set; }
}

public class CachePolicyProvider : ICachePolicyProvider
{
    private readonly Dictionary<string, CachePolicy> _policies = new();

    public CachePolicy? GetPolicyForResource(string resourceUrl)
    {
        _policies.TryGetValue(resourceUrl, out var policy);
        return policy;
    }

    public void SetPolicyForResource(string resourceUrl, CachePolicy policy)
    {
        _policies[resourceUrl] = policy;
    }

    public Dictionary<string, string> ExportPoliciesForJs()
    {
        // Export as { pattern: strategyName }
        return _policies.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Strategy.ToString()
        );
    }
}
