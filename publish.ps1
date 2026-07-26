# Publishes Framework-dependent and Self-contained builds into ./publish
# Usage: .\publish.ps1
# Optional: .\publish.ps1 -Configuration Release -Runtime win-x64

[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",

    [string] $Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $repoRoot "VtApp\VtApp.csproj"
$publishRoot = Join-Path $repoRoot "publish"
$fddOut = Join-Path $publishRoot "framework-dependent"
$scOut = Join-Path $publishRoot "self-contained"

if (-not (Test-Path $project)) {
    throw "Project not found: $project"
}

Write-Host "Cleaning $publishRoot ..."
if (Test-Path $publishRoot) {
    Remove-Item $publishRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $fddOut | Out-Null
New-Item -ItemType Directory -Path $scOut | Out-Null

Write-Host ""
Write-Host "=== Framework-dependent ($Configuration, $Runtime) ==="
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o $fddOut
if ($LASTEXITCODE -ne 0) {
    throw "Framework-dependent publish failed with exit code $LASTEXITCODE"
}

Write-Host ""
Write-Host "=== Self-contained ($Configuration, $Runtime) ==="
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -o $scOut
if ($LASTEXITCODE -ne 0) {
    throw "Self-contained publish failed with exit code $LASTEXITCODE"
}

Write-Host ""
Write-Host "Done."
Write-Host "  Framework-dependent: $fddOut"
Write-Host "  Self-contained:      $scOut"
