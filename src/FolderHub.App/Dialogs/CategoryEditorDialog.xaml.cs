using System.Windows;
using FolderHub.App.Services;

namespace FolderHub.App.Dialogs;

public partial class CategoryEditorDialog : Window
{
    private readonly IconPickerHelper _iconPicker;

    public CategoryEditorDialog(string title, string? initialName = null, string? initialIcon = null)
    {
        InitializeComponent();
        TitleText.Text = title;
        Title = title;
        if (!string.IsNullOrWhiteSpace(initialName))
            NameBox.Text = initialName;

        var icons = AppIconCatalog.CategoryIcons
            .Where(i => !string.Equals(i, "category-all.png", StringComparison.OrdinalIgnoreCase))
            .ToList();
        _iconPicker = new IconPickerHelper(
            IconPanel,
            icons,
            AppIconCatalog.NormalizeCategoryIcon(initialIcon));

        Loaded += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    public string CategoryName => NameBox.Text.Trim();
    public string SelectedIcon => _iconPicker.SelectedIcon;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show(this, "分类名称不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
