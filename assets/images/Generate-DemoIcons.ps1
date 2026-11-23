# Regenerates FolderHub icon PNGs into this folder.
# Folder icons: Twemoji color emoji (transparent) — matches design-demo emoji look in WPF.
# Category icons: geometric glyphs on transparent (tone comes from UI).
# Twemoji: https://github.com/twitter/twemoji (CC-BY 4.0)
$ErrorActionPreference = "Stop"
$outDir = $PSScriptRoot
$twemojiBase = "https://cdn.jsdelivr.net/gh/twitter/twemoji@14.0.2/assets/72x72"

Add-Type -AssemblyName System.Drawing

function New-Rgb([int]$r, [int]$g, [int]$b) {
    return [System.Drawing.Color]::FromArgb(255, $r, $g, $b)
}

function U([int]$code) { return [char]$code }

function Save-TwemojiPng {
    param(
        [string]$FileName,
        [string]$Codepoint
    )

    $url = "$twemojiBase/$Codepoint.png"
    $target = Join-Path $outDir $FileName
    Write-Host "GET $url"
    Invoke-WebRequest -Uri $url -OutFile $target -UseBasicParsing
    Write-Host ("Wrote {0} ({1} bytes)" -f $FileName, (Get-Item $target).Length)
}

function Get-OpaqueBounds([System.Drawing.Bitmap]$Bitmap) {
    $minX = $Bitmap.Width; $minY = $Bitmap.Height; $maxX = -1; $maxY = -1
    for ($y = 0; $y -lt $Bitmap.Height; $y++) {
        for ($x = 0; $x -lt $Bitmap.Width; $x++) {
            if ($Bitmap.GetPixel($x, $y).A -eq 0) { continue }
            if ($x -lt $minX) { $minX = $x }
            if ($y -lt $minY) { $minY = $y }
            if ($x -gt $maxX) { $maxX = $x }
            if ($y -gt $maxY) { $maxY = $y }
        }
    }
    if ($maxX -lt 0) { return $null }
    return [System.Drawing.Rectangle]::FromLTRB($minX, $minY, $maxX + 1, $maxY + 1)
}

function Write-SymbolIcon {
    param(
        [string]$Target,
        [string]$Glyph,
        [System.Drawing.Color]$Ink,
        [int]$Size = 128
    )

    $fontSize = [float]($Size * 0.55)
    try {
        $font = New-Object System.Drawing.Font "Segoe UI Symbol", $fontSize, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    } catch {
        $font = New-Object System.Drawing.Font "Segoe UI", $fontSize, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    }

    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $inkBrush = New-Object System.Drawing.SolidBrush $Ink

    # Pass 1: draw on oversized canvas, then recentre by opaque pixel bounds
    # (GDI+ symbol metrics are often top-heavy; a fixed Y offset made sidebar icons look high.)
    $padCanvas = $Size * 2
    $tmp = New-Object System.Drawing.Bitmap $padCanvas, $padCanvas
    $tg = [System.Drawing.Graphics]::FromImage($tmp)
    $tg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $tg.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $tg.Clear([System.Drawing.Color]::Transparent)
    $tg.DrawString($Glyph, $font, $inkBrush, (New-Object System.Drawing.RectangleF 0, 0, $padCanvas, $padCanvas), $sf)
    $tg.Dispose()

    $bounds = Get-OpaqueBounds $tmp
    $bmp = New-Object System.Drawing.Bitmap $Size, $Size
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    $gfx.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $gfx.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $gfx.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $gfx.Clear([System.Drawing.Color]::Transparent)

    if ($null -ne $bounds) {
        $dest = [int](($Size - $bounds.Width) / 2)
        $destY = [int](($Size - $bounds.Height) / 2)
        $gfx.DrawImage($tmp, $dest, $destY, $bounds, [System.Drawing.GraphicsUnit]::Pixel)
    }

    $bmp.Save($Target, [System.Drawing.Imaging.ImageFormat]::Png)
    $gfx.Dispose(); $tmp.Dispose(); $font.Dispose(); $inkBrush.Dispose(); $sf.Dispose(); $bmp.Dispose()
    Write-Host ("Wrote {0} ({1} bytes)" -f (Split-Path $Target -Leaf), (Get-Item $Target).Length)
}

# design-demo folder emoji → Twemoji codepoints
$folderTwemoji = @(
    @{ File = "folder.png"; Code = "1f4c1" }      # 📁
    @{ File = "code.png"; Code = "1f4bb" }        # 💻
    @{ File = "design.png"; Code = "1f3a8" }      # 🎨
    @{ File = "docs.png"; Code = "1f5c2" }        # 🗂️
    @{ File = "photos.png"; Code = "1f5bc" }      # 🖼️
    @{ File = "learning.png"; Code = "1f4da" }    # 📚
    @{ File = "package.png"; Code = "1f4e6" }     # 📦
    @{ File = "archive.png"; Code = "1f5c4" }     # 🗄️
)

foreach ($s in $folderTwemoji) {
    Save-TwemojiPng -FileName $s.File -Codepoint $s.Code
}

$ink = New-Rgb 32 42 48
$categorySpecs = @(
    @{ File = "category-all.png"; Glyph = (U 0x25A6) }
    @{ File = "category-work.png"; Glyph = (U 0x25A3) }
    @{ File = "category-project.png"; Glyph = (U 0x25C8) }
    @{ File = "category-personal.png"; Glyph = (U 0x2661) }
    @{ File = "category-uncategorized.png"; Glyph = (U 0x25A1) }
    @{ File = "category-default.png"; Glyph = (U 0x25C7) }
)

foreach ($s in $categorySpecs) {
    Write-SymbolIcon -Target (Join-Path $outDir $s.File) -Glyph $s.Glyph -Ink $ink -Size 128
}

Write-Host "Done."
