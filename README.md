<p align="center">
  <img src="assets/fixtrail-security-banner.svg" alt="FixTrail Security — vulnerability remediation tracking from discovery to retest" width="100%" />
</p>

<h1 align="center">FixTrail Security</h1>

<p align="center">
  <strong>From finding to verified remediation.</strong><br/>
  Track assets, normalize scanner findings, assign remediation work, preserve evidence, and record retest outcomes.
</p>

<p align="center">
  <a href="https://github.com/tracolerd/fixtrail-security/actions/workflows/ci.yml"><img src="https://github.com/tracolerd/fixtrail-security/actions/workflows/ci.yml/badge.svg?branch=main" alt="FixTrail Security CI status" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-2563EB?style=for-the-badge" alt="MIT License" /></a>
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Frontend-Angular%2021-DD0031?style=for-the-badge&logo=angular&logoColor=white" alt="Angular 21" />
  <img src="https://img.shields.io/badge/Database-PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Stage-Functional%20MVP-D97706?style=for-the-badge" alt="Functional MVP" />
</p>

<p align="center">
  <a href="#overview">Overview</a> ·
  <a href="#implemented-capabilities">Capabilities</a> ·
  <a href="#architecture">Architecture</a> ·
  <a href="#quick-start">Quick start</a> ·
  <a href="#security-audit-and-known-limitations">Security audit</a> ·
  <a href="#ci-and-tests">CI & tests</a>
</p>

---

> [!IMPORTANT]
> **Functional local MVP — not production-ready.** The repository contains a .NET API, PostgreSQL persistence through Entity Framework Core, an Angular client, JSON/Nmap XML imports, remediation workflow endpoints, and xUnit tests. The client currently targets `http://localhost:5000/api`, while the checked-in API launch profile uses port `5241`; follow the local-start instructions below to align them. There is no authentication/authorization implementation, and the current UI does not yet display a real findings list or expose the full remediation workflow. Do not expose the API to an untrusted network.

## Overview

FixTrail Security is a vulnerability-remediation workflow MVP. It is designed to connect the operational steps that often follow a scanner result: associate the observation with an asset, record the finding, assign a remediation task, attach a reference to supporting evidence, and record a retest outcome.

Rather than scanning networks or exploiting systems itself, FixTrail Security provides a workflow layer for findings that are entered manually or imported from supported files. The current Nmap importer treats discovered open ports as **informational findings**; it does not establish that those ports are vulnerable.

## Implemented capabilities

| Area | Current implementation |
|---|---|
| Asset inventory | Create assets with name, environment, owner, and creation timestamp; list assets and retrieve one by ID |
| Finding records | Create findings linked to an asset, with title, description, source, source-provided severity, and optional external identifier |
| Duplicate observations | Match on asset ID, finding title, and source; record a new occurrence for a matching existing finding |
| Import endpoint | Upload a JSON array of finding records or an Nmap XML report for a selected asset |
| Upload boundary | Rejects missing/empty files, files larger than 5 MiB, unsupported extensions, and missing assets |
| Nmap XML parsing | Extracts open ports and service metadata from Nmap XML output |
| Remediation assignment | Create a task with assignee, due date, and notes; update the finding status and write an audit event |
| Evidence reference | Attach a URL/path and description to a remediation task; mark the finding as mitigated pending retest |
| Retesting | Record a successful or unsuccessful retest and close or reopen the finding accordingly |
| Persistence | PostgreSQL through EF Core, with committed schema migrations |
| Frontend | Angular client for asset registration/listing, manual finding submission, and file import |
| Quality checks | xUnit unit tests and GitHub Actions workflow for .NET restore/build/test and Angular build |

**Scope is important:** the current UI is a functional starting point, not a complete vulnerability-management dashboard. The Findings screen submits manual findings and imports files, but the table is currently a placeholder; it does not fetch and render actual findings. Remediation actions exist as API endpoints but are not wired into complete UI screens.

## Architecture

