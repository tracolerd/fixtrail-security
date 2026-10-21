# FixTrail Security
A workflow-oriented vulnerability management application to track security observations, associate them with assets, assign remediation tasks, collect evidence, and record retests.
Created, designed, developed, and maintained by [Nurul Faiyaz](https://github.com/tracolerd).
## Project Architecture & Scope
* **Backend:** C#, ASP.NET Core Web API (.NET 8).
* **Database:** PostgreSQL via Entity Framework Core & Npgsql.
* **Frontend:** Angular 21 (Standalone Components) & TypeScript.
* **Security:** ASP.NET Core Identity (PostgreSQL backed), JWT Bearer Tokens, Role-Based Access Control (RBAC), and security headers.
* **Importers:** Nmap XML and structured JSON batch imports with deterministic deduplication logic.
> **Note:** This project is a workflow management tool, NOT an automated network scanner or exploit framework.
## Roles & Authentication
The system uses strict RBAC verified via JWT claims.
- **Admin**: Full access, including user/role management.
- **SecurityAnalyst**: Can import findings, assign tasks, and verify retests.
- **RemediationOwner**: Can view assigned tasks and upload remediation evidence.
- **ReadOnly**: Can view dashboards and reports.
To bootstrap the first administrator account (only works on a fresh database):
`POST /api/auth/bootstrap` with `Email`, `Password`, and `FullName`.
## Local Windows Development Setup
1. **Prerequisites:**
   - Install .NET 8 SDK.
   - Install Node.js (v24+).
   - Install PostgreSQL 16+ and remember the password.
2. **Backend Configuration:**
   - Navigate to `FixTrailSecurity.Api`.
   - Update `appsettings.Development.json` with your local PostgreSQL credentials (default is `postgres`/`postgres`).
   - Run Entity Framework migrations: `dotnet ef database update`.
   - Start the API server: `dotnet run --urls "http://localhost:5000"`.
3. **Frontend Configuration:**
   - Navigate to `FixTrailSecurity.Client`.
   - Install dependencies: `npm install`.
   - Start the Angular server: `npm start`.
   - Access the dashboard at `http://localhost:4200/login`.
## Security Implementations & Hardening
- **Secrets Management:** JWT secrets are excluded from source control and meant to be stored in `.NET User Secrets` or Environment Variables.
- **Deduplication Strategy:** A strict Unique Constraint (`AssetId`, `Title`, `Source`) exists on the database level to prevent concurrent race conditions during bulk JSON/XML imports.
- **Audit Trails:** Status transitions create immutable `AuditEvent` records, attributing the action to the verified JWT email claim.
## Automated Testing
Tests cover domain logic, Nmap parsing, and Angular UI generation.
- Run .NET Tests: `dotnet test`
- Run Angular Tests: `cd FixTrailSecurity.Client && npm test -- --watch=false`
Automated CI checks run on GitHub Actions for every push to `main`.
