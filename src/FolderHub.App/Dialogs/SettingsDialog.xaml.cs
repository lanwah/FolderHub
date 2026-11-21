using System.Windows;
using FolderHub.UseCases.Persistence;

namespace FolderHub.App.Dialogs;

public partial class SettingsDialog : Window
{
    public SettingsDialog(MainSettingsSnapshot settings)
    {
        InitializeComponent();
        PinBox.IsChecked = settings.MainPinned;
        LockBox.IsChecked = settings.MainLocked;
        KeepCardsBox.IsChecked = settings.KeepCardsAfterMainExit;
        var percent = (int)Math.Round(Math.Clamp(settings.MainOpacity, 0.20, 1.0) * 100);
        OpacitySlider.Value = percent;
        OpacityLabel.Text = $"{percent}%";
    }

    public bool MainPinned => PinBox.IsChecked == true;
    public bool MainLocked => LockBox.IsChecked == true;
    public bool KeepCardsAfterMainExit => KeepCardsBox.IsChecked == true;
    public double MainOpacity => Math.Clamp(OpacitySlider.Value / 100.0, 0.20, 1.0);

    private void OpacitySlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityLabel is null)
            return;
        OpacityLabel.Text = $"{(int)Math.Round(OpacitySlider.Value)}%";
    }

    private void Save_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