~~~mermaid
flowchart LR
    UI[Angular Client] -->|HTTP / JSON or multipart upload| API[ASP.NET Core API]
    API --> ASSET[Asset and Finding Controllers]
    API --> IMPORT[JSON / Nmap XML Importer]
    API --> WORKFLOW[Remediation Workflow]
    ASSET --> CORE[Core Domain Entities]
    IMPORT --> CORE
    WORKFLOW --> CORE
    CORE --> EF[Entity Framework Core]
    EF --> DB[(PostgreSQL)]
    WORKFLOW --> AUDIT[Audit Events]
    IMPORT --> OCC[Finding Occurrences]
~~~

### Domain model

| Entity | Purpose |
|---|---|
| `Asset` | System or service with environment and ownership metadata |
| `Finding` | A potential security issue reported by a person or scanner source |
| `FindingOccurrence` | A record that a known finding was observed again |
| `RemediationTask` | Assignee, due date, notes, and parent finding |
| `EvidenceReference` | URL/path plus description linked to a remediation task |
| `RetestRecord` | Retest outcome, operator name, notes, and timestamp |
| `AuditEvent` | A record of selected assignment, evidence, and retest actions |

The current duplicate check is a simple match on asset, title, and source. It is useful as a first pass, but it is not a canonical vulnerability identity algorithm: similar titles can refer to different issues, and the same issue may be reported under different titles.

## Finding lifecycle

The implemented workflow includes these main transitions:

~~~mermaid
stateDiagram-v2
    [*] --> Open
    Open --> Assigned: Assign remediation task
    Assigned --> Mitigated: Submit evidence reference
    Mitigated --> Closed: Successful retest
    Mitigated --> Reopened: Unsuccessful retest
    Closed --> Reopened: Later failed retest
~~~

The API changes the status when the corresponding endpoint is called. A strict transition policy is not yet enforced as a centralized state machine, and authentication does not currently establish whether the caller is authorized to perform each transition.

## Supported imports

### 1. JSON finding array

The endpoint accepts a JSON array of finding objects at `POST /api/importer/upload/{assetId}`. The selected asset is supplied in the URL; the imported object fields are:

~~~json
[
  {
    "title": "Example security finding",
    "description": "A sample observation from an approved assessment.",
    "source": "ExampleScanner",
    "sourceSeverity": "High",
    "externalId": "CVE-2025-00000"
  }
]
~~~

This is an illustrative payload, not a real vulnerability report. The importer currently validates rows only partially; malformed records can be skipped or lead to a database error. Do not treat the 5 MiB file cap as a substitute for full schema validation.

### 2. Nmap XML report

Upload an Nmap XML output file generated for a system you own or are authorized to assess. The parser records ports whose state is `open` and uses available service/product/version metadata to create findings with source `Nmap` and source severity `Info`.

The current importer does **not** identify CVEs, infer exploitability, determine that an open service is vulnerable, or import all information Nmap may provide. Open ports are observations that require context—not vulnerability verdicts.

## API surface

