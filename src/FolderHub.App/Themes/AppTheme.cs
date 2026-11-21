using System.Windows;
using FolderHub.UseCases.Persistence;

namespace FolderHub.App.Themes;

/// <summary>切换 Application 级浅/深色 ResourceDictionary（对齐 design-demo data-theme）。</summary>
public static class AppTheme
{
    private const string ThemeDictionaryTag = "FolderHub.ThemeColors";

    public static string Current { get; private set; } = UiTheme.Light;

    /// <summary>主题字典已切换；主窗口据此刷新透明度笔刷（非 DynamicResource）。</summary>
    public static event Action? Changed;

    public static void Apply(string? theme)
    {
        var next = UiTheme.Normalize(theme);
        var app = Application.Current;
        if (app is null)
            return;

        var dicts = app.Resources.MergedDictionaries;
        for (var i = dicts.Count - 1; i >= 0; i--)
        {
            var d = dicts[i];
            if (d.Contains(ThemeDictionaryTag))
            {
                dicts.RemoveAt(i);
                continue;
            }

            var path = d.Source?.OriginalString ?? string.Empty;
            if (path.EndsWith("LightTheme.xaml", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase))
                dicts.RemoveAt(i);
        }

        var source = next == UiTheme.Dark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        var themeDict = new ResourceDictionary { Source = source };
        themeDict[ThemeDictionaryTag] = next;
        dicts.Insert(0, themeDict);
        Current = next;
        Changed?.Invoke();
    }

    public static void Toggle()
        => Apply(Current == UiTheme.Dark ? UiTheme.Light : UiTheme.Dark);

    /// <summary>与 Light/DarkTheme.xaml 中 PanelR/G/B 一致；按 Current 取值，避免切主题时序读到旧资源。</summary>
    public static (byte R, byte G, byte B) GetPanelRgb()
        => Current == UiTheme.Dark
            ? ((byte)21, (byte)38, (byte)44)
            : ((byte)255, (byte)255, (byte)255);
}
