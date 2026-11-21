#Requires -Version 5.1
<#
.SYNOPSIS
  Rasterize FolderHub App / CardHost icons into multi-size .ico files.
#>
[CmdletBinding()]
param(
    [string]$OutDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($OutDir)) {
    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        $OutDir = $PSScriptRoot
    }
    else {
        $OutDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    }
}
$OutDir = [System.IO.Path]::GetFullPath($OutDir)

Add-Type -AssemblyName System.Drawing

function New-RoundedRectPath {
    param(
        [float]$X, [float]$Y, [float]$W, [float]$H, [float]$Radius
    )
    $r = [Math]::Min($Radius, [Math]::Min($W, $H) / 2)
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $path.AddArc($X, $Y, $d, $d, 180, 90)
    $path.AddArc($X + $W - $d, $Y, $d, $d, 270, 90)
    $path.AddArc($X + $W - $d, $Y + $H - $d, $d, $d, 0, 90)
    $path.AddArc($X, $Y + $H - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    return $path
}

function Get-S([int]$size, [float]$n) {
    return [float]($size * $n)
}

function Draw-AppIcon {
    param([System.Drawing.Graphics]$g, [int]$size)

    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = Get-S $size 0.0625
    $tile = $size - (2 * $pad)
    $radius = Get-S $size 0.20

    $tilePath = New-RoundedRectPath -X $pad -Y $pad -W $tile -H $tile -Radius $radius
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        ([System.Drawing.RectangleF]::new($pad, $pad, $tile, $tile)),
        [System.Drawing.Color]::FromArgb(255, 61, 155, 138),
        [System.Drawing.Color]::FromArgb(255, 28, 102, 92),
        45.0)
    $g.FillPath($brush, $tilePath)
    $brush.Dispose()

    # folder body
    $fx = Get-S $size 0.265
    $fy = Get-S $size 0.39
    $fw = Get-S $size 0.47
    $fh = Get-S $size 0.35
    $fr = Get-S $size 0.055
    $folderPath = New-RoundedRectPath -X $fx -Y $fy -W $fw -H $fh -Radius $fr
    $folderBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 247, 252, 250))
    $g.FillPath($folderBrush, $folderPath)
    $folderBrush.Dispose()
    $folderPath.Dispose()

    # folder tab
    $tx = $fx
    $ty = Get-S $size 0.34
    $tw = Get-S $size 0.20
    $th = Get-S $size 0.075
    $tab = New-RoundedRectPath -X $tx -Y $ty -W $tw -H $th -Radius (Get-S $size 0.03)
    $tabBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(240, 238, 248, 244))
    $g.FillPath($tabBrush, $tab)
    $tabBrush.Dispose()
    $tab.Dispose()

    # hub geometry
    $cx = Get-S $size 0.50
    $cy = Get-S $size 0.575
    $hub = [System.Drawing.Color]::FromArgb(255, 40, 124, 112)
    $hubBrush = New-Object System.Drawing.SolidBrush($hub)
    $coreBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 244, 251, 248))
    $penW = [Math]::Max(1.5, $size * 0.018)
    $pen = New-Object System.Drawing.Pen($hub, $penW)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round

    $nodeR = Get-S $size 0.028
    $centerR = Get-S $size 0.048
    $nodes = @(
        @{ X = ($cx - (Get-S $size 0.115)); Y = ($cy - (Get-S $size 0.06)) },
        @{ X = ($cx + (Get-S $size 0.115)); Y = ($cy - (Get-S $size 0.06)) },
        @{ X = $cx; Y = ($cy + (Get-S $size 0.105)) }
    )

    foreach ($n in $nodes) {
        $g.DrawLine($pen, $cx, $cy, [float]$n.X, [float]$n.Y)
    }

    $g.FillEllipse($hubBrush, $cx - $centerR, $cy - $centerR, $centerR * 2, $centerR * 2)
    $inner = $centerR * 0.42
    $g.FillEllipse($coreBrush, $cx - $inner, $cy - $inner, $inner * 2, $inner * 2)

    foreach ($n in $nodes) {
        $g.FillEllipse($hubBrush, [float]($n.X - $nodeR), [float]($n.Y - $nodeR), $nodeR * 2, $nodeR * 2)
    }

    $pen.Dispose()
    $hubBrush.Dispose()
    $coreBrush.Dispose()
    $tilePath.Dispose()
}