The following routes are implemented in the API source. They are available when the local API is running.

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/assets` | List assets |
| `POST` | `/api/assets` | Create an asset |
| `GET` | `/api/assets/{id}` | Retrieve one asset |
| `POST` | `/api/findings` | Create a finding or record a repeated occurrence |
| `GET` | `/api/findings/{id}` | Retrieve one finding |
| `POST` | `/api/importer/upload/{assetId}` | Import a JSON array or Nmap XML file |
| `POST` | `/api/findings/{findingId}/remediation/assign` | Assign a remediation task |
| `POST` | `/api/findings/{findingId}/remediation/tasks/{taskId}/evidence` | Record an evidence reference |
| `POST` | `/api/findings/{findingId}/remediation/retest` | Record a retest and update the finding status |

There is currently no endpoint to list findings across an asset, retrieve the audit timeline, or list remediation tasks and evidence. The API is therefore ahead of the visible UI in some workflows, but it is not yet a complete REST interface for every stored entity.

## Technology

| Layer | Technology |
|---|---|
| Backend API | C#, ASP.NET Core Web API, .NET 8 |
| Domain layer | C# entities and domain methods |
| ORM and migrations | Entity Framework Core 8 |
| Database provider | Npgsql for PostgreSQL |
| Database | PostgreSQL |
| Frontend | Angular 21, TypeScript, RxJS |
| Upload handling | ASP.NET Core multipart form upload |
| Automated tests | xUnit, .NET Test SDK |
| CI | GitHub Actions |
| License | MIT |

## Quick start

### Prerequisites

- .NET SDK 8.x
- Node.js 24.x (the version configured in CI) and npm
- PostgreSQL running locally
- Git
- EF Core command-line tool, if you need to apply migrations from the CLI

### 1. Clone the repository

Run these commands in PowerShell:

~~~powershell
git clone https://github.com/tracolerd/fixtrail-security.git
cd fixtrail-security
~~~

### 2. Create the local PostgreSQL database

Create a database named `fixtrail` in your local PostgreSQL instance. For example, using `psql`:

~~~sql
CREATE DATABASE fixtrail;
~~~

The committed `appsettings.Development.json` contains a **local-only sample connection string** for `localhost` using the PostgreSQL username/password `postgres/postgres`. This is a development convenience, not a secure credential. Replace it or supply the connection string through environment configuration; never reuse these defaults in a shared or production environment.

### 3. Restore, build, and test the .NET solution

From the repository root:

~~~powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
~~~

Install the EF Core CLI tool once if it is not already available:

~~~powershell
dotnet tool install --global dotnet-ef --version 8.0.10
~~~

### 4. Apply database migrations

Set the development environment and ensure the connection string points to your local PostgreSQL database:

~~~powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=fixtrail;Username=postgres;Password=postgres"
dotnet ef database update --project FixTrailSecurity.Api
~~~

Use a different local password in the connection string if your PostgreSQL setup requires it. Keep credentials out of source control.

### 5. Start the API on the port expected by the Angular client

The Angular service currently targets `http://localhost:5000/api`. The checked-in launch profile uses a different port, so start the API explicitly on port 5000 for this local setup.

In a PowerShell terminal from the repository root:

~~~powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=fixtrail;Username=postgres;Password=postgres"
dotnet run --project FixTrailSecurity.Api --no-launch-profile --urls http://localhost:5000
~~~

Open `http://localhost:5000/swagger` to explore the API while running in Development. The API's configured CORS policy permits the Angular development origin `http://localhost:4200`; do not broaden it to public origins without an explicit deployment design.

### 6. Start the Angular client

Open a second PowerShell terminal:

~~~powershell
cd FixTrailSecurity.Client
npm ci
npm start
~~~

Open the local Angular URL printed in the terminal (by default, `http://localhost:4200`). Keep PostgreSQL and the API running. The current client uses a hard-coded API URL; for a different port or remote deployment, configure an environment-specific API base URL before using the client.

## CI and tests

The workflow at `.github/workflows/ci.yml` runs on pushes and pull requests targeting `main`. It restores and builds the .NET solution, runs xUnit tests, installs Angular dependencies, and builds the Angular client.

