namespace ReconForge.Core.Models;

public sealed record PortScanResult(
    string TargetIpAddress,
    DateTimeOffset StartedAt,
    DateTimeOffset FinishedAt,
    PortScanStatus Status,
    string Message,
    IReadOnlyList<Port> CheckedPorts,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public IReadOnlyList<Port> OpenPorts => CheckedPorts
        .Where(port => port.State == PortState.Open)
        .ToArray();

    public IReadOnlyList<Port> ClosedPorts => CheckedPorts
        .Where(port => port.State == PortState.Closed)
        .ToArray();

    public IReadOnlyList<Port> TimedOutPorts => CheckedPorts
        .Where(port => port.State == PortState.Timeout)
        .ToArray();
}
