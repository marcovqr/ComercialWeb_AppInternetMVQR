[CmdletBinding()]
param([string]$Destino)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$frontend = Join-Path $raiz 'src\frontend'
$api = Join-Path $raiz 'src\backend\Comercial.Api'
if ([string]::IsNullOrWhiteSpace($Destino)) { $Destino = Join-Path $raiz 'publish\ComercialWeb' }
$Destino = [System.IO.Path]::GetFullPath($Destino)
$publico = Join-Path $api 'wwwroot'

if (-not $Destino.StartsWith($raiz, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'Por seguridad, el destino debe estar dentro de ComercialWebMigration.'
}

Write-Host '1/3 Compilando la interfaz React...' -ForegroundColor Cyan
Push-Location $frontend
try { npm run build } finally { Pop-Location }

Write-Host '2/3 Integrando la interfaz con ASP.NET Core...' -ForegroundColor Cyan
if (Test-Path $publico) { Remove-Item -LiteralPath $publico -Recurse -Force }
New-Item -ItemType Directory -Path $publico | Out-Null
Copy-Item -Path (Join-Path $frontend 'dist\*') -Destination $publico -Recurse -Force

Write-Host '3/3 Generando el paquete de publicación...' -ForegroundColor Cyan
dotnet publish $api -c Release -o $Destino --no-restore -p:UseAppHost=false
Copy-Item -LiteralPath (Join-Path $api 'appsettings.Production.example.json') -Destination (Join-Path $Destino 'appsettings.Production.example.json') -Force
Write-Host "Publicación terminada en: $Destino" -ForegroundColor Green
Write-Host 'Configure la conexión mediante ConnectionStrings__Comercial antes de iniciar.' -ForegroundColor Yellow
