namespace ReconForge.Core.Models;

public sealed record Domain(
    string OriginalInput,
    string NormalizedName,
    DateTimeOffset CreatedAt,
    bool IsValid)
{
    public string NormalizedHost => NormalizedName;

    public string RootDomain { get; init; } = NormalizedName;
}
