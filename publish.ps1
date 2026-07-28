# Publishes Framework-dependent and Self-contained builds as zip archives into ./publish/{major}_{minor}
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
$versionFile = Join-Path $repoRoot "VtApp\version.json"
$publishRoot = Join-Path $repoRoot "publish"

if (-not (Test-Path $project)) {
    throw "Project not found: $project"
}

if (-not (Test-Path $versionFile)) {
    throw "Version file not found: $versionFile"
}

$version = Get-Content $versionFile -Raw | ConvertFrom-Json
if ($null -eq $version.major -or $null -eq $version.minor) {
    throw "version.json must contain major and minor fields"
}

$versionFolderName = "{0}_{1}" -f $version.major, $version.minor
$versionOut = Join-Path $publishRoot $versionFolderName
$fddDir = Join-Path $versionOut "framework-dependent"
$scDir = Join-Path $versionOut "self-contained"
$fddZip = Join-Path $versionOut "framework-dependent.zip"
$scZip = Join-Path $versionOut "self-contained.zip"

function Compress-PublishFolder {
    param(
        [Parameter(Mandatory)]
        [string] $SourceDir,

        [Parameter(Mandatory)]
        [string] $ZipPath
    )

    if (Test-Path $ZipPath) {
        Remove-Item $ZipPath -Force
    }

    Compress-Archive -Path (Join-Path $SourceDir "*") -DestinationPath $ZipPath -CompressionLevel Optimal
    Remove-Item $SourceDir -Recurse -Force
}

Write-Host "Publishing version $versionFolderName ..."
Write-Host "Cleaning $publishRoot ..."
if (Test-Path $publishRoot) {
    Remove-Item $publishRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $fddDir -Force | Out-Null
New-Item -ItemType Directory -Path $scDir -Force | Out-Null
Copy-Item $versionFile (Join-Path $publishRoot "version.json")

Write-Host ""
Write-Host "=== Framework-dependent ($Configuration, $Runtime) ==="
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o $fddDir
if ($LASTEXITCODE -ne 0) {
    throw "Framework-dependent publish failed with exit code $LASTEXITCODE"
}
Compress-PublishFolder -SourceDir $fddDir -ZipPath $fddZip

Write-Host ""
Write-Host "=== Self-contained ($Configuration, $Runtime) ==="
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -o $scDir
if ($LASTEXITCODE -ne 0) {
    throw "Self-contained publish failed with exit code $LASTEXITCODE"
}
Compress-PublishFolder -SourceDir $scDir -ZipPath $scZip

Write-Host ""
Write-Host "Done."
Write-Host "  Version folder:       $versionOut"
Write-Host "  version.json:         $(Join-Path $publishRoot 'version.json')"
Write-Host "  Framework-dependent:  $fddZip"
Write-Host "  Self-contained:       $scZip"
