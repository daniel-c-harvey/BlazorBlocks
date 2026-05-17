#Requires -Version 5.1
param(
    [Parameter(Mandatory)]
    [string]$ApiKey
)

$ErrorActionPreference = 'Stop'

$OutputDir = './nupkgs'

# --- Clean output directory ---
Write-Host "Cleaning output directory '$OutputDir'..."
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item $OutputDir -ItemType Directory | Out-Null

# --- Pack in dependency order ---
$projects = @(
    'Data.Shared/Data.Shared.csproj',   # Cerebellum.BlazorBlocks.Data
    'API.Shared/API.Shared.csproj',     # Cerebellum.BlazorBlocks.Api (depends on Data)
    'Web.Shared/Web.Shared.csproj'      # Cerebellum.BlazorBlocks.Web (depends on Api)
)

foreach ($proj in $projects) {
    Write-Host "Packing $proj..."
    dotnet pack $proj -c Release -o $OutputDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed for $proj (exit code $LASTEXITCODE)"
    }
}

# --- Push all packages ---
Write-Host "Pushing all packages to nuget.org..."
dotnet nuget push "$OutputDir/*.nupkg" `
    --api-key $ApiKey `
    --source https://api.nuget.org/v3/index.json `
    --skip-duplicate
if ($LASTEXITCODE -ne 0) {
    throw "dotnet nuget push failed (exit code $LASTEXITCODE)"
}

Write-Host ""
Write-Host "Done. Packages pushed successfully:"
Write-Host "  Cerebellum.BlazorBlocks.Data 1.0.0"
Write-Host "  Cerebellum.BlazorBlocks.Api  1.0.0"
Write-Host "  Cerebellum.BlazorBlocks.Web  1.0.0"
