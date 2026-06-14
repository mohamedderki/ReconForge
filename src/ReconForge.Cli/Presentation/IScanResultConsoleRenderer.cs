using ReconForge.Core.Models;

namespace ReconForge.Cli.Presentation;

public interface IScanResultConsoleRenderer
{
    void Render(DomainScanResult result, bool verbose = false);
}
