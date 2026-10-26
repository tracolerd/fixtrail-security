# Prioritized Engineering Backlog

This backlog is based on the documented .NET 8, PostgreSQL, and Angular local MVP. Treat each item as a candidate until reproduced with focused tests.

## Candidate issues

1. **Make first-admin bootstrap concurrency-safe and one-time** — Prevent concurrent bootstrap requests from creating ambiguous administrative state and require explicit proof that initial provisioning is closed.
2. **Validate JWT signing key strength at startup** — Reject missing, short, placeholder, or otherwise unsafe production signing keys with a clear startup error.
3. **Make scanner imports atomic or explicitly partial** — Define all-or-nothing versus partial import semantics and return a structured per-record result without leaking parser internals.
4. **Centralize workflow transition rules** — Enforce allowed finding/remediation state transitions in a domain service instead of relying on individual controllers.
5. **Complete remediation and audit read endpoints** — Make current state, verification evidence, and historical transitions inspectable with authorization checks.
6. **Add PostgreSQL integration tests to CI** — Provision a disposable PostgreSQL service, apply migrations, and test core workflows against the actual database engine.
7. **Verify ownership/authorization for every finding and asset** — Ensure authenticated users cannot read or mutate records outside their permitted scope.
8. **Bound scanner import size and parsing resources** — Limit upload/body size, row count, parser duration, and error output for untrusted input.
9. **Define append-only audit semantics** — Preserve who changed a finding, when, and why; prevent ordinary update/delete paths from silently rewriting history.
10. **Make risk prioritization explainable** — Document and test how severity, exploitability, asset criticality, and remediation status influence any priority score.

## Suggested order

Prioritize bootstrap/JWT and object-level authorization before network exposure (1, 2, 7). Then harden imports and state transitions, and only claim production readiness after PostgreSQL-backed CI and audit-history behavior are verified.
