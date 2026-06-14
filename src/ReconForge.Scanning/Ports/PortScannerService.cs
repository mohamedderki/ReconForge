using Microsoft.Extensions.Logging;
using ReconForge.Core.Abstractions;
using ReconForge.Core.Models;
using ReconForgeIpAddress = ReconForge.Core.Models.IpAddress;

namespace ReconForge.Scanning.Ports;

public sealed class PortScannerService : IPortScannerService
{
    private const string TcpProtocol = "TCP";

    private static readonly int[] DefaultPorts =
    [
        21,
        22,
        25,
        53,
        80,
        110,
        143,
        443,
        587,
        993,
        995,
        3306,
        5432,
        8080
    ];

    private static readonly IReadOnlyDictionary<int, string> ServiceNames = new Dictionary<int, string>
    {
        [21] = "FTP",
        [22] = "SSH",
        [25] = "SMTP",
        [53] = "DNS",
        [80] = "HTTP",
        [110] = "POP3",
        [143] = "IMAP",
        [443] = "HTTPS",
        [587] = "SMTP Submission",
        [993] = "IMAPS",
        [995] = "POP3S",
        [3306] = "MySQL",
        [5432] = "PostgreSQL",
        [8080] = "HTTP Alternate"
    };

    private readonly ITcpConnectionChecker _connectionChecker;
    private readonly ILogger<PortScannerService> _logger;

    public PortScannerService(
        ITcpConnectionChecker connectionChecker,
        ILogger<PortScannerService> logger)
    {
        _connectionChecker = connectionChecker;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PortScanResult>> ScanAsync(
        IEnumerable<ReconForgeIpAddress> resolvedIpAddresses,
        IEnumerable<int>? ports = null,
        int timeoutMs = 1000,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetIpAddresses = NormalizeIpAddresses(resolvedIpAddresses);
        var warnings = new List<string>();
        var portsToCheck = BuildPortList(ports, warnings);

        LogPortScanStarted(targetIpAddresses, portsToCheck);

        if (targetIpAddresses.Count == 0)
        {
            _logger.LogWarning("Port Scan completed with no IP addresses to scan.");
            return Array.Empty<PortScanResult>();
        }

        if (portsToCheck.Count == 0)
        {
            _logger.LogWarning("Port Scan completed with no valid ports to check.");
            return CreateNoValidPortsResults(targetIpAddresses, warnings);
        }

        var results = new List<PortScanResult>();

        foreach (var ipAddress in targetIpAddresses)
        {
            var result = await ScanIpAddressAsync(
                ipAddress,
                portsToCheck,
                warnings,
                timeoutMs,
                cancellationToken);

            results.Add(result);
        }

        LogPortScanCompleted(results);

        return results;
    }

    private async Task<PortScanResult> ScanIpAddressAsync(
        string ipAddress,
        IReadOnlyList<int> portsToCheck,
        IReadOnlyList<string> warnings,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var checkedPorts = new List<Port>();
        var errors = new List<string>();

        foreach (var portNumber in portsToCheck)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var checkResult = await CheckPortAsync(
                ipAddress,
                portNumber,
                timeoutMs,
                cancellationToken);

            checkedPorts.Add(checkResult.Port);

            if (!string.IsNullOrWhiteSpace(checkResult.ErrorMessage))
            {
                errors.Add(checkResult.ErrorMessage);
            }
        }

        return CreatePortScanResult(
            ipAddress,
            startedAt,
            checkedPorts,
            errors,
            warnings);
    }

