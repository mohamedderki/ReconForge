using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Core.Validation;

public sealed class DomainNameValidator : IDomainValidator
{
    public DomainValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return DomainValidationResult.Invalid("A domain must be provided with --scan.");
        }

        var trimmedInput = input.Trim();

        if (trimmedInput.Any(char.IsWhiteSpace))
        {
            return DomainValidationResult.Invalid("The domain must not contain spaces.");
        }

        if (!TryExtractDomain(trimmedInput, out var extractedDomain, out var extractionError))
        {
            return DomainValidationResult.Invalid(extractionError);
        }

        var normalizedHost = extractedDomain.TrimEnd('/').ToLowerInvariant();

        if (!IsDomainLike(normalizedHost, out var validationError))
        {
            return DomainValidationResult.Invalid(validationError);
        }

        var rootDomain = DomainRootExtractor.ExtractRootDomain(normalizedHost);

        var domain = new Domain(
            trimmedInput,
            normalizedHost,
            DateTimeOffset.UtcNow,
            IsValid: true)
        {
            RootDomain = rootDomain
        };

        return DomainValidationResult.Valid(domain);
    }

    private static bool TryExtractDomain(
        string input,
        out string domain,
        out string errorMessage)
    {
        domain = string.Empty;
        errorMessage = string.Empty;

        if (input.Contains("://", StringComparison.Ordinal))
        {
            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                errorMessage = "The domain input is not a valid URL.";
                return false;
            }

            if (uri.Scheme is not ("http" or "https"))
            {
                errorMessage = "Only plain domains or http/https URLs are supported.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(uri.Host))
            {
                errorMessage = "The URL does not contain a valid host.";
                return false;
            }

            domain = uri.Host;
            return true;
        }

        if (input.Contains('/'))
        {
            var withoutTrailingSlash = input.TrimEnd('/');
            if (withoutTrailingSlash.Contains('/'))
            {
                errorMessage = "Use a domain name without a path, or provide a full http/https URL.";
                return false;
            }

            domain = withoutTrailingSlash;
            return true;
        }

        domain = input;
        return true;
    }

    private static bool IsDomainLike(string domain, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (domain.Length > 253)
        {
            errorMessage = "The domain is too long.";
            return false;
        }

        if (!domain.Contains('.'))
        {
            errorMessage = "The domain must include at least one dot, for example example.com.";
            return false;
        }

        if (Uri.CheckHostName(domain) != UriHostNameType.Dns)
        {
            errorMessage = "The domain contains invalid characters.";
            return false;
        }

        var labels = domain.Split('.');
        if (labels.Any(label => !IsValidLabel(label)))
        {
            errorMessage = "Each domain label must contain letters, digits, or hyphens and cannot start or end with a hyphen.";
            return false;
        }

        return true;
    }

    private static bool IsValidLabel(string label)
    {
        if (label.Length is 0 or > 63)
        {
            return false;
        }

        if (label.StartsWith('-') || label.EndsWith('-'))
        {
            return false;
        }

        return label.All(character =>
            char.IsAsciiLetterOrDigit(character) || character == '-');
    }
}
