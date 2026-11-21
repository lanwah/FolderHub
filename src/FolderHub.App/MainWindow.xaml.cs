using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FolderHub.App.Dialogs;
using FolderHub.App.Services;
using FolderHub.App.ViewModels;

namespace FolderHub.App;

public partial class MainWindow : Window
{
    private readonly MainDesktopHost _desktopHost = new();
    private MainViewModel? _vm;
    private FolderListItem? _contextFolder;
    private DateTime _lastClickUtc;
    private Guid? _lastClickFolderId;

    public MainWindow()
    {
        InitializeComponent();
        AllowDrop = true;
        DragOver += Window_DragOver;
        Drop += Window_Drop;
        IsVisibleChanged += MainWindow_IsVisibleChanged;
    }

    public void Attach(MainViewModel viewModel)
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.RequestAddCategory += OnAddCategory;
        viewModel.RequestRenameCategory += OnRenameCategory;
        viewModel.RequestEditFolder += OnEditFolder;
        viewModel.RequestOpenSettings += OnOpenSettings;
        viewModel.RequestRetargetUnavailablePath += OnRetargetUnavailablePath;
        viewModel.ApplyMainPinnedVisual += OnApplyMainPinnedVisual;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainViewModel.PanelOpacity) or nameof(MainViewModel.MainLocked))
                ApplyChrome();
        };

        var s = viewModel.CurrentSettings;
        Width = Math.Max(720, s.MainWidth);
        Height = Math.Max(420, s.MainHeight);
        var restored = viewModel.ResolveMainPlacement(Width, Height);
        Left = restored.Left;
        Top = restored.Top;
        if (restored.MigratedToPrimary)
            PersistBounds();

        ApplyChrome();
        Closed += (_, _) => PersistBounds();
    }

    /// <summary>托盘「设置」入口：确保可在主窗口已显示时打开。</summary>
    public void OpenSettings() => OnOpenSettings();

    /// <summary>退出前由 App 调用，确保隐藏态下也能落盘窗口位置。</summary>
    public void PersistBoundsIfNeeded() => PersistBounds();

    /// <summary>按 MainPinned 尝试壁纸层；返回失败说明（成功 null）。</summary>
    public string? ApplyDesktopPinState(bool pinned)
        => _desktopHost.ApplyPinnedState(this, pinned);

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

    private void Minimize_Click(object sender, RoutedEventArgs e) => Hide();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private string? OnApplyMainPinnedVisual(bool pinned)
    {
        // 隐藏态下先不嵌入；显示时由 IsVisibleChanged 再应用。
        if (!IsVisible)
            return pinned ? null : ApplyDesktopPinState(pinned: false);

        return ApplyDesktopPinState(pinned);
    }

    private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is null)
            return;

        if (IsVisible)
        {
            var error = ApplyDesktopPinState(_vm.MainPinned);
            if (_vm.MainPinned && error is not null)
                _vm.StatusMessage = "壁纸层嵌入失败，已回退普通窗口：" + error;
        }
        else
        {
            ApplyDesktopPinState(pinned: false);
        }
    }

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
        var dialog = new SettingsDialog(_vm.CurrentSettings) { Owner = IsVisible ? this : null };
        if (dialog.ShowDialog() != true)
            return;

        _vm.ApplyAndSaveSettings(
            dialog.MainPinned,
            dialog.MainLocked,
            dialog.KeepCardsAfterMainExit,
            dialog.MainOpacity,
            dialog.StartupEnabled,
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

    private void OnRetargetUnavailablePath(Guid folderId, string path)
    {
        var result = MessageBox.Show(
            this,
            $"目标路径不可用：\n{path}\n\n记录已保留。是否立即重新指定路径？",
            "路径异常",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
            OnEditFolder(folderId);
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = HasFolderDrop(e.Data) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (_vm is null || !HasFolderDrop(e.Data))
            return;

        var folders = ExtractDroppedFolders(e.Data);
        if (!_vm.TryAddDroppedFolders(folders, out var message))
            MessageBox.Show(this, message, "拖拽添加", MessageBoxButton.OK, MessageBoxImage.Information);

        e.Handled = true;
    }

    private static bool HasFolderDrop(IDataObject data)
        => ExtractDroppedFolders(data).Count > 0;

    private static IReadOnlyList<string> ExtractDroppedFolders(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.FileDrop))
            return Array.Empty<string>();

        if (data.GetData(DataFormats.FileDrop) is not string[] paths)
            return Array.Empty<string>();

        return paths
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
