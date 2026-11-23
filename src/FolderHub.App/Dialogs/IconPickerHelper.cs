using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FolderHub.App.Services;
using FolderHub.App.Themes;
using FolderHub.UseCases.Icons;

namespace FolderHub.App.Dialogs;

/// <summary>在 WrapPanel 中渲染可选图标；优先 images PNG（WPF emoji 常空白）。</summary>
internal sealed class IconPickerHelper
{
    private readonly WrapPanel _host;
    private readonly IReadOnlyList<string> _icons;
    private string _selected;

    public IconPickerHelper(WrapPanel host, IReadOnlyList<string> icons, string? initial)
    {
        _host = host;
        _icons = icons;
        _selected = icons.Contains(initial ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            ? icons.First(i => string.Equals(i, initial, StringComparison.OrdinalIgnoreCase))
            : icons[0];
        Rebuild();
    }

    public string SelectedIcon => _selected;

    private void Rebuild()
    {
        _host.Children.Clear();
        foreach (var fileName in _icons)
        {
            var image = AppIconCatalog.TryLoadImage(fileName);
            var tone = FolderIconTones.FromHex(PackagedIconCatalog.GetToneHex(fileName));
            UIElement child;
            if (image is not null)
            {
                child = new Image
                {
                    Source = image,
                    Stretch = Stretch.UniformToFill
                };
            }
            else
            {
                child = new TextBlock
                {
                    Text = PackagedIconCatalog.GetGlyph(fileName),
                    FontSize = 20,
                    FontFamily = new FontFamily("Segoe UI Symbol, Segoe UI Emoji, Microsoft YaHei UI"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
            }

            var border = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(11),
                Margin = new Thickness(0, 0, 8, 8),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = fileName,
                BorderThickness = new Thickness(2),
                Background = image is null ? tone : Brushes.Transparent,
                ClipToBounds = true,
                Child = child
            };
            ApplySelectionChrome(border, string.Equals(fileName, _selected, StringComparison.OrdinalIgnoreCase));
            border.MouseLeftButtonUp += (_, _) => Select(fileName);
            _host.Children.Add(border);
        }
    }

    private void Select(string fileName)
    {
        _selected = fileName;
        foreach (Border border in _host.Children.OfType<Border>())
        {
            var name = border.Tag as string;
            ApplySelectionChrome(border, string.Equals(name, _selected, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static void ApplySelectionChrome(Border border, bool selected)
    {
        border.BorderBrush = selected
            ? TryBrush("AccentBrush") ?? Brushes.SteelBlue
            : Brushes.Transparent;
    }

    private static Brush? TryBrush(string key)
        => Application.Current?.TryFindResource(key) as Brush;
}
