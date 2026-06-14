Prompt 8 – Improve Unit Testing Support and Test Structure

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change the existing CLI behavior or scan workflow unless a change is required to improve testability without changing functionality.

Goal:
Improve the unit testing support for ReconForge and make the existing codebase easier to test.

The current application already includes:

* CLI foundation
* Logging
* Domain Scan
* Subdomain Discovery
* IP Resolution
* Safe Port Scan
* Export system with JSON, CSV, XML, HTML and YAML
* Generic exporter interface
* ResultExporterFactory

Now focus on testability, test organization and quality assurance.

Scope of this step:

* Review the current test project.
* Improve the structure of the test project.
* Add missing unit tests for important services.
* Avoid unstable tests caused by real DNS or real network calls.
* Use abstractions or mocks where needed.
* Keep tests readable, focused and maintainable.

Please implement the following:

1. Review the existing test project

Inspect the current test project and check:

* project references
* namespaces
* test folder structure
* existing test classes
* duplicate or weak tests
* missing tests for important modules

Keep useful existing tests and improve them if needed. Do not delete tests unless they are broken, duplicated or no longer meaningful.

2. Organize tests by module

Structure the test project in a clear way, for example:

* Domain
* Subdomains
* IpResolution
* PortScanning
* Export
* Cli
* Workflow
* TestData

Each test class should focus on one service, model or behavior.

3. Domain tests

Add or improve tests for domain validation and normalization.

Cover:

* valid domain
* uppercase domain normalization
* leading and trailing whitespace
* empty input
* invalid input
* input with spaces
* URL input behavior, depending on the current implementation
* validation errors are returned clearly

4. Subdomain Discovery tests

Add or improve tests for Subdomain Discovery.

Cover:

* default prefixes generate expected subdomains
* generated subdomains contain the root domain
* duplicates are removed
* prefixes are normalized
* empty prefixes are ignored
* result status is correct
* subdomain count is correct
* no IP addresses are added in this service
* no port information is added in this service

5. IP Resolution tests

Review the IP Resolution implementation.

If the service currently performs direct DNS calls, introduce or improve an abstraction such as:

`IDnsLookupClient`

Use this abstraction so unit tests do not depend on real internet access or external DNS behavior.

Add tests for:

* successful DNS resolution
* failed host resolution
* duplicate IP addresses are removed
* failed hosts are stored separately
* empty host list is handled gracefully
* service continues when one host fails
* result status is correct
* no port scanning is performed in IP Resolution

6. Port Scan tests

Review the Port Scan implementation.

If the service currently performs direct TCP connections, introduce or improve an abstraction such as:

`ITcpConnectionChecker`

Use this abstraction so unit tests do not depend on real network connections.

Add tests for:

* default port list is used
* duplicate ports are removed
* invalid port numbers are ignored
* empty IP address list is handled gracefully
* open port result is handled correctly
* closed port result is handled correctly
* timeout result is handled correctly
* result status is correct
* no DNS resolution is performed in Port Scan
* no export logic is performed in Port Scan

7. Export tests

Add or improve tests for the export system.

Cover:

* ResultExporterFactory returns the correct exporter for JSON
* ResultExporterFactory returns the correct exporter for CSV
* ResultExporterFactory returns the correct exporter for XML
* ResultExporterFactory returns the correct exporter for HTML
* ResultExporterFactory returns the correct exporter for YAML
* unsupported export format is rejected clearly
* output file extension matches the selected format
* each exporter creates a file
* JSON output contains expected domain data
* CSV output contains expected rows
* XML output is well-formed
* HTML output contains expected sections
* YAML output contains expected fields
* exporters do not modify the scan result
* exporters do not perform scanning

Use temporary directories for file system tests and clean them up after the test.

8. Workflow tests

Add tests for the scan workflow if the architecture allows it.

Cover:

* workflow calls Domain Scan before Subdomain Discovery
* workflow calls Subdomain Discovery before IP Resolution
* workflow calls IP Resolution before Port Scan
* workflow calls Export after scan steps
* workflow handles validation failure correctly
* workflow does not continue when domain validation fails
* workflow returns a structured result

Use fake or mock services where needed.

9. CLI tests

If practical, add simple tests for CLI-related behavior.

Cover:

* missing scan argument returns an error
* invalid domain returns an error
* valid scan input triggers the workflow
* unsupported export format is rejected
* verbose option does not break execution

Do not overcomplicate CLI tests. If direct command execution is difficult, test the command class or settings validation instead.

10. Test data builders

If many tests need sample scan results, create simple test data helpers or builders.

Examples:

* ScanResultTestFactory
* SampleScanResultBuilder
* TestDataFactory

Keep them simple. Do not introduce unnecessary complexity.

11. Code quality for tests

Apply these rules:

* test names should describe the expected behavior
* tests should be small and focused
* avoid testing too many things in one test
* avoid relying on real network access
* avoid relying on the current date/time if it makes tests unstable
* use deterministic test data
* keep Arrange, Act, Assert structure readable

12. Build and test execution

Make sure the project can be checked with:

`dotnet restore`
`dotnet build`
`dotnet test`

All tests should pass.

If something fails, fix the problem and explain the reason.

13. Summary after implementation

At the end, provide a short summary:

* which test areas were added or improved
* which abstractions were introduced for testability
* which files were changed
* whether `dotnet build` passes
* whether `dotnet test` passes
* what the next logical step should be

Expected result:
After this step, ReconForge should have a clean and meaningful unit test structure. Important modules should be testable without relying on real DNS lookups or real network connections. The tests should support the non-functional requirement of testability and improve the maintainability of the project.
