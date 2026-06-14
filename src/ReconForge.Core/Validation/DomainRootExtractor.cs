namespace ReconForge.Core.Validation;

public static class DomainRootExtractor
{
    private static readonly HashSet<string> CommonSecondLevelPublicSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ac",
        "co",
        "com",
        "edu",
        "gov",
        "net",
        "org"
    };

    public static string ExtractRootDomain(string host)
    {
        var normalizedHost = NormalizeHost(host);
        if (string.IsNullOrWhiteSpace(normalizedHost))
        {
            return string.Empty;
        }

        var labels = normalizedHost.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (labels.Length <= 2)
        {
            return normalizedHost;
        }

        if (HasCommonSecondLevelPublicSuffix(labels))
        {
            return string.Join('.', labels.TakeLast(3));
        }

        return string.Join('.', labels.TakeLast(2));
    }

    private static string NormalizeHost(string host)
    {
        return host
            .Trim()
            .TrimEnd('/', '.')
            .ToLowerInvariant();
    }

    private static bool HasCommonSecondLevelPublicSuffix(IReadOnlyList<string> labels)
    {
        var topLevelDomain = labels[^1];
        var secondLevelDomain = labels[^2];

        return topLevelDomain.Length == 2
            && CommonSecondLevelPublicSuffixes.Contains(secondLevelDomain);
    }
}
