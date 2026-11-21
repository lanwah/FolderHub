<#
.SYNOPSIS
  将已发布的 FolderHub 二进制安装到本机（升级时替换 app，保留 SQLite 数据）。

.DESCRIPTION
  默认安装目录：%LocalAppData%\FolderHub\app\
  数据目录：%LocalAppData%\FolderHub\（folderhub.db 等）— 升级不删除。
  升级前可选 -BackupDatabase 备份数据库。
  安装前停止 App/CardHost 进程。

.EXAMPLE
  .\packaging\Install-FolderHub.ps1 -SourceDir .\artifacts\publish\win-x64
  .\packaging\Install-FolderHub.ps1 -SourceDir .\artifacts\publish\win-x64 -BackupDatabase
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$SourceDir,

    [string]$InstallDir = '',

    [switch]$BackupDatabase,

    [switch]$StartAfterInstall
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'FolderHub.Packaging.psm1') -Force

$SourceDir = (Resolve-Path -LiteralPath $SourceDir).Path
if ([string]::IsNullOrWhiteSpace($InstallDir)) {
    $InstallDir = Get-FolderHubDefaultInstallDir
}

$manifest = Assert-FolderHubPublishLayout -PublishDir $SourceDir
$dataRoot = Get-FolderHubAppDataRoot

Write-Host "Install FolderHub $($manifest.packageVersion)"
Write-Host "  Source : $SourceDir"
Write-Host "  App    : $InstallDir"
Write-Host "  Data   : $dataRoot (保留)"

Stop-FolderHubProcesses

New-Item -ItemType Directory -Force -Path $dataRoot | Out-Null

if ($BackupDatabase) {
    $bak = Backup-FolderHubDatabase
    if ($bak) { Write-Host "Database backup → $bak" }
    else { Write-Host 'No existing database to backup.' }
}

# 升级：清空安装目录再复制，避免旧版残留文件；绝不触碰数据根。
Write-Host 'Refreshing install directory...'
Clear-FolderHubInstallDirectory -InstallDir $InstallDir

Write-Host 'Copying binaries...'
Copy-FolderHubDirectoryContents -SourceDir $SourceDir -DestinationDir $InstallDir

$installManifest = [ordered]@{
    product         = 'FolderHub'
    packageVersion  = $manifest.packageVersion
    installDir      = $InstallDir
    dataDir         = $dataRoot
    databasePath    = (Get-FolderHubDatabasePath)
    installedUtc    = (Get-Date).ToUniversalTime().ToString('o')
    sourceManifest  = $manifest
    releaseGate     = 'DEV_ONLY_NP_POC_PENDING'
    notes           = '骨架安装：未写开始菜单/卸载程序；开机启动仍由应用内设置写入 HKCU Run。'
}

$json = $installManifest | ConvertTo-Json -Depth 8
$installManifestInApp = Join-Path $InstallDir 'install-manifest.json'
$json | Set-Content -LiteralPath $installManifestInApp -Encoding UTF8

# 仅当安装到默认 app 目录时，同步数据根清单，避免自定义 InstallDir 污染本机数据目录。
$defaultInstall = Get-FolderHubDefaultInstallDir
if ([string]::Equals(
        [System.IO.Path]::GetFullPath($InstallDir).TrimEnd('\'),
        [System.IO.Path]::GetFullPath($defaultInstall).TrimEnd('\'),
        [System.StringComparison]::OrdinalIgnoreCase)) {
    $installManifestPath = Join-Path $dataRoot 'install-manifest.json'
    $json | Set-Content -LiteralPath $installManifestPath -Encoding UTF8
    Write-Host "Install OK. Manifest → $installManifestPath"
}
else {
    Write-Host "Install OK (custom InstallDir). Manifest → $installManifestInApp"
    Write-Host "未写入数据根 install-manifest.json（避免非默认安装污染）。"
}

if ($StartAfterInstall) {
    $exe = Join-Path $InstallDir 'FolderHub.App.exe'
    Start-Process -FilePath $exe -ArgumentList '--show'
    Write-Host "Started $exe --show"
}
else {
    Write-Host "启动：& '$(Join-Path $InstallDir 'FolderHub.App.exe')' --show"
}