function Draw-CardHostIcon {
    param([System.Drawing.Graphics]$g, [int]$size)

    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = Get-S $size 0.0625
    $tile = $size - (2 * $pad)
    $radius = Get-S $size 0.20

    $tilePath = New-RoundedRectPath -X $pad -Y $pad -W $tile -H $tile -Radius $radius
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        ([System.Drawing.RectangleF]::new($pad, $pad, $tile, $tile)),
        [System.Drawing.Color]::FromArgb(255, 47, 138, 124),
        [System.Drawing.Color]::FromArgb(255, 23, 79, 72),
        45.0)
    $g.FillPath($brush, $tilePath)
    $brush.Dispose()
    $tilePath.Dispose()

    # soft shadow card
    $cx = Get-S $size 0.23
    $cy = Get-S $size 0.28
    $cw = Get-S $size 0.54
    $ch = Get-S $size 0.45
    $cr = Get-S $size 0.08

    $shadow = New-RoundedRectPath -X ($cx + (Get-S $size 0.012)) -Y ($cy + (Get-S $size 0.02)) -W $cw -H $ch -Radius $cr
    $shadowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(55, 13, 44, 40))
    $g.FillPath($shadowBrush, $shadow)
    $shadowBrush.Dispose()
    $shadow.Dispose()

    $card = New-RoundedRectPath -X $cx -Y $cy -W $cw -H $ch -Radius $cr
    $cardBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 247, 252, 250))
    $g.FillPath($cardBrush, $card)
    $cardBrush.Dispose()
    $card.Dispose()

    # mini folder
    $fx = Get-S $size 0.29
    $fy = Get-S $size 0.36
    $fw = Get-S $size 0.20
    $fh = Get-S $size 0.145
    $folder = New-RoundedRectPath -X $fx -Y $fy -W $fw -H $fh -Radius (Get-S $size 0.025)
    $folderBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 40, 124, 112))
    $g.FillPath($folderBrush, $folder)
    $folderBrush.Dispose()
    $folder.Dispose()

    $tab = New-RoundedRectPath -X $fx -Y ($fy - (Get-S $size 0.025)) -W (Get-S $size 0.09) -H (Get-S $size 0.04) -Radius (Get-S $size 0.015)
    $tabBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 61, 155, 138))
    $g.FillPath($tabBrush, $tab)
    $tabBrush.Dispose()
    $tab.Dispose()

    # text lines
    $lineBrush1 = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 197, 221, 214))
    $lineBrush2 = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 215, 232, 226))
    $ly1 = Get-S $size 0.58
    $ly2 = Get-S $size 0.635
    $lx = Get-S $size 0.29
    $lh = [Math]::Max(2, (Get-S $size 0.028))
    $g.FillRectangle($lineBrush1, $lx, $ly1, (Get-S $size 0.34), $lh)
    $g.FillRectangle($lineBrush2, $lx, $ly2, (Get-S $size 0.22), $lh)
    $lineBrush1.Dispose()
    $lineBrush2.Dispose()
}

function Save-Png {
    param(
        [string]$DrawerName,
        [int]$Size,
        [string]$Path
    )
    $bmp = New-Object System.Drawing.Bitmap $Size, $Size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try {
        & $DrawerName $g $Size
        $dir = Split-Path -Parent $Path
        if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
        $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $g.Dispose()
        $bmp.Dispose()
    }
}

function New-IcoFromPngs {
    param(
        [string[]]$PngPaths,
        [string]$IcoPath
    )

    $images = @()
    foreach ($p in $PngPaths) {
        $bytes = [System.IO.File]::ReadAllBytes($p)
        $img = [System.Drawing.Image]::FromFile($p)
        $w = $img.Width
        $h = $img.Height
        $img.Dispose()
        $images += [pscustomobject]@{
            Width  = $w
            Height = $h
            Bytes  = $bytes
        }
    }

    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter $ms
    try {
        # ICONDIR
        $bw.Write([uint16]0)           # reserved
        $bw.Write([uint16]1)           # type = icon
        $bw.Write([uint16]$images.Count)

        $headerSize = 6 + (16 * $images.Count)
        $offset = $headerSize

        foreach ($img in $images) {
            $wByte = if ($img.Width -ge 256) { [byte]0 } else { [byte]$img.Width }
            $hByte = if ($img.Height -ge 256) { [byte]0 } else { [byte]$img.Height }
            $bw.Write($wByte)
            $bw.Write($hByte)
            $bw.Write([byte]0)         # color count
            $bw.Write([byte]0)         # reserved
            $bw.Write([uint16]1)       # planes
            $bw.Write([uint16]32)      # bit count
            $bw.Write([uint32]$img.Bytes.Length)
            $bw.Write([uint32]$offset)
            $offset += $img.Bytes.Length
        }

        foreach ($img in $images) {
            $bw.Write($img.Bytes)
        }

        $bw.Flush()
        [System.IO.File]::WriteAllBytes($IcoPath, $ms.ToArray())
    }
    finally {
        $bw.Dispose()
        $ms.Dispose()
    }
}

function Build-IconSet {
    param(
        [string]$Name,
        [string]$DrawerName,
        [string]$Root
    )

    $sizes = @(16, 24, 32, 48, 64, 128, 256)
    $pngDir = Join-Path $Root "png\$Name"
    $pngs = @()
    foreach ($s in $sizes) {
        $path = Join-Path $pngDir "$Name-$s.png"
        Save-Png -DrawerName $DrawerName -Size $s -Path $path
        $pngs += $path
    }

    $icoPath = Join-Path $Root "$Name.ico"
    New-IcoFromPngs -PngPaths $pngs -IcoPath $icoPath
    Write-Host "Wrote $icoPath"
    return $icoPath
}

$appIco = Build-IconSet -Name 'folderhub-app' -DrawerName 'Draw-AppIcon' -Root $OutDir
$cardIco = Build-IconSet -Name 'folderhub-cardhost' -DrawerName 'Draw-CardHostIcon' -Root $OutDir

# Copy into project Assets (assets/icons -> repo root)
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $OutDir '..\..'))
$appAssets = Join-Path $repoRoot 'src\FolderHub.App\Assets'
$cardAssets = Join-Path $repoRoot 'src\FolderHub.CardHost\Assets'
New-Item -ItemType Directory -Force -Path $appAssets | Out-Null
New-Item -ItemType Directory -Force -Path $cardAssets | Out-Null

Copy-Item -Force $appIco (Join-Path $appAssets 'FolderHub.ico')
Copy-Item -Force $cardIco (Join-Path $cardAssets 'FolderHub.CardHost.ico')
Copy-Item -Force (Join-Path $OutDir 'png\folderhub-app\folderhub-app-256.png') (Join-Path $appAssets 'FolderHub-256.png')
Copy-Item -Force (Join-Path $OutDir 'png\folderhub-cardhost\folderhub-cardhost-256.png') (Join-Path $cardAssets 'FolderHub.CardHost-256.png')

Write-Host "Copied project ApplicationIcon assets to $appAssets and $cardAssets"
Write-Host "App=$appIco"
Write-Host "CardHost=$cardIco"
