### Vorher
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
        var results = new List<PortScanResult>();

        _logger.LogInformation("Port Scan started.");
        _logger.LogInformation("Target IP addresses received: {TargetIpAddressCount}", targetIpAddresses.Count);
        _logger.LogInformation("Port list loaded: {PortCount}", portsToCheck.Count);

        if (targetIpAddresses.Count == 0)
        {
            _logger.LogWarning("Port Scan completed with no IP addresses to scan.");
            return results;
        }

        if (portsToCheck.Count == 0)
        {
            _logger.LogWarning("Port Scan completed with no valid ports to check.");
            return targetIpAddresses
                .Select(ipAddress => CreateNoPortsResult(ipAddress, warnings))
                .ToArray();
        }

        foreach (var ipAddress in targetIpAddresses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startedAt = DateTimeOffset.UtcNow;
            var checkedPorts = new List<Port>();
            var errors = new List<string>();
            var resultWarnings = new List<string>(warnings);

            foreach (var portNumber in portsToCheck)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Port check started: {IpAddress}:{Port}", ipAddress, portNumber);

                try
                {
                    var check = await _connectionChecker.CheckAsync(
                        ipAddress,
                        portNumber,
                        timeoutMs,
                        cancellationToken);

                    var port = new Port(
                        portNumber,
                        TcpProtocol,
                        GetServiceName(portNumber),
                        check.State,
                        DateTimeOffset.UtcNow,
                        check.ResponseTimeMs);

                    checkedPorts.Add(port);
                    LogPortState(ipAddress, port, check.ErrorMessage);

                    if (check.State == PortState.Error)
                    {
                        errors.Add($"{ipAddress}:{portNumber}: {check.ErrorMessage ?? "Unknown error"}");
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogError(exception, "Unexpected exception during port check for {IpAddress}:{Port}", ipAddress, portNumber);
                    errors.Add($"{ipAddress}:{portNumber}: {exception.Message}");
                    checkedPorts.Add(new Port(
                        portNumber,
                        TcpProtocol,
                        GetServiceName(portNumber),
                        PortState.Error,
                        DateTimeOffset.UtcNow,
                        ResponseTimeMs: null));
                }
            }

            var status = DetermineStatus(checkedPorts, errors);
            var message = status switch
            {
                PortScanStatus.Success => "Port scan completed and at least one open port was found.",
                PortScanStatus.PartialSuccess => "Port scan completed with errors.",
                PortScanStatus.Failed => "Port scan failed for this target.",
                _ => "Port scan completed with no open ports."
            };

            results.Add(new PortScanResult(
                ipAddress,
                startedAt,
                DateTimeOffset.UtcNow,
                status,
                message,
                checkedPorts,
                errors,
                resultWarnings));
        }

        var openPortCount = results.Sum(result => result.OpenPorts.Count);

        _logger.LogInformation("Port Scan completed.");
        _logger.LogInformation("Open ports found: {OpenPortCount}", openPortCount);

        return results;
    }

________________________________________________________________________________________________________________________________________________________________________________________________________________


### Nachher
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

private sealed record CheckedPortResult(
    Port Port,
    string? ErrorMessage);