<#
.SYNOPSIS
  FolderHub WP10 兼容冒烟：校验发布布局、采集本机 OS，并对照 NFR-001 矩阵标记结果。

.DESCRIPTION
  不声称 Server 2012+ 全矩阵 PASS。本机抽检结果写入 artifacts/smoke/。
  RISK-001 验证义务仍有效；未测版本保持 NOT_RUN。

.EXAMPLE
  .\packaging\Invoke-CompatSmoke.ps1 -PublishDir .\artifacts\publish\win-x64
#>
[CmdletBinding()]
param(
    [string]$PublishDir = '',

    [string]$OutputDir = '',

    [switch]$SkipPublishAssert,

    [switch]$LaunchAppSmoke
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'FolderHub.Packaging.psm1') -Force

$repoRoot = Get-FolderHubRepoRoot
if ([string]::IsNullOrWhiteSpace($PublishDir)) {
    $PublishDir = Join-Path $repoRoot 'artifacts\publish\win-x64'
}
if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $stamp = (Get-Date).ToString('yyyyMMdd-HHmmss')
    $OutputDir = Join-Path $repoRoot "artifacts\smoke\$stamp"
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$os = Get-CimInstance Win32_OperatingSystem
$cs = Get-CimInstance Win32_ComputerSystem
$arch = [Environment]::Is64BitOperatingSystem
$caption = [string]$os.Caption
$productName = try {
    (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name ProductName).ProductName
} catch { $null }
$displayVersion = try {
    (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name DisplayVersion -ErrorAction SilentlyContinue).DisplayVersion
} catch { $null }
$editionId = try {
    (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name EditionID -ErrorAction SilentlyContinue).EditionID
} catch { $null }

$sessionName = $env:SESSIONNAME
$sessionType = if ($sessionName -like 'RDP*') { 'RDP' } else { 'LocalInteractive' }

$environment = [ordered]@{
    capturedUtc     = (Get-Date).ToUniversalTime().ToString('o')
    caption         = $caption
    productName     = $productName
    version         = $os.Version
    buildNumber     = $os.BuildNumber
    displayVersion  = $displayVersion
    editionId       = $editionId
    architecture    = $(if ($arch) { 'x64' } else { 'x86' })
    sessionName     = $sessionName
    sessionType     = $sessionType
    manufacturer    = $cs.Manufacturer
    model           = $cs.Model
    totalPhysicalMemoryBytes = [int64]$cs.TotalPhysicalMemory
}

# NFR-001 矩阵：本脚本仅能对本机行给出抽检结论，其余保持 NOT_RUN。
function Resolve-MatrixRowStatus {
    param([string]$Id, [hashtable]$Env)

    $cap = "$($Env.caption) $($Env.productName) $($Env.displayVersion)"
    switch ($Id) {
        'WIN11-X64' {
            if ($Env.architecture -ne 'x64') { return 'NOT_MATCH' }
            if ($cap -match 'Windows 11' -or ($Env.buildNumber -as [int]) -ge 22000) {
                return 'SMOKE_CANDIDATE'
            }
            return 'NOT_MATCH'
        }
        'WIN10-22H2' {
            if ($Env.architecture -ne 'x64') { return 'NOT_MATCH' }
            if ($cap -match 'Windows 10' -and $Env.displayVersion -eq '22H2') { return 'SMOKE_CANDIDATE' }
            if ($cap -match 'Windows 10' -and ($Env.buildNumber -as [int]) -lt 22000) { return 'SMOKE_CANDIDATE_UNVERIFIED_SKU' }
            return 'NOT_MATCH'
        }
        default { return 'NOT_RUN' }
    }
}