    private async Task<CheckedPortResult> CheckPortAsync(
        string ipAddress,
        int portNumber,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Port check started: {IpAddress}:{Port}", ipAddress, portNumber);

        try
        {
            var check = await _connectionChecker.CheckAsync(
                ipAddress,
                portNumber,
                timeoutMs,
                cancellationToken);

            var port = CreatePort(portNumber, check);

            LogPortState(ipAddress, port, check.ErrorMessage);

            var errorMessage = check.State == PortState.Error
                ? $"{ipAddress}:{portNumber}: {check.ErrorMessage ?? "Unknown error"}"
                : null;

            return new CheckedPortResult(port, errorMessage);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Unexpected exception during port check for {IpAddress}:{Port}",
                ipAddress,
                portNumber);

            var port = CreateErrorPort(portNumber);
            var errorMessage = $"{ipAddress}:{portNumber}: {exception.Message}";

            return new CheckedPortResult(port, errorMessage);
        }
    }

    private static Port CreatePort(int portNumber, TcpConnectionCheckResult check)
    {
        return new Port(
            portNumber,
            TcpProtocol,
            GetServiceName(portNumber),
            check.State,
            DateTimeOffset.UtcNow,
            check.ResponseTimeMs);
    }

    private static Port CreateErrorPort(int portNumber)
    {
        return new Port(
            portNumber,
            TcpProtocol,
            GetServiceName(portNumber),
            PortState.Error,
            DateTimeOffset.UtcNow,
            ResponseTimeMs: null);
    }

    private static PortScanResult CreatePortScanResult(
        string ipAddress,
        DateTimeOffset startedAt,
        IReadOnlyList<Port> checkedPorts,
        IReadOnlyList<string> errors,
        IReadOnlyList<string> warnings)
    {
        var status = DetermineStatus(checkedPorts, errors);
        var message = GetPortScanMessage(status);

        return new PortScanResult(
            ipAddress,
            startedAt,
            DateTimeOffset.UtcNow,
            status,
            message,
            checkedPorts,
            errors,
            warnings);
    }

    private static string GetPortScanMessage(PortScanStatus status)
    {
        return status switch
        {
            PortScanStatus.Success => "Port scan completed and at least one open port was found.",
            PortScanStatus.PartialSuccess => "Port scan completed with errors.",
            PortScanStatus.Failed => "Port scan failed for this target.",
            _ => "Port scan completed with no open ports."
        };
    }

    private static IReadOnlyList<PortScanResult> CreateNoValidPortsResults(
        IReadOnlyList<string> targetIpAddresses,
        IReadOnlyList<string> warnings)
    {
        return targetIpAddresses
            .Select(ipAddress => CreateNoPortsResult(ipAddress, warnings))
            .ToArray();
    }

    private void LogPortScanStarted(
        IReadOnlyList<string> targetIpAddresses,
        IReadOnlyList<int> portsToCheck)
    {
        _logger.LogInformation("Port Scan started.");
        _logger.LogInformation("Target IP addresses received: {TargetIpAddressCount}", targetIpAddresses.Count);
        _logger.LogInformation("Port list loaded: {PortCount}", portsToCheck.Count);
    }

    private void LogPortScanCompleted(IReadOnlyList<PortScanResult> results)
    {
        var openPortCount = results.Sum(result => result.OpenPorts.Count);

        _logger.LogInformation("Port Scan completed.");
        _logger.LogInformation("Open ports found: {OpenPortCount}", openPortCount);
    }

    private static IReadOnlyList<string> NormalizeIpAddresses(IEnumerable<ReconForgeIpAddress> resolvedIpAddresses)
    {
        return resolvedIpAddresses
            .Select(ipAddress => ipAddress.Address.Trim())
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private IReadOnlyList<int> BuildPortList(IEnumerable<int>? ports, List<string> warnings)
    {
        var sourcePorts = ports ?? DefaultPorts;
        var validPorts = new List<int>();
        var seenPorts = new HashSet<int>();

        foreach (var port in sourcePorts)
        {
            if (port is < 1 or > 65535)
            {
                _logger.LogWarning("Invalid port ignored: {Port}", port);
                warnings.Add($"Invalid port ignored: {port}");
                continue;
            }

            if (seenPorts.Add(port))
            {
                validPorts.Add(port);
            }
            else
            {
                _logger.LogInformation("Duplicate port removed: {Port}", port);
            }
        }

        return validPorts;
    }

    private static PortScanResult CreateNoPortsResult(string ipAddress, IReadOnlyList<string> warnings)
    {
        var now = DateTimeOffset.UtcNow;

        return new PortScanResult(
            ipAddress,
            now,
            now,
            PortScanStatus.NoOpenPorts,
            "No valid ports were available to check.",
            Array.Empty<Port>(),
            Array.Empty<string>(),
            warnings);
    }

    private static PortScanStatus DetermineStatus(
        IReadOnlyCollection<Port> checkedPorts,
        IReadOnlyCollection<string> errors)
    {
        if (checkedPorts.Count == 0)
        {
            return PortScanStatus.NoOpenPorts;
        }

        if (errors.Count > 0 || checkedPorts.Any(port => port.State == PortState.Error))
        {
            return checkedPorts.Any(port => port.State == PortState.Open || port.State == PortState.Closed)
                ? PortScanStatus.PartialSuccess
                : PortScanStatus.Failed;
        }

        return checkedPorts.Any(port => port.State == PortState.Open)
            ? PortScanStatus.Success
            : PortScanStatus.NoOpenPorts;
    }

    private void LogPortState(string ipAddress, Port port, string? errorMessage)
    {
        switch (port.State)
        {
            case PortState.Open:
                _logger.LogInformation("Port open: {IpAddress}:{Port}", ipAddress, port.Number);
                break;
            case PortState.Closed:
                _logger.LogInformation("Port closed: {IpAddress}:{Port}", ipAddress, port.Number);
                break;
            case PortState.Timeout:
                _logger.LogInformation("Port timeout: {IpAddress}:{Port}", ipAddress, port.Number);
                break;
            case PortState.Error:
                _logger.LogWarning(
                    "Port check error for {IpAddress}:{Port}: {Reason}",
                    ipAddress,
                    port.Number,
                    errorMessage ?? "Unknown error");
                break;
        }
    }

    private static string GetServiceName(int port)
    {
        return ServiceNames.TryGetValue(port, out var name)
            ? name
            : "Unknown";
    }

    private sealed record CheckedPortResult(
        Port Port,
        string? ErrorMessage);
}
