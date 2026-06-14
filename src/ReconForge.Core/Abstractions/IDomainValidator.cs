using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface IDomainValidator
{
    DomainValidationResult Validate(string? input);
}
