# build-package.ps1 - build the module in Release mode and zip the output for
# upload via the host's /admin/modules page.
#
# Usage:  .\build-package.ps1
# Output: dist\FlexCms.InvestPro-{version}.zip
#
# Requires: .NET 10 SDK, Windows PowerShell 5.1+ or PowerShell 7+

$ErrorActionPreference = 'Stop'
Set-Location -Path $PSScriptRoot

$ModuleId = 'FlexCms.InvestPro'
$Csproj   = "$ModuleId.csproj"
$Manifest = 'module.json'

if (-not (Test-Path $Csproj)) {
    Write-Error "$Csproj not found in $PSScriptRoot"
}

# Read version out of module.json so the zip name matches the release.
$Version = 'unknown'
try {
    $json = Get-Content $Manifest -Raw | ConvertFrom-Json
    if ($json.Version) { $Version = $json.Version }
} catch {
    Write-Warning "Could not parse $Manifest - version will be 'unknown'."
}

Write-Host "==> Building $ModuleId v$Version (Release)" -ForegroundColor Cyan
& dotnet build $Csproj -c Release --nologo
if ($LASTEXITCODE -ne 0) { Write-Error 'dotnet build failed.' }

$OutDir  = Join-Path 'bin' 'Release\net10.0'
$DllPath = Join-Path $OutDir "$ModuleId.dll"
if (-not (Test-Path $DllPath)) {
    Write-Error "build did not produce $DllPath"
}

$Staging = Join-Path ([System.IO.Path]::GetTempPath()) ("fcms-pkg-" + [System.Guid]::NewGuid().ToString('N').Substring(0,8))
$Staged  = Join-Path $Staging $ModuleId
New-Item -ItemType Directory -Path $Staged -Force | Out-Null

try {
    Write-Host "==> Staging output -> $Staged" -ForegroundColor Cyan
    Copy-Item -Path (Join-Path $OutDir '*') -Destination $Staged -Recurse -Force

    # Framework-side DLLs the host already supplies. Bundling them risks type
    # identity bugs because the host's loader will pick whichever copy loads first.
    Write-Host '==> Removing host-provided framework files' -ForegroundColor Cyan
    Remove-Item -Force -ErrorAction SilentlyContinue (Join-Path $Staged 'FlexCms.Framework.dll')
    Remove-Item -Force -ErrorAction SilentlyContinue (Join-Path $Staged 'FlexCms.Framework.pdb')

    if (-not (Test-Path 'dist')) { New-Item -ItemType Directory -Path 'dist' | Out-Null }
    $ZipPath = Join-Path 'dist' "$ModuleId-$Version.zip"
    if (Test-Path $ZipPath) { Remove-Item -Force $ZipPath }

    Write-Host "==> Zipping -> $ZipPath" -ForegroundColor Cyan
    Compress-Archive -Path (Join-Path $Staged '*') -DestinationPath $ZipPath -Force

    $SizeKb = [math]::Round((Get-Item $ZipPath).Length / 1KB, 0)
    Write-Host ''
    $msg = '[OK] Built ' + $ZipPath + ' (' + $SizeKb + ' KB)'
    Write-Host $msg -ForegroundColor Green
    Write-Host '     Upload via the host''s /admin/modules page, then restart the host.'
}
finally {
    if (Test-Path $Staging) { Remove-Item -Recurse -Force $Staging }
}
