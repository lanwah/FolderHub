<#
.SYNOPSIS
  发布 FolderHub.App + FolderHub.CardHost 到同一目录（安装包骨架输入）。

.DESCRIPTION
  默认 framework-dependent win-x64（体积小，需本机安装 .NET 8 Desktop Runtime）。
  使用 -SelfContained 可打出自包含包（便于无 Runtime 环境冒烟）。
  输出含 package-manifest.json；标记 DEV_ONLY_NP_POC_PENDING，禁止对外部署。

.EXAMPLE
  .\packaging\Publish-FolderHub.ps1
  .\packaging\Publish-FolderHub.ps1 -SelfContained -Configuration Release
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$Runtime = 'win-x64',

    [switch]$SelfContained,

    [string]$OutputDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'FolderHub.Packaging.psm1') -Force

$repoRoot = Get-FolderHubRepoRoot
$version = Get-FolderHubProductVersionFromProps -RepoRoot $repoRoot
if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $repoRoot "artifacts\publish\$Runtime"
}

$sln = Join-Path $repoRoot 'src\FolderHub.sln'
$appProj = Join-Path $repoRoot 'src\FolderHub.App\FolderHub.App.csproj'
$cardProj = Join-Path $repoRoot 'src\FolderHub.CardHost\FolderHub.CardHost.csproj'
$staging = Join-Path $repoRoot "artifacts\publish\.staging-cardhost-$Runtime"

Write-Host "FolderHub publish $version ($Configuration, $Runtime, SelfContained=$SelfContained)"
Write-Host "Output: $OutputDir"

if (Test-Path -LiteralPath $OutputDir) {
    Remove-Item -LiteralPath $OutputDir -Recurse -Force
}
if (Test-Path -LiteralPath $staging) {
    Remove-Item -LiteralPath $staging -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$commonArgs = @(
    '-c', $Configuration,
    '-r', $Runtime,
    '--self-contained', $(if ($SelfContained) { 'true' } else { 'false' }),
    '-p:PublishSingleFile=false',
    '-p:IncludeNativeLibrariesForSelfExtract=true'
)

Write-Host 'Building solution...'
dotnet build $sln -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet build 失败 ($LASTEXITCODE)" }

Write-Host 'Publishing App...'
dotnet publish $appProj @commonArgs -o $OutputDir --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish App 失败 ($LASTEXITCODE)" }

Write-Host 'Publishing CardHost (staging)...'
dotnet publish $cardProj @commonArgs -o $staging --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish CardHost 失败 ($LASTEXITCODE)" }

Write-Host 'Merging CardHost beside App (preserve subdirs)...'
Copy-FolderHubDirectoryContents -SourceDir $staging -DestinationDir $OutputDir

$gitCommit = ''
try {
    Push-Location $repoRoot
    $gitCommit = (git rev-parse --short HEAD 2>$null)
    if (-not $gitCommit) { $gitCommit = '' }
}
finally {
    Pop-Location
}

$manifest = New-FolderHubPackageManifest `
    -Version $version `
    -PublishDir $OutputDir `
    -Configuration $Configuration `
    -Runtime $Runtime `
    -SelfContained:([bool]$SelfContained) `
    -GitCommit $gitCommit

$manifestPath = Join-Path $OutputDir 'package-manifest.json'
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

Assert-FolderHubPublishLayout -PublishDir $OutputDir | Out-Null

Remove-Item -LiteralPath $staging -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Publish OK → $OutputDir"
Write-Host "Manifest → $manifestPath (releaseGate=$($manifest.releaseGate))"
