using System.Windows;
using FolderHub.App.Themes;
using FolderHub.UseCases.Persistence;
using ThemeId = FolderHub.UseCases.Persistence.UiTheme;

namespace FolderHub.App.Dialogs;

public partial class SettingsDialog : Window
{
    private readonly string _initialTheme;
    private string _selectedTheme;

    public SettingsDialog(MainSettingsSnapshot settings)
    {
        InitializeComponent();
        StartupBox.IsChecked = settings.StartupEnabled;
        PinBox.IsChecked = settings.MainPinned;
        LockBox.IsChecked = settings.MainLocked;
        KeepCardsBox.IsChecked = settings.KeepCardsAfterMainExit;
        var percent = (int)Math.Round(Math.Clamp(settings.MainOpacity, 0.20, 1.0) * 100);
        OpacitySlider.Value = percent;
        OpacityLabel.Text = $"{percent}%";
        _initialTheme = ThemeId.Normalize(settings.UiTheme);
        _selectedTheme = _initialTheme;
        SyncThemeToggle();
    }

    public bool StartupEnabled => StartupBox.IsChecked == true;
    public bool MainPinned => PinBox.IsChecked == true;
    public bool MainLocked => LockBox.IsChecked == true;
    public bool KeepCardsAfterMainExit => KeepCardsBox.IsChecked == true;
    public double MainOpacity => Math.Clamp(OpacitySlider.Value / 100.0, 0.20, 1.0);
    public string SelectedUiTheme => _selectedTheme;

    private void OpacitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityLabel is null)
            return;
        OpacityLabel.Text = $"{(int)Math.Round(OpacitySlider.Value)}%";
    }

    private void ThemeLight_Click(object sender, RoutedEventArgs e)
    {
        _selectedTheme = ThemeId.Light;
        AppTheme.Apply(_selectedTheme);
        SyncThemeToggle();
    }

    private void ThemeDark_Click(object sender, RoutedEventArgs e)
    {
        _selectedTheme = ThemeId.Dark;
        AppTheme.Apply(_selectedTheme);
        SyncThemeToggle();
    }

    private void SyncThemeToggle()
    {
        var dark = ThemeId.IsDark(_selectedTheme);
        ThemeLightBtn.Tag = dark ? null : "active";
        ThemeDarkBtn.Tag = dark ? "active" : null;
    }

    private void Save_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (!string.Equals(_selectedTheme, _initialTheme, StringComparison.Ordinal))
            AppTheme.Apply(_initialTheme);
        DialogResult = false;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult != true
            && !string.Equals(_selectedTheme, _initialTheme, StringComparison.Ordinal))
            AppTheme.Apply(_initialTheme);
        base.OnClosing(e);
    }
}
