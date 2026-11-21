<#
.SYNOPSIS
  编译 FolderHub 解决方案（dotnet build）。

.DESCRIPTION
  构建 src\FolderHub.sln。默认 Debug；可用 -Configuration Release。
  加 -Test 时在编译成功后执行 dotnet test。

.EXAMPLE
  .\packaging\Build-FolderHub.ps1
  .\packaging\Build-FolderHub.ps1 -Configuration Release
  .\packaging\Build-FolderHub.ps1 -Test
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [switch]$Test
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'FolderHub.Packaging.psm1') -Force

$repoRoot = Get-FolderHubRepoRoot
$sln = Join-Path $repoRoot 'src\FolderHub.sln'

if (-not (Test-Path -LiteralPath $sln)) {
    throw "未找到解决方案：$sln"
}

Write-Host "FolderHub build ($Configuration)"
Write-Host "Solution: $sln"

dotnet build $sln -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build 失败 ($LASTEXITCODE)"
}

if ($Test) {
    Write-Host 'Running tests...'
    dotnet test $sln -c $Configuration --nologo --no-build
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test 失败 ($LASTEXITCODE)"
    }
}

Write-Host "Build OK ($Configuration)"
