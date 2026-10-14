<#
.SYNOPSIS
Builds the application and signs its sparse MSIX package.
Run packaging\New-DevCert.ps1 once in elevated Windows PowerShell first.
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "src\WinNotiMirror\WinNotiMirror.csproj"
$certificate = Join-Path $PSScriptRoot "packaging\WinNotiMirrorDevCert.pfx"

if (-not (Test-Path -LiteralPath $certificate)) {
    throw "Development certificate missing. Run packaging\New-DevCert.ps1 in elevated Windows PowerShell first."
}

& dotnet build $project --configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw "Application build failed with exit code $LASTEXITCODE" }

& (Join-Path $PSScriptRoot "packaging\Build-Package.ps1")
Write-Host "Application: $(Join-Path $PSScriptRoot "src\WinNotiMirror\bin\$Configuration\net8.0-windows10.0.19041.0\WinNotiMirror.exe")"
