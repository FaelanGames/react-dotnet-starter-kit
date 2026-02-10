$ErrorActionPreference = "Stop"

Write-Host "Running backend tests..."
Push-Location "$PSScriptRoot\..\backend"
dotnet test
Pop-Location

Write-Host "Running frontend tests..."
Push-Location "$PSScriptRoot\..\frontend"
npm run test -- --run
Pop-Location
