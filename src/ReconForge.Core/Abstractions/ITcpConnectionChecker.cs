using ReconForge.Core.Models;

namespace ReconForge.Core.Abstractions;

public interface ITcpConnectionChecker
{
    Task<TcpConnectionCheckResult> CheckAsync(
        string ipAddress,
        int port,
        int timeoutMs,
        CancellationToken cancellationToken = default);
}
