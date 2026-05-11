<#
.SYNOPSIS
    Builds a single-file, self-contained DiagCore.exe that can be carried
    to any Windows x64 machine without installing the .NET runtime.

.DESCRIPTION
    Runs `dotnet publish` with:
      - PublishSingleFile=true         => one .exe, no DLLs next to it
      - SelfContained=true             => embeds the .NET 10 runtime
      - IncludeNativeLibrariesForSelfExtract=true
                                         => native libraries are extracted
                                            to a temp folder at first run
      - EnableCompressionInSingleFile=true
                                         => roughly halves the final size
      - DebugType=embedded             => no separate .pdb shipped
      - RuntimeIdentifier=win-x64      => target architecture

    The output goes to a `publish/` folder at the repo root, which is
    already in .gitignore.

.PARAMETER Configuration
    Build configuration. Default is Release.

.PARAMETER RuntimeIdentifier
    Target RID. Default is win-x64. Use win-arm64 for ARM Windows.

.PARAMETER OutputDirectory
    Where to drop the .exe. Default is "publish" at the repo root.

.EXAMPLE
    pwsh scripts/publish.ps1
    # Produces publish/DiagCore.exe (~64 MB) ready to carry.

.EXAMPLE
    pwsh scripts/publish.ps1 -RuntimeIdentifier win-arm64
    # ARM build, lands in publish/.
#>

[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$OutputDirectory = 'publish'
)

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot   = Resolve-Path (Join-Path $scriptRoot '..')
$project    = Join-Path $repoRoot 'src/DiagCore.App/DiagCore.App.csproj'
$outFull    = Join-Path $repoRoot $OutputDirectory

Write-Host ""
Write-Host "Publishing DiagCore.exe" -ForegroundColor Cyan
Write-Host "  configuration : $Configuration"
Write-Host "  runtime       : $RuntimeIdentifier"
Write-Host "  output        : $outFull"
Write-Host ""

# Clean previous publish output so old binaries do not leak through.
if (Test-Path $outFull) {
    Remove-Item $outFull -Recurse -Force
}

dotnet publish $project `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=embedded `
    -o $outFull

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$exe = Join-Path $outFull 'DiagCore.exe'
if (-not (Test-Path $exe)) {
    throw "Expected $exe was not produced."
}

$sizeMb = [math]::Round((Get-Item $exe).Length / 1MB, 1)

Write-Host ""
Write-Host "✅ Done." -ForegroundColor Green
Write-Host "   $exe ($sizeMb MB)"
Write-Host ""
Write-Host "   Smartscreen will warn the first time on a new machine because"
Write-Host "   the binary is not code-signed yet. That is expected - click"
Write-Host "   'More info' -> 'Run anyway'. Code signing lands in Phase 5."
Write-Host ""
