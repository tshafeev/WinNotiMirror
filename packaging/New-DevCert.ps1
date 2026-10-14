<#
.SYNOPSIS
One-time setup: creates a self-signed code-signing certificate matching the
AppxManifest.xml Publisher, and trusts it so Windows will sideload the
sparse package signed with it.

Run this in an ELEVATED PowerShell window (Run as Administrator).
#>
$ErrorActionPreference = "Stop"

$subject = "CN=WinNotiMirror Dev Cert"
$pfxPath = Join-Path $PSScriptRoot "WinNotiMirrorDevCert.pfx"
$cerPath = Join-Path $PSScriptRoot "WinNotiMirrorDevCert.cer"
$pfxPassword = ConvertTo-SecureString -String "WinNotiMirror!2026" -Force -AsPlainText

$existing = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $subject }
if ($existing) {
    Write-Host "Certificate with subject '$subject' already exists (thumbprint $($existing.Thumbprint)); reusing it."
    $cert = $existing | Select-Object -First 1
} else {
    Write-Host "Creating self-signed certificate: $subject"
    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $subject `
        -KeyUsage DigitalSignature `
        -FriendlyName "WinNotiMirror sparse package signing cert" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}Subject Type:End Entity")
}

Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pfxPassword | Out-Null
Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null

Write-Host "Trusting certificate in LocalMachine\TrustedPeople..."
Import-Certificate -FilePath $cerPath -CertStoreLocation "Cert:\LocalMachine\TrustedPeople" | Out-Null

Write-Host "Done. Thumbprint: $($cert.Thumbprint)"
Write-Host "PFX: $pfxPath"
