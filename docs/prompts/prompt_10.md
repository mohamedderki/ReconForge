Prompt 10 – Make terminal result display the default and export optional

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not remove the existing export system. Keep the existing scan workflow, services, models, logging and tests unless changes are required to correct the CLI behavior.

Problem:
The current implementation focuses too much on exporting scan results to files. This is not the intended default behavior.

Expected CLI behavior:
When the user runs:

`reconforge --scan example.com`

ReconForge should display the scan results directly in the terminal in a clean, structured and readable way.

Export should happen only when the user explicitly requests it, for example:

`reconforge --scan example.com --export --format json --output result.json`

Goal:
Change the CLI behavior so that terminal output is the default result presentation. Export should be optional and only executed when the user explicitly enables it.

Please implement the following:

1. Default terminal output

The default behavior must be:

`reconforge --scan example.com`

This should:

* run the scan workflow
* collect the structured scan result
* display the results in the terminal
* not create any export file
* not call any exporter
* return exit code 0 on success

2. Add explicit export option

Add a boolean CLI option such as:

`--export`

Export should only run when this option is provided.

Example:

`reconforge --scan example.com --export --format json --output result.json`

Rules:

* If `--export` is not provided, do not export anything.
* If `--export` is provided, use the selected export format.
* If `--export` is provided and no format is selected, use `json` as the default format.
* If `--export` is provided and no output path is selected, generate a default output file name.
* If `--format` is provided without `--export`, show a clear warning that the format option is ignored unless export is enabled.
* If `--output` is provided without `--export`, show a clear warning or validation message that output requires export mode.

3. Keep export system

Do not remove the existing export system.

Keep:

* `IResultExporter<T>`
* `JsonResultExporter`
* `CsvResultExporter`
* `XmlResultExporter`
* `HtmlResultExporter`
* `YamlResultExporter`
* `ResultExporterFactory`

The export system should remain available, but it must not run by default.

4. Create terminal result renderer

Create a dedicated presentation class in the CLI layer, for example:

`ScanResultConsoleRenderer`

or:

`TerminalResultRenderer`

This class should be responsible only for displaying scan results in the terminal.

Do not put terminal rendering logic inside:

* Domain Scan service
* Subdomain Discovery service
* IP Resolution service
* Port Scanner service
* Exporters

5. Use Spectre.Console for structured output

Use Spectre.Console to render the result in a professional and readable way.

The terminal output should include sections such as:

* Target domain
* Normalized domain
* Scan status
* Subdomains
* Resolved IP addresses
* Port scan results
* Summary
* Warnings or errors, if available

Use suitable Spectre.Console elements such as:

* Panels
* Tables
* Trees
* Rules
* Markup
* Status messages

Keep the output clean. Do not make it overloaded.

6. Suggested terminal layout

The output may look conceptually like this:

Title:
`ReconForge Scan Results`

Section 1:
Target Information

* Original input
* Normalized domain
* Scan start time
* Scan end time
* Duration
* Status

Section 2:
Subdomains
Table columns:

* Subdomain
* Source
* DiscoveredAt

Section 3:
Resolved IP Addresses
Table columns:

* Host
* IP Address
* Address Family
* IsPrivate
* IsLoopback

Section 4:
Port Scan Results
Table columns:

* IP Address
* Port
* Protocol
* State
* Service Name
* Response Time

Section 5:
Summary

* Number of subdomains
* Number of resolved IP addresses
* Number of checked ports
* Number of open ports
* Number of warnings or errors

If no results exist in a section, display a clear message such as:
`No subdomains found.`
or:
`No open ports detected.`

7. Console output and export together

If export is enabled, ReconForge should still show the terminal results first or at least show a useful terminal summary.

Recommended behavior:

* Always display scan results in the terminal.
* If `--export` is provided, additionally export the results to the selected file format.
* After export, show a short message:
  `Results exported successfully to: <path>`

8. Logging

Update logging behavior if needed.

Log:

* scan started
* terminal rendering started
* terminal rendering completed
* export requested
* export skipped because not requested
* export completed
* export failed

Do not log the full terminal table output.
Do not log the full exported content.

9. CLI options

Update the CLI settings so they clearly represent the intended behavior.

Useful options:

* `--scan <domain>`: starts scan
* `--verbose`: enables detailed output/logging
* `--export`: enables export
* `--format <format>`: selected export format, default json
* `--output <path>`: output file path for export

Important:
`--format` and `--output` should not cause export by themselves unless the project intentionally decides to infer export mode. Prefer explicit export mode with `--export`.

10. Tests

Add or update xUnit tests for the corrected behavior.

Include tests for:

* running scan without export does not call exporter
* running scan without export displays terminal result
* running scan with `--export` calls exporter
* `--format` without `--export` does not export
* `--output` without `--export` is handled clearly
* terminal renderer can render a result without throwing exceptions
* empty subdomain list is displayed gracefully
* empty IP list is displayed gracefully
* empty port results are displayed gracefully
* existing export tests still pass

If console rendering is hard to test directly, make the renderer return or write through an abstraction that can be tested.

11. Code quality

* Keep terminal rendering in the CLI/presentation layer.
* Keep scan services free from UI logic.
* Keep exporters free from scan logic.
* Do not duplicate result formatting logic unnecessarily.
* Keep the output readable and maintainable.
* Do not break existing scan workflow.
* Do not break existing export functionality.
* Do not introduce unrelated features.

Expected result:
After this step, the default command:

`reconforge --scan example.com`

should display the scan result directly in the terminal in a structured and professional way.

No export file should be created by default.

Export should only happen when explicitly requested:

`reconforge --scan example.com --export --format json --output result.json`

This change should make ReconForge behave like a real CLI tool: terminal output by default, optional file export when needed.
