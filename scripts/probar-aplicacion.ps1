[CmdletBinding()]
param(
    [string]$ApiUrl = 'http://localhost:5191',
    [Parameter(Mandatory)] [string]$Usuario,
    [Parameter(Mandatory)] [SecureString]$Clave
)

$ErrorActionPreference = 'Stop'
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$plainPassword = [System.Net.NetworkCredential]::new('', $Clave).Password

try {
    Write-Host '1/4 Verificando API...' -ForegroundColor Cyan
    $health = Invoke-RestMethod "$ApiUrl/health" -TimeoutSec 10
    if ($health.status -ne 'ok') { throw 'La API no reportó estado OK.' }

    Write-Host '2/4 Verificando conexión Database First...' -ForegroundColor Cyan
    $database = Invoke-RestMethod "$ApiUrl/health/database" -TimeoutSec 15
    if ($database.status -ne 'ok') { throw 'No se pudo conectar con la base comercial.' }

    Write-Host '3/4 Verificando autenticación...' -ForegroundColor Cyan
    $body = @{ usuario = $Usuario; clave = $plainPassword } | ConvertTo-Json
    Invoke-RestMethod "$ApiUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body $body -WebSession $session -TimeoutSec 15 | Out-Null

    Write-Host '4/4 Verificando módulos de solo lectura...' -ForegroundColor Cyan
    $paths = @('resumen','clientes','proveedores','productos','inventario','compras','facturas','pagos','pendientes','instalaciones','mensualidades')
    foreach ($path in $paths) {
        $url = if ($path -eq 'clientes') { "$ApiUrl/api/clientes" } else { "$ApiUrl/api/modulos/$path" }
        Invoke-RestMethod $url -WebSession $session -TimeoutSec 30 | Out-Null
        Write-Host "  OK $path" -ForegroundColor DarkGreen
    }
    Write-Host 'Prueba completada: API, base, autenticación y consultas funcionan.' -ForegroundColor Green
}
finally {
    $plainPassword = $null
}
