$ErrorActionPreference = "Stop"

Write-Host "Running backend coverage..."
Push-Location "$PSScriptRoot\..\"
npm run test:coverage:backend
npm run coverage:report:backend
Pop-Location

Write-Host "Running frontend coverage..."
Push-Location "$PSScriptRoot\..\frontend"
npm run test:coverage
Pop-Location

Write-Host "Coverage reports:"
Write-Host "  Backend HTML: backend/TestResults/CoverageReport/index.html"
Write-Host "  Frontend HTML: frontend/coverage/index.html"
