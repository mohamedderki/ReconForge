# ReconForge

**ReconForge** is a modular .NET command-line reconnaissance tool built for educational and authorized testing scenarios.

The project focuses on clean software engineering practices such as modular architecture, structured CLI interaction, safe scan behavior, testability, exportability and maintainable code organization.

> **Disclaimer:** ReconForge is intended for learning, documentation and authorized testing only. It does not implement exploit code, brute-force attacks, stealth techniques, vulnerability exploitation, denial-of-service behavior or unauthorized access functionality.

---

## Features

ReconForge currently supports:

* Domain validation and normalization
* Controlled subdomain discovery
* IP resolution using .NET DNS APIs
* Safe TCP port checks with a small default port list
* Structured terminal output by default
* Optional export to multiple formats
* Logging
* Unit tests
* Modular architecture

---

## Technology Stack

| Technology                   | Usage                                        |
| ---------------------------- | -------------------------------------------- |
| C# / .NET                    | Main implementation platform                 |
| Spectre.Console.Cli          | Command-line interface and argument handling |
| Spectre.Console              | Structured and colored terminal output       |
| Microsoft.Extensions.Logging | Application logging                          |
| System.Text.Json             | JSON serialization                           |
| xUnit                        | Unit testing                                 |

---

## Project Structure

```text
ReconForge.sln

src/
  ReconForge.Cli/             CLI entry point, command handling and terminal rendering
  ReconForge.Core/            Shared domain models, contracts and abstractions
  ReconForge.Scanning/        Domain scan, subdomain discovery, IP resolution and port scanning
  ReconForge.Export/          Export system for JSON, CSV, XML, HTML and YAML
  ReconForge.Infrastructure/  Dependency registration and composition helpers

tests/
  ReconForge.Tests/           xUnit tests for core behavior, services, workflow and exporters
```

---

## Architecture Overview

ReconForge is designed as a modular CLI application.

The **CLI layer** is responsible for command-line input, terminal output and user interaction. It stays thin and does not contain business logic.

The **Core layer** contains shared models, interfaces and abstractions. These contracts help keep the application independent from concrete implementations.

The **Scanning layer** contains the scan-related functionality, including domain handling, subdomain discovery, IP resolution and safe port checking.

The **Export layer** is separated from scanning. Exporters receive structured scan results and write them to the selected output format.

The **Infrastructure layer** wires the application together through dependency injection.

The **Tests project** verifies the behavior of the most important modules. DNS and TCP behavior can be tested through fakes or abstractions, so unit tests do not depend on external DNS or real network connections.

---

## CLI Usage

During development, ReconForge can be started with:

```powershell
dotnet run --project src\ReconForge.Cli\ReconForge.Cli.csproj -- --scan example.com
```

With verbose output:

```powershell
dotnet run --project src\ReconForge.Cli\ReconForge.Cli.csproj -- --scan example.com --verbose
```

Export results to JSON:

```powershell
dotnet run --project src\ReconForge.Cli\ReconForge.Cli.csproj -- --scan example.com --export --format json --output result.json
```

Export results to HTML:

```powershell
dotnet run --project src\ReconForge.Cli\ReconForge.Cli.csproj -- --scan example.com --export --format html --output reports\example.html
```

If the application is published and added to the system `PATH`, it can be called directly:

```powershell
reconforge --scan example.com
```

---

## Default Behavior

By default, ReconForge displays scan results directly in the terminal.

Example:

```powershell
reconforge --scan example.com
```

This command:

* validates and normalizes the input domain
* performs controlled subdomain discovery
* resolves IP addresses
* checks a small default TCP port list
* displays the results in the terminal
* does not create export files automatically

File export only happens when the `--export` option is explicitly used.

---

## Scan Workflow

The scan workflow is executed in several controlled steps:

1. Domain validation and normalization
2. Subdomain discovery
3. IP resolution
4. Safe TCP port check
5. Terminal result rendering
6. Optional file export

Plain domains such as:

```text
example.com
```

are accepted.

URLs such as:

```text
https://www.example.com/path
```

are normalized safely. The normalized host becomes:

```text
www.example.com
```

and the root domain used for generated subdomain candidates becomes:

```text
example.com
```

---

## Subdomain Discovery

Subdomain discovery is intentionally small and controlled.

Default prefixes:

```text
www
mail
api
dev
test
staging
admin
```

For `example.com`, ReconForge may generate candidates such as:

```text
www.example.com
mail.example.com
api.example.com
dev.example.com
test.example.com
staging.example.com
admin.example.com
```

ReconForge does not use large wordlists, aggressive brute forcing, crawling, scraping or third-party reconnaissance APIs.

---

## IP Resolution

ReconForge resolves the root host and generated subdomain candidates using .NET DNS APIs.

The result stores:

* related host
* resolved IP address
* address family
* private address flag
* loopback flag

Failed DNS lookups are stored separately, so one failed host does not stop the whole scan.

---

## Port Scanning

Port scanning is intentionally limited and safe.

ReconForge uses TCP connect checks with short timeouts and a small default port list:

```text
21, 22, 25, 53, 80, 110, 143, 443, 587, 993, 995, 3306, 5432, 8080
```

ReconForge does not scan all 65535 ports by default and does not implement stealth scanning, SYN scanning, UDP scanning, vulnerability checks or exploit behavior.

By default, the terminal output focuses on open ports. Detailed port results can be shown with verbose output.

---

## Export System

Terminal output is the default.

File export is optional and must be enabled explicitly:

```powershell
reconforge --scan example.com --export --format json --output result.json
```

Supported export formats:

```text
json
csv
xml
html
yaml
```

The export system uses a generic exporter abstraction and a factory-based design. This keeps the scan workflow independent from concrete export formats and makes it easier to add new exporters later.

If export is enabled and no output path is provided, ReconForge can generate a default file name using the normalized domain, timestamp and matching file extension.

---

## Build and Test

ReconForge can be built and tested locally with the .NET CLI.

Restore dependencies:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build ReconForge.sln
```

Run tests:

```powershell
dotnet test ReconForge.sln
```

Build in Release configuration:

```powershell
dotnet build ReconForge.sln --configuration Release
```

---

## Test Structure

Unit tests are organized by module:

```text
tests/
  ReconForge.Tests/
    Domain/
    Subdomains/
    IpResolution/
    PortScanning/
    Export/
    Cli/
    Workflow/
    TestData/
    TestDoubles/
```

The tests focus on:

* domain validation and normalization
* subdomain generation
* IP resolution behavior
* port scan result handling
* export format selection
* exporter output
* CLI behavior
* scan workflow order

External behavior such as DNS lookups and TCP checks can be tested through fakes or abstractions. This keeps unit tests stable and independent from real network conditions.

---

## Safety Scope

ReconForge is intentionally limited.

It does not implement:

* exploit execution
* vulnerability exploitation
* brute-force login attacks
* denial-of-service behavior
* stealth scanning
* firewall bypassing
* malware-like behavior
* unauthorized access functionality
* aggressive full-range scanning by default

The tool is built for educational purposes, software engineering documentation and authorized testing only.

---

## Current Status

ReconForge currently provides a working modular foundation with CLI execution, terminal result rendering, safe scan steps, optional multi-format export and unit test support.

The project can be extended step by step because the main responsibilities are separated into dedicated modules.
