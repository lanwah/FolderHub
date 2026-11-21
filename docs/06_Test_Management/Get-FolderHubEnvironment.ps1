# FolderHub P3 Windows Environment Inventory
# Compatible target: Windows PowerShell 5.1+
# Read-only inventory; does not change system settings or install software.

[CmdletBinding()]
param(
    [string]$OutputPath = ".\FolderHub-Environment-Inventory.json"
)

$ErrorActionPreference = "Stop"

function Get-RegistryValue {
    param([string]$Path, [string]$Name)
    try {
        (Get-ItemProperty -LiteralPath $Path -Name $Name -ErrorAction Stop).$Name
    } catch { $null }
}

$os = Get-CimInstance -ClassName Win32_OperatingSystem
$cs = Get-CimInstance -ClassName Win32_ComputerSystem
$proc = Get-CimInstance -ClassName Win32_Processor | Select-Object -First 1
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)

$displayVersion = Get-RegistryValue "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion" "DisplayVersion"
$releaseId = Get-RegistryValue "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion" "ReleaseId"
$ubr = Get-RegistryValue "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion" "UBR"
$editionId = Get-RegistryValue "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion" "EditionID"
$productName = Get-RegistryValue "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion" "ProductName"

$sessionId = [System.Diagnostics.Process]::GetCurrentProcess().SessionId
$sessionName = $env:SESSIONNAME
$sessionType = if ($sessionName -like "RDP-*") { "RDP" } elseif ($sessionId -eq 0) { "Session0/Service" } else { "ConsoleOrOtherInteractive" }

$displays = @()
try {
    $displays = @(Get-CimInstance -ClassName Win32_DesktopMonitor | ForEach-Object {
        [ordered]@{
            Name = $_.Name
            DeviceID = $_.DeviceID
            ScreenWidth = $_.ScreenWidth
            ScreenHeight = $_.ScreenHeight
            PNPDeviceID = $_.PNPDeviceID
        }
    })
} catch { }

$dpi = $null
try {
    Add-Type -AssemblyName System.Windows.Forms
    $dpi = [System.Windows.Forms.Screen]::AllScreens | ForEach-Object {
        [ordered]@{
            DeviceName = $_.DeviceName
            Bounds = $_.Bounds.ToString()
            WorkingArea = $_.WorkingArea.ToString()
            Primary = $_.Primary
        }
    }
} catch {
    $dpi = "Unavailable in this session/context"
}

$logonSid = $null
try {
    $logonSid = $identity.Groups |
        Where-Object { $_.Value -match '^S-1-5-5-' } |
        ForEach-Object { $_.Value }
} catch { }

$result = [ordered]@{
    CollectedAtUtc = [DateTime]::UtcNow.ToString("o")
    ComputerName = $env:COMPUTERNAME
    UserName = $identity.Name
    UserSid = $identity.User.Value
    LogonSid = @($logonSid)
    IsAdministrator = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    SessionId = $sessionId
    SessionName = $sessionName
    SessionType = $sessionType
    OS = [ordered]@{
        Caption = $os.Caption
        ProductName = $productName
        Version = $os.Version
        DisplayVersion = $displayVersion
        ReleaseId = $releaseId
        BuildNumber = $os.BuildNumber
        UBR = $ubr
        EditionID = $editionId
        ServicePackMajorVersion = $os.ServicePackMajorVersion
        OSArchitecture = $os.OSArchitecture
        InstallDate = $os.InstallDate
        LastBootUpTime = $os.LastBootUpTime
    }
    Hardware = [ordered]@{
        Manufacturer = $cs.Manufacturer
        Model = $cs.Model
        SystemType = $cs.SystemType
        ProcessorName = $proc.Name
        NumberOfLogicalProcessors = $cs.NumberOfLogicalProcessors
        TotalPhysicalMemoryBytes = $cs.TotalPhysicalMemory
    }
    Displays = $displays
    ScreenBounds = $dpi
    PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    DotNetReleaseKey = Get-RegistryValue "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
}

$json = $result | ConvertTo-Json -Depth 8
$fullPath = [System.IO.Path]::GetFullPath($OutputPath)
$json | Set-Content -LiteralPath $fullPath -Encoding UTF8

Write-Host "FolderHub environment inventory written to: $fullPath"
Write-Host "OS: $($os.Caption) | Build: $($os.BuildNumber) | Session: $sessionType | SessionId: $sessionId"
Write-Warning "Review the output before sharing: it contains machine and user identity metadata."
