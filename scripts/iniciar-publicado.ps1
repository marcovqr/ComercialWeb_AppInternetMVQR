[CmdletBinding()]
param([string]$Url = 'http://localhost:5191')

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$publicado = Join-Path $raiz 'publish\ComercialWeb'
$dll = Join-Path $publicado 'Comercial.Api.dll'

if (-not (Test-Path $dll)) { throw 'No existe el paquete publicado. Ejecute primero .\scripts\publicar-aplicacion.ps1.' }
if ([string]::IsNullOrWhiteSpace($env:ConnectionStrings__Comercial)) { throw 'Defina ConnectionStrings__Comercial en esta consola antes de iniciar la aplicación.' }

$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:ASPNETCORE_URLS = $Url
Write-Host "Sistema Comercial disponible en $Url" -ForegroundColor Green
Start-Process $Url
dotnet $dll
