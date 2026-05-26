#requires -Version 5.1
<#
.SYNOPSIS
    Kör Happy Homes testsvit, genererar en HTML-rapport över täckning och
    öppnar rapporten i webbläsaren.

.DESCRIPTION
    1. Kör `dotnet test` med XPlat Code Coverage (Cobertura) över hela lösningen.
    2. Installerar ReportGenerator som lokalt dotnet-verktyg om det saknas.
    3. Slår ihop alla coverage.cobertura.xml-filer till en HTML-rapport.
    4. Öppnar rapporten i standardwebbläsaren.

    Körs från projektets rot.
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [switch]$NoBrowser
)

$ErrorActionPreference = 'Stop'

$repoRoot   = $PSScriptRoot
$solution   = Join-Path $repoRoot 'HappyHome.sln'
$resultsDir = Join-Path $repoRoot 'TestResults'
$reportDir  = Join-Path $repoRoot 'TestReport'

Write-Host ""
Write-Host "==> Happy Home: kör tester och bygger HTML-rapport" -ForegroundColor Cyan
Write-Host "    Rot:        $repoRoot"
Write-Host "    Solution:   $solution"
Write-Host "    Resultat:   $resultsDir"
Write-Host "    Rapport:    $reportDir"
Write-Host ""

# 1. Städa gamla resultat så vi inte blandar in äldre täckningsfiler i rapporten.
if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
if (Test-Path $reportDir)  { Remove-Item $reportDir  -Recurse -Force }
New-Item -ItemType Directory -Path $resultsDir | Out-Null

# 2. Säkerställ ReportGenerator som lokalt dotnet-verktyg.
$manifest = Join-Path $repoRoot '.config\dotnet-tools.json'
if (-not (Test-Path $manifest)) {
    Write-Host "==> Skapar lokalt dotnet-verktygsmanifest" -ForegroundColor Yellow
    dotnet new tool-manifest | Out-Null
}

$toolList = dotnet tool list --local 2>$null
if (-not ($toolList -match 'dotnet-reportgenerator-globaltool')) {
    Write-Host "==> Installerar ReportGenerator (lokalt dotnet-verktyg)" -ForegroundColor Yellow
    dotnet tool install dotnet-reportgenerator-globaltool | Out-Null
}

# 3. Kör testerna med täckning. Cobertura är ett brett stött format som
#    ReportGenerator kan läsa direkt.
Write-Host "==> Kör dotnet test" -ForegroundColor Cyan
dotnet test $solution `
    --configuration $Configuration `
    --collect:"XPlat Code Coverage" `
    --results-directory $resultsDir `
    --logger "console;verbosity=normal"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Tester misslyckades (exitkod $LASTEXITCODE). Avbryter." -ForegroundColor Red
    exit $LASTEXITCODE
}

# 4. Generera HTML-rapport. Reportgenerator letar igenom underkataloger.
$coverageFiles = Get-ChildItem -Path $resultsDir -Recurse -Filter 'coverage.cobertura.xml'
if (-not $coverageFiles) {
    Write-Host "Hittade ingen coverage.cobertura.xml under $resultsDir. Avbryter." -ForegroundColor Red
    exit 1
}

$reportsArg = ($coverageFiles | ForEach-Object { $_.FullName }) -join ';'

Write-Host "==> Genererar HTML-rapport" -ForegroundColor Cyan
dotnet reportgenerator `
    "-reports:$reportsArg" `
    "-targetdir:$reportDir" `
    "-reporttypes:Html;HtmlSummary" `
    "-title:Happy Home — testrapport" | Out-Null

$indexFile = Join-Path $reportDir 'index.html'
if (-not (Test-Path $indexFile)) {
    Write-Host "Rapportfilen saknas: $indexFile" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Rapport klar: $indexFile" -ForegroundColor Green

if (-not $NoBrowser) {
    Write-Host "==> Öppnar rapporten i webbläsaren" -ForegroundColor Cyan
    Start-Process $indexFile
}
