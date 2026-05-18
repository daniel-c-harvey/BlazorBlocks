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
    'Models.Shared/Models.Shared.csproj',     # Cerebellum.BlazorBlocks.Models (foundation, no intra-solution dependencies)
    'Data/Data.csproj',                       # Cerebellum.BlazorBlocks.Data
    'Data.Postgres/Data.Postgres.csproj',     # Cerebellum.BlazorBlocks.Data.Postgres (depends on Data)
    'API/API.csproj',                         # Cerebellum.BlazorBlocks.Api (depends on Data)
    'Web/Web.csproj'                          # Cerebellum.BlazorBlocks.Web (depends on Api)
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
Get-ChildItem "$OutputDir/*.nupkg" | ForEach-Object {
    Write-Host "Pushing $($_.Name)..."
    dotnet nuget push $_.FullName `
        --api-key $ApiKey `
        --source https://api.nuget.org/v3/index.json
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet nuget push failed for $($_.Name) (exit code $LASTEXITCODE)"
    }
}

$csproj = [xml](Get-Content 'Data/Data.csproj')
$Version = $csproj.Project.PropertyGroup | Where-Object { $_.Version } | Select-Object -ExpandProperty Version

Write-Host ""
Write-Host "Done. Packages pushed successfully:"
Write-Host "  Cerebellum.BlazorBlocks.Models        $Version"
Write-Host "  Cerebellum.BlazorBlocks.Data          $Version"
Write-Host "  Cerebellum.BlazorBlocks.Data.Postgres $Version"
Write-Host "  Cerebellum.BlazorBlocks.Api           $Version"
Write-Host "  Cerebellum.BlazorBlocks.Web           $Version"
