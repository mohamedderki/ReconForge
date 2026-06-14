namespace ReconForge.Cli.Presentation;

public static class StartupBannerContent
{
    public const string Antenna = """
                             .-.
                        ____/___\____
                       /   _\   /_   \
                      /___/  \_/  \___\
                           \  |  /
                            \ | /
            ________________ \|/ ________________
           /  __   __   __   / \   __   __   __  \
          /__/  \_/  \_/  \_/   \_/  \_/  \_/  \__\
         <____   H E R M E S   S I G N A L   ____>
          \  \__/ \__/ \__/ \   / \__/ \__/ \__/  /
           \_________________\ /__________________/
    """;

    public const string Title = """
     ____  _____ ____ ___  _   _ _____ ___  ____   ____ _____
    |  _ \| ____/ ___/ _ \| \ | |  ___/ _ \|  _ \ / ___| ____|
    | |_) |  _|| |  | | | |  \| | |_ | | | | |_) | |  _|  _|
    |  _ <| |__| |__| |_| | |\  |  _|| |_| |  _ <| |_| | |___
    |_| \_\_____\____\___/|_| \_|_|   \___/|_| \_\\____|_____|
    """;

    public const string ProjectName = "RECONFORGE";

    public const string SignalMotif = "H E R M E S   S I G N A L";

    public const string Subtitle = "Modular Reconnaissance CLI Tool";

    public const string Description = "Fast signals. Structured results. Modular workflow.";

    public const string Disclaimer = "For educational and authorized testing only.";

    public static string PlainText => string.Join(
        Environment.NewLine,
        Antenna.TrimEnd(),
        string.Empty,
        Title.TrimEnd(),
        $"        {ProjectName}",
        string.Empty,
        $"        {Subtitle}",
        $"        {Description}",
        $"        {Disclaimer}");
}
