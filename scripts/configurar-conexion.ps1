$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot '..\src\backend\Comercial.Api\Comercial.Api.csproj'
$securePassword = Read-Host 'Contraseña SQL del usuario mqr' -AsSecureString
$credential = [System.Net.NetworkCredential]::new('', $securePassword)
$connectionString = "Server=localhost;Database=comercial;User Id=mqr;Password=$($credential.Password);Encrypt=True;TrustServerCertificate=True"

try {
    dotnet user-secrets set 'ConnectionStrings:Comercial' $connectionString --project $project
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet user-secrets terminó con el código $LASTEXITCODE."
    }

    Write-Host 'Conexión guardada en los secretos de usuario de .NET.'
}
finally {
    $connectionString = $null
    $credential = $null
    $securePassword.Dispose()
}
