<#
.SYNOPSIS
  卸载 FolderHub 已安装二进制（默认保留用户数据与数据库）。

.EXAMPLE
  .\packaging\Uninstall-FolderHub.ps1
  .\packaging\Uninstall-FolderHub.ps1 -RemoveUserData
#>
[CmdletBinding()]
param(
    [string]$InstallDir = '',

    [switch]$RemoveUserData,

    # 默认会移除「指向本次 InstallDir」的 HKCU Run；此开关强制删除 FolderHub 值（不论路径）。
    [switch]$ForceRemoveStartupEntry
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'FolderHub.Packaging.psm1') -Force

if ([string]::IsNullOrWhiteSpace($InstallDir)) {
    $InstallDir = Get-FolderHubDefaultInstallDir
}
$dataRoot = Get-FolderHubAppDataRoot

Write-Host "Uninstall FolderHub"
Write-Host "  App  : $InstallDir"
Write-Host "  Data : $dataRoot (RemoveUserData=$RemoveUserData)"

Stop-FolderHubProcesses

if ($ForceRemoveStartupEntry) {
    $runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
    $valueName = 'FolderHub'
    if (Get-ItemProperty -Path $runKey -Name $valueName -ErrorAction SilentlyContinue) {
        Remove-ItemProperty -Path $runKey -Name $valueName -Force
        Write-Host "Removed HKCU Run\$valueName (forced)"
    }
}
elseif (Remove-FolderHubStartupEntryIfOwned -InstallDir $InstallDir) {
    Write-Host "Removed HKCU Run\FolderHub (owned by InstallDir)"
}

if (Test-Path -LiteralPath $InstallDir) {
    Remove-Item -LiteralPath $InstallDir -Recurse -Force
    Write-Host "Removed $InstallDir"
}

$manifests = @(
    (Join-Path $InstallDir 'install-manifest.json'),
    (Join-Path $dataRoot 'install-manifest.json')
)
foreach ($installManifest in $manifests) {
    if ((Test-Path -LiteralPath $installManifest) -and -not $RemoveUserData) {
        # InstallDir 已整目录删除时跳过；数据根清单在保留数据时仍删除。
        if (Test-Path -LiteralPath $installManifest) {
            Remove-Item -LiteralPath $installManifest -Force -ErrorAction SilentlyContinue
            if (-not (Test-Path -LiteralPath $installManifest)) {
                Write-Host "Removed $installManifest"
            }
        }
    }
}

if ($RemoveUserData) {
    if (Test-Path -LiteralPath $dataRoot) {
        Remove-Item -LiteralPath $dataRoot -Recurse -Force
        Write-Host "Removed user data $dataRoot"
    }
}
else {
    Write-Host "保留用户数据目录 $dataRoot（含 folderhub.db / backups）。"
}

Write-Host 'Uninstall OK.'
