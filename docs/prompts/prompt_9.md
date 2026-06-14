Prompt 9 – Add Buildmanagement and GitHub Actions CI Pipeline

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change application behavior unless a change is required to make build, test or CI execution reliable.

Goal:
Add buildmanagement support and a GitHub Actions CI pipeline for ReconForge.

The current application already includes:

* CLI foundation
* Logging
* Domain Scan
* Subdomain Discovery
* IP Resolution
* Safe Port Scan
* Export system
* Unit tests

Now focus on build automation and CI.

Scope of this step:

* Ensure the project can be built with .NET CLI / MSBuild.
* Add a GitHub Actions workflow.
* The workflow should restore dependencies, build the solution and run tests.
* Keep the workflow simple, understandable and suitable for documentation.
* Make sure the commands can also be executed locally.

Please implement the following:

1. Verify local build commands

Check that the following commands work locally:

`dotnet restore`

`dotnet build`

`dotnet test`

If any command fails:

* identify the reason
* fix the problem
* explain what was changed

Do not ignore failing tests or build errors.

2. Add clean build configuration

Make sure the solution can be built in Release configuration.

The following command should work:

`dotnet build --configuration Release`

If needed:

* fix project references
* fix package references
* fix warnings that indicate real problems
* keep configuration simple

3. Add GitHub Actions workflow

Create a workflow file:

`.github/workflows/ci.yml`

The workflow should run on:

* push to main
* pull request to main
* manual workflow dispatch

The workflow should include:

* checkout repository
* setup .NET
* restore dependencies
* build solution
* run tests

Use clear job and step names.

4. Workflow structure

Create a workflow with a job such as:

`build-and-test`

Use a Linux runner such as:

`ubuntu-latest`

The workflow should be easy to understand for a university documentation.

5. .NET version

Use the .NET version that fits the project.

If the project targets .NET 8, use:

`8.0.x`

If the project targets .NET 9, use:

`9.0.x`

If the project targets .NET 10, use:

`10.0.x`

Check the existing project files and choose the correct version.

Do not change the target framework without a clear reason.

6. Recommended workflow commands

The workflow should execute commands similar to:

`dotnet restore`

`dotnet build --configuration Release --no-restore`

`dotnet test --configuration Release --no-build`

If test execution requires build output, adjust the commands correctly.

7. Optional test result output

If simple and useful, add test result logging or TRX output.

Do not overcomplicate the workflow.

The main goal is:

* restore
* build
* test

8. README documentation

Update or add a short section in the README explaining how to build and test the project locally.

Include commands:

`dotnet restore`

`dotnet build`

`dotnet test`

Also mention that GitHub Actions runs these steps automatically on push and pull request.

9. Error handling and documentation support

If the workflow initially fails, do not hide the error.

Instead:

* identify the cause
* fix it
* briefly explain the problem and the solution

This is important because the university assignment expects practical experience, not only a copied script.

10. Keep responsibilities clean

Do not add deployment steps yet.
Do not publish binaries yet unless it is necessary.
Do not add Docker unless it already exists and is required.
Do not add unnecessary complexity.

11. Security and safety

The workflow should not contain secrets.
The workflow should not upload sensitive files.
The workflow should not execute unsafe scripts.
Only use standard .NET build and test commands.

12. Final summary

At the end, provide a short summary:

* which files were added or changed
* which local build commands were tested
* whether build succeeded
* whether tests passed
* what the GitHub Actions workflow does
* what the next logical step should be

Expected result:
After this step, ReconForge should have a working local build process with .NET CLI / MSBuild and a GitHub Actions CI workflow that automatically restores, builds and tests the project.

The repository should contain:

`.github/workflows/ci.yml`

and the project should be verifiable with:

`dotnet restore`

`dotnet build`

`dotnet test`
