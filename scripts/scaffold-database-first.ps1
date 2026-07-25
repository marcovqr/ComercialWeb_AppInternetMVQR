param(
  [string]$ConnectionString = 'Server=Z3ME-DESARR-L05\SQLEXPRESS;Database=comercial;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True'
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$infra = Join-Path $root 'src\backend\Comercial.Infrastructure\Comercial.Infrastructure.csproj'
$api = Join-Path $root 'src\backend\Comercial.Api\Comercial.Api.csproj'
$ef = Get-ChildItem -LiteralPath (Join-Path $env:USERPROFILE '.nuget\packages\dotnet-ef') -Recurse -Filter dotnet-ef.dll |
  Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
if (-not $ef) { throw 'No se encontró dotnet-ef. Instálelo con: dotnet tool install --global dotnet-ef --version 9.0.2' }

dotnet $ef dbcontext scaffold $ConnectionString Microsoft.EntityFrameworkCore.SqlServer `
  --project $infra --startup-project $api `
  --context ScaffoldedComercialContext `
  --context-dir 'Persistence\Scaffolded' `
  --output-dir 'Persistence\Scaffolded\Entities' `
  --namespace 'Comercial.Infrastructure.Persistence.Scaffolded.Entities' `
  --context-namespace 'Comercial.Infrastructure.Persistence.Scaffolded' `
  --no-onconfiguring --use-database-names --force
if ($LASTEXITCODE -ne 0) { throw 'El scaffolding falló; revise conexión, cifrado y permisos.' }
Write-Host 'Database First generado correctamente dentro de ComercialWebMigration.' -ForegroundColor Green
