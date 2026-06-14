using System.Diagnostics;
using System.Net.Sockets;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;

namespace ReconForge.Scanning.Ports;

public sealed class TcpConnectionChecker : ITcpConnectionChecker
{
    public async Task<TcpConnectionCheckResult> CheckAsync(
        string ipAddress,
        int port,
        int timeoutMs,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(timeoutMs));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(ipAddress, port, timeoutCts.Token);

            stopwatch.Stop();
            return new TcpConnectionCheckResult(PortState.Open, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return new TcpConnectionCheckResult(PortState.Timeout, stopwatch.ElapsedMilliseconds);
        }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.ConnectionRefused)
        {
            stopwatch.Stop();
            return new TcpConnectionCheckResult(PortState.Closed, stopwatch.ElapsedMilliseconds);
        }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.TimedOut)
        {
            stopwatch.Stop();
            return new TcpConnectionCheckResult(PortState.Timeout, stopwatch.ElapsedMilliseconds, exception.Message);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            stopwatch.Stop();
            return new TcpConnectionCheckResult(PortState.Error, stopwatch.ElapsedMilliseconds, exception.Message);
        }
    }
}
