Prompt 5 – Implement IP Resolution

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change the existing CLI interface or command-line options. Keep the current `--scan` workflow as the entry point and extend only the internal scan workflow by adding IP Resolution after the existing Subdomain Discovery step.

Goal:
Implement the next feature of ReconForge: IP Resolution.

Scope of this step:

* Use the already validated and normalized domain from the Domain Scan step.
* Use the subdomains produced by the Subdomain Discovery step.
* Resolve the root domain and discovered subdomains to IP addresses.
* Return the resolved IP addresses as part of the structured scan result.
* Keep the implementation simple, controlled and educational.

Do not implement:

* Port Scanning
* JSON Export
* Vulnerability checks
* Exploit code
* Brute-force behavior
* Denial-of-service behavior
* Stealth mechanisms
* Malware-like behavior
* Unauthorized access functionality

Please implement the following:

1. IP address model

Create or update an `IpAddress` model in the appropriate Core layer.

The model should contain:

* Address
* AddressFamily
* RelatedHost
* ResolvedAt
* IsPrivate
* IsLoopback

Keep the model simple and extendable.

2. IP Resolution result

Create a result model such as `IpResolutionResult`.

It should contain:

* TargetDomain
* StartedAt
* FinishedAt
* Status
* Message
* ResolvedAddresses
* FailedHosts
* Errors or warnings if needed

Use clear status values such as:

* Success
* PartialSuccess
* Failed
* NoResults

3. IP Resolution service

Create an interface such as `IIpResolutionService`.

Create an implementation such as `IpResolutionService`.

The service should:

* accept the normalized root domain
* accept the list of discovered subdomains
* resolve the root domain and subdomains to IP addresses
* remove duplicate IP addresses
* handle DNS failures gracefully
* return a structured result

Use safe DNS resolution through .NET APIs such as `System.Net.Dns`.

4. Host handling

The service should resolve:

* the root domain, for example `example.com`
* all discovered subdomains, for example `www.example.com`, `api.example.com`, `mail.example.com`

Rules:

* Ignore empty host names.
* Normalize host names before resolving.
* Do not crash if one host cannot be resolved.
* Continue resolving the remaining hosts if one lookup fails.
* Store failed hosts separately in the result.

5. Integration into the scan workflow

Update the existing scan workflow so that after a successful Subdomain Discovery step, the IP Resolution service is called.

The final scan result should now include:

* original target domain
* normalized domain
* domain scan status
* discovered subdomains
* resolved IP addresses
* failed hosts, if any
* number of resolved IP addresses

Keep responsibilities separated:

* Domain validation remains in the domain validation logic.
* Subdomain Discovery should only produce subdomains.
* IP Resolution should only resolve hosts to IP addresses.
* IP Resolution should not scan ports.
* CLI logic should not contain DNS resolution logic.

6. Console output

Update the existing console output so the user can see that IP Resolution was executed.

Show:

* normalized target domain
* discovered subdomains count
* resolved IP addresses
* failed hosts, if any
* short success or warning message

Keep the output readable and professional. Use Spectre.Console only where it improves readability.

7. Logging

Use the existing Microsoft.Extensions.Logging setup.

Log the following steps:

* IP Resolution started
* root domain received
* subdomains received
* host resolution started
* host resolved successfully
* host resolution failed
* duplicate IP removed, if applicable
* IP Resolution completed
* number of resolved IP addresses
* unexpected exceptions

Use suitable log levels:

* Information for normal workflow
* Warning for failed host resolution or no results
* Error for unexpected exceptions

8. Tests

Add or update xUnit tests for IP Resolution.

Include tests for:

* empty host list is handled gracefully
* duplicate IP addresses are removed
* failed hosts are stored separately
* result contains correct status
* result contains correct resolved IP count
* service does not add port information
* service does not perform port scanning
* service continues if one host fails

If direct DNS calls make tests unstable, introduce an abstraction such as `IDnsLookupClient` so DNS behavior can be mocked in unit tests.

9. Code quality

* Keep the code modular.
* Use interfaces where useful.
* Keep models simple.
* Use clear class names and method names.
* Avoid unnecessary complexity.
* Do not introduce unrelated features.
* Do not break existing tests.
* Keep the implementation easy to extend in the next step.

Expected result:
After this step, the existing scan workflow should validate and normalize the domain, run Subdomain Discovery, resolve the root domain and discovered subdomains to IP addresses, return a structured result, log the important steps and display a clear summary to the user.

The implementation should prepare the codebase for the next feature, which will be Port Scanning, without mixing port-related logic into the IP Resolution feature.
