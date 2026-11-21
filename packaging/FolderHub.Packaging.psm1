# FolderHub 打包/安装骨架共用逻辑（WP10）。
# 安全边界：本骨架仅供本机开发与冒烟；RISK-002 未完成 NP PoC 前禁止对外部署/发布。

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-FolderHubRepoRoot {
    param([string]$StartPath = $PSScriptRoot)
    return (Resolve-Path (Join-Path $StartPath '..')).Path
}

function Get-FolderHubAppDataRoot {
    return (Join-Path $env:LOCALAPPDATA 'FolderHub')
}

function Get-FolderHubDefaultInstallDir {
    return (Join-Path (Get-FolderHubAppDataRoot) 'app')
}

function Get-FolderHubDatabasePath {
    return (Join-Path (Get-FolderHubAppDataRoot) 'folderhub.db')
}

function Get-FolderHubProductVersionFromProps {
    param([string]$RepoRoot = (Get-FolderHubRepoRoot))
    $propsPath = Join-Path $RepoRoot 'Directory.Build.props'
    if (-not (Test-Path -LiteralPath $propsPath)) {
        throw "未找到 Directory.Build.props：$propsPath"
    }
    [xml]$xml = Get-Content -LiteralPath $propsPath -Raw
    $version = $xml.Project.PropertyGroup.Version | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "Directory.Build.props 缺少 Version。"
    }
    return $version.Trim()
}

function Stop-FolderHubProcesses {
    param([int]$TimeoutSeconds = 15)
    $names = @('FolderHub.App', 'FolderHub.CardHost')
    foreach ($name in $names) {
        Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "停止进程 $($_.ProcessName) (PID $($_.Id))..."
            $_.CloseMainWindow() | Out-Null
        }
    }

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $alive = @()
        foreach ($name in $names) {
            $alive += @(Get-Process -Name $name -ErrorAction SilentlyContinue)
        }
        if ($alive.Count -eq 0) { return }
        Start-Sleep -Milliseconds 400
    }

    foreach ($name in $names) {
        Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Warning "强制结束 $($_.ProcessName) (PID $($_.Id))"
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

function New-FolderHubPackageManifest {
    param(
        [Parameter(Mandatory)][string]$Version,
        [Parameter(Mandatory)][string]$PublishDir,
        [string]$Configuration = 'Release',
        [string]$Runtime = 'win-x64',
        [bool]$SelfContained = $false,
        [string]$GitCommit = ''
    )

    $appExe = Join-Path $PublishDir 'FolderHub.App.exe'
    $hostExe = Join-Path $PublishDir 'FolderHub.CardHost.exe'
    if (-not (Test-Path -LiteralPath $appExe)) { throw "缺少 FolderHub.App.exe：$appExe" }
    if (-not (Test-Path -LiteralPath $hostExe)) { throw "缺少 FolderHub.CardHost.exe：$hostExe" }

    $appFi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($appExe)
    $hostFi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($hostExe)

    return [ordered]@{
        product           = 'FolderHub'
        packageVersion    = $Version
        configuration     = $Configuration
        runtime           = $Runtime
        selfContained     = $SelfContained
        targetFramework   = 'net8.0-windows'
        schemaVersionMax  = 2
        gitCommit         = $GitCommit
        createdUtc        = (Get-Date).ToUniversalTime().ToString('o')
        releaseGate       = 'DEV_ONLY_NP_POC_PENDING'
        files             = [ordered]@{
            appExe      = 'FolderHub.App.exe'
            cardHostExe = 'FolderHub.CardHost.exe'
            appFileVersion      = $appFi.FileVersion
            cardHostFileVersion = $hostFi.FileVersion
            appProductVersion   = $appFi.ProductVersion
            cardHostProductVersion = $hostFi.ProductVersion
        }
    }
}

function Assert-FolderHubPublishLayout {
    param([Parameter(Mandatory)][string]$PublishDir)

    $required = @(
        'FolderHub.App.exe',
        'FolderHub.CardHost.exe',
        'package-manifest.json'
    )
    $missing = @()
    foreach ($name in $required) {
        if (-not (Test-Path -LiteralPath (Join-Path $PublishDir $name))) {
            $missing += $name
        }
    }
    if ($missing.Count -gt 0) {
        throw ("发布布局不完整，缺少：{0}" -f ($missing -join ', '))
    }

    $manifest = Get-Content -LiteralPath (Join-Path $PublishDir 'package-manifest.json') -Raw | ConvertFrom-Json
    $appFi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $PublishDir 'FolderHub.App.exe'))
    $hostFi = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $PublishDir 'FolderHub.CardHost.exe'))

    if ($appFi.ProductVersion -ne $hostFi.ProductVersion) {
        throw "App/CardHost ProductVersion 不一致：$($appFi.ProductVersion) vs $($hostFi.ProductVersion)"
    }
    if ($manifest.packageVersion -and ($appFi.ProductVersion -notlike "$($manifest.packageVersion)*")) {
        Write-Warning "manifest.packageVersion=$($manifest.packageVersion) 与 App ProductVersion=$($appFi.ProductVersion) 不完全匹配（InformationalVersion 可能含后缀）。"
    }

    return $manifest
}

