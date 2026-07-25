[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$api = Join-Path $root 'src\backend\Comercial.Api'
$frontend = Join-Path $root 'src\frontend'

Write-Host 'Iniciando Comercial API en http://localhost:5191 ...' -ForegroundColor Cyan
Start-Process powershell -WorkingDirectory $api -ArgumentList @('-NoExit', '-Command', 'dotnet run --no-restore')

Write-Host 'Iniciando React en http://localhost:5173 ...' -ForegroundColor Cyan
Start-Process powershell -WorkingDirectory $frontend -ArgumentList @('-NoExit', '-Command', 'npm run dev')

Write-Host 'Espere unos segundos y abra http://localhost:5173' -ForegroundColor Green
Start-Sleep -Seconds 3
Start-Process 'http://localhost:5173'
