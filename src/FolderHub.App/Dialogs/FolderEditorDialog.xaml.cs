using System.Windows;
using System.Windows.Controls;
using FolderHub.App.Services;
using FolderHub.Domain.Entities;

namespace FolderHub.App.Dialogs;

public partial class FolderEditorDialog : Window
{
    public sealed record CategoryChoice(Guid Id, string Name);

    private readonly IconPickerHelper _iconPicker;

    public FolderEditorDialog(
        string title,
        IEnumerable<Category> categories,
        IEnumerable<Guid>? selectedCategoryIds = null,
        string? displayName = null,
        string? alias = null,
        string? path = null,
        string? description = null,
        string? icon = null)
    {
        InitializeComponent();
        TitleText.Text = title;
        Title = title;

        foreach (var category in categories)
            CategoryList.Items.Add(new CategoryChoice(category.Id, category.Name));

        var selected = new HashSet<Guid>(selectedCategoryIds ?? Array.Empty<Guid>());
        foreach (CategoryChoice item in CategoryList.Items)
        {
            if (selected.Contains(item.Id))
                CategoryList.SelectedItems.Add(item);
        }

        NameBox.Text = displayName ?? string.Empty;
        AliasBox.Text = alias ?? string.Empty;
        PathBox.Text = path ?? string.Empty;
        DescBox.Text = description ?? string.Empty;

        _iconPicker = new IconPickerHelper(
            IconPanel,
            AppIconCatalog.FolderIcons,
            AppIconCatalog.NormalizeFolderIcon(icon));
    }

    public string DisplayName => NameBox.Text.Trim();
    public string? Alias => string.IsNullOrWhiteSpace(AliasBox.Text) ? null : AliasBox.Text.Trim();
    public string PathValue => PathBox.Text.Trim();
    public string? Description => string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim();
    public string SelectedIcon => _iconPicker.SelectedIcon;

    public IReadOnlyList<Guid> SelectedCategoryIds
        => CategoryList.SelectedItems.Cast<CategoryChoice>().Select(c => c.Id).ToList();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show(this, "文件夹名称不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(PathBox.Text))
        {
            MessageBox.Show(this, "真实路径不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
