<#
.SYNOPSIS
Generates the placeholder PNG images required by AppxManifest.xml.
#>
Add-Type -AssemblyName System.Drawing

$assetsDir = Join-Path $PSScriptRoot "Assets"
New-Item -ItemType Directory -Force -Path $assetsDir | Out-Null

function New-SolidPng {
    param([string]$Path, [int]$Size)

    $bmp = New-Object System.Drawing.Bitmap($Size, $Size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::FromArgb(255, 32, 96, 200))

    $brush = [System.Drawing.Brushes]::White
    $font = New-Object System.Drawing.Font("Segoe UI", [Math]::Max(8, $Size * 0.4), [System.Drawing.FontStyle]::Bold)
    $text = "N"
    $sizeF = $g.MeasureString($text, $font)
    $x = ($Size - $sizeF.Width) / 2
    $y = ($Size - $sizeF.Height) / 2
    $g.DrawString($text, $font, $brush, $x, $y)

    $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $bmp.Dispose()
}

New-SolidPng -Path (Join-Path $assetsDir "StoreLogo.png") -Size 50
New-SolidPng -Path (Join-Path $assetsDir "Square44x44Logo.png") -Size 44
New-SolidPng -Path (Join-Path $assetsDir "Square150x150Logo.png") -Size 150

Write-Host "Placeholder assets written to $assetsDir"
