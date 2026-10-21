# Architecture & Design Decisions
## 001 - Initial Tech Stack
- **Backend**: C# + ASP.NET Core
- **Frontend**: Angular + TypeScript
- **Database**: PostgreSQL with Entity Framework Core explicit migrations
- **Reasoning**: Strongly-typed ecosystem suitable for strict security data domain, structured API responses, and strict state transitions.
## 002 - Severity Handling & Provenance
- **Decision**: Preserve source-provided severity and identifiers without mutation. 
- **Reasoning**: System must not invent CVSS scores to maintain audit integrity. Finding occurrences will be deduplicated into observations without discarding history.
## 003 - Mocking External APIs (MVP Scope)
- **Decision**: MVP will run in local/mock mode without real scanner execution.
- **Reasoning**: Reduces dependency on external services and ensures the product functions purely as a vulnerability management workflow platform.
