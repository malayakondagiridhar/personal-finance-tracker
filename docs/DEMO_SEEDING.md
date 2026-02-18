# Demo Seed Pipeline (Local/Staging)

This project supports a repeatable demo seed command for local and staging environments.

## What it seeds

A deterministic demo user and sample finance data:
- demo user (`demo.user@personal-finance.local`)
- categories (`Food`, `Salary`, `Utilities`)
- monthly income/expense transactions
- a current-month budget

The seed is idempotent: running it multiple times does not create duplicates.

## Commands

### Option A (recommended)
```powershell
./backend/scripts/seed-demo.ps1
```

### Option B (manual)
```powershell
dotnet ef database update --project backend/src/PersonalFinanceTracker.Infrastructure --startup-project backend/src/PersonalFinanceTracker.Api
dotnet run --project backend/src/PersonalFinanceTracker.Api -- --seed-demo
```

## Notes

- Intended for local/staging showcase data only.
- Safe to run repeatedly.
- Do not use demo identities in production.
