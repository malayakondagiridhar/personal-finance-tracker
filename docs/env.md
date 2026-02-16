# Environment Matrix

## Local Development
- `ASPNETCORE_ENVIRONMENT=Development`
- `DatabaseProvider=SqlServer`
- `ConnectionStrings__DefaultConnection` points to local SQL Server.
- `Cors__AllowedOrigins=http://localhost:5173`

## Test (Integration)
- `DatabaseProvider=InMemory` (current integration default)
- `InMemoryDatabaseName=PersonalFinanceTrackerIntegrationTests`

## CI
- Use repository secrets for sensitive values.
- Never commit real JWT signing keys.

## Staging/Production (target)
- Set all configuration via environment variables (no hardcoded secrets).
- Require strong JWT secret or external metadata-based key management.
- Restrict CORS to trusted frontend origin(s) only.
- Use managed SQL and rotated credentials.
