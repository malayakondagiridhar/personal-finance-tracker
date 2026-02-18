# Database Concurrency and Rollback Safety Notes

## Scope
This note covers Phase 3 database safety hardening:
1. RowVersion concurrency tokens
2. Normalized category uniqueness constraint

## Added Migrations
- `20260218090723_AddRowVersionConcurrencyTokens`
- `20260218120939_AddNormalizedCategoryNameConstraint`

## Why this matters
- `RowVersion` enables optimistic concurrency detection for concurrent updates.
- Normalized category uniqueness (`UserId + NormalizedName`) enforces case-insensitive duplicate protection at DB level.

## Forward Migration
From repository root:

```powershell
dotnet ef database update \
  --project backend/src/PersonalFinanceTracker.Infrastructure \
  --startup-project backend/src/PersonalFinanceTracker.Api
```

## Rollback Strategy (single step)
To roll back the latest migration safely:

```powershell
dotnet ef database update 20260218090723_AddRowVersionConcurrencyTokens \
  --project backend/src/PersonalFinanceTracker.Infrastructure \
  --startup-project backend/src/PersonalFinanceTracker.Api
```

To roll back both Phase 3 migrations, target the migration before them (`InitialFinanceSchema`):

```powershell
dotnet ef database update 20260216153959_InitialFinanceSchema \
  --project backend/src/PersonalFinanceTracker.Infrastructure \
  --startup-project backend/src/PersonalFinanceTracker.Api
```

## Operational Safety Checklist
Before applying in production:
- Take a verified database backup/snapshot.
- Apply in staging first using production-like data volume.
- Confirm API smoke tests pass after migration.
- Monitor error rate for `409 Concurrency conflict` spikes post-deploy.

After deploy:
- Validate category create flow rejects case-insensitive duplicates.
- Validate update endpoints still behave correctly under concurrent writes.
- Keep rollback command ready and tested.

## Notes
- API now maps `DbUpdateConcurrencyException` to HTTP 409.
- Clients should treat 409 as retry/reload-required conflict state.
