Prompt 4 – Implement Subdomain Discovery

Continue from the current ReconForge codebase.

Do not recreate the project structure. Do not change the existing CLI interface or command-line options. Keep the current `--scan` workflow as the entry point and extend only the internal scan workflow by adding Subdomain Discovery after the existing Domain Scan step.

Goal:
Implement the next feature of ReconForge: Subdomain Discovery.

Scope of this step:

* Use the already validated and normalized domain from the Domain Scan step.
* Generate or collect possible subdomains for that domain.
* Return the discovered subdomains as part of the structured scan result.
* Keep the implementation simple, controlled and educational.

Do not implement:

* IP Resolution
* Port Scanning
* JSON Export
* Large wordlists
* Aggressive brute-force enumeration
* Scraping or crawling
* Third-party API integrations
* Exploit code, brute-force login attacks, denial-of-service behavior, stealth mechanisms, malware-like behavior or unauthorized access functionality

Please implement the following:

1. Subdomain model
   Create or update a `Subdomain` model in the appropriate Core layer.

The model should contain:

* Name
* RootDomain
* FullName
* Source
* DiscoveredAt

Keep the model simple and extendable. Do not include IP addresses yet, because IP Resolution will be implemented later.

2. Subdomain Discovery result
   Create a result model such as `SubdomainDiscoveryResult`.

It should contain:

* TargetDomain
* StartedAt
* FinishedAt
* Status
* Message
* DiscoveredSubdomains
* Errors or warnings if needed

Use clear status values such as:

* Success
* PartialSuccess
* Failed
* InvalidInput

3. Subdomain Discovery service
   Create an interface such as `ISubdomainDiscoveryService`.

Create an implementation such as `SubdomainDiscoveryService`.

The service should:

* accept a validated and normalized root domain
* use a small safe default list of common prefixes
* generate full subdomain names
* normalize generated subdomains
* remove duplicates
* return a structured result

Use this small default prefix list:

* www
* mail
* api
* dev
* test
* staging
* admin

Example:
For `example.com`, the service may produce:

* [www.example.com](http://www.example.com)
* mail.example.com
* api.example.com
* dev.example.com
* test.example.com
* staging.example.com
* admin.example.com

4. Optional custom prefixes
   If it fits the current architecture, allow the service to accept an optional list of custom prefixes.

Rules:

* If custom prefixes are provided, normalize them.
* Remove duplicates.
* Ignore empty entries.
* Do not use large wordlists.
* Do not implement aggressive enumeration.

5. Integration into the scan workflow
   Update the existing scan workflow so that after a successful Domain Scan, the Subdomain Discovery service is called.

The final scan result should now include:

* the original target domain
* the normalized domain
* the domain scan status
* the discovered subdomains
* the number of discovered subdomains

Keep responsibilities separated:

* Domain validation remains in the domain validation logic.
* Subdomain Discovery should not duplicate domain validation.
* Subdomain Discovery should not resolve IP addresses.
* Subdomain Discovery should not scan ports.
* CLI logic should not contain discovery logic.

6. Console output
   Update the existing console output so the user can see that Subdomain Discovery was executed.

Show:

* normalized target domain
* discovered subdomains
* number of discovered subdomains
* short success or warning message

Keep the output readable and professional. Use Spectre.Console only where it improves readability.

7. Logging
   Use the existing Microsoft.Extensions.Logging setup.

Log the following steps:

* Subdomain Discovery started
* target domain received
* default prefixes loaded
* custom prefixes received, if applicable
* subdomain candidate generated
* duplicate removed, if applicable
* Subdomain Discovery completed
* number of discovered subdomains
* unexpected exceptions

Use suitable log levels:

* Information for normal workflow
* Warning for unusual but non-critical situations
* Error for exceptions

8. Tests
   Add or update xUnit tests for Subdomain Discovery.

Include tests for:

* valid root domain generates expected subdomains
* generated subdomains contain the root domain
* generated subdomains are normalized
* duplicate prefixes are removed
* empty prefixes are ignored
* empty prefix list is handled gracefully
* result contains correct status
* result contains correct subdomain count
* service does not add IP addresses
* service does not add port information

Keep tests focused and understandable.

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
After this step, the existing scan workflow should validate and normalize the domain, run Subdomain Discovery, return a structured result with discovered subdomains, log the important steps and display a clear summary to the user.

The implementation should prepare the codebase for the next feature, which will be IP Resolution, without mixing IP-related logic into the Subdomain Discovery feature.
