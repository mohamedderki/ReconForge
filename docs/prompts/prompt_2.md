Prompt 2 – CLI foundation with executable-style command options and logging

Continue from the current ReconForge solution you created in the previous step.

Do not recreate the project structure from scratch. Keep the existing architecture, projects, folders, namespaces and responsibilities. Only extend the current codebase in a clean and consistent way.

Now implement the first real foundation for the application: a command-line interface that can later be used like a real CLI tool.

Goal:
Create a working CLI foundation using Spectre.Console.Cli and integrate Microsoft.Extensions.Logging.

The intended final usage should look like this:

`reconforge --scan example.com`

Additional options should also be possible, for example:

`reconforge --scan example.com --verbose --format json --output result.json`

During development it is also acceptable to run it with:

`dotnet run -- --scan example.com --verbose --format json`

Important:
Do not design the CLI mainly as `reconforge scan example.com`.
For this project, the first version should support option-based execution like `reconforge --scan example.com`.

Please implement the following:

1. Configure the CLI application.

   * Set up the main command-line application in the CLI project.
   * Support a `--scan` option that accepts a domain value.
   * Add useful additional options:

     * `--verbose`
     * `--output`
     * `--format`, with `json` as the default value.
   * The CLI should show helpful messages when the input is missing or invalid.

2. Implement command-line settings.

   * Create a settings class for the root command or main command.
   * The settings should contain:

     * ScanTarget or Domain value from `--scan`
     * Verbose flag
     * Output path
     * Export format
   * Use Spectre.Console.Cli attributes where appropriate.
   * Keep the settings simple and readable.

3. Implement the main execution logic.

   * If `--scan` is provided, validate the domain.
   * If the domain is empty or invalid, show a clear error message and return exit code `1`.
   * If the domain is valid, call the existing or placeholder scan service.
   * Return exit code `0` for success.
   * Return exit code `1` for validation or execution errors.

4. Integrate logging.

   * Use Microsoft.Extensions.Logging.
   * Log important steps:

     * application started
     * scan option received
     * domain received
     * validation failed
     * scan service called
     * command completed
     * unexpected exception.
   * Use suitable log levels:

     * Information for normal workflow
     * Warning for validation problems
     * Error for exceptions.
   * If `--verbose` is used, show more detailed log output.

5. Keep the scan logic simple for now.

   * Do not implement real scanning yet.
   * The scan service may return a placeholder result.
   * The focus of this step is CLI structure, option parsing, validation, dependency injection and logging.

6. Add simple console output.

   * Use Spectre.Console where useful.
   * Show clear success and error messages.
   * Keep the output simple and professional.

7. Add or update tests.

   * Add basic xUnit tests for:

     * missing scan target
     * empty domain validation
     * successful execution with a valid domain
     * placeholder scan service returning a result.

Important constraints:

* Do not implement real network scanning in this step.
* Do not implement exploit code, brute-force behavior, denial-of-service behavior, stealth mechanisms, malware-like behavior or unauthorized access functionality.
* Keep the tool educational and suitable only for authorized testing.
* Keep the code modular and easy to extend.
* Separate CLI handling from business logic.
* Do not over-engineer.

Expected result:
After this step, I should be able to run the tool during development like this:

`dotnet run -- --scan example.com --verbose --format json`

And the planned final executable usage should be:

`reconforge --scan example.com --verbose --format json --output result.json`

The command should validate the input, write useful logs, call the placeholder scan service and show a simple result message.
