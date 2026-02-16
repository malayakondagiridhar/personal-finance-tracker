# Changelog

All notable changes to this project are documented here.

## [1.0.0] - 2026-02-17

### Added
- Phase 1 repository hygiene: README baseline, LICENSE, CONTRIBUTING, issue/PR templates.
- Backend layered solution (`Api`, `Application`, `Domain`, `Infrastructure`) with tests projects.
- EF Core SQL Server setup with initial migration (`InitialFinanceSchema`).
- Domain entities: `User`, `Category`, `Transaction`, `Budget` and `TransactionType` enum.
- Services for categories, transactions, budgets, and monthly summary.
- API controller contracts for core flows.
- Validation baseline (FluentValidation) and standardized validation error payloads.
- Global exception middleware with mapped HTTP semantics (`400/404/409/500`).
- Integration test suite covering categories, transactions, budgets, summary, and error contracts.
- Unit tests for core service guard behavior.
- GitHub Actions CI pipeline for frontend build, backend build, unit tests, and integration tests.
- Branch protection gate requiring `build-and-test` on `main`.

### Changed
- README expanded with architecture, runbook, API examples, and phase completion status.

### Fixed
- CI frontend Rollup optional dependency issue on Ubuntu runner.
- Summary query projection compatibility for EF translation.
