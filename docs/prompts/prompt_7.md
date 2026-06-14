Prompt 7 – Implement Generic Export System with Factory Pattern

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change the existing CLI interface unless a small adjustment is required for the existing `--format` and `--output` options.

Goal:
Implement a clean, generic and extendable export system for ReconForge using a generic exporter abstraction and the Factory Pattern.

The export system should support these formats:

* JSON
* CSV
* XML
* HTML
* YAML

Scope of this step:

* Export the final structured scan result after the existing scan workflow.
* Keep scan logic separated from export logic.
* Use a generic interface so exporters can work with result objects in a reusable way.
* Use a Factory Pattern to select the correct exporter based on the selected format.
* Keep the design simple, testable and easy to extend.

Please implement the following:

1. Generic exporter interface

Create a generic interface such as:

`IResultExporter<T>`

The interface should define a clean export contract.

It should include:

* Supported export format
* File extension
* An asynchronous export method

Example concept:

`Task<ExportResult> ExportAsync(T data, string outputPath, CancellationToken cancellationToken);`

The interface should be generic so that it can export `ScanResult` now and possibly other result types in the future.

2. Export format model

Create an `ExportFormat` enum or value object.

It should support:

* Json
* Csv
* Xml
* Html
* Yaml

Add safe parsing from the CLI `--format` value.

Rules:

* Parsing must be case-insensitive.
* `json` should be the default format.
* Unsupported formats should produce a clear validation error.
* Do not silently fall back to another format when the user enters an invalid value.

3. Export result model

Create or update an `ExportResult` model.

It should contain:

* Success
* OutputPath
* Format
* StartedAt
* FinishedAt
* Message
* ErrorMessage, if needed

Keep the model simple and understandable.

4. Implement JsonResultExporter

Create `JsonResultExporter : IResultExporter<ScanResult>`.

Rules:

* Use `System.Text.Json`.
* Create readable indented JSON.
* Export the complete scan result.
* Include domain information, discovered subdomains, resolved IP addresses, port scan results, timestamps, status, summary data, warnings and errors if available.

5. Implement CsvResultExporter

Create `CsvResultExporter : IResultExporter<ScanResult>`.

Rules:

* Create a simple readable CSV file.
* The CSV should be usable in spreadsheet tools.
* Escape commas, quotes and line breaks correctly.
* Do not use unnecessary external packages unless clearly needed.
* Include important fields such as target domain, normalized domain, subdomain, IP address, port, port state, service name and timestamp.

6. Implement XmlResultExporter

Create `XmlResultExporter : IResultExporter<ScanResult>`.

Rules:

* Use standard .NET XML APIs.
* Create well-formed and readable XML.
* Escape special characters correctly.
* Keep the structure clear and understandable.

7. Implement HtmlResultExporter

Create `HtmlResultExporter : IResultExporter<ScanResult>`.

Rules:

* Create a simple static HTML report.
* Include sections for:

  * Target domain
  * Subdomains
  * IP addresses
  * Port scan results
  * Summary
* Use simple semantic HTML.
* Do not add JavaScript.
* Do not load external CSS or external resources.
* Escape dynamic values correctly.

8. Implement YamlResultExporter

Create `YamlResultExporter : IResultExporter<ScanResult>`.

Rules:

* Create a readable YAML file.
* Keep the YAML simple and understandable.
* If no YAML package is already used, either implement a simple safe YAML writer for the current scan result structure or use a lightweight common package only if it fits the project.
* Escape or quote strings where necessary.

9. ResultExporterFactory

Create a factory such as `ResultExporterFactory`.

The factory should:

* receive the requested `ExportFormat`
* return the correct `IResultExporter<ScanResult>`
* keep exporter selection out of the CLI and scan services
* provide a clear error for unsupported formats

The scan workflow should not contain long if/else chains for exporter selection. The factory should be responsible for choosing the correct exporter.

10. Dependency injection

Register all exporters and the factory in the existing dependency injection setup.

Rules:

* Do not hard-code exporter creation inside the scan workflow.
* Do not mix export logic with CLI logic.
* The CLI may read the selected format, but the factory should select the exporter.
* Keep the setup simple and readable.

11. Output path handling

Use the existing `--output` option.

Rules:

* If an output path is provided, use it.
* If no output path is provided, generate a default file name.
* The default file name should include the normalized domain and a timestamp.
* The file extension must match the selected export format.
* Create the output directory if it does not exist.
* Handle invalid paths gracefully.
* Do not overwrite existing files silently unless this behavior is clearly intended.

Example default file names:

* reconforge-example-com-2026-06-13.json
* reconforge-example-com-2026-06-13.csv
* reconforge-example-com-2026-06-13.xml
* reconforge-example-com-2026-06-13.html
* reconforge-example-com-2026-06-13.yaml

12. Integration into the scan workflow

Update the existing scan workflow so that after the current scan steps are completed, the final result can be exported in the selected format.

The workflow should be:

* Domain Scan
* Subdomain Discovery
* IP Resolution
* Port Scan
* Export selected format

Keep responsibilities separated:

* Scan services must not write export files directly.
* Exporters must only handle export logic.
* The factory must select the correct exporter.
* The CLI should coordinate the workflow and display messages.
* Export logic must not perform scanning.
* Export logic must not modify scan results.

13. Console output

Update the console output so the user can see the export result.

Show:

* selected export format
* output path
* success message
* error message if export fails

Keep the output readable and professional.

14. Logging

Use the existing logging setup.

Log:

* Export started
* selected export format
* exporter selected by factory
* output path selected
* output directory created, if applicable
* serialization or report generation started
* file written successfully
* unsupported format requested
* export failed
* export completed

Use:

* Information for normal workflow
* Warning for unsupported formats or unusual situations
* Error for exceptions

15. Tests

Add or update xUnit tests for the export system.

Include tests for:

* Factory returns JsonResultExporter for JSON
* Factory returns CsvResultExporter for CSV
* Factory returns XmlResultExporter for XML
* Factory returns HtmlResultExporter for HTML
* Factory returns YamlResultExporter for YAML
* Unsupported export format is rejected
* JsonResultExporter creates a JSON file
* CsvResultExporter creates a CSV file
* XmlResultExporter creates a valid XML file
* HtmlResultExporter creates a valid HTML report
* YamlResultExporter creates a YAML file
* Output file extension matches the selected format
* Exporters do not perform scanning
* Exporters do not modify the scan result
* Invalid output paths are handled gracefully if possible

If file system tests are used, write to a temporary test directory and clean it up after the test.

16. Code quality

* Keep the code modular.
* Use the generic exporter abstraction cleanly.
* Use the Factory Pattern cleanly.
* Keep each exporter focused on one format.
* Avoid unnecessary complexity.
* Do not introduce unrelated features.
* Do not break existing tests.
* Use clear class names, method names and namespaces.
* Keep the implementation easy to extend later.

Expected result:
After this step, ReconForge should be able to export the final scan result in JSON, CSV, XML, HTML or YAML.

The selected format should come from the existing `--format` option, for example:

* `--format json`
* `--format csv`
* `--format xml`
* `--format html`
* `--format yaml`

The implementation should use `IResultExporter<T>` and `ResultExporterFactory` so that future export formats can be added without changing the scan workflow.
