Prompt 6 – Implement Safe Port Scan

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change the existing CLI interface or command-line options. Keep the current `--scan` workflow as the entry point and extend only the internal scan workflow by adding a safe Port Scan step after the existing IP Resolution step.

Goal:
Implement the next feature of ReconForge: a simple and controlled Port Scan.

Scope of this step:

* Use the IP addresses resolved by the IP Resolution step.
* Check only a small predefined list of common ports.
* Detect whether a port is open, closed or timed out.
* Return port scan results as part of the structured scan result.
* Keep the implementation simple, safe, controlled and educational.

Do not implement:

* aggressive scanning
* full port range scanning by default
* stealth scanning
* SYN scanning
* UDP scanning
* vulnerability checks
* exploit code
* brute-force behavior
* denial-of-service behavior
* evasion techniques
* malware-like behavior
* unauthorized access functionality

Please implement the following:

1. Port model

Create or update a `Port` model in the appropriate Core layer.

The model should contain:

* Number
* Protocol
* ServiceName
* State
* CheckedAt
* ResponseTimeMs, if available

Use clear port states such as:

* Open
* Closed
* Timeout
* Error

Keep the model simple and extendable.

2. Port scan result model

Create a result model such as `PortScanResult`.

It should contain:

* TargetIpAddress
* StartedAt
* FinishedAt
* Status
* Message
* CheckedPorts
* OpenPorts
* ClosedPorts
* TimedOutPorts
* Errors or warnings if needed

Use clear status values such as:

* Success
* PartialSuccess
* Failed
* NoOpenPorts

3. Port scanner service

Create an interface such as `IPortScannerService`.

Create an implementation such as `PortScannerService`.

The service should:

* accept a list of resolved IP addresses
* use a small predefined list of common TCP ports
* check whether each configured port is reachable
* use short timeouts
* handle connection failures gracefully
* return a structured result

Use a small default port list:

* 21
* 22
* 25
* 53
* 80
* 110
* 143
* 443
* 587
* 993
* 995
* 3306
* 5432
* 8080

Do not scan all 65535 ports by default.

4. Safe scanning behavior

The port scan must be limited and controlled.

Rules:

* Use TCP connect checks only.
* Use a configurable timeout, for example 1000 ms or 2000 ms.
* Avoid high concurrency.
* Do not retry aggressively.
* Do not hide or disguise the scan.
* Do not bypass firewalls or security systems.
* Continue scanning remaining ports if one port fails.
* Handle unreachable hosts gracefully.

5. Optional configuration

If it fits the current architecture, allow the port list and timeout to come from the existing scan configuration.

Rules:

* If no custom ports are configured, use the safe default list.
* Validate port numbers.
* Ignore invalid port numbers.
* Remove duplicate ports.
* Keep the configuration simple.

6. Integration into the scan workflow

Update the existing scan workflow so that after a successful IP Resolution step, the Port Scanner service is called.

The final scan result should now include:

* original target domain
* normalized domain
* discovered subdomains
* resolved IP addresses
* port scan results
* open ports count
* failed or timed-out checks, if any

Keep responsibilities separated:

* IP Resolution should only resolve hosts to IP addresses.
* Port Scanner should only check ports for already resolved IP addresses.
* Port Scanner should not perform DNS resolution.
* Port Scanner should not export results.
* CLI logic should not contain port scanning logic.

7. Console output

Update the existing console output so the user can see that the Port Scan was executed.

Show:

* number of resolved IP addresses
* checked ports
* open ports
* timeouts or errors, if any
* short summary message

Keep the output readable and professional. Use Spectre.Console only where it improves readability.

8. Logging

Use the existing Microsoft.Extensions.Logging setup.

Log the following steps:

* Port Scan started
* target IP addresses received
* port list loaded
* port check started
* port open
* port closed
* port timeout
* invalid port ignored
* Port Scan completed
* number of open ports
* unexpected exceptions

Use suitable log levels:

* Information for normal workflow
* Warning for timeouts, unreachable hosts or invalid ports
* Error for unexpected exceptions

9. Tests

Add or update xUnit tests for the Port Scan feature.

Include tests for:

* default port list is used when no custom ports are provided
* duplicate ports are removed
* invalid port numbers are ignored
* empty IP address list is handled gracefully
* result contains correct status
* result contains checked ports
* service does not perform DNS resolution
* service does not export results
* timeout handling works through an abstraction or mocked connection checker

Important:
Direct network calls can make unit tests unstable. If needed, introduce an abstraction such as `ITcpConnectionChecker` so port check behavior can be mocked in tests.

10. Code quality

* Keep the code modular.
* Use interfaces where useful.
* Keep models simple.
* Use clear class names and method names.
* Avoid unnecessary complexity.
* Do not introduce unrelated features.
* Do not break existing tests.
* Keep the implementation easy to extend in the next step.

Expected result:
After this step, the existing scan workflow should validate and normalize the domain, run Subdomain Discovery, resolve IP addresses, perform a safe and limited Port Scan, return a structured result, log the important steps and display a clear summary to the user.

The implementation should prepare the codebase for the next feature, which will be JSON Export, without mixing export logic into the Port Scan feature.
