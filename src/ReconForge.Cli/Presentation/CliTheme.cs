namespace ReconForge.Cli.Presentation;

public sealed record CliTheme(
    string PrimaryColor,
    string AccentColor,
    string MutedColor,
    string TextColor,
    string SuccessColor,
    string WarningColor,
    string ErrorColor)
{
    public static CliTheme Default { get; } = new(
        PrimaryColor: "deepskyblue1",
        AccentColor: "gold1",
        MutedColor: "grey58",
        TextColor: "grey85",
        SuccessColor: "green",
        WarningColor: "yellow",
        ErrorColor: "red");
}