$matrix = @(
    @{ id = 'WIN10-22H2'; target = 'Windows 10 22H2 x64 本地交互式桌面'; status = 'NOT_RUN'; notes = 'NFR-001；需实机/VM 抽检' },
    @{ id = 'WIN11-X64'; target = 'Windows 11 x64 本地交互式桌面'; status = 'NOT_RUN'; notes = 'NFR-001' },
    @{ id = 'WS2012'; target = 'Windows Server 2012 Server with a GUI x64'; status = 'NOT_RUN'; notes = 'RISK-001；.NET 8 官方支持需单独核验' },
    @{ id = 'WS2012R2'; target = 'Windows Server 2012 R2 Server with a GUI x64'; status = 'NOT_RUN'; notes = 'RISK-001' },
    @{ id = 'WS2016'; target = 'Windows Server 2016 Desktop Experience x64'; status = 'NOT_RUN'; notes = 'RISK-001' },
    @{ id = 'WS2019'; target = 'Windows Server 2019 Desktop Experience x64'; status = 'NOT_RUN'; notes = 'RISK-001' },
    @{ id = 'WS2022'; target = 'Windows Server 2022 Desktop Experience x64'; status = 'NOT_RUN'; notes = 'RISK-001' }
)

$checks = @()
$overall = 'PASS'

function Add-Check {
    param([string]$Name, [string]$Status, [string]$Detail)
    $script:checks += [ordered]@{ name = $Name; status = $Status; detail = $Detail }
    if ($Status -eq 'FAIL') { $script:overall = 'FAIL' }
    elseif ($Status -eq 'WARN' -and $script:overall -eq 'PASS') { $script:overall = 'PASS_WITH_NOTES' }
}

if (-not $SkipPublishAssert) {
    if (-not (Test-Path -LiteralPath $PublishDir)) {
        Add-Check -Name 'PublishLayout' -Status 'FAIL' -Detail "PublishDir 不存在：$PublishDir。请先运行 Publish-FolderHub.ps1。"
    }
    else {
        try {
            $manifest = Assert-FolderHubPublishLayout -PublishDir $PublishDir
            Add-Check -Name 'PublishLayout' -Status 'PASS' -Detail "App+CardHost+manifest OK；version=$($manifest.packageVersion)"
            if ($manifest.releaseGate -ne 'DEV_ONLY_NP_POC_PENDING') {
                Add-Check -Name 'ReleaseGate' -Status 'WARN' -Detail "意外 releaseGate=$($manifest.releaseGate)"
            }
            else {
                Add-Check -Name 'ReleaseGate' -Status 'PASS' -Detail 'DEV_ONLY_NP_POC_PENDING（禁止对外部署）'
            }
        }
        catch {
            Add-Check -Name 'PublishLayout' -Status 'FAIL' -Detail $_.Exception.Message
        }
    }
}

$runtimeOk = $false
try {
    $dotnetList = & dotnet --list-runtimes 2>$null
    if ($dotnetList -match 'Microsoft\.WindowsDesktop\.App 8\.') {
        $runtimeOk = $true
        Add-Check -Name 'DotNetDesktopRuntime8' -Status 'PASS' -Detail (($dotnetList | Where-Object { $_ -match 'WindowsDesktop\.App 8\.' }) -join '; ')
    }
    else {
        Add-Check -Name 'DotNetDesktopRuntime8' -Status 'WARN' -Detail '未检测到 WindowsDesktop.App 8.x；framework-dependent 包可能无法启动，可改用 -SelfContained 发布。'
    }
}
catch {
    Add-Check -Name 'DotNetDesktopRuntime8' -Status 'WARN' -Detail $_.Exception.Message
}

for ($i = 0; $i -lt $matrix.Count; $i++) {
    $row = $matrix[$i]
    $match = Resolve-MatrixRowStatus -Id $row.id -Env $environment
    if ($match -eq 'SMOKE_CANDIDATE' -or $match -eq 'SMOKE_CANDIDATE_UNVERIFIED_SKU') {
        $row.status = 'SMOKE_HOST'
        $row.notes = "本机匹配为抽检宿主（$match）；完整功能冒烟见 checks。"
    }
    $matrix[$i] = $row
}

$hostRows = @($matrix | Where-Object { $_.status -eq 'SMOKE_HOST' })
if ($hostRows.Count -eq 0) {
    Add-Check -Name 'OsMatrixHost' -Status 'WARN' -Detail '本机未匹配 WIN10/WIN11 抽检行；环境已记录，矩阵其余行仍为 NOT_RUN。'
}
else {
    Add-Check -Name 'OsMatrixHost' -Status 'PASS' -Detail ("本机作为 {0} 抽检宿主" -f (($hostRows | ForEach-Object { $_.id }) -join ', '))
}

