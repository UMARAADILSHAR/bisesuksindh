<#
.SYNOPSIS
    Applies EF Core migrations to SmarterASP.NET MS SQL Server database.
.DESCRIPTION
    This script assists in configuring and executing EF Core migrations against your SmarterASP.NET
    database. It supports:
    1. Direct connection string parameter.
    2. Interactive prompt for Server, Database, User ID, and Password.
    3. Auto-updating src\BiseSukkur.Web\appsettings.Production.json.
    4. Running 'dotnet ef database update' directly.
    5. Generating an idempotent SQL migration script for the SmarterASP.NET Control Panel Query Tool.
#>

[CmdletBinding()]
param(
    [string]$ConnectionString = "",
    [string]$Server = "",
    [string]$Database = "",
    [string]$UserId = "",
    [string]$Password = "",
    [switch]$UpdateAppsettings = $true,
    [switch]$GenerateSqlScriptOnly = $false
)

$ErrorActionPreference = "Stop"
$rootDir = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  BISE Sukkur - SmarterASP.NET Database Migration Tool" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Resolve Connection String
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    if ([string]::IsNullOrWhiteSpace($Server)) {
        Write-Host "`nPlease enter your SmarterASP.NET MS SQL details (from Control Panel):" -ForegroundColor Yellow
        $Server = Read-Host "  SQL Server / Host (e.g. MSSQL6001.site4now.net)"
        $Database = Read-Host "  Database Name (e.g. db_a1234_bise)"
        $UserId = Read-Host "  User ID / Username (e.g. db_a1234_bise_admin)"
        $Password = Read-Host "  Password" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
        $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    }

    if ([string]::IsNullOrWhiteSpace($Server) -or [string]::IsNullOrWhiteSpace($Database) -or [string]::IsNullOrWhiteSpace($UserId)) {
        Write-Error "Server, Database, and User ID cannot be empty."
        exit 1
    }

    $ConnectionString = "Server=$Server;Database=$Database;User Id=$UserId;Password=$Password;TrustServerCertificate=True;MultipleActiveResultSets=true;"
}

# Mask password for display
$displayConn = $ConnectionString -replace 'Password=[^;]+', 'Password=********'
Write-Host "`nTarget Connection:" -ForegroundColor Green
Write-Host "  $displayConn" -ForegroundColor DarkGray

# 2. Update appsettings.Production.json
if ($UpdateAppsettings) {
    $prodSettingsPath = Join-Path $rootDir "src\BiseSukkur.Web\appsettings.Production.json"
    if (Test-Path $prodSettingsPath) {
        Write-Host "`n[1/3] Updating appsettings.Production.json with your connection string..." -ForegroundColor Yellow
        $jsonContent = Get-Content $prodSettingsPath -Raw | ConvertFrom-Json
        $jsonContent.ConnectionStrings.DefaultConnection = $ConnectionString
        $newJson = $jsonContent | ConvertTo-Json -Depth 10
        Set-Content -Path $prodSettingsPath -Value $newJson -Encoding UTF8
        Write-Host "  appsettings.Production.json updated successfully." -ForegroundColor Green
    }
}

# 3. Check for dotnet-ef tool
Write-Host "`n[2/3] Checking dotnet-ef CLI tool..." -ForegroundColor Yellow
$efInstalled = $false
try {
    $null = dotnet ef --version 2>$null
    $efInstalled = ($LASTEXITCODE -eq 0)
} catch {
    $efInstalled = $false
}

if (-not $efInstalled) {
    Write-Host "  dotnet-ef is not installed globally. Installing dotnet-ef..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

$infraProject = Join-Path $rootDir "src\BiseSukkur.Infrastructure\BiseSukkur.Infrastructure.csproj"
$webProject = Join-Path $rootDir "src\BiseSukkur.Web\BiseSukkur.Web.csproj"

# 4. Generate idempotent SQL script (always created for backup & control panel execution)
$sqlScriptPath = Join-Path $rootDir "database-migrations-smarterasp.sql"
Write-Host "`n[3/3] Generating complete idempotent SQL script: database-migrations-smarterasp.sql..." -ForegroundColor Yellow
dotnet ef migrations script --idempotent --project $infraProject --startup-project $webProject --output $sqlScriptPath --connection "$ConnectionString"

if (Test-Path $sqlScriptPath) {
    $sqlSize = (Get-Item $sqlScriptPath).Length / 1KB
    Write-Host "  Generated SQL Script: $sqlScriptPath ($([math]::Round($sqlSize, 1)) KB)" -ForegroundColor Green
}

# 5. Apply migrations directly if not script-only
if (-not $GenerateSqlScriptOnly) {
    Write-Host "`nApplying EF Core migrations directly to SmarterASP.NET database..." -ForegroundColor Yellow
    Write-Host "Executing: dotnet ef database update..." -ForegroundColor DarkGray
    
    $env:ConnectionStrings__DefaultConnection = $ConnectionString
    dotnet ef database update --project $infraProject --startup-project $webProject --connection "$ConnectionString"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n============================================================" -ForegroundColor Green
        Write-Host "  DATABASE MIGRATION TO SMARTERASP.NET SUCCESSFUL!" -ForegroundColor Green
        Write-Host "============================================================" -ForegroundColor Green
        Write-Host "All tables, indexes, and foreign keys have been created on your SmarterASP.NET database." -ForegroundColor White
        Write-Host "`nNext Step: Run .\publish-smarterasp.ps1 and deploy your website." -ForegroundColor Cyan
    } else {
        Write-Warning "`nDirect remote connection to SmarterASP.NET failed or timed out."
        Write-Host "Note: SmarterASP.NET sometimes restricts external remote IP connections." -ForegroundColor Yellow
        Write-Host "`nAlternative 100% Guaranteed Solutions:" -ForegroundColor Cyan
        Write-Host "  Solution A (Zero manual SQL):" -ForegroundColor White
        Write-Host "    Since appsettings.Production.json is updated, once you upload the site ZIP to"
        Write-Host "    SmarterASP.NET, Program.cs automatically runs 'MigrateAsync()' and seeds data"
        Write-Host "    on the server locally during the first request!"
        Write-Host "  Solution B (Control Panel SQL Query):" -ForegroundColor White
        Write-Host "    1. Open SmarterASP.NET Control Panel -> MS SQL -> Database Manager / Query Tool."
        Write-Host "    2. Copy and paste the contents of 'database-migrations-smarterasp.sql' and click Execute."
    }
}
