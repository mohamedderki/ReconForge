Prompt 3 – Implement Domain Scan feature

Continue from the current ReconForge solution and the CLI foundation created in the previous steps.

Do not recreate the project structure. Keep the existing architecture, projects, namespaces, dependency injection setup, CLI option style and logging setup. Extend the current codebase only where necessary.

Goal:
Implement the first real feature of ReconForge: Domain Scan.

Important:
In this step, Domain Scan should only handle the domain as a scan target. It should validate, normalize and prepare the domain for later steps. Do not implement Subdomain Discovery, IP Resolution or Port Scanning yet. These will be implemented in later prompts.

The CLI should continue to support this usage:

`reconforge --scan example.com`

During development it should also work with:

`dotnet run -- --scan example.com --verbose --format json`

Please implement the following:

1. Domain model

   * Create or update a Domain model in the appropriate Core layer.
   * The model should contain useful properties such as:

     * OriginalInput
     * NormalizedName
     * CreatedAt
     * IsValid
   * Keep the model simple and extendable.

2. Domain validation

   * Create a domain validation service or improve the existing placeholder.
   * The validator should check:

     * input is not null or empty
     * input does not contain spaces
     * input has a valid domain-like format
     * input does not include unsupported schemes like `http://` or `https://` unless you decide to normalize them safely
   * If a URL is provided, either reject it with a clear message or normalize it to the host part only.
   * Choose the cleaner solution and explain it briefly.

3. Domain normalization

   * Normalize the domain before scanning.
   * Examples:

     * trim whitespace
     * convert to lowercase
     * remove trailing slash if appropriate
     * optionally remove `http://` or `https://` and keep only the host
   * Make sure the normalized value is used consistently.

4. Domain scan service

   * Create or update a service such as `IDomainScanService` and `DomainScanService`.
   * The service should:

     * accept a domain input
     * validate and normalize the domain
     * create a domain scan result
     * return a structured result object
   * Do not perform real network scanning yet.
   * Do not resolve IP addresses yet.
   * Do not discover subdomains yet.
   * Do not scan ports yet.

5. Result model

   * Create or update a result model for the domain scan.
   * The result should include:

     * TargetDomain
     * NormalizedDomain
     * StartedAt
     * FinishedAt
     * Status
     * Message
     * ValidationErrors if any
   * Use clear status values, for example:

     * Success
     * Failed
     * InvalidInput

6. CLI integration

   * Connect the existing `--scan` option to the new Domain Scan service.
   * If the domain is valid, show a clear success message.
   * If the domain is invalid, show a clear error message and return exit code `1`.
   * On success, return exit code `0`.
   * Keep the CLI output simple and readable.

7. Logging

   * Use the existing Microsoft.Extensions.Logging setup.
   * Log important steps:

     * domain scan started
     * original domain input received
     * normalized domain value
     * validation success
     * validation failure
     * domain scan completed
     * unexpected exceptions
   * Use suitable log levels:

     * Information for normal workflow
     * Warning for validation problems
     * Error for exceptions

8. Tests

   * Add or update xUnit tests for the Domain Scan feature.
   * Include tests for:

     * valid domain input, for example `example.com`
     * uppercase input normalization, for example `Example.COM`
     * input with leading and trailing spaces
     * empty input
     * input with spaces inside
     * invalid domain input
     * optional URL normalization or rejection, depending on your implementation decision
   * Add tests for the DomainScanService result status.
   * Keep tests understandable and focused.

9. Code quality

   * Keep CLI logic separated from domain scan logic.
   * Keep validation logic separated from command handling.
   * Use interfaces where useful.
   * Avoid unnecessary complexity.
   * Keep class and method names clear.
   * Do not add unrelated features.

Important constraints:

* Do not implement Subdomain Discovery in this step.
* Do not implement IP Resolution in this step.
* Do not implement Port Scanning in this step.
* Do not implement exploit code, brute-force behavior, denial-of-service behavior, stealth mechanisms, malware-like behavior or unauthorized access functionality.
* The tool must remain educational and suitable only for authorized testing.

Expected result:
After this step, the command

`reconforge --scan example.com`

should validate and normalize the domain, create a structured domain scan result, log the important steps and display a clear message to the user.

The implementation should prepare the codebase for the next feature, which will be Subdomain Discovery or IP Resolution, without mixing those responsibilities into the Domain Scan feature.
