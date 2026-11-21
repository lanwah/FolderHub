using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FolderHub.App.Dialogs;
using FolderHub.App.ViewModels;

namespace FolderHub.App;

public partial class MainWindow : Window
{
    private MainViewModel? _vm;
    private FolderListItem? _contextFolder;
    private DateTime _lastClickUtc;
    private Guid? _lastClickFolderId;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void Attach(MainViewModel viewModel)
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.RequestAddCategory += OnAddCategory;
        viewModel.RequestRenameCategory += OnRenameCategory;
        viewModel.RequestEditFolder += OnEditFolder;
        viewModel.RequestOpenSettings += OnOpenSettings;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainViewModel.PanelOpacity) or nameof(MainViewModel.MainLocked))
                ApplyChrome();
        };

        var s = viewModel.CurrentSettings;
        Width = Math.Max(720, s.MainWidth);
        Height = Math.Max(420, s.MainHeight);
        Left = s.MainLeft;
        Top = s.MainTop;
        if (Left < 0 || Top < 0 || Left > SystemParameters.VirtualScreenWidth - 80
            || Top > SystemParameters.VirtualScreenHeight - 80)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        ApplyChrome();
        Closed += (_, _) => PersistBounds();
    }

    private void ApplyChrome()
    {
        if (_vm is null)
            return;

        var alpha = (byte)Math.Clamp((int)Math.Round(_vm.PanelOpacity * 255), 51, 255);
        ContentPanel.Background = new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 255));
        RootChrome.Background = new SolidColorBrush(Color.FromArgb(alpha, 255, 255, 255));
        ResizeMode = _vm.MainLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
    }

    private void PersistBounds()
    {
        if (_vm is null || WindowState != WindowState.Normal)
            return;
        _vm.PersistWindowBounds(new Rect(Left, Top, Width, Height));
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm?.MainLocked == true)
            return;
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.K && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }
    }

    private void OnAddCategory()
    {
        if (_vm is null) return;
        var dialog = new CategoryEditorDialog("新增分类") { Owner = this };
        if (dialog.ShowDialog() != true)
            return;
        if (!_vm.TryAddCategory(dialog.CategoryName, out var error))
            MessageBox.Show(this, error, "无法创建分类", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void OnRenameCategory(CategoryListItem item)
    {
        if (_vm is null || item.CategoryId is not Guid id)
            return;
        var dialog = new CategoryEditorDialog("重命名分类", item.Name) { Owner = this };
        if (dialog.ShowDialog() != true)
            return;
        if (!_vm.TryRenameCategory(id, dialog.CategoryName, out var error))
            MessageBox.Show(this, error, "无法重命名", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void OnEditFolder(Guid? folderId)
    {
        if (_vm is null) return;

        string? name = null, alias = null, path = null, desc = null;
        IEnumerable<Guid>? selected = null;
        if (folderId is Guid id)
        {
            var folder = _vm.GetFolder(id);
            if (folder is null) return;
            name = folder.DisplayName;
            alias = folder.Alias;
            path = folder.Path;
            desc = folder.Description;
            selected = _vm.GetFolderCategoryIds(id);
        }
        else if (_vm.SelectedCategory is { IsVirtualAll: false, CategoryId: Guid catId })
        {
            selected = new[] { catId };
        }

        var dialog = new FolderEditorDialog(
            folderId is null ? "添加文件夹" : "编辑文件夹",
            _vm.GetEditableCategories(),
            selected,
            name,
            alias,
            path,
            desc)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
            return;

        if (!_vm.TrySaveFolder(
                folderId,
                dialog.DisplayName,
                dialog.Alias,
                dialog.PathValue,
                dialog.Description,
                dialog.SelectedCategoryIds,
                out var error))
        {
            MessageBox.Show(this, error, "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnOpenSettings()
    {
        if (_vm is null) return;
        var dialog = new SettingsDialog(_vm.CurrentSettings) { Owner = this };
        if (dialog.ShowDialog() != true)
            return;

        _vm.ApplyAndSaveSettings(
            dialog.MainPinned,
            dialog.MainLocked,
            dialog.KeepCardsAfterMainExit,
            dialog.MainOpacity,
            new Rect(Left, Top, Width, Height));
        ApplyChrome();
    }

    private void FolderCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: FolderListItem item } || _vm is null)
            return;

        _vm.SelectFolder(item);
        var now = DateTime.UtcNow;
        if (_lastClickFolderId == item.Id && (now - _lastClickUtc).TotalMilliseconds < 400)
        {
            _vm.OpenFolderCommand.Execute(item);
            _lastClickFolderId = null;
        }
        else
        {
            _lastClickFolderId = item.Id;
            _lastClickUtc = now;
        }
    }

    private void FolderCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // selection handled in down for double-click timing
    }

    private void FolderCard_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: FolderListItem item } || _vm is null)
            return;

        _vm.SelectFolder(item);
        OpenFolderMenu(item, sender as UIElement);
        e.Handled = true;
    }

    private void FolderMore_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: FolderListItem item } || _vm is null)
            return;
        _vm.SelectFolder(item);
        OpenFolderMenu(item, sender as UIElement);
    }

    private void OpenFolderMenu(FolderListItem item, UIElement? placementTarget)
    {
        _contextFolder = item;
        if (FindResource("FolderMenu") is not ContextMenu menu)
            return;
        menu.PlacementTarget = placementTarget ?? this;
        menu.IsOpen = true;
    }

    private void Pin_Click(object sender, RoutedEventArgs e)
        => _vm?.PinFolderCommand.Execute((sender as FrameworkElement)?.Tag);

    private void ContextOpen_Click(object sender, RoutedEventArgs e)
        => _vm?.OpenFolderCommand.Execute(_contextFolder);

    private void ContextEdit_Click(object sender, RoutedEventArgs e)
        => _vm?.EditFolderCommand.Execute(_contextFolder);

    private void ContextPin_Click(object sender, RoutedEventArgs e)
        => _vm?.PinFolderCommand.Execute(_contextFolder);

    private void ContextDelete_Click(object sender, RoutedEventArgs e)
        => _vm?.DeleteFolderCommand.Execute(_contextFolder);
}
