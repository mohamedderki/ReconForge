using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForge.Core.Validation;

namespace ReconForge.Scanning.Subdomains;

public sealed class SubdomainDiscoveryService : ISubdomainDiscoveryService
{
    private const string DefaultSource = "DefaultPrefix";
    private const string CustomSource = "CustomPrefix";

    private static readonly string[] DefaultPrefixes =
    [
        "www",
        "mail",
        "api",
        "dev",
        "test",
        "staging",
        "admin"
    ];

    private readonly ILogger<SubdomainDiscoveryService> _logger;

    public SubdomainDiscoveryService(ILogger<SubdomainDiscoveryService> logger)
    {
        _logger = logger;
    }

    public Task<SubdomainDiscoveryResult> DiscoverAsync(
        string rootDomain,
        IEnumerable<string>? customPrefixes = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var startedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("Subdomain Discovery started.");
        _logger.LogInformation("Target domain received: {RootDomain}", rootDomain);

        if (string.IsNullOrWhiteSpace(rootDomain))
        {
            _logger.LogWarning("Subdomain Discovery received an empty root domain.");

            return Task.FromResult(new SubdomainDiscoveryResult(
                string.Empty,
                startedAt,
                DateTimeOffset.UtcNow,
                SubdomainDiscoveryStatus.InvalidInput,
                "A normalized root domain is required for subdomain discovery.",
                Array.Empty<Subdomain>(),
                new[] { "A normalized root domain is required for subdomain discovery." },
                Array.Empty<string>()));
        }

        try
        {
            var normalizedRootDomain = DomainRootExtractor.ExtractRootDomain(rootDomain);
            var warnings = new List<string>();
            var discoveredSubdomains = new List<Subdomain>();
            var seenFullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _logger.LogInformation("Default prefixes loaded: {PrefixCount}", DefaultPrefixes.Length);

            foreach (var prefix in DefaultPrefixes)
            {
                AddCandidate(
                    NormalizePrefix(prefix),
                    normalizedRootDomain,
                    DefaultSource,
                    discoveredSubdomains,
                    seenFullNames,
                    warnings);
            }

            if (customPrefixes is not null)
            {
                _logger.LogInformation("Custom prefixes received.");

                foreach (var customPrefix in customPrefixes)
                {
                    var normalizedPrefix = NormalizePrefix(customPrefix);

                    if (string.IsNullOrWhiteSpace(normalizedPrefix))
                    {
                        _logger.LogWarning("Empty subdomain prefix ignored.");
                        warnings.Add("An empty subdomain prefix was ignored.");
                        continue;
                    }

                    AddCandidate(
                        normalizedPrefix,
                        normalizedRootDomain,
                        CustomSource,
                        discoveredSubdomains,
                        seenFullNames,
                        warnings);
                }
            }

            var status = warnings.Count > 0
                ? SubdomainDiscoveryStatus.PartialSuccess
                : SubdomainDiscoveryStatus.Success;

            _logger.LogInformation("Subdomain Discovery completed.");
            _logger.LogInformation("Discovered subdomains: {SubdomainCount}", discoveredSubdomains.Count);

            return Task.FromResult(new SubdomainDiscoveryResult(
                normalizedRootDomain,
                startedAt,
                DateTimeOffset.UtcNow,
                status,
                "Subdomain discovery completed using a small safe prefix list. No network probing was performed.",
                discoveredSubdomains,
                Array.Empty<string>(),
                warnings));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Unexpected exception during Subdomain Discovery.");

            return Task.FromResult(new SubdomainDiscoveryResult(
                rootDomain,
                startedAt,
                DateTimeOffset.UtcNow,
                SubdomainDiscoveryStatus.Failed,
                "Subdomain discovery failed unexpectedly.",
                Array.Empty<Subdomain>(),
                new[] { exception.Message },
                Array.Empty<string>()));
        }
    }

    private void AddCandidate(
        string prefix,
        string rootDomain,
        string source,
        List<Subdomain> discoveredSubdomains,
        HashSet<string> seenFullNames,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return;
        }

        if (!IsSafePrefix(prefix))
        {
            _logger.LogWarning("Invalid subdomain prefix ignored: {Prefix}", prefix);
            warnings.Add($"Invalid subdomain prefix ignored: {prefix}");
            return;
        }

        var fullName = $"{prefix}.{rootDomain}".ToLowerInvariant();

        if (!seenFullNames.Add(fullName))
        {
            _logger.LogInformation("Duplicate subdomain candidate removed: {FullName}", fullName);
            return;
        }

        _logger.LogInformation("Subdomain candidate generated: {FullName}", fullName);

        discoveredSubdomains.Add(new Subdomain(
            prefix,
            rootDomain,
            fullName,
            source,
            DateTimeOffset.UtcNow));
    }

    private static string NormalizePrefix(string? prefix)
    {
        return (prefix ?? string.Empty)
            .Trim()
            .Trim('.')
            .ToLowerInvariant();
    }

    private static bool IsSafePrefix(string prefix)
    {
        return prefix.All(character =>
            char.IsAsciiLetterOrDigit(character) || character == '-');
    }
}
