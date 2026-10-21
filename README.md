<p align="center">
  <img src="assets/fixtrail-security-banner.svg" alt="FixTrail Security — vulnerability remediation tracking from discovery to retest" width="100%" />
</p>

<h1 align="center">FixTrail Security</h1>

<p align="center">
  <strong>From finding to verified remediation.</strong><br/>
  A vulnerability-remediation workflow MVP for assets, findings, scanner imports, evidence, and retesting.
</p>

<p align="center">
  <a href="https://github.com/tracolerd/fixtrail-security/actions/workflows/ci.yml"><img src="https://github.com/tracolerd/fixtrail-security/actions/workflows/ci.yml/badge.svg?branch=main" alt="FixTrail Security CI status" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-2563EB?style=for-the-badge" alt="MIT License" /></a>
  <img src="https://img.shields.io/badge/Backend-.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Frontend-Angular%2021-DD0031?style=for-the-badge&logo=angular&logoColor=white" alt="Angular 21" />
  <img src="https://img.shields.io/badge/Database-PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Stage-Local%20MVP-D97706?style=for-the-badge" alt="Local MVP" />
</p>

<p align="center">
  <a href="#overview">Overview</a> ·
  <a href="#implemented-capabilities">Capabilities</a> ·
  <a href="#architecture-and-workflow">Architecture</a> ·
  <a href="#quick-start-windows-powershell">Quick start</a> ·
  <a href="#security-review-and-known-limitations">Security review</a> ·
  <a href="#ci-and-test-status">CI & tests</a>
</p>

---

> [!IMPORTANT]
> **Current status: functional local MVP; not production-ready.** The repository contains ASP.NET Core Identity/JWT authentication, role-based API policies, PostgreSQL persistence, JSON/Nmap XML imports, and an Angular client. However, the latest audited CI run fails at the Angular test step. The source also still contains a development PostgreSQL password, an unauthenticated first-admin bootstrap endpoint, incomplete import validation, and other hardening gaps documented below. Do not expose this application to an untrusted network until those issues have been addressed and verified.

## Overview

FixTrail Security is a workflow-oriented vulnerability-management application. It aims to connect a security observation to the asset it affects, track repeated findings, record who is responsible for remediation, retain evidence references, and capture retest outcomes.

It is **not** a network scanner, exploit framework, CVE-enrichment service, or automatic vulnerability-verification engine. It consumes manually entered findings and selected external report formats. In particular, the current Nmap importer turns open-port observations into informational findings; an open port alone does not establish that a vulnerability exists.

## Project at a glance

| Area | Current implementation |
|---|---|
| Backend | C#, ASP.NET Core Web API on .NET 8 |
| Authentication | ASP.NET Core Identity with JWT bearer tokens |
| Authorization | Role-based policies for API actions |
| Database | PostgreSQL through EF Core and Npgsql |
| Schema management | Committed Entity Framework Core migrations |
| Finding intake | Manual creation, JSON finding arrays, and Nmap XML |
| Workflow | Asset association, occurrence tracking, task assignment, evidence references, and retest records |
| Frontend | Angular 21 standalone components and TypeScript |
| Tests | xUnit domain/parser tests; Angular unit-test setup exists but current CI invocation fails |
| Automation | GitHub Actions for .NET restore/build/test and Angular test/build |
| Persistence | PostgreSQL; no hosted public deployment is included in this repository |

## Implemented capabilities

| Capability | What the source currently implements |
|---|---|
| Asset inventory | Create assets, list assets, retrieve an asset by ID, and associate findings with an asset |
| Finding records | Create a finding with title, description, source, source-provided severity, and optional external ID |
| Finding list | Retrieve findings for a specific asset, ordered by creation time |
| Duplicate handling | Match findings by asset ID, title, and source; record repeated observations; enforce a database unique index on those fields |
| JSON import | Deserialize a JSON array of finding records |
| Nmap XML import | Parse open-port/service metadata from an Nmap XML report |
| Import transaction | Database writes are wrapped in a transaction, with rollback on database failures |
| Authentication | Login using ASP.NET Core Identity and receive a JWT access token |
| First administrator | An unauthenticated bootstrap endpoint creates the initial Admin account when the current bootstrap checks permit it |
| Remediation assignment | Create a remediation task, update finding status, and record an audit event |
| Evidence reference | Associate a URL/path and description with a remediation task, and mark the finding as mitigated |
| Retesting | Store a retest result and set the finding to Closed or Reopened |
| Audit data | Store selected workflow events in PostgreSQL with a timestamp and actor email derived from JWT claims |
| Security middleware | JWT validation, role policies, a local Angular CORS origin, HTTPS redirection, and basic response headers |

