Prompt 1 – Projektstruktur und Grundarchitektur für ReconForge erstellen

You are an experienced senior .NET software architect and C# developer.

I am building a modular command-line reconnaissance tool called ReconForge as part of a software engineering university project. The project should be implemented in C# with .NET and should follow a clean, modular structure. The application is intended for learning and demonstration purposes only.

Please create the initial project structure and basic architecture for ReconForge.

Technical stack:

* C# with .NET
* Spectre.Console.Cli for the command-line interface
* Microsoft.Extensions.Logging for logging
* System.Text.Json for later JSON export
* xUnit for unit tests
* GitHub Actions will be added later for CI/CD

Project goals:
ReconForge should later support the following functions:

* Domain Scan
* Subdomain Discovery
* IP Resolution
* Port Scan
* JSON Export
* CLI Interface
* Logging
* Unit Testing

Important architectural requirements:

* The project must be modular and easy to extend.
* Each module should have a clear responsibility.
* The CLI layer must be separated from the business logic.
* Scanning logic must be separated from export logic.
* Interfaces should be used where they make sense.
* The structure should support unit testing.
* Do not implement all features completely yet.
* Focus on a clean foundation and project skeleton.

Please create a suitable solution structure with separate projects or folders for:

* ReconForge.Cli
* ReconForge.Core
* ReconForge.Scanning
* ReconForge.Export
* ReconForge.Infrastructure
* ReconForge.Tests

For each project or folder, briefly explain its responsibility.

Please include:

1. Recommended folder structure
2. Required .NET projects
3. Basic interfaces and placeholder classes
4. A minimal CLI entry point using Spectre.Console.Cli
5. Basic dependency registration if useful
6. Basic logging setup
7. One simple placeholder command, for example "scan"
8. One simple xUnit test project setup
9. Short explanation of the architecture decisions

Constraints:

* Keep the code simple and understandable.
* Do not add unnecessary complexity.
* Do not generate a full production-ready security scanner.
* Do not implement aggressive scanning behavior.
* Do not include real attack functionality.
* The tool is for educational and authorized testing only.
* Make sure the code can be extended step by step in later tasks.
* Use meaningful names for classes, interfaces, namespaces and folders.
* Add short comments only where they improve understanding.

Expected result:
I want a clean initial .NET solution for ReconForge that can be used as the foundation for the next implementation steps.
