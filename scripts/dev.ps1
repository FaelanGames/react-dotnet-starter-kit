$ErrorActionPreference = "Stop"

# Start backend (new window)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\..\backend`"; dotnet watch --project src/StarterKit.Api run"

# Start frontend (new window)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PSScriptRoot\..\frontend`"; npm run dev"
