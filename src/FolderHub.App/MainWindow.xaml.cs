using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FolderHub.App.Dialogs;
using FolderHub.App.Services;
using FolderHub.App.Themes;
using FolderHub.App.ViewModels;
using FolderHub.UseCases.Persistence;

namespace FolderHub.App;

public partial class MainWindow : Window
{
    /// <summary>RootChrome 外层 Margin，持久化/恢复时按内容区几何换算。</summary>
    private const double ChromeShadowPad = 20;
    private const double ChromeShadowPadTotal = ChromeShadowPad * 2;
    private const double ChromeCornerRadius = 18;

    private readonly MainDesktopHost _desktopHost = new();
    private ItemsControlReorderDragDrop? _categoryReorderDrag;
    private ItemsControlReorderDragDrop? _folderReorderDrag;
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
        AppTheme.Changed += OnAppThemeChanged;
        Closed += (_, _) =>
        {
            AppTheme.Changed -= OnAppThemeChanged;
            PersistBounds();
        };
    }

    public void Attach(MainViewModel viewModel)
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.RequestAddCategory += OnAddCategory;
        viewModel.RequestRenameCategory += OnRenameCategory;
        viewModel.RequestEditFolder += OnEditFolder;
        viewModel.RequestOpenSettings += OnOpenSettings;
        viewModel.RequestOpenDonation += OnOpenDonation;
        viewModel.RequestRetargetUnavailablePath += OnRetargetUnavailablePath;
        viewModel.ApplyMainPinnedVisual += OnApplyMainPinnedVisual;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainViewModel.PanelOpacity)
                or nameof(MainViewModel.MainLocked)
                or nameof(MainViewModel.UiTheme))
                ApplyChrome();
            if (e.PropertyName is nameof(MainViewModel.IsListMode))
                SyncFolderItemsPanel();
        };
        SyncFolderItemsPanel();
        AttachReorderDrag();

        var s = viewModel.CurrentSettings;
        var contentW = Math.Max(720, s.MainWidth);
        var contentH = Math.Max(520, s.MainHeight);
        Width = contentW + ChromeShadowPadTotal;
        Height = contentH + ChromeShadowPadTotal;
        var restored = viewModel.ResolveMainPlacement(contentW, contentH);
        Left = restored.Left - ChromeShadowPad;
        Top = restored.Top - ChromeShadowPad;
        if (restored.MigratedToPrimary)
            PersistBounds();

        SyncThemeToggleGlyph();
        ApplyChrome();
    }

    private void OnAppThemeChanged()
    {
        if (Dispatcher.CheckAccess())
            ApplyChrome();
        else
            Dispatcher.BeginInvoke(ApplyChrome);
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

        var (r, g, b) = AppTheme.GetPanelRgb();
        var alpha = (byte)Math.Clamp((int)Math.Round(_vm.PanelOpacity * 255), 51, 255);
        var brush = new SolidColorBrush(Color.FromArgb(alpha, r, g, b));
        ContentPanel.Background = brush;
        RootChrome.Background = brush;
        ResizeMode = _vm.MainLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
        SyncThemeToggleGlyph();
    }

    /// <summary>按圆角裁剪子元素（对齐 design-demo overflow:hidden），消除四角直角露出。</summary>
    private void RootChrome_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (RootChrome.ActualWidth <= 0 || RootChrome.ActualHeight <= 0)
            return;
        RootChrome.Clip = new RectangleGeometry(
            new Rect(0, 0, RootChrome.ActualWidth, RootChrome.ActualHeight),
            ChromeCornerRadius,
            ChromeCornerRadius);
    }

    private void SyncThemeToggleGlyph()
    {
        if (ThemeToggleBtn is null)
            return;
        var dark = UiTheme.IsDark(_vm?.UiTheme ?? AppTheme.Current);
        ThemeToggleBtn.Content = dark ? "☀" : "☾";
        ThemeToggleBtn.ToolTip = dark ? "切换到浅色主题" : "切换到深色主题";
    }

    private void AttachReorderDrag()
    {
        if (_vm is null)
            return;

        _categoryReorderDrag = new ItemsControlReorderDragDrop(
            CategoryList,
            ItemsControlReorderDragDrop.CategoryPayloadFormat,
            index => _vm.Categories.ElementAtOrDefault(index)?.IsReorderable == true,
            index => _vm.Categories.ElementAtOrDefault(index)?.IsReorderable == true,
            (from, to) => _vm.TryReorderCategories(from, to),
            index => _vm.NormalizeCategoryDropIndex(index));

        _folderReorderDrag = new ItemsControlReorderDragDrop(
            FolderItems,
            ItemsControlReorderDragDrop.FolderPayloadFormat,
            _ => _vm.CanReorderFolders,
            _ => _vm.CanReorderFolders,
            (from, to) => _vm.TryReorderFolders(from, to));
    }

    /// <summary>列表=单列 StackPanel；网格=双列 UniformGrid。须在代码里换板，Style Trigger 改 ItemsPanel 不生效。</summary>
    private void SyncFolderItemsPanel()
    {
        if (FolderItems is null || _vm is null)
            return;

        if (_vm.IsListMode)
        {
            FolderItems.ItemsPanel = new ItemsPanelTemplate
            {
                VisualTree = new FrameworkElementFactory(typeof(StackPanel))
            };
            return;
        }

        var factory = new FrameworkElementFactory(typeof(UniformGrid));
        factory.SetValue(UniformGrid.ColumnsProperty, 2);
        FolderItems.ItemsPanel = new ItemsPanelTemplate { VisualTree = factory };
    }

    private Rect ContentBoundsRect()
        => new(
            Left + ChromeShadowPad,
            Top + ChromeShadowPad,
            Math.Max(720, Width - ChromeShadowPadTotal),
            Math.Max(520, Height - ChromeShadowPadTotal));

    private void PersistBounds()
    {
        if (_vm is null || WindowState != WindowState.Normal)
            return;
        _vm.PersistWindowBounds(ContentBoundsRect());
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm?.MainLocked == true)
            return;
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        => _vm?.ToggleTheme();

    private void Minimize_Click(object sender, RoutedEventArgs e) => Hide();

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void SearchBorder_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        SearchBorder.BorderBrush = (Brush)FindResource("AccentMidBrush");
        SearchBorder.Background = (Brush)FindResource("InputFocusBgBrush");
    }

    private void SearchBorder_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        SearchBorder.BorderBrush = (Brush)FindResource("LineStrongBrush");
        SearchBorder.Background = (Brush)FindResource("PanelSoftBrush");
        SearchBorder.Effect = null;
    }

    private void FolderCard_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border border || border.Tag is not FolderListItem item)
            return;
        if (item.IsPathUnavailable)
            return;

        // demo: translateY(-2px) + card-hover-border + soft shadow（选中时保留 ring，只抬升）
        border.RenderTransform = new TranslateTransform(0, -2);
        if (!item.IsSelected)
            border.BorderBrush = (Brush)FindResource("CardHoverBorderBrush");
        border.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            BlurRadius = 24,
            ShadowDepth = 10,
            Opacity = 0.08,
            Color = Color.FromRgb(0x0d, 0x2a, 0x24)
        };
    }

    private void FolderCard_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border border || border.Tag is not FolderListItem item)
            return;
        border.RenderTransform = null;
        border.Effect = null;
        if (item.IsSelected)
        {
            border.BorderBrush = (Brush)FindResource("AccentMidBrush");
            return;
        }

        if (item.IsPathUnavailable)
        {
            border.BorderBrush = (Brush)FindResource("UnavailableBorderBrush");
            return;
        }

        border.BorderBrush = (Brush)FindResource("LineBrush");
    }

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
        if (!_vm.TryAddCategory(dialog.CategoryName, dialog.SelectedIcon, out var error))
            MessageBox.Show(this, error, "无法创建分类", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void OnRenameCategory(CategoryListItem item)
    {
        if (_vm is null || item.CategoryId is not Guid id)
            return;
        var dialog = new CategoryEditorDialog("编辑分类", item.Name, item.IconFileName) { Owner = this };
        if (dialog.ShowDialog() != true)
            return;
        if (!_vm.TryRenameCategory(id, dialog.CategoryName, dialog.SelectedIcon, out var error))
            MessageBox.Show(this, error, "无法编辑分类", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void OnEditFolder(Guid? folderId)
    {
        if (_vm is null) return;

        string? name = null, alias = null, path = null, desc = null, icon = null;
        IEnumerable<Guid>? selected = null;
        if (folderId is Guid id)
        {
            var folder = _vm.GetFolder(id);
            if (folder is null) return;
            name = folder.DisplayName;
            alias = folder.Alias;
            path = folder.Path;
            desc = folder.Description;
            icon = folder.Icon;
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
            desc,
            icon)
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
                dialog.SelectedIcon,
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
            dialog.SelectedUiTheme,
            ContentBoundsRect());
        ApplyChrome();
    }

    private void OnOpenDonation()
    {
        var dialog = new DonationDialog { Owner = this };
        dialog.ShowDialog();
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
