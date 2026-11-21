using System.Windows;
using FolderHub.UseCases.Persistence;

namespace FolderHub.CardHost.Themes;

/// <summary>CardHost 浅/深色主题切换（与 Main 共用 token 文件）。</summary>
public static class AppTheme
{
    private const string ThemeDictionaryTag = "FolderHub.ThemeColors";

    public static string Current { get; private set; } = UiTheme.Light;

    public static void Apply(string? theme)
    {
        var next = UiTheme.Normalize(theme);
        var app = Application.Current;
        if (app is null)
            return;

        var dicts = app.Resources.MergedDictionaries;
        for (var i = dicts.Count - 1; i >= 0; i--)
        {
            if (dicts[i].Contains(ThemeDictionaryTag))
                dicts.RemoveAt(i);
        }

        var source = next == UiTheme.Dark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        var themeDict = new ResourceDictionary { Source = source };
        themeDict[ThemeDictionaryTag] = next;
        dicts.Insert(0, themeDict);
        Current = next;
    }
}
