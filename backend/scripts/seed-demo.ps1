param(
  [switch]$ApplyMigrations = $true
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir '..\..')
Set-Location $repoRoot

if ($ApplyMigrations) {
  dotnet ef database update --project backend/src/PersonalFinanceTracker.Infrastructure --startup-project backend/src/PersonalFinanceTracker.Api
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

dotnet run --project backend/src/PersonalFinanceTracker.Api -- --seed-demo
exit $LASTEXITCODE
