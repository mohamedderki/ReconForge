using ReconForge.Core.Validation;

namespace ReconForge.Tests.Domain;

public sealed class DomainNameValidatorTests
{
    private readonly DomainNameValidator _validator = new();

    [Theory]
    [InlineData("example.com", "example.com")]
    [InlineData("Example.COM", "example.com")]
    [InlineData("  example.com  ", "example.com")]
    [InlineData("example.com/", "example.com")]
    [InlineData("https://Example.COM/path", "example.com")]
    [InlineData("http://example.com/", "example.com")]
    public void Validate_ReturnsNormalizedDomain_ForValidInput(string input, string expectedDomain)
    {
        var result = _validator.Validate(input);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Domain);
        Assert.Equal(expectedDomain, result.Domain.NormalizedName);
        Assert.True(result.Domain.IsValid);
    }

    [Fact]
    public void Validate_SeparatesOriginalInputNormalizedHostAndRootDomain_ForUrlInput()
    {
        var result = _validator.Validate("https://www.hackthissite.org/");

        Assert.True(result.IsValid);
        Assert.NotNull(result.Domain);
        Assert.Equal("https://www.hackthissite.org/", result.Domain.OriginalInput);
        Assert.Equal("www.hackthissite.org", result.Domain.NormalizedHost);
        Assert.Equal("hackthissite.org", result.Domain.RootDomain);
    }

    [Fact]
    public void Validate_ExtractsRootDomain_FromWwwHost()
    {
        var result = _validator.Validate("www.example.com");

        Assert.True(result.IsValid);
        Assert.NotNull(result.Domain);
        Assert.Equal("www.example.com", result.Domain.NormalizedHost);
        Assert.Equal("example.com", result.Domain.RootDomain);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("exa mple.com")]
    [InlineData("not-a-domain")]
    [InlineData("-example.com")]
    [InlineData("example-.com")]
    [InlineData("ftp://example.com")]
    [InlineData("example.com/path")]
    public void Validate_ReturnsErrors_ForInvalidInput(string input)
    {
        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Null(result.Domain);
        Assert.NotEmpty(result.Errors);
    }

    [Theory]
    [InlineData("", "A domain must be provided with --scan.")]
    [InlineData("exa mple.com", "The domain must not contain spaces.")]
    [InlineData("ftp://example.com", "Only plain domains or http/https URLs are supported.")]
    [InlineData("not-a-domain", "The domain must include at least one dot")]
    public void Validate_ReturnsClearValidationErrors(string input, string expectedMessage)
    {
        var result = _validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains(expectedMessage));
    }
}
