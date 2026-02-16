# Personal Finance Tracker

![CI](https://github.com/malayakondagiridhar/personal-finance-tracker/actions/workflows/ci.yml/badge.svg)

A practical finance app to track income/expenses, monitor monthly spending, and build better money habits.

This project is being evolved into a **portfolio-quality full-stack app** with a React frontend and an implemented **ASP.NET Core + SQL Server backend foundation**.

## Why this project

Most finance demos stop at UI. This project is focused on real-world engineering skills:
- clean domain modeling for transactions and budgets
- reliable data flow and validation
- reporting/summaries that are useful for decisions
- production-style project hygiene (docs, issue templates, PR templates, clean commit history)

## Current status

- Frontend scaffold is available (React + TypeScript + Vite)
- Backend foundation is implemented:
  - layered backend solution (API/Application/Domain/Infrastructure)
  - EF Core + SQL Server DbContext and initial migration
  - core domain entities and mappings
  - service contracts + service implementations (Category, Transaction, Budget, Summary)
  - API controller skeletons
  - global exception middleware + validation baseline
  - exception-to-HTTP mapping (400/404/409/500)
  - integration tests for categories, transactions, budgets, and summary

## Tech stack

### Frontend
- React 19
- TypeScript
- Vite
- Tailwind CSS

### Backend
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- SQL Server
- FluentValidation

## Project structure

```text
personal-finance-tracker/
├─ src/                                  # React frontend
├─ backend/
│  ├─ PersonalFinanceTracker.slnx
│  ├─ src/
│  │  ├─ PersonalFinanceTracker.Api
│  │  ├─ PersonalFinanceTracker.Application
│  │  ├─ PersonalFinanceTracker.Domain
│  │  └─ PersonalFinanceTracker.Infrastructure
│  └─ tests/
│     ├─ PersonalFinanceTracker.UnitTests
│     └─ PersonalFinanceTracker.IntegrationTests
├─ .github/
├─ README.md
├─ CONTRIBUTING.md
└─ LICENSE
```

## Local setup

### Frontend
```bash
npm install
npm run dev
```

### Backend
From repository root:

```bash
dotnet build backend/PersonalFinanceTracker.slnx
```

Apply database migration:

```bash
dotnet ef database update \
  --project backend/src/PersonalFinanceTracker.Infrastructure/PersonalFinanceTracker.Infrastructure.csproj \
  --startup-project backend/src/PersonalFinanceTracker.Api/PersonalFinanceTracker.Api.csproj
```

Run API:

```bash
dotnet run --project backend/src/PersonalFinanceTracker.Api/PersonalFinanceTracker.Api.csproj
```

Run integration tests:

```bash
dotnet test backend/tests/PersonalFinanceTracker.IntegrationTests/PersonalFinanceTracker.IntegrationTests.csproj
```

## Architecture (backend)

```text
Controllers (API)
    -> Application service contracts
        -> Infrastructure service implementations
            -> EF Core DbContext
                -> SQL Server
```

Layer intent:
- `Api`: routing, validation, middleware, HTTP contracts
- `Application`: service interfaces + DTO contracts
- `Domain`: entities/enums and business vocabulary
- `Infrastructure`: EF persistence + service implementations

## Engineering decisions (high-level)

- Chose layered architecture over monolith-style controllers to keep responsibilities clear.
- Used EF Core + migrations for schema versioning and repeatable database setup.
- Added global exception mapping to keep API semantics predictable (`400/404/409/500`).
- Added CI quality gates and integration tests early to prevent regressions.

## API endpoints (v1)

> Base path: `/api/v1`

- `POST /api/v1/categories`
- `GET /api/v1/categories`
- `POST /api/v1/transactions`
- `GET /api/v1/transactions?fromDateUtc=&toDateUtc=&categoryId=&type=`
- `PUT /api/v1/transactions/{transactionId}`
- `DELETE /api/v1/transactions/{transactionId}`
- `POST /api/v1/budgets`
- `GET /api/v1/budgets/status?year={year}&month={month}`
- `GET /api/v1/summary/monthly?year={year}&month={month}`

### Auth requirement

All finance endpoints require:
- `Authorization: Bearer <jwt>`
- JWT claim `scope=finance-api`

User context is derived from token claims and is no longer accepted from query/body inputs on protected operations.

## Sample requests

Create category:

```http
POST /api/v1/categories
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "name": "Food",
  "description": "Groceries and dining",
  "isDefault": false
}
```

Create transaction:

```http
POST /api/v1/transactions
Content-Type: application/json
Authorization: Bearer <jwt>

{
  "categoryId": "00000000-0000-0000-0000-000000000010",
  "amount": 950.50,
  "type": 2,
  "transactionDateUtc": "2026-02-16T08:30:00Z",
  "note": "Weekly groceries"
}
```

## Roadmap

- [x] Phase 1: Documentation + repo hygiene
- [x] Phase 2: .NET API scaffold + SQL schema + migrations + core services
- [x] Phase 3: production-grade endpoint behavior + richer validations + integration tests
- [x] Phase 4: CI quality gates + test coverage expansion
- [ ] Phase 5: Release v1.0.0 with portfolio case study

## CI quality gates

GitHub Actions CI runs on push/PR to `main` and validates:
- frontend install + build
- backend restore + build
- unit tests
- integration tests

## Phase 2 completion checklist

- [x] Clean architecture skeleton in place
- [x] SQL Server migration baseline committed
- [x] Category/Transaction/Budget/Summary services implemented
- [x] Validation baseline and global exception middleware added
- [x] PR-driven, single-responsibility commit history maintained

## Phase 3 completion checklist

- [x] Exception mapping aligned to API semantics (400/404/409/500)
- [x] Service hardening for conflict and invalid-range guardrails
- [x] Integration coverage for Categories + Transactions flows
- [x] Integration coverage for Budgets + Summary flows
- [x] All Phase 3 PRs completed from dedicated phase branch

## Phase 4 completion checklist

- [x] GitHub Actions CI pipeline added for frontend/backend build + tests
- [x] Unit test baseline expanded for core service guard behavior
- [x] API error-contract integration coverage added (400/404/409 payload checks)
- [x] Branch protection enabled on `main` with required status check: `build-and-test`

## Release artifacts

- [Changelog](./CHANGELOG.md)
- [v1.0.0 release notes](./docs/RELEASE_NOTES_v1.0.0.md)

## What I built (Phase 1–4)

- Portfolio-structured repository with disciplined PR/commit workflow
- ASP.NET Core Web API with layered architecture and EF Core persistence
- SQL schema/migrations for users, categories, transactions, and budgets
- Category/Transaction/Budget/Summary service implementations
- Validation + error contract consistency
- CI workflow enforcing frontend/backend build + test quality gates
- Unit and integration test coverage for key business and API paths

## What I learned

- How to structure backend work into small, reviewable, single-purpose PRs
- How to design API behavior with reliable status-code semantics
- How to combine unit tests + integration tests for confidence and speed
- How to set up CI and branch protection to enforce quality automatically

## Learning goals

- Build backend depth in **.NET and SQL Server**
- Practice clean architecture and API design
- Produce recruiter-friendly project evidence (docs, commits, releases)

---

If you want to contribute, see [CONTRIBUTING.md](./CONTRIBUTING.md).

