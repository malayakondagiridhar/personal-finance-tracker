# Personal Finance Tracker v1.0.0 Release Notes

## Release date
2026-02-17

## Highlights
- Full backend foundation completed with ASP.NET Core + SQL Server.
- End-to-end API coverage for categories, transactions, budgets, and summary.
- CI quality gates and branch protection enabled.
- Portfolio-oriented documentation and engineering process evidence.

## Included in v1.0.0

### Backend platform
- Layered architecture (`Api/Application/Domain/Infrastructure`).
- EF Core DbContext + SQL Server migration baseline.
- Service implementations:
  - CategoryService
  - TransactionService
  - BudgetService
  - SummaryService

### API behavior quality
- FluentValidation request validation.
- Global exception middleware.
- Stable error semantics:
  - 400 Bad Request
  - 404 Not Found
  - 409 Conflict
  - 500 Internal Server Error

### Quality & testing
- Unit tests for service guard logic.
- Integration tests for core API workflows and error payload contracts.
- GitHub Actions CI enforcing build/test quality checks.

## Known limitations
- Authentication/authorization hardening is not finalized yet.
- Deployment automation is not yet part of this release.
- Coverage thresholds are not yet enforced in CI.

## Next (Phase 5+)
- deployment profile and environment hardening
- coverage thresholds and reports
- release automation and semantic versioning workflow