The most recent workflow run at the time of this README update completed successfully: [GitHub Actions run #1](https://github.com/tracolerd/fixtrail-security/actions/runs/39640105739). This validates the pipeline's current build/test steps. It does not prove production security, exercise a deployed database-backed application end to end, or verify a full browser workflow against a hosted API.

The current test suite covers entity behavior, finding status changes, basic workflow entity construction, and Nmap parser behavior. It does not yet provide broad controller/API integration coverage, authorization tests, import fuzzing, database transaction tests, or full remediation lifecycle tests.

## Security audit and known limitations

This section records material gaps visible in the committed source. The project should remain a **local development MVP** until the highest-priority issues are addressed.

### Critical before any public deployment

- **No authentication or authorization.** API routes are not protected by a verified identity or role-based policy. `UseAuthorization()` without configured authentication/authorization policies does not establish who is calling. Do not expose these endpoints to the internet.
- **Audit actor identity is caller-supplied.** Request payloads provide values such as `PerformedBy` and `RetestedBy`. Without authenticated identity claims, those fields are not reliable proof of who performed an action.
- **Development database credentials are committed.** The checked-in development configuration contains the simple local `postgres/postgres` connection string. Keep it strictly local and move secrets to environment variables or a secret store for deployment.
- **API URL and launch port differ.** The Angular client calls `localhost:5000`, while the standard launch profile uses `localhost:5241`. The local quick-start commands align the API listener with the client; longer-term, use environment-specific configuration rather than hard-coded URLs.
- **Production configuration is incomplete.** A production database connection string, migration strategy, trusted HTTPS termination, logging policy, backups, and deployment-specific allowed origins must be configured before hosting.

### Data integrity and workflow gaps

- **Finding deduplication is heuristic.** Matching only asset ID, title, and source may miss duplicates or combine distinct observations. There is no database uniqueness constraint protecting the check from concurrent duplicate requests.
- **Import validation is partial.** The upload checks filename extension and file size, and the loop skips some incomplete records, but every JSON row is not fully validated against all DTO constraints before persistence. Return structured row-level errors and use a transaction for reliable batch behavior.
- **Batch persistence is not fully atomic by design.** Import operations should define whether a malformed row fails the whole batch or is reported individually; that policy is not yet explicit in the API contract.
- **Lifecycle transitions are not centrally enforced.** Domain status updates allow status replacement rather than validating the allowed transition graph. A stricter transition policy and tests are needed.
- **Audit history is not immutable or independently attributable.** Audit records are regular database rows; the current implementation has no append-only or tamper-evident protection and no endpoint to query the timeline.
- **Retest is not tied to an explicit remediation task.** The retest endpoint is scoped to a finding, not a particular assigned task/evidence record. A stronger workflow should preserve which fix and evidence were actually retested.
- **UI and API feature coverage differ.** The UI has asset creation/listing and finding submission/import forms, but it does not render the findings table from the backend or expose assignment, evidence, retest, and audit history as complete screens.
- **No end-to-end integration test against PostgreSQL.** Current CI builds and tests the code, but it does not provision a database and exercise the API/controllers as a deployed stack.
- **Nmap import is not vulnerability analysis.** It maps open-port observations to informational findings. It does not derive a CVE or severity assessment from service/version data.

## Roadmap

### P0 — Safe and predictable API

- [ ] Add authentication and server-side authorization policies.
- [ ] Derive audit actors from authenticated claims instead of request payloads.
- [ ] Move the API base URL into Angular environment configuration.
- [ ] Replace committed development credential defaults with safer local setup and documented secret configuration.
- [ ] Harden import validation, batch error reporting, transaction semantics, and duplicate handling.

### P1 — Trustworthy remediation workflow

- [ ] Add a findings list endpoint with pagination/filtering and connect it to the UI.
- [ ] Build the remediation assignment, evidence, retest, and audit-history screens.
- [ ] Enforce legal finding status transitions in the domain layer.
- [ ] Link retests to the task/evidence that was verified.
- [ ] Add PostgreSQL-backed API integration tests and authorization tests.

### P2 — Operational quality

- [ ] Add database provisioning/migration guidance and a deployment-safe configuration example.
- [ ] Add import regression fixtures for representative Nmap XML and JSON inputs.
- [ ] Add row-level import outcomes and provenance for each source observation.
- [ ] Add structured logging, documented retention, and backup/recovery expectations.
- [ ] Separate an open port observation from a confirmed vulnerability in the domain model and reporting.

## Repository layout

~~~text
.
├── FixTrailSecurity.Api/
│   ├── Controllers/       # Asset, finding, import, and remediation endpoints
│   ├── DTOs/              # API request/import models
│   ├── Migrations/        # EF Core database migrations and snapshot
│   └── Program.cs         # API, PostgreSQL, CORS, and middleware setup
├── FixTrailSecurity.Client/
│   └── src/app/
│       ├── components/    # Asset and finding screens
│       ├── models/        # Client-side domain interfaces
│       └── services/      # API client
├── FixTrailSecurity.Core/
│   ├── Data/              # EF Core DbContext and model configuration
│   ├── Entities/          # Assets, findings, tasks, evidence, retests, audit
│   ├── Enums/
│   └── Services/          # Nmap XML parser
├── FixTrailSecurity.Tests/ # xUnit unit tests
├── docs/
│   ├── DECISIONS.md
│   ├── DEPLOYMENT.md
│   └── PROJECT_STATE.md
├── assets/
│   └── fixtrail-security-banner.svg
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
  <a href="https://nurul-faiyaz.netlify.app/">Website</a>
</p>

## License

MIT. See [LICENSE](LICENSE).