<#
.SYNOPSIS
Packs AppxManifest.xml + Assets into sparse.msix and signs it with the dev
certificate created by New-DevCert.ps1. Run New-DevCert.ps1 (elevated) first.
No admin rights needed for this script.
#>
$ErrorActionPreference = "Stop"

$sdkBin = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64"
$makeappx = Join-Path $sdkBin "makeappx.exe"
$signtool = Join-Path $sdkBin "signtool.exe"

if (-not (Test-Path $makeappx)) { throw "makeappx.exe not found at $makeappx" }
if (-not (Test-Path $signtool)) { throw "signtool.exe not found at $signtool" }

$packagingDir = $PSScriptRoot
$msixPath = Join-Path $packagingDir "sparse.msix"
$pfxPath = Join-Path $packagingDir "WinNotiMirrorDevCert.pfx"
$pfxPassword = "WinNotiMirror!2026"

if (-not (Test-Path $pfxPath)) {
    throw "Signing cert not found at $pfxPath. Run New-DevCert.ps1 in an elevated PowerShell first."
}

# Pack only the manifest + assets (never the .pfx/.cer/scripts) from a clean staging folder.
$payloadDir = Join-Path $packagingDir "payload"
if (Test-Path $payloadDir) { Remove-Item $payloadDir -Recurse -Force }
New-Item -ItemType Directory -Path $payloadDir | Out-Null
Copy-Item (Join-Path $packagingDir "AppxManifest.xml") $payloadDir
Copy-Item (Join-Path $packagingDir "Assets") (Join-Path $payloadDir "Assets") -Recurse

if (Test-Path $msixPath) { Remove-Item $msixPath -Force }

Write-Host "Packing sparse.msix..."
& $makeappx pack /d $payloadDir /p $msixPath /nv
if ($LASTEXITCODE -ne 0) { throw "makeappx failed with exit code $LASTEXITCODE" }

Remove-Item $payloadDir -Recurse -Force

Write-Host "Signing sparse.msix..."
& $signtool sign /fd SHA256 /a /f $pfxPath /p $pfxPassword $msixPath
if ($LASTEXITCODE -ne 0) { throw "signtool failed with exit code $LASTEXITCODE" }

Write-Host "Built and signed: $msixPath"
