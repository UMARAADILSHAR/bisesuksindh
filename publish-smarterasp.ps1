<#
.SYNOPSIS
    Builds, publishes, and packages BISE Sukkur Blazor Server for SmarterASP.NET (IIS).
.DESCRIPTION
    This script publishes the application with Release configuration, ensures IIS web.config
    and logging folders are created, and packages the output into a ZIP archive ready for
    1-click upload in the SmarterASP.NET Control Panel File Manager or FTP deployment.
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$PublishDir = "",
    [string]$ZipFile = ""
)

$ErrorActionPreference = "Stop"

$rootDir = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }
if ([string]::IsNullOrWhiteSpace($PublishDir)) {
    $PublishDir = Join-Path $rootDir "publish"
}
if ([string]::IsNullOrWhiteSpace($ZipFile)) {
    $ZipFile = Join-Path $rootDir "BiseSukkur-SmarterASP-Deploy.zip"
}

$projectPath = Join-Path $rootDir "src\BiseSukkur.Web\BiseSukkur.Web.csproj"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  BISE Sukkur - SmarterASP.NET (IIS) Publish & Packaging" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Check Prerequisites
Write-Host "`n[1/5] Checking .NET SDK version..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "Found .NET SDK: $dotnetVersion" -ForegroundColor Green

# 2. Clean previous artifacts
Write-Host "`n[2/5] Cleaning previous publish artifacts..." -ForegroundColor Yellow
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
    Write-Host "Removed old: $PublishDir" -ForegroundColor DarkGray
}
if (Test-Path $ZipFile) {
    Remove-Item -Force $ZipFile
    Write-Host "Removed old: $ZipFile" -ForegroundColor DarkGray
}

# 3. Publish Web Application
Write-Host "`n[3/5] Publishing $projectPath (Configuration: $Configuration)..." -ForegroundColor Yellow
dotnet publish $projectPath -c $Configuration -o $PublishDir --no-self-contained

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# 4. Verify IIS Configuration & Create Logs Directory
Write-Host "`n[4/5] Verifying IIS web.config and logs folder..." -ForegroundColor Yellow
$webConfigFile = Join-Path $PublishDir "web.config"
if (-not (Test-Path $webConfigFile)) {
    Write-Warning "web.config not found in publish folder. Copying from src/BiseSukkur.Web..."
    $sourceWebConfig = Join-Path $rootDir "src\BiseSukkur.Web\web.config"
    Copy-Item $sourceWebConfig $webConfigFile -Force
}

$logsDir = Join-Path $PublishDir "logs"
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
    Write-Host "Created IIS stdout logs directory: $logsDir" -ForegroundColor DarkGray
}

# 5. Create ZIP Archive for 1-Click Upload
Write-Host "`n[5/5] Creating deployment ZIP archive..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem
Start-Sleep -Milliseconds 500
[System.IO.Compression.ZipFile]::CreateFromDirectory($PublishDir, $ZipFile, [System.IO.Compression.CompressionLevel]::Optimal, $false)

$zipSize = (Get-Item $ZipFile).Length / 1MB
Write-Host "`n============================================================" -ForegroundColor Green
Write-Host "  PUBLISH & PACKAGING SUCCESSFUL!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host "Publish Folder : $PublishDir" -ForegroundColor White
Write-Host "Deploy Archive : $ZipFile ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor White
Write-Host "`nHOW TO DEPLOY TO SMARTERASP.NET:" -ForegroundColor Cyan
Write-Host "  Option A (Recommended - File Manager):" -ForegroundColor White
Write-Host "    1. Log in to SmarterASP.NET Control Panel."
Write-Host "    2. Go to 'Files' -> 'File Manager' -> Select your site root (e.g. /site1)."
Write-Host "    3. Upload '$([System.IO.Path]::GetFileName($ZipFile))'."
Write-Host "    4. Click 'Extract' on the uploaded ZIP file."
Write-Host "  Option B (FTP):" -ForegroundColor White
Write-Host "    1. Connect via FileZilla / WinSCP using your SmarterASP.NET FTP credentials."
Write-Host "    2. Upload the contents of '$PublishDir' to your site root folder."
Write-Host "  Option C (Automated GitHub Actions):" -ForegroundColor White
Write-Host "    Push to GitHub with SMARTERASP_FTP_* secrets configured."
Write-Host "============================================================`n"
