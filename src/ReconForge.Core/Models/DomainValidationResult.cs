namespace ReconForge.Core.Models;

public sealed record DomainValidationResult(
    bool IsValid,
    Domain? Domain,
    IReadOnlyList<string> Errors)
{
    public static DomainValidationResult Valid(Domain domain)
    {
        return new DomainValidationResult(true, domain, Array.Empty<string>());
    }

    public static DomainValidationResult Invalid(params string[] errors)
    {
        return new DomainValidationResult(false, null, errors);
    }
}
