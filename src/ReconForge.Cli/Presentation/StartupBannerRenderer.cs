using Spectre.Console;

namespace ReconForge.Cli.Presentation;

public sealed class StartupBannerRenderer
{
    public const string DisableBannerEnvironmentVariable = "RECONFORGE_NO_BANNER";

    private readonly IAnsiConsole _console;
    private readonly CliTheme _theme;
    private readonly Func<string, string?> _getEnvironmentVariable;

    public StartupBannerRenderer(
        IAnsiConsole console,
        CliTheme? theme = null,
        Func<string, string?>? getEnvironmentVariable = null)
    {
        _console = console;
        _theme = theme ?? CliTheme.Default;
        _getEnvironmentVariable = getEnvironmentVariable ?? Environment.GetEnvironmentVariable;
    }

    public bool ShouldRender()
    {
        return !string.Equals(
            _getEnvironmentVariable(DisableBannerEnvironmentVariable),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    public void Render()
    {
        if (!ShouldRender())
        {
            return;
        }

        RenderAntennaBlock();
        _console.WriteLine();
        RenderBlock(StartupBannerContent.Title, _theme.PrimaryColor);
        RenderLine($"        {StartupBannerContent.ProjectName}", _theme.PrimaryColor);
        _console.WriteLine();
        RenderLine($"        {StartupBannerContent.Subtitle}", _theme.TextColor);
        RenderLine($"        {StartupBannerContent.Description}", _theme.TextColor);
        RenderLine($"        {StartupBannerContent.Disclaimer}", _theme.AccentColor);
        _console.WriteLine();
    }

    private void RenderBlock(string block, string color)
    {
        foreach (var line in block.Split(Environment.NewLine))
        {
            RenderLine(line, color);
        }
    }

    private void RenderAntennaBlock()
    {
        foreach (var line in StartupBannerContent.Antenna.Split(Environment.NewLine))
        {
            if (line.Contains(StartupBannerContent.SignalMotif, StringComparison.Ordinal))
            {
                RenderLineWithAccent(line, StartupBannerContent.SignalMotif);
                continue;
            }

            RenderLine(line, _theme.MutedColor);
        }
    }

    private void RenderLineWithAccent(string line, string accentText)
    {
        var accentStart = line.IndexOf(accentText, StringComparison.Ordinal);
        if (accentStart < 0)
        {
            RenderLine(line, _theme.MutedColor);
            return;
        }

        var before = line[..accentStart];
        var after = line[(accentStart + accentText.Length)..];

        _console.MarkupLine(
            "[{0}]{1}[/][{2}]{3}[/][{0}]{4}[/]",
            _theme.MutedColor,
            Markup.Escape(before),
            _theme.AccentColor,
            Markup.Escape(accentText),
            Markup.Escape(after));
    }

    private void RenderLine(string line, string color)
    {
        _console.MarkupLine("[{0}]{1}[/]", color, Markup.Escape(line));
    }
}
