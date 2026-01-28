$ErrorActionPreference = "Continue"
Set-Location "C:\dev\eir-med"
$env:Path = "C:\Users\marce\.dotnet\tools;C:\Program Files\dotnet;$env:Path"
$dotnetEf = "C:\Users\marce\.dotnet\tools\dotnet-ef.exe"

Write-Host "=== Checking dotnet-ef ===" -ForegroundColor Green
$output = & $dotnetEf --version 2>&1
Write-Host $output

Write-Host ""
Write-Host "=== Listing migrations ===" -ForegroundColor Green
$output = & $dotnetEf migrations list --project src/EirMed.Infrastructure --startup-project src/EirMed.API 2>&1
Write-Host $output

Write-Host ""
Write-Host "=== Applying migrations ===" -ForegroundColor Green
$output = & $dotnetEf database update --project src/EirMed.Infrastructure --startup-project src/EirMed.API 2>&1
Write-Host $output

Write-Host ""
Write-Host "Done. Exit code: $LASTEXITCODE"
exit $LASTEXITCODE
