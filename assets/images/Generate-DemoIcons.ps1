# Generates design-demo style icon PNGs into this folder (pastel tone + glyph).
$ErrorActionPreference = "Stop"
$outDir = $PSScriptRoot

Add-Type -AssemblyName System.Drawing

function New-Rgb([int]$r, [int]$g, [int]$b) {
    return [System.Drawing.Color]::FromArgb(255, $r, $g, $b)
}

function U([int]$code) { return [char]$code }
function E([int]$code) { return [char]::ConvertFromUtf32($code) }

function Write-DemoIcon {
    param(
        [string]$Target,
        [string]$Glyph,
        [System.Drawing.Color]$Tone,
        [System.Drawing.Color]$Ink,
        [int]$Size = 128,
        [bool]$EmojiFont = $false
    )

    $bmp = New-Object System.Drawing.Bitmap $Size, $Size
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    $gfx.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $gfx.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $gfx.Clear([System.Drawing.Color]::Transparent)

    $pad = [int]($Size * 0.06)
    $toneBrush = New-Object System.Drawing.SolidBrush $Tone
    # Soft square with slight inset (demo folder-icon feel).
    $gfx.FillRectangle($toneBrush, $pad, $pad, $Size - 2 * $pad, $Size - 2 * $pad)

    $fontName = if ($EmojiFont) { "Segoe UI Emoji" } else { "Segoe UI Symbol" }
    $fontSize = [float]($Size * 0.42)
    try {
        $font = New-Object System.Drawing.Font $fontName, $fontSize, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    } catch {
        $font = New-Object System.Drawing.Font "Segoe UI", $fontSize, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    }

    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $inkBrush = New-Object System.Drawing.SolidBrush $Ink
    $textRect = New-Object System.Drawing.RectangleF 0, ($Size * 0.02), $Size, $Size
    $gfx.DrawString($Glyph, $font, $inkBrush, $textRect, $sf)

    $bmp.Save($Target, [System.Drawing.Imaging.ImageFormat]::Png)
    $gfx.Dispose(); $font.Dispose(); $toneBrush.Dispose(); $inkBrush.Dispose(); $sf.Dispose(); $bmp.Dispose()
}

$ink = New-Rgb 32 42 48
$specs = @(
    @{ File = "folder.png"; Glyph = (E 0x1F4C1); Tone = (New-Rgb 237 243 255); Emoji = $true }
    @{ File = "code.png"; Glyph = (E 0x1F4BB); Tone = (New-Rgb 232 244 239); Emoji = $true }
    @{ File = "design.png"; Glyph = (E 0x1F3A8); Tone = (New-Rgb 245 237 255); Emoji = $true }
    @{ File = "docs.png"; Glyph = (E 0x1F5C2); Tone = (New-Rgb 255 243 230); Emoji = $true }
    @{ File = "photos.png"; Glyph = (E 0x1F5BC); Tone = (New-Rgb 255 240 240); Emoji = $true }
    @{ File = "learning.png"; Glyph = (E 0x1F4DA); Tone = (New-Rgb 237 244 255); Emoji = $true }
    @{ File = "package.png"; Glyph = (E 0x1F4E6); Tone = (New-Rgb 238 242 240); Emoji = $true }
    @{ File = "archive.png"; Glyph = (E 0x1F5C4); Tone = (New-Rgb 242 240 233); Emoji = $true }
    @{ File = "category-all.png"; Glyph = (U 0x25A6); Tone = (New-Rgb 234 242 255); Emoji = $false }
    @{ File = "category-work.png"; Glyph = (U 0x25A3); Tone = (New-Rgb 232 244 239); Emoji = $false }
    @{ File = "category-project.png"; Glyph = (U 0x25C8); Tone = (New-Rgb 245 237 255); Emoji = $false }
    @{ File = "category-personal.png"; Glyph = (U 0x2661); Tone = (New-Rgb 255 240 240); Emoji = $false }
    @{ File = "category-uncategorized.png"; Glyph = (U 0x25A1); Tone = (New-Rgb 238 242 240); Emoji = $false }
    @{ File = "category-default.png"; Glyph = (U 0x25C7); Tone = (New-Rgb 237 243 255); Emoji = $false }
)

foreach ($s in $specs) {
    $target = Join-Path $outDir $s.File
    Write-DemoIcon -Target $target -Glyph $s.Glyph -Tone $s.Tone -Ink $ink -Size 128 -EmojiFont:$s.Emoji
    Write-Host ("Wrote {0} ({1} bytes)" -f $s.File, (Get-Item $target).Length)
}
