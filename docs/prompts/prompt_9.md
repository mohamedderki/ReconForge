Prompt 9 – Add professional CLI banner, color theme and startup design

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change the scan workflow, export system, tests or existing CLI behavior unless a small change is required to display the banner cleanly.

Goal:
Add a professional startup banner and visual CLI identity for ReconForge using Spectre.Console.

The banner should make the tool recognizable, but it must not make the CLI noisy, unreadable or hard to test.

Before implementing, briefly explain the design idea:

* which colors you will use
* which parts of the banner will receive which colors
* why this color palette fits a reconnaissance CLI tool
* how you will keep the output readable on dark and light terminals
* how the banner can be disabled in tests and CI

Use this ASCII banner as the visual identity:

```text
                         .-.
                    ____/___\____
                   /   _\   /_   \
                  /___/  \_/  \___\
                       \  |  /
                        \ | /
        ________________ \|/ ________________
       /  __   __   __   / \   __   __   __  \
      /__/  \_/  \_/  \_/   \_/  \_/  \_/  \__\
     <____   H E R M E S   S I G N A L   ____>
      \  \__/ \__/ \__/ \   / \__/ \__/ \__/  /
       \_________________\ /__________________/

██████╗ ███████╗ ██████╗ ██████╗ ███╗   ██╗███████╗ ██████╗ ██████╗  ██████╗ ███████╗
██╔══██╗██╔════╝██╔════╝██╔═══██╗████╗  ██║██╔════╝██╔═══██╗██╔══██╗██╔════╝ ██╔════╝
██████╔╝█████╗  ██║     ██║   ██║██╔██╗ ██║█████╗  ██║   ██║██████╔╝██║  ███╗█████╗
██╔══██╗██╔══╝  ██║     ██║   ██║██║╚██╗██║██╔══╝  ██║   ██║██╔══██╗██║   ██║██╔══╝
██║  ██║███████╗╚██████╗╚██████╔╝██║ ╚████║██║     ╚██████╔╝██║  ██║╚██████╔╝███████╗
╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝      ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝

        Modular Reconnaissance CLI Tool
        Fast signals. Structured results. Modular workflow.
        For educational and authorized testing only.
```

Design requirements:

1. Banner placement

* Show the banner when the application starts.
* The banner should appear before the normal CLI workflow output.
* Do not show the banner repeatedly during the same command execution.
* The banner must not hide validation errors, logs or scan results.

2. Branding meaning

* The main project name is ReconForge.
* “Hermes Signal” is used only as a visual signal motif inside the banner, not as a second project name.
* Keep the educational disclaimer visible.

3. Color concept
   Use a professional terminal color palette.

Suggested color idea:

* Cyan or bright blue for the large RECONFORGE text.
* Yellow or gold for “HERMES SIGNAL” to represent signal, transmission and attention.
* Dark gray or gray for the antenna / signal ASCII part.
* White or light gray for subtitles.
* Green only for success messages.
* Yellow only for warnings.
* Red only for errors.
* Avoid too many colors at once.

The colors should feel technical, clean and suitable for a security/reconnaissance CLI tool.

4. Spectre.Console implementation

* Use Spectre.Console for rendering.
* Prefer a dedicated class such as `BannerRenderer`, `StartupBannerRenderer` or `ConsoleBannerRenderer`.
* Keep the banner rendering inside the CLI or presentation layer.
* Do not put banner code inside scan services, export services or domain logic.
* Preserve the ASCII formatting and spacing.
* If Spectre.Console markup is used, make sure special characters are handled safely.
* If necessary, split the banner into logical sections:

  * antenna / Hermes Signal part
  * RECONFORGE title part
  * subtitle / description part
  * disclaimer part

5. Theme configuration
   Create a simple theme or options structure if useful.

For example:

* `BannerTheme`
* `BannerOptions`
* `CliTheme`

It may define:

* PrimaryColor
* AccentColor
* MutedColor
* SuccessColor
* WarningColor
* ErrorColor

Keep it simple. Do not over-engineer.

6. Disable banner for tests and CI
   The banner must be disableable for tests, CI and automation.

Implement support for this environment variable:

`RECONFORGE_NO_BANNER=true`

Rules:

* If `RECONFORGE_NO_BANNER` is set to `true`, the banner should not be displayed.
* The check should be case-insensitive if possible.
* This should not affect the scan workflow.
* This should not affect logging.
* This should not affect export behavior.
* This is mainly for automated tests and CI output.

Do not add a new public CLI option like `--no-banner` unless it is absolutely necessary. Prefer the environment variable because it keeps the user-facing CLI clean.

7. Accessibility and readability

* The output should remain readable even if colors are not supported.
* Do not rely only on color to communicate important information.
* Keep all important text visible in plain text.
* Avoid blinking text, animations or overly decorative effects.
* Do not add external dependencies only for styling.

8. CLI integration

* Integrate the banner at the application startup level.
* Keep the existing `--scan`, `--format`, `--output` and `--verbose` behavior unchanged.
* The banner should work together with the existing workflow.
* The banner should not be printed during pure unit tests if `RECONFORGE_NO_BANNER=true` is set.

9. Logging

* Do not log the full banner.
* If useful, log only that the CLI application started.
* Avoid polluting log output with ASCII art.

10. Tests
    Add or update simple tests only if useful.

Possible tests:

* banner content is not empty
* banner contains “RECONFORGE”
* banner contains the educational disclaimer
* banner can be disabled with `RECONFORGE_NO_BANNER=true`
* existing scan workflow tests still pass
* existing export tests still pass

Do not overcomplicate banner tests.

11. Code quality

* Keep the implementation modular.
* Keep the banner renderer small and focused.
* Do not mix UI presentation with business logic.
* Use clear class names and method names.
* Do not introduce unrelated features.
* Do not break existing functionality.
* Keep the code easy to maintain.

Expected result:
ReconForge should display a professional colored startup banner when the CLI starts.

The banner should:

* preserve the ASCII design
* use a clean Spectre.Console color theme
* make ReconForge visually recognizable
* keep the educational disclaimer visible
* be disableable with `RECONFORGE_NO_BANNER=true`
* not interfere with scan, export, logging or test behavior

After implementation, provide a short summary:

* which design and colors were chosen
* where the banner renderer was added
* how the banner is integrated
* how the banner can be disabled in tests or CI
* which files were changed
* whether build and tests still pass