**Implementation boundaries matter.** The Angular UI exposes login, asset registration/listing, finding entry, finding listing, and import forms. It does not yet provide complete remediation assignment, evidence, retest, audit-history, or user-management screens. The corresponding remediation API actions exist, but an API endpoint alone does not mean the entire UI workflow is finished.

## Roles and API access

The source defines these role names:

- **Admin**
- **SecurityAnalyst**
- **RemediationOwner**
- **ReadOnly**

The current authorization policies behave as follows:

| Operation | Admin | SecurityAnalyst | RemediationOwner | ReadOnly |
|---|---:|---:|---:|---:|
| Read assets and findings | Yes | Yes | Yes | Yes |
| Create assets and findings | Yes | Yes | No | No |
| Import JSON/Nmap reports | Yes | Yes | No | No |
| Assign remediation tasks | Yes | Yes | No | No |
| Submit remediation evidence | Yes | Yes | Yes | No |
| Record retest outcomes | Yes | Yes | No | No |
| Manage users and roles through a dedicated API | Not implemented | Not implemented | Not implemented | Not implemented |

Server-side authorization policies protect the relevant controller actions. The Angular route guard is only a convenience for navigation; it is not a replacement for backend authorization.

The current token is stored in browser <code>localStorage</code>, and there is no refresh-token flow. This storage strategy increases exposure if an exploitable XSS vulnerability is introduced. Review the authentication/session design before production deployment.

### First administrator bootstrap

The API exposes <code>POST /api/auth/bootstrap</code> to create the initial administrator. It is intentionally callable without a JWT so the first account can be created on a new database.

This endpoint is **not suitable for public exposure as currently implemented**. The bootstrap check is based on current role membership and is not protected by a separate one-time secret or a concurrency-safe provisioning lock. Restrict initial provisioning to a trusted local setup or implement a stronger bootstrap mechanism before hosting the API publicly. After the administrator exists, the endpoint is expected to refuse another bootstrap attempt.

The application currently has login and first-admin bootstrap endpoints, but no complete administrator-facing user/role management API.

## Architecture and workflow

~~~mermaid
flowchart LR
    UI[Angular Client] --> AUTH[Login / JWT]
    AUTH --> API[ASP.NET Core API]
    API --> POL[Authentication + RBAC Policies]
    POL --> ASSETS[Assets and Findings]
    POL --> IMPORT[JSON / Nmap XML Import]
    POL --> REM[Remediation Actions]
    ASSETS --> DB[(PostgreSQL / EF Core)]
    IMPORT --> DB
    REM --> DB
    REM --> AUDIT[Audit Events and Retest Records]
    AUDIT --> DB
~~~

### Finding lifecycle

The current remediation endpoints can apply these status changes:

~~~mermaid
stateDiagram-v2
    [*] --> Open
    Open --> Assigned: Assign task
    Assigned --> Mitigated: Submit evidence
    Mitigated --> Closed: Successful retest
    Mitigated --> Reopened: Failed retest
~~~

This describes the intended common path, **not a fully enforced state machine**. The current domain method allows status replacement without a central transition policy, and endpoints do not yet reject every illegal transition. The lifecycle needs additional implementation and tests before it can be relied on as a release control.

### Domain entities

| Entity | Purpose |
|---|---|
| <code>Asset</code> | System/service name, environment, owner, creation time |
| <code>Finding</code> | An observation linked to an asset, preserving source-provided severity and identifiers |
| <code>FindingOccurrence</code> | A record that an existing finding was observed again |
| <code>RemediationTask</code> | Assignee, due date, notes, and parent finding |
| <code>EvidenceReference</code> | A URL/path and optional description associated with a task |
| <code>RetestRecord</code> | Retest result, operator identity, notes, and timestamp |
| <code>AuditEvent</code> | A record of selected assignment, evidence, and retest actions |
| <code>ApplicationUser</code> | ASP.NET Core Identity user with a full-name field |

## API reference

