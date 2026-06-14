using ReconForge.Cli.Presentation;

namespace ReconForge.Tests.Cli.Presentation;

public sealed class StartupBannerRendererTests
{
    [Fact]
    public void PlainText_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(StartupBannerContent.PlainText));
    }

    [Fact]
    public void PlainText_ContainsReconForge()
    {
        Assert.Contains("RECONFORGE", StartupBannerContent.PlainText);
    }

    [Fact]
    public void PlainText_ContainsSignalMotif()
    {
        Assert.Contains(StartupBannerContent.SignalMotif, StartupBannerContent.PlainText);
    }

    [Fact]
    public void PlainText_ContainsEducationalDisclaimer()
    {
        Assert.Contains("For educational and authorized testing only.", StartupBannerContent.PlainText);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("TrUe")]
    public void ShouldRender_ReturnsFalse_WhenEnvironmentVariableIsTrue(string value)
    {
        var renderer = new StartupBannerRenderer(
            Spectre.Console.AnsiConsole.Console,
            getEnvironmentVariable: name => name == StartupBannerRenderer.DisableBannerEnvironmentVariable ? value : null);

        Assert.False(renderer.ShouldRender());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("false")]
    public void ShouldRender_ReturnsTrue_WhenEnvironmentVariableIsNotTrue(string? value)
    {
        var renderer = new StartupBannerRenderer(
            Spectre.Console.AnsiConsole.Console,
            getEnvironmentVariable: name => name == StartupBannerRenderer.DisableBannerEnvironmentVariable ? value : null);

        Assert.True(renderer.ShouldRender());
    }
}
