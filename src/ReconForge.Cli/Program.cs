using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReconForge.Cli.Commands;
using ReconForge.Cli.DependencyInjection;
using ReconForge.Cli.Presentation;
using ReconForge.Infrastructure.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

var verbose = args.Any(arg => string.Equals(arg, "--verbose", StringComparison.OrdinalIgnoreCase));
var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
    builder.SetMinimumLevel(verbose ? LogLevel.Information : LogLevel.Error);
});

services.AddReconForge();
services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
services.AddSingleton<IScanResultConsoleRenderer, ScanResultConsoleRenderer>();
services.AddTransient<CliScanExecutor>();
services.AddTransient<RootCommand>();

new StartupBannerRenderer(AnsiConsole.Console).Render();

var app = new CommandApp<RootCommand>(new DependencyInjectionTypeRegistrar(services));

app.Configure(config =>
{
    config.SetApplicationName("reconforge");
    config.SetExceptionHandler((exception, resolver) =>
    {
        var loggerFactory = resolver?.Resolve(typeof(ILoggerFactory)) as ILoggerFactory;
        var logger = loggerFactory?.CreateLogger("ReconForge.Cli");
        var message = exception?.Message ?? "An unexpected error occurred.";

        if (exception is null)
        {
            logger?.LogError("Unexpected exception.");
        }
        else
        {
            logger?.LogError(exception, "Unexpected exception.");
        }

        AnsiConsole.MarkupLine("[red]Unexpected error:[/] {0}", Markup.Escape(message));
        return 1;
    });
});

return await app.RunAsync(args);