The following routes are present in the current source. All domain routes listed below require a JWT, subject to their action-specific policies. The auth routes are unauthenticated by design.

| Method | Route | Access / purpose |
|---|---|---|
| <code>POST</code> | <code>/api/auth/bootstrap</code> | Unauthenticated initial administrator bootstrap; restrict to trusted local setup |
| <code>POST</code> | <code>/api/auth/login</code> | Authenticate and return an access token |
| <code>GET</code> | <code>/api/assets</code> | Authenticated: list assets |
| <code>POST</code> | <code>/api/assets</code> | Admin or SecurityAnalyst: create an asset |
| <code>GET</code> | <code>/api/assets/{id}</code> | Authenticated: retrieve an asset |
| <code>POST</code> | <code>/api/findings</code> | Admin or SecurityAnalyst: create a finding or record an occurrence |
| <code>GET</code> | <code>/api/findings/{id}</code> | Authenticated: retrieve a finding |
| <code>GET</code> | <code>/api/findings/asset/{assetId}</code> | Authenticated: list findings for an asset |
| <code>POST</code> | <code>/api/importer/upload/{assetId}</code> | Admin or SecurityAnalyst: upload JSON or Nmap XML |
| <code>POST</code> | <code>/api/findings/{findingId}/remediation/assign</code> | Admin or SecurityAnalyst: assign remediation |
| <code>POST</code> | <code>/api/findings/{findingId}/remediation/tasks/{taskId}/evidence</code> | Admin, SecurityAnalyst, or RemediationOwner: submit evidence |
| <code>POST</code> | <code>/api/findings/{findingId}/remediation/retest</code> | Admin or SecurityAnalyst: record retest outcome |

The current API does not yet expose dedicated endpoints for listing remediation tasks, retrieving evidence collections, or reading an audit timeline. The visible Angular UI also does not yet complete those workflows.

## Import formats

### JSON finding array

The importer expects a JSON array of finding objects. Example:

~~~json
[
  {
    "title": "Example security observation",
    "description": "A sample observation from an approved assessment.",
    "source": "ExampleScanner",
    "sourceSeverity": "High",
    "externalId": "CVE-2025-00000"
  }
]
~~~

This is illustrative sample data, not a real vulnerability report. The current importer checks file existence, size (5 MiB maximum), extension, and asset existence. However, row validation is incomplete: rows missing some fields can be skipped rather than rejecting the entire batch. Consequently, the advertised all-or-nothing behavior is not yet guaranteed for every malformed input. A database transaction protects writes from database exceptions, but it does not turn partial input validation into complete batch validation.

### Nmap XML

The parser looks at hosts and ports in an Nmap XML report and creates informational findings for ports whose state is <code>open</code>. It uses available service/product/version metadata where present.

It does not establish whether an exposed service is vulnerable, perform CVE matching, calculate CVSS, test exploitability, or run Nmap itself. Only import reports from systems you own or are explicitly authorized to assess.

## Quick start (Windows PowerShell)

### Prerequisites

- .NET SDK 8.x
- Node.js 24.x and npm (as used by the current CI workflow)
- PostgreSQL 16 or later
- Git
- EF Core CLI tool <code>dotnet-ef</code> 8.x

### 1. Clone the repository

~~~powershell
git clone https://github.com/tracolerd/fixtrail-security.git
Set-Location .\fixtrail-security
~~~

### 2. Create the local PostgreSQL database

Create an empty database named <code>fixtrail</code> in your local PostgreSQL instance:

~~~sql
CREATE DATABASE fixtrail;
~~~

**Important configuration finding:** the current tracked <code>FixTrailSecurity.Api/appsettings.Development.json</code> still contains a local <code>postgres/postgres</code> database password. Do not reuse this sample credential, and do not treat it as safe production configuration. Supply your own local connection string through .NET User Secrets so it overrides the tracked fallback.

From the repository root:

~~~powershell
dotnet user-secrets --project .\FixTrailSecurity.Api set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=fixtrail;Username=postgres;Password=REPLACE_WITH_YOUR_LOCAL_PASSWORD"
dotnet user-secrets --project .\FixTrailSecurity.Api set "JwtSettings:Secret" "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_BYTES"
~~~