function Backup-FolderHubDatabase {
    param(
        [string]$DatabasePath = (Get-FolderHubDatabasePath),
        [string]$BackupDirectory = $null
    )

    if (-not (Test-Path -LiteralPath $DatabasePath)) {
        return $null
    }

    if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
        $BackupDirectory = Join-Path (Get-FolderHubAppDataRoot) 'backups'
    }
    New-Item -ItemType Directory -Force -Path $BackupDirectory | Out-Null
    $stamp = (Get-Date).ToString('yyyyMMdd-HHmmss')
    $destDir = Join-Path $BackupDirectory "folderhub.db.$stamp"
    New-Item -ItemType Directory -Force -Path $destDir | Out-Null

    # WAL 模式下须一并备份 -wal/-shm，否则仅拷主库可能不一致。
    foreach ($path in @(
            $DatabasePath,
            ($DatabasePath + '-wal'),
            ($DatabasePath + '-shm')
        )) {
        if (Test-Path -LiteralPath $path) {
            $leaf = Split-Path -Leaf $path
            Copy-Item -LiteralPath $path -Destination (Join-Path $destDir $leaf) -Force
        }
    }

    return $destDir
}

function Clear-FolderHubInstallDirectory {
    param([Parameter(Mandatory)][string]$InstallDir)

    $full = [System.IO.Path]::GetFullPath($InstallDir).TrimEnd('\')
    $dataRoot = [System.IO.Path]::GetFullPath((Get-FolderHubAppDataRoot)).TrimEnd('\')

    # 禁止把数据根本身或其父路径当作「安装目录」清空。
    if ([string]::Equals($full, $dataRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "拒绝清空数据根目录：$full"
    }
    if ($dataRoot.StartsWith($full + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw "拒绝清空数据根的父目录：$full（会波及 $dataRoot）"
    }

    if (Test-Path -LiteralPath $full) {
        Remove-Item -LiteralPath $full -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $full | Out-Null
}

function Copy-FolderHubDirectoryContents {
    param(
        [Parameter(Mandatory)][string]$SourceDir,
        [Parameter(Mandatory)][string]$DestinationDir
    )

    $SourceDir = [System.IO.Path]::GetFullPath($SourceDir)
    $DestinationDir = [System.IO.Path]::GetFullPath($DestinationDir)
    New-Item -ItemType Directory -Force -Path $DestinationDir | Out-Null

    Get-ChildItem -LiteralPath $SourceDir -Force | ForEach-Object {
        $dest = Join-Path $DestinationDir $_.Name
        if ($_.PSIsContainer) {
            Copy-Item -LiteralPath $_.FullName -Destination $dest -Recurse -Force
        }
        else {
            Copy-Item -LiteralPath $_.FullName -Destination $dest -Force
        }
    }
}

function Remove-FolderHubStartupEntryIfOwned {
    param([Parameter(Mandatory)][string]$InstallDir)

    $runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
    $valueName = 'FolderHub'
    $prop = Get-ItemProperty -Path $runKey -Name $valueName -ErrorAction SilentlyContinue
    if (-not $prop) { return $false }

    $raw = [string]$prop.$valueName
    if ([string]::IsNullOrWhiteSpace($raw)) { return $false }

    $exe = Join-Path ([System.IO.Path]::GetFullPath($InstallDir)) 'FolderHub.App.exe'
    # Run 值可能带引号与参数；匹配指向本次安装目录下的 App.exe。
    $owned = $raw.IndexOf($exe, [StringComparison]::OrdinalIgnoreCase) -ge 0
    if (-not $owned) {
        Write-Warning "HKCU Run\$valueName 存在但不指向本次 InstallDir，已保留：$raw"
        return $false
    }

    Remove-ItemProperty -Path $runKey -Name $valueName -Force
    return $true
}

Export-ModuleMember -Function @(
    'Get-FolderHubRepoRoot',
    'Get-FolderHubAppDataRoot',
    'Get-FolderHubDefaultInstallDir',
    'Get-FolderHubDatabasePath',
    'Get-FolderHubProductVersionFromProps',
    'Stop-FolderHubProcesses',
    'New-FolderHubPackageManifest',
    'Assert-FolderHubPublishLayout',
    'Backup-FolderHubDatabase',
    'Clear-FolderHubInstallDirectory',
    'Copy-FolderHubDirectoryContents',
    'Remove-FolderHubStartupEntryIfOwned'
)
