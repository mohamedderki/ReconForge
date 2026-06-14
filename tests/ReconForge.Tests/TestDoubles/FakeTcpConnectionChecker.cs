using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Tests.TestDoubles;

public sealed class FakeTcpConnectionChecker : ITcpConnectionChecker
{
    private readonly Dictionary<(string IpAddress, int Port), TcpConnectionCheckResult> _results = [];
    private readonly List<int> _checkedPorts = [];

    public IReadOnlyList<int> CheckedPorts => _checkedPorts;

    public void SetResult(string ipAddress, int port, TcpConnectionCheckResult result)
    {
        _results[(ipAddress, port)] = result;
    }

    public Task<TcpConnectionCheckResult> CheckAsync(
        string ipAddress,
        int port,
        int timeoutMs,
        CancellationToken cancellationToken = default)
    {
        _checkedPorts.Add(port);

        return Task.FromResult(
            _results.TryGetValue((ipAddress, port), out var result)
                ? result
                : new TcpConnectionCheckResult(PortState.Closed, 5));
    }
}
