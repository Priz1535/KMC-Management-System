$ErrorActionPreference = "Stop"

Write-Host "Restoring KMC solution..." -ForegroundColor Cyan
dotnet restore "$PSScriptRoot\KMCEventManagement.sln"

$apiCommand = "Set-Location '$PSScriptRoot\KMCEventAPI'; dotnet run --launch-profile https"
$clientCommand = "Set-Location '$PSScriptRoot\KMCEventClient'; dotnet run --launch-profile https"

Write-Host "Starting API..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", $apiCommand

Start-Sleep -Seconds 5

Write-Host "Starting MVC client..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", $clientCommand

Write-Host "API Swagger: https://localhost:7047/swagger" -ForegroundColor Green
Write-Host "MVC Client:   https://localhost:7057" -ForegroundColor Green