The project already defines a <code>UserSecretsId</code>. Use a unique, randomly generated signing secret locally. Never paste a real secret into a public repository, issue, screenshot, or README. The current source verifies that the JWT secret is non-empty, but does not itself enforce a minimum key length; this must be hardened before production use.

The development configuration currently sets JWT issuer to <code>FixTrailSecurityApi</code>, audience to <code>FixTrailSecurityClient</code>, and access-token lifetime to 120 minutes. Production deployments must explicitly configure and validate all token settings.

### 3. Restore, build, and run .NET tests

From the repository root:

~~~powershell
dotnet restore
dotnet build --no-restore
dotnet test .\FixTrailSecurity.Tests\FixTrailSecurity.Tests.csproj --no-build --verbosity normal
~~~

The current xUnit suite contains 12 tests covering entity initialization/validation, status assignment, basic workflow entities, and Nmap XML parsing. These tests do not constitute comprehensive API authorization, PostgreSQL integration, or end-to-end workflow coverage.

### 4. Install EF Core tooling and apply migrations

Install the tool if it is not already available:

~~~powershell
dotnet tool install --global dotnet-ef --version 8.0.10
~~~

If it is already installed, update it only when needed.

Apply the committed migrations to your **local development database only**:

~~~powershell
dotnet ef database update --project .\FixTrailSecurity.Api --startup-project .\FixTrailSecurity.Api
~~~

Do not run migrations against a production database without a reviewed migration plan and backup.

### 5. Start the API

In a PowerShell terminal from the repository root:

~~~powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\FixTrailSecurity.Api --urls "http://localhost:5000"
~~~

The Angular service currently calls <code>http://localhost:5000/api</code>. The API uses HTTPS redirection middleware; if your local client receives an unexpected redirect, use the HTTPS URL supplied by your active ASP.NET Core launch profile and update the API base URL/CORS settings accordingly.

When running in Development, Swagger UI is available at <code>/swagger</code> on the API's active base URL.

### 6. Bootstrap the first administrator

Only do this for a database where the initial administrator has not already been created. Keep the API on localhost and do not expose the bootstrap endpoint to the internet.

~~~powershell
$bootstrapBody = @{
    email    = "admin@example.local"
    password = "REPLACE_WITH_A_STRONG_UNIQUE_PASSWORD"
    fullName = "Local Administrator"
} | ConvertTo-Json

Invoke-RestMethod `
    -Method Post `
    -Uri "http://localhost:5000/api/auth/bootstrap" `
    -ContentType "application/json" `
    -Body $bootstrapBody
~~~

Replace the placeholder password with a unique value that satisfies the configured ASP.NET Core Identity password policy. Keep it private.

### 7. Start the Angular client

Open a second PowerShell terminal:

~~~powershell
Set-Location .\FixTrailSecurity.Client
npm ci
npm start
~~~