if ($LaunchAppSmoke -and (Test-Path -LiteralPath (Join-Path $PublishDir 'FolderHub.App.exe'))) {
    $exe = Join-Path $PublishDir 'FolderHub.App.exe'
    try {
        $p = Start-Process -FilePath $exe -ArgumentList '--show' -PassThru
        Start-Sleep -Seconds 3
        if ($p.HasExited) {
            Add-Check -Name 'LaunchApp' -Status 'FAIL' -Detail "进程已退出 ExitCode=$($p.ExitCode)"
        }
        else {
            Add-Check -Name 'LaunchApp' -Status 'PASS' -Detail "PID $($p.Id) 存活 ≥3s（未做 UI 自动化验收）"
            Stop-FolderHubProcesses
        }
    }
    catch {
        Add-Check -Name 'LaunchApp' -Status 'FAIL' -Detail $_.Exception.Message
    }
}

# 本机宿主行：仅当布局+运行时基础检查未 FAIL 时标 SMOKE_PASS_LOCAL
if ($overall -ne 'FAIL' -and $hostRows.Count -gt 0) {
    for ($i = 0; $i -lt $matrix.Count; $i++) {
        if ($matrix[$i].status -eq 'SMOKE_HOST') {
            $matrix[$i].status = 'SMOKE_PASS_LOCAL'
            $matrix[$i].notes = '本机发布布局/版本一致性抽检通过；非全量功能验收；Server 行仍 NOT_RUN。'
        }
    }
}

$result = [ordered]@{
    document        = 'WP10 Compat Smoke'
    overall         = $overall
    nfr001          = 'NEEDS_VALIDATION'
    risk001         = 'ACCEPTED (verification obligation remains)'
    risk002Gate     = 'DEV_ONLY_NP_POC_PENDING'
    environment     = $environment
    publishDir      = $PublishDir
    checks          = $checks
    osMatrix        = $matrix
    limitations     = @(
        '未覆盖 Windows Server 2012+ 逐版本实机。',
        '未执行 Named Pipes NP-01～11 完整 PoC。',
        'LaunchApp 仅可选进程存活抽检，不含 UI/IPC 全路径。',
        '不得将本结果记为 NFR-001 PASS 或对外发布批准。'
    )
}

$jsonPath = Join-Path $OutputDir 'compat-smoke-result.json'
$mdPath = Join-Path $OutputDir 'compat-smoke-result.md'
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$md = @()
$md += '# FolderHub WP10 兼容冒烟结果'
$md += ''
$md += "- overall: **$overall**"
$md += "- NFR-001: NEEDS_VALIDATION（不得标 PASS）"
$md += "- 采集时间(UTC): $($environment.capturedUtc)"
$md += "- OS: $($environment.caption) / $($environment.productName) Build $($environment.buildNumber) $($environment.displayVersion)"
$md += "- Session: $($environment.sessionType) ($($environment.sessionName))"
$md += ''
$md += '## Checks'
$md += ''
$md += '| Name | Status | Detail |'
$md += '|---|---|---|'
foreach ($c in $checks) {
    $md += "| $($c.name) | $($c.status) | $($c.detail) |"
}
$md += ''
$md += '## OS Matrix (NFR-001 / RISK-001)'
$md += ''
$md += '| Id | Target | Status | Notes |'
$md += '|---|---|---|---|'
foreach ($row in $matrix) {
    $md += "| $($row.id) | $($row.target) | $($row.status) | $($row.notes) |"
}
$md += ''
$md += '## Limitations'
foreach ($line in $result.limitations) {
    $md += "- $line"
}
$md -join "`n" | Set-Content -LiteralPath $mdPath -Encoding UTF8

Write-Host "Compat smoke overall=$overall"
Write-Host "JSON → $jsonPath"
Write-Host "MD   → $mdPath"

if ($overall -eq 'FAIL') { exit 1 }
exit 0
