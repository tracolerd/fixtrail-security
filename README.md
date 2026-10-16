# FixTrail Security

**Follow vulnerabilities from discovery through verified remediation.**

FixTrail Security is a planned vulnerability-management application for tracking assets, normalized findings, remediation owners, deadlines, evidence, and retesting. It is intended to support a disciplined remediation workflow rather than act as a scanner or exploit framework.

> **Project status:** Initial planning and repository setup. The API, database, dashboard, importers, and authorization controls are not implemented yet.

## Problem

Scanner output is rarely a complete remediation workflow. Teams still need to map findings to assets, deduplicate repeated observations, assign an owner, document a fix, prioritize work, and confirm whether a vulnerability is resolved.

FixTrail Security aims to make these steps traceable and auditable.

## Planned MVP

- Maintain an asset inventory and ownership metadata.
- Import validated sample findings from JSON or CSV.
- Normalize core fields such as source, identifier, title, affected asset, severity metadata, and observation time.
- Track finding lifecycle, remediation owner, due date, notes, and retest result.
- Preserve change history for important workflow transitions.
- Support filtering, export, and remediation summaries.
- Link findings to established identifiers such as CVE or CWE when the source provides them.
- Provide role-based permissions for read, triage, and remediation actions.

The application is a workflow layer. It will not run network scans or exploit systems as part of the initial release.

## Proposed architecture

~~~mermaid
flowchart LR
    A[Approved Scanner Export] --> B[Validation and Normalization]
    B --> C[Finding and Asset Domain]
    C --> D[Remediation Workflow]
    D --> E[Evidence and Retest]
    C --> F[Audit Events]
    D --> G[Dashboard and Reports]
~~~

Importers should be separate modules. A source-specific parser must validate its actual documented input format and produce the same internal finding model.

## Proposed technology

| Area | Planned choice |
|---|---|
| Backend | C# and ASP.NET Core |
| Frontend | Angular and TypeScript |
| Database | PostgreSQL |
| API style | REST with explicit request/response contracts |
| Initial imports | Versioned JSON/CSV fixtures |
| Later imports | Tested formats from tools such as OWASP ZAP or Nmap |

Commercial tools and file formats may vary by version and configuration. An integration should not be advertised as supported until it has a representative fixture, parser tests, and documentation.

## Domain model

- **Asset:** A system or service with an owner and environment metadata.
- **Finding:** A source observation about a potential security issue.
- **Finding occurrence:** A finding observed on a particular asset or at a particular time.
- **Remediation task:** An assigned activity with due date and status.
- **Evidence:** A reference or attachment describing validation of a fix.
- **Retest:** A recorded re-check with outcome and timestamp.
- **Audit event:** An append-oriented record of significant changes.

Deduplication should preserve source references and observations rather than discard useful history.

## Prioritization and severity

Use source-provided severity and documented standards when available. Do not invent CVSS scores or imply that a custom business priority is an official severity. Any additional prioritization model should be transparent, configurable, and distinguish urgency from technical severity.

## Development roadmap

1. Define the domain model, finding lifecycle, and authorization matrix.
2. Build ASP.NET Core endpoints and database migrations.
3. Add validation and unit tests for state transitions.
4. Build Angular asset and finding workflows.
5. Implement one documented sample-import format end to end.
6. Add audit events, remediation evidence, and retesting.
7. Add verified scanner import adapters and report exports.
8. Add CI checks, security review, and deployment documentation.

## Quality strategy

- Unit tests for normalizing imported fields.
- Integration tests for persistence and transaction boundaries.
- Authorization tests for cross-role access.
- Negative tests for malformed and oversized imports.
- Tests for duplicate observations and source provenance.
- State-transition tests for remediation and retesting.
- Regression fixtures for each supported importer.
- Dependency and secret-scanning checks when CI is configured.

## Security considerations

Imported files are untrusted input. Validate file size and content, avoid unsafe deserialization, sanitize any rendered text, and protect audit records. Enforce authorization on the server for every sensitive operation; hiding a button in the UI is not access control. Never store passwords, API tokens, or unredacted secrets in findings or logs.

## Current status

The repository currently contains the project brief and license only. The vulnerability workflow, integrations, dashboards, and security controls described above remain planned.

## Author & Development

**Project author and solo developer:** [Nurul Faiyaz](https://github.com/tracolerd).

This project is independently developed and maintained by Nurul Faiyaz. AI-assisted tools may be used as part of the development workflow; the project author remains responsible for technical decisions, code review, validation, and maintenance.

## License

MIT. See [LICENSE](LICENSE).