Open the local Angular URL printed in the terminal (normally <code>http://localhost:4200</code>). Keep the API and PostgreSQL server running. Login through the application's login page.

The Angular service currently hard-codes the API base URL; it should be moved into environment-specific configuration before using the application across different environments.

## CI and test status

The workflow at <code>.github/workflows/ci.yml</code> runs .NET restore, build, and xUnit tests, followed by Angular tests and a production build.

**Latest audited source commit:** <code>8ae5f27c1fbf0aa769f171195626739ceaacfa53</code>.

- **.NET restore/build:** passed.
- **xUnit:** passed, 12 tests and 0 failures in the audited run.
- **Angular unit tests:** failed before the suite could complete.
- **Angular production build in that run:** skipped because the preceding test step failed.
- **Overall GitHub Actions run:** failed. See [workflow run #12](https://github.com/tracolerd/fixtrail-security/actions/runs/39651550340) and the [Actions page](https://github.com/tracolerd/fixtrail-security/actions).

### Angular CI blockers found in the workflow/source

The captured CI log shows that the test command passes <code>--browsers=ChromeHeadless</code>, but the project uses Angular's <code>@angular/build:unit-test</code> builder with Vitest configuration. That browser option requires an additional <code>@vitest/browser-*</code> provider that is not installed. This is the first observed reason the Angular test step exits with code 1.

There is also a separate stale test in <code>src/app/app.spec.ts</code>: it imports <code>App</code>, while the application exports <code>AppComponent</code>, and it expects the old scaffold heading <code>Hello, FixTrailSecurity.Client</code> instead of the current application UI. Fix the test command/builder configuration and update the test to reflect the actual component; do not simply weaken or remove the test.

The same CI log's <code>npm install</code> output reported four dependency audit findings (one low and three high) at that time. Re-run <code>npm audit</code> against the current lockfile and review each finding before claiming the dependency set is clean. Those counts are from the captured run and are not a guarantee of the current audit count.

Use the workflow badge at the top or open the Actions page for the latest status. A passing CI pipeline is evidence that its configured checks pass; it is not proof of production security.

## Security review and known limitations

The following findings are based on the currently audited source. They are release blockers or hardening priorities, not claims that a live penetration test has been performed.

### High-priority risks

1. **Tracked development database password.** The current development settings still include a sample <code>postgres/postgres</code> connection string. Move it out of tracked configuration and remove the fallback; local User Secrets do not make the committed password disappear.
2. **Bootstrap endpoint is unauthenticated and race-prone.** It is necessary to provision the first admin, but the current checks are not a strong one-time bootstrap mechanism. Restrict it to trusted local setup or require a secure bootstrap token/operational gate and concurrency-safe provisioning before deployment.
3. **No login throttling is configured in the application pipeline.** The login action uses password checking directly and does not visibly apply a rate-limit policy. Add rate limiting and reliable failed-login/lockout behavior before public access.
4. **JWT configuration validation is incomplete.** The source rejects a missing secret but does not enforce a minimum key length or fully validate all JWT configuration at startup. There is no refresh-token mechanism; the frontend restores the access token from <code>localStorage</code>.
5. **Angular test pipeline is red.** The current CI invocation asks the Vitest-based test builder for browser execution without installing a browser provider, and the scaffold spec uses a removed component export and stale title assertion.
6. **Import validation is incomplete.** The importer skips some invalid rows and includes parser exception details in an error response. Validate every row and XML document, return safe structured errors, and make the all-or-nothing policy real.
7. **Finding deduplication needs migration and domain scrutiny.** The database unique index helps concurrent duplicate prevention for <code>(AssetId, Title, Source)</code>, but the migration creates the index without a preceding deduplication step for existing rows. Verify existing databases before applying it, and review whether this identity key can merge distinct findings or treat equivalent findings as separate.
8. **Workflow state transitions are not strictly enforced.** The current entity method directly replaces status. Centralize allowed transitions and add negative tests.
9. **Audit storage is not tamper-proof.** Selected actions create audit rows, but there is no audit timeline endpoint or immutability guarantee against privileged database changes. Existing cascade-delete relationships also mean related records may be removed if a parent is deleted.
10. **Remediation UI is incomplete.** Assignment, evidence submission, retest, and audit-history endpoints do not yet have complete connected screens. User/role management routes are also absent.
11. **Validation and integration coverage are limited.** The 12 xUnit tests focus on domain and parser behavior. The workflow does not currently run a PostgreSQL-backed integration suite for authentication, authorization, import transactions, and full remediation paths.

### Additional hardening recommendations

- Make API base URL and allowed CORS origins environment-configurable rather than hard-coded to localhost.
- Add explicit XML reader security settings and regression tests for malformed/hostile XML; do not rely on implicit parser defaults as the only documented control.
- Avoid returning raw parser exception text to API callers.
- Define whether imports reject the entire file or return partial row outcomes; do not silently skip invalid rows under an advertised all-or-nothing contract.
- Add bounded pagination and filter validation for finding/asset queries.
- Validate evidence URL/path formats and render evidence descriptions as text. Do not fetch arbitrary evidence URLs server-side.
- Review the legacy <code>X-XSS-Protection</code> header; do not treat it as a substitute for an appropriate Content Security Policy and safe output handling.
- Add explicit audit-read access controls, retention policy, backup guidance, and integration tests.
- Review package vulnerabilities based on fresh reports rather than assuming an earlier audit result is current.

Do not use this application to manage sensitive production findings or real credentials until these limitations have been resolved and independently verified.

## Roadmap

### P0 — Safe local and deployment configuration

- [ ] Remove the committed database password fallback.
- [ ] Harden first-admin bootstrap and login throttling/lockout.
- [ ] Validate JWT settings and choose an appropriate production token/session design.
- [ ] Configure CORS and the frontend API URL per environment.
- [ ] Fix the Angular test builder invocation and stale component spec.
- [ ] Review current npm dependency audit findings.

### P1 — Trustworthy workflow and import handling

- [ ] Enforce legal state transitions in the domain layer.
- [ ] Validate all import rows, return structured error results, and implement an actual all-or-nothing contract.
- [ ] Use explicit hardened XML reader settings and add hostile-input tests.
- [ ] Review deduplication identity, concurrency handling, and migrations against existing data.
- [ ] Add PostgreSQL-backed API integration tests for authentication, RBAC, import transactions, and remediation workflows.
- [ ] Add endpoints and UI to review remediation tasks, evidence, and audit history.
- [ ] Add secure administrator user/role management.

### P2 — Production operations

- [ ] Add deployment-specific configuration and secret management.
- [ ] Add tested backup/recovery and audit-retention procedures.
- [ ] Add bounded query/pagination contracts and operational health checks.
- [ ] Add dependency/security scanning to CI with a documented triage policy.
- [ ] Complete a separate pre-deployment security review.

## Repository layout

~~~text
.
├── assets/
│   └── fixtrail-security-banner.svg
├── FixTrailSecurity.Api/
│   ├── Controllers/       # Auth, assets, findings, importer, remediation
│   ├── DTOs/              # Request and response models
│   └── Migrations/        # EF Core migrations and model snapshot
├── FixTrailSecurity.Client/
│   └── src/app/
│       ├── components/    # Login, assets, findings
│       ├── guards/        # Auth route guard
│       ├── interceptors/  # JWT authorization header
│       ├── models/
│       └── services/
├── FixTrailSecurity.Core/
│   ├── Constants/         # Role names
│   ├── Data/              # EF Core Identity DbContext
│   ├── Entities/          # Assets, findings, remediation, audit, users
│   └── Services/          # Nmap XML parser
├── FixTrailSecurity.Tests/ # xUnit tests
├── docs/
│   ├── DECISIONS.md
│   ├── DEPLOYMENT.md
│   └── PROJECT_STATE.md
├── .github/workflows/ci.yml
├── FixTrailSecurity.sln
└── LICENSE
~~~

---

## Creator, Developer & Maintainer

**Created, designed, developed, and maintained by [Nurul Faiyaz](https://github.com/tracolerd).**

This project is independently built and maintained by Nurul Faiyaz. The project's design, implementation, and documentation are his work.

<p align="center">
  <a href="https://www.facebook.com/faiyaz.888"><img src="https://img.shields.io/badge/Facebook-1877F2?style=for-the-badge&logo=facebook&logoColor=white" alt="Facebook" /></a>
  <a href="https://www.instagram.com/tracolerd"><img src="https://img.shields.io/badge/Instagram-E4405F?style=for-the-badge&logo=instagram&logoColor=white" alt="Instagram" /></a>
  <a href="https://www.linkedin.com/in/faiyaz-xyz"><img src="https://img.shields.io/badge/LinkedIn-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white" alt="LinkedIn" /></a>
  <a href="https://x.com/faiyaz_explores"><img src="https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white" alt="X" /></a>
  <a href="https://github.com/tracolerd"><img src="https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white" alt="GitHub" /></a>
  <a href="mailto:scrollfaiyaz@gmail.com"><img src="https://img.shields.io/badge/Email-D14836?style=for-the-badge&logo=gmail&logoColor=white" alt="Email" /></a>
  <a href="https://nurul-faiyaz.netlify.app/"><img src="https://img.shields.io/badge/Portfolio-Visit%20Website-0F766E?style=for-the-badge&logo=googlechrome&logoColor=white" alt="Personal website" /></a>
</p>

<p align="center">
  <a href="https://www.facebook.com/faiyaz.888">Facebook</a> ·
  <a href="https://www.instagram.com/tracolerd">Instagram</a> ·
  <a href="https://www.linkedin.com/in/faiyaz-xyz">LinkedIn</a> ·
  <a href="https://x.com/faiyaz_explores">X / Twitter</a> ·
  <a href="https://github.com/tracolerd">GitHub</a> ·
  <a href="mailto:scrollfaiyaz@gmail.com">Email</a> ·
  <a href="https://nurul-faiyaz.netlify.app/">Portfolio</a>
</p>

## License

MIT. See [LICENSE](LICENSE).