using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FolderHub.App.Services;
using FolderHub.App.Themes;
using FolderHub.Domain.Entities;
using FolderHub.Infrastructure.Ipc;
using FolderHub.UseCases.Display;
using FolderHub.UseCases.Ipc;
using FolderHub.UseCases.Persistence;
using ThemeId = FolderHub.UseCases.Persistence.UiTheme;

namespace FolderHub.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly AppSession _session;
    private readonly CardHostProcessManager _cardHost;
    private string _searchText = string.Empty;
    private string _statusMessage = "就绪";
    private CategoryListItem? _selectedCategory;
    private FolderListItem? _selectedFolder;
    private string _currentCategoryTitle = "全部文件夹";
    private string _currentCategorySubtitle = "所有常用位置，一处快速访问";
    private string _resultCountText = "0 个项目";
    private bool _isEmpty;
    private double _panelOpacity = 1.0;
    private bool _mainLocked;
    private bool _mainPinned;
    private bool _keepCardsAfterMainExit;
    private string _uiTheme = ThemeId.Light;
    private bool _isListMode;

    public MainViewModel(AppSession session, CardHostProcessManager cardHost)
    {
        _session = session;
        _cardHost = cardHost;
        Categories = new ObservableCollection<CategoryListItem>();
        Folders = new ObservableCollection<FolderListItem>();

        SelectCategoryCommand = new RelayCommand(p =>
        {
            if (p is CategoryListItem item)
                SelectCategory(item);
        });
        SelectFolderCommand = new RelayCommand(p =>
        {
            if (p is FolderListItem item)
                SelectFolder(item);
        });
        OpenFolderCommand = new RelayCommand(p => OpenFolder(ResolveFolder(p)));
        PinFolderCommand = new RelayCommand(
            p => _ = TogglePinAsync(ResolveFolder(p)),
            p => ResolveFolder(p) is not null);
        AddCategoryCommand = new RelayCommand(_ => RequestAddCategory?.Invoke());
        // 始终可点：选中「全部/未分类」时给出提示，避免 CanExecute=false 静默无响应。
        RenameCategoryCommand = new RelayCommand(_ => RenameSelectedCategory());
        DeleteCategoryCommand = new RelayCommand(_ => DeleteSelectedCategory());
        AddFolderCommand = new RelayCommand(_ => RequestEditFolder?.Invoke(null));
        EditFolderCommand = new RelayCommand(
            p =>
            {
                var folder = ResolveFolder(p);
                if (folder is not null)
                    RequestEditFolder?.Invoke(folder.Id);
            },
            p => ResolveFolder(p) is not null);
        DeleteFolderCommand = new RelayCommand(
            p => DeleteFolder(ResolveFolder(p)),
            p => ResolveFolder(p) is not null);
        OpenSettingsCommand = new RelayCommand(_ => RequestOpenSettings?.Invoke());
        OpenDonationCommand = new RelayCommand(_ => RequestOpenDonation?.Invoke());
        SetGridViewCommand = new RelayCommand(_ => SetListMode(false));
        SetListViewCommand = new RelayCommand(_ => SetListMode(true));

        ApplySettingsSnapshot(_session.Settings);
        RefreshAll(selectAll: true);
    }

    public ObservableCollection<CategoryListItem> Categories { get; }
    public ObservableCollection<FolderListItem> Folders { get; }

    public ICommand SelectCategoryCommand { get; }
    public ICommand SelectFolderCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand PinFolderCommand { get; }
    public ICommand AddCategoryCommand { get; }
    public ICommand RenameCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand AddFolderCommand { get; }
    public ICommand EditFolderCommand { get; }
    public ICommand DeleteFolderCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenDonationCommand { get; }
    public ICommand SetGridViewCommand { get; }
    public ICommand SetListViewCommand { get; }

    public event Action? RequestAddCategory;
    public event Action<CategoryListItem>? RequestRenameCategory;
    public event Action<Guid?>? RequestEditFolder;
    public event Action? RequestOpenSettings;
    public event Action? RequestOpenDonation;
    /// <summary>打开失败且路径不可用时，询问是否进入重新指定（FR-010）。</summary>
    public event Action<Guid, string>? RequestRetargetUnavailablePath;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                RefreshFolders();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string CurrentCategoryTitle
    {
        get => _currentCategoryTitle;
        private set => SetProperty(ref _currentCategoryTitle, value);
    }

    public string CurrentCategorySubtitle
    {
        get => _currentCategorySubtitle;
        private set => SetProperty(ref _currentCategorySubtitle, value);
    }

    public string ResultCountText
    {
        get => _resultCountText;
        private set => SetProperty(ref _resultCountText, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    public int TotalFolderCount => _session.Catalog.Folders.Count;

    public CategoryListItem? SelectedCategory
    {
        get => _selectedCategory;
        private set => SetProperty(ref _selectedCategory, value);
    }

    public FolderListItem? SelectedFolder
    {
        get => _selectedFolder;
        private set => SetProperty(ref _selectedFolder, value);
    }

    public double PanelOpacity
    {
        get => _panelOpacity;
        private set => SetProperty(ref _panelOpacity, value);
    }

    public bool MainLocked
    {
        get => _mainLocked;
        private set => SetProperty(ref _mainLocked, value);
    }

    public bool MainPinned
    {
        get => _mainPinned;
        private set => SetProperty(ref _mainPinned, value);
    }

    public bool KeepCardsAfterMainExit
    {
        get => _keepCardsAfterMainExit;
        private set => SetProperty(ref _keepCardsAfterMainExit, value);
    }

    public string UiTheme
    {
        get => _uiTheme;
        private set => SetProperty(ref _uiTheme, value);
    }

    /// <summary>false = 网格（demo 默认），true = 单列紧凑列表。</summary>
    /// <summary>仅在某一分分类视图且未搜索时可拖动排序文件夹。</summary>
    public bool CanReorderFolders =>
        string.IsNullOrWhiteSpace(SearchText)
        && SelectedCategory is { IsVirtualAll: false, CategoryId: not null };

    public bool IsListMode
    {
        get => _isListMode;
        private set
        {
            if (SetProperty(ref _isListMode, value))
                OnPropertyChanged(nameof(IsGridMode));
        }
    }

    public bool IsGridMode => !IsListMode;

    public MainSettingsSnapshot CurrentSettings => _session.Settings;

    public IReadOnlyList<Category> GetEditableCategories()
        => _session.Catalog.Categories.ToList();

    public FolderRecord? GetFolder(Guid id)
        => _session.Catalog.Folders.FirstOrDefault(f => f.Id == id);

    public IReadOnlyList<Guid> GetFolderCategoryIds(Guid folderId)
        => _session.Catalog.GetCategoriesForFolder(folderId).Select(c => c.Id).ToList();

    public int NormalizeCategoryDropIndex(int index)
    {
        if (index < 0 || Categories.Count == 0)
            return index;

        if (Categories[index].IsReorderable)
            return index;

        for (var i = index - 1; i >= 0; i--)
        {
            if (Categories[i].IsReorderable)
                return i;
        }

        for (var i = index + 1; i < Categories.Count; i++)
        {
            if (Categories[i].IsReorderable)
                return i;
        }

        return index;
    }

    public bool TryReorderCategories(int fromIndex, int toIndex)
    {
        toIndex = NormalizeCategoryDropIndex(toIndex);
        if (!IsCategoryIndexReorderable(fromIndex) || !IsCategoryIndexReorderable(toIndex) || fromIndex == toIndex)
            return false;

        try
        {
            MoveListItem(Categories, fromIndex, toIndex);
            var orderedIds = Categories
                .Where(c => c.CategoryId is Guid)
                .Select(c => c.CategoryId!.Value)
                .ToList();
            _session.Catalog.ReorderCategories(orderedIds);
            PersistCatalog("分类顺序已更新");
            return true;
        }
        catch (Exception ex)
        {
            var selectedId = SelectedCategory?.CategoryId;
            var wasAll = SelectedCategory?.IsVirtualAll == true;
            RefreshAll(selectAll: wasAll, preferCategoryId: selectedId);
            StatusMessage = $"无法更新分类顺序：{ex.Message}";
            return false;
        }
    }

    public bool TryReorderFolders(int fromIndex, int toIndex)
    {
        if (!CanReorderFolders
            || SelectedCategory?.CategoryId is not Guid categoryId
            || fromIndex < 0
            || toIndex < 0
            || fromIndex >= Folders.Count
            || toIndex >= Folders.Count
            || fromIndex == toIndex)
        {
            return false;
        }

        try
        {
            MoveListItem(Folders, fromIndex, toIndex);
            var orderedIds = Folders.Select(f => f.Id).ToList();
            _session.Catalog.ReorderFoldersInCategory(categoryId, orderedIds);
            PersistCatalog("文件夹顺序已更新");
            return true;
        }
        catch (Exception ex)
        {
            RefreshFolders();
            StatusMessage = $"无法更新文件夹顺序：{ex.Message}";
            return false;
        }
    }

    public void SelectCategory(CategoryListItem item)
    {
        foreach (var c in Categories)
            c.IsSelected = ReferenceEquals(c, item);

        SelectedCategory = item;
        CurrentCategoryTitle = item.Name;
        CurrentCategorySubtitle = item.IsVirtualAll
            ? "所有常用位置，一处快速访问"
            : $"{item.Name} 分类中的文件夹";
        RefreshFolders();
        CommandManager.InvalidateRequerySuggested();
    }

    public void SelectFolder(FolderListItem item)
    {
        foreach (var f in Folders)
            f.IsSelected = f.Id == item.Id;
        SelectedFolder = item;
        CommandManager.InvalidateRequerySuggested();
    }

    public bool TryAddCategory(string name, string? icon, out string error)
    {
        error = string.Empty;
        try
        {
            _session.Catalog.AddCategory(name, icon);
            PersistCatalog($"已创建分类「{name.Trim()}」");
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool TryRenameCategory(Guid categoryId, string name, string? icon, out string error)
    {
        error = string.Empty;
        try
        {
            _session.Catalog.RenameCategory(categoryId, name);
            _session.Catalog.SetCategoryIcon(categoryId, icon);
            PersistCatalog($"已重命名分类「{name.Trim()}」");
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool TrySaveFolder(
        Guid? existingId,
        string displayName,
        string? alias,
        string path,
        string? description,
        IReadOnlyList<Guid> categoryIds,
        string? icon,
        out string error)
    {
        error = string.Empty;
        try
        {
            var cats = categoryIds.Count == 0
                ? new[] { _session.Catalog.Uncategorized.Id }
                : categoryIds.ToArray();

            if (existingId is null)
            {
                // 添加流：按分类增量 AddFolder（创建 / 关联 / 已存在），禁止 SetFolderCategories 覆盖既有多归属（FR-018）。
                var kinds = new List<Domain.Catalog.AddFolderResult.Kind>(cats.Length);
                foreach (var categoryId in cats)
                {
                    var result = _session.Catalog.AddFolder(
                        path,
                        displayName,
                        categoryId,
                        alias,
                        description,
                        icon);
                    kinds.Add(result.ResultKind);
                }

                if (kinds.TrueForAll(k => k == Domain.Catalog.AddFolderResult.Kind.AlreadyLinked))
                {
                    error = "该路径已在所选分类中存在，未创建重复记录。";
                    return false;
                }

                PersistCatalog(kinds.Contains(Domain.Catalog.AddFolderResult.Kind.Created)
                    ? "文件夹记录已添加"
                    : "已将已有路径关联到所选分类");
            }
            else
            {
                // 编辑流：用户显式设定完整分类集合。
                _session.Catalog.RetargetFolderPath(existingId.Value, path, displayName);
                _session.Catalog.UpdateFolder(existingId.Value, alias, description, icon);
                _session.Catalog.SetFolderCategories(existingId.Value, cats);
                PersistCatalog("文件夹记录已更新");
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 从资源管理器拖入的目录路径批量添加（FR-009）。无效项跳过并汇总提示。
    /// </summary>
    public bool TryAddDroppedFolders(IReadOnlyList<string> paths, out string message)
    {
        message = string.Empty;
        if (paths.Count == 0)
        {
            message = "未检测到可添加的文件夹。";
            return false;
        }

        var categoryId = SelectedCategory is { IsVirtualAll: false, CategoryId: Guid catId }
            ? catId
            : _session.Catalog.Uncategorized.Id;

        var created = 0;
        var linked = 0;
        var skipped = 0;
        var invalid = 0;

        try
        {
            foreach (var raw in paths)
            {
                if (!_session.Shell.IsDirectoryAccessible(raw))
                {
                    invalid++;
                    continue;
                }

                var displayName = new DirectoryInfo(raw.Trim()).Name;
                if (string.IsNullOrWhiteSpace(displayName))
                    displayName = raw.Trim();

                var result = _session.Catalog.AddFolder(raw, displayName, categoryId);
                switch (result.ResultKind)
                {
                    case Domain.Catalog.AddFolderResult.Kind.Created:
                        created++;
                        break;
                    case Domain.Catalog.AddFolderResult.Kind.LinkedExisting:
                        linked++;
                        break;
                    default:
                        skipped++;
                        break;
                }
            }

            if (created + linked == 0)
            {
                message = invalid > 0
                    ? "拖入项不是有效文件夹，或路径已全部存在于当前分类。"
                    : "拖入路径已全部存在于当前分类，未创建重复记录。";
                return false;
            }

            PersistCatalog($"已从拖拽添加 {created + linked} 项"
                + (invalid > 0 ? $"（跳过 {invalid} 个无效路径）" : string.Empty)
                + (skipped > 0 ? $"（{skipped} 项已在分类中）" : string.Empty));
            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    public void ApplyAndSaveSettings(
        bool mainPinned,
        bool mainLocked,
        bool keepCards,
        double opacity,
        bool startupEnabled,
        string uiTheme,
        Rect bounds)
    {
        opacity = Math.Clamp(opacity, 0.20, 1.0);
        var next = BuildSettingsWithPlacement(
            keepCards,
            mainPinned,
            mainLocked,
            opacity,
            startupEnabled,
            uiTheme,
            bounds);

        var startupOk = _session.SaveSettingsWithStartup(next, out var startupError);
        ApplySettingsSnapshot(next);
        var wallpaperError = ApplyMainPinnedVisual?.Invoke(mainPinned);
        _ = _cardHost.NotifyThemeAsync(next.UiTheme);

        if (!startupOk)
        {
            StatusMessage = $"设置已保存，但开机启动同步失败：{startupError}";
            return;
        }

        if (mainPinned && wallpaperError is not null)
            StatusMessage = "设置已保存；壁纸层嵌入失败，已回退普通窗口：" + wallpaperError;
        else if (mainPinned)
            StatusMessage = "设置已保存（已嵌入桌面壁纸层）";
        else
            StatusMessage = "设置已保存";
    }

    public void ToggleTheme()
    {
        var nextTheme = ThemeId.IsDark(_uiTheme) ? ThemeId.Light : ThemeId.Dark;
        var s = _session.Settings;
        var next = s with { UiTheme = nextTheme };
        _session.SaveSettings(next);
        ApplySettingsSnapshot(next);
        StatusMessage = nextTheme == ThemeId.Dark ? "已切换到深色主题" : "已切换到浅色主题";
        _ = _cardHost.NotifyThemeAsync(nextTheme);
    }

    public void SetListMode(bool listMode)
    {
        if (IsListMode == listMode)
            return;
        IsListMode = listMode;
        StatusMessage = listMode ? "已切换为紧凑列表布局" : "已切换为网格布局";
    }

    public void PersistWindowBounds(Rect bounds)
    {
        var s = _session.Settings;
        if (s.MainLocked)
            return;

        var next = BuildSettingsWithPlacement(
            s.KeepCardsAfterMainExit,
            s.MainPinned,
            s.MainLocked,
            s.MainOpacity,
            s.StartupEnabled,
            s.UiTheme,
            bounds);
        _session.SaveSettings(next);
    }

    /// <summary>
    /// 设置中切换「固定到桌面」后，由窗口层应用壁纸层；返回失败说明（成功 null）。
    /// </summary>
    public event Func<bool, string?>? ApplyMainPinnedVisual;

    /// <summary>按 ADR-040 将持久化主组件位置恢复为当前 DIP 绝对坐标。</summary>
    public AbsoluteWindowPlacement ResolveMainPlacement(double width, double height)
    {
        var s = _session.Settings;
        var monitors = _session.Displays.GetWorkAreas();
        return DisplayPlacement.Restore(
            s.MainMonitorReference,
            s.MainLeft,
            s.MainTop,
            width,
            height,
            s.MainDpiReference,
            monitors,
            _session.Displays.PrimaryMonitorId);
    }

    private MainSettingsSnapshot BuildSettingsWithPlacement(
        bool keepCards,
        bool mainPinned,
        bool mainLocked,
        double opacity,
        bool startupEnabled,
        string uiTheme,
        Rect bounds)
    {
        var width = Math.Max(360, bounds.Width);
        var height = Math.Max(420, bounds.Height);
        var monitors = _session.Displays.GetWorkAreas();
        var captured = DisplayPlacement.Capture(bounds.Left, bounds.Top, width, height, monitors);

        return new MainSettingsSnapshot(
            KeepCardsAfterMainExit: keepCards,
            MainPinned: mainPinned,
            MainLeft: captured.NormX,
            MainTop: captured.NormY,
            MainWidth: width,
            MainHeight: height,
            MainLocked: mainLocked,
            MainOpacity: opacity,
            StartupEnabled: startupEnabled,
            MainMonitorReference: captured.MonitorId,
            MainDpiReference: captured.DpiReference,
            UiTheme: ThemeId.Normalize(uiTheme));
    }

    private void OpenFolder(FolderListItem? item)
    {
        if (item is null)
            return;

        // BR-010：主要在打开时检查路径；不可用则保留记录并标记异常（FR-010 / BR-009）。
        if (!_session.Shell.IsDirectoryAccessible(item.Path))
        {
            _session.Catalog.MarkFolderPathUnavailable(item.Id);
            PersistCatalog($"路径不可用，已保留记录并标记异常：{item.Path}");
            RequestRetargetUnavailablePath?.Invoke(item.Id, item.Path);
            return;
        }

        var record = _session.Catalog.Folders.FirstOrDefault(f => f.Id == item.Id);
        if (record?.IsPathUnavailable == true)
        {
            _session.Catalog.MarkFolderPathAvailable(item.Id);
            _session.SaveCatalog();
            RefreshFolders();
        }

        if (_session.Shell.TryOpenInExplorer(item.Path, out var error))
        {
            StatusMessage = $"已在资源管理器中打开：{item.Path}";
            return;
        }

        StatusMessage = $"无法打开文件夹：{error}";
        MessageBox.Show(error ?? "无法打开文件夹。", "打开失败", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void RenameSelectedCategory()
    {
        if (!TryGetEditableCategory(out var item, out var error))
        {
            MessageBox.Show(error, "无法编辑分类", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        RequestRenameCategory?.Invoke(item);
    }

    private void DeleteSelectedCategory()
    {
        if (!TryGetEditableCategory(out var item, out var error)
            || item.CategoryId is not Guid id)
        {
            MessageBox.Show(error, "无法删除分类", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = item.Name;
        var confirm = MessageBox.Show(
            $"删除分类「{name}」？\n其下文件夹将保留并在失去其他归属时回到「未分类」。",
            "删除分类",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            _session.Catalog.DeleteCategory(id);
            PersistCatalog($"已删除分类「{name}」", selectAll: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "无法删除分类", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>「全部文件夹」与内建「未分类」不可编辑/删除。</summary>
    private bool TryGetEditableCategory(out CategoryListItem item, out string error)
    {
        error = string.Empty;
        item = null!;

        if (SelectedCategory is null)
        {
            error = "请先在左侧选择一个分类。";
            return false;
        }

        if (SelectedCategory.IsVirtualAll || SelectedCategory.CategoryId is null)
        {
            error = "「全部文件夹」不可编辑，请先在左侧选择一个自定义分类。";
            return false;
        }

        if (SelectedCategory.CategoryId == _session.Catalog.Uncategorized.Id)
        {
            error = "内建「未分类」不可编辑或删除。";
            return false;
        }

        item = SelectedCategory;
        return true;
    }

    private void DeleteFolder(FolderListItem? item)
    {
        if (item is null)
            return;

        var confirm = MessageBox.Show(
            $"删除 FolderHub 中的「{item.Title}」记录？\n仅删除应用登记，不删除磁盘真实目录。",
            "删除记录",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            var cards = _session.CardDefinitions.LoadAll();
            var removedCard = cards.FirstOrDefault(c => c.FolderId == item.Id);
            _session.Catalog.DeleteFolder(item.Id);
            PersistCatalog("记录已删除，磁盘目录不受影响");
            if (removedCard is not null)
                _ = _cardHost.CloseCardAsync(removedCard.CardId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "无法删除", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public void HandleCardNotice(CardStateChangedNoticePayload notice)
    {
        if (!string.Equals(notice.Action, "closeRequested", StringComparison.OrdinalIgnoreCase))
            return;

        var cards = _session.CardDefinitions.LoadAll().ToList();
        var next = cards.Where(c => c.CardId != notice.CardId).ToList();
        if (next.Count != cards.Count)
            _session.CardDefinitions.ReplaceAll(next);

        CardHostPendingClosesFile.Remove(notice.CardId);
        StatusMessage = "已取消桌面卡片固定";
        RefreshFolders();
    }

    private async Task TogglePinAsync(FolderListItem? item)
    {
        if (item is null)
            return;

        var cardsBefore = _session.CardDefinitions.LoadAll().ToList();
        try
        {
            var existing = cardsBefore.FirstOrDefault(c => c.FolderId == item.Id);

            if (existing is { Enabled: true })
            {
                // 取消固定：移除定义（展示态随外键级联删除）。
                var next = cardsBefore.Where(c => c.FolderId != item.Id).ToList();
                _session.CardDefinitions.ReplaceAll(next);
                await _cardHost.CloseCardAsync(existing.CardId);
                StatusMessage = $"已取消固定「{item.Title}」";
                RefreshFolders();
                return;
            }

            var cardId = existing?.CardId ?? Guid.NewGuid();
            var definition = new CardDefinitionSnapshot(cardId, item.Id, Enabled: true);
            var merged = cardsBefore.Where(c => c.FolderId != item.Id).Append(definition).ToList();
            _session.CardDefinitions.ReplaceAll(merged);

            var payload = _cardHost.BuildUpsertPayload(definition);
            if (payload is null)
            {
                _session.CardDefinitions.ReplaceAll(cardsBefore);
                StatusMessage = "无法固定：找不到文件夹记录。";
                RefreshFolders();
                return;
            }

            await _cardHost.PushDefinitionAsync(payload);
            StatusMessage = $"已固定到桌面卡片：「{item.Title}」";
            RefreshFolders();
        }
        catch (Exception ex)
        {
            // IPC/启动失败回滚定义，避免列表显示已固定但桌面无卡。
            try
            {
                _session.CardDefinitions.ReplaceAll(cardsBefore);
            }
            catch
            {
                // ignore rollback failure
            }

            RefreshFolders();
            StatusMessage = $"固定卡片失败：{ex.Message}";
            MessageBox.Show(ex.Message, "固定到桌面", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void PersistCatalog(string status, bool selectAll = false)
    {
        _session.SaveCatalog();
        StatusMessage = status;
        var selectedId = selectAll ? null : SelectedCategory?.CategoryId;
        var wasAll = selectAll || SelectedCategory?.IsVirtualAll == true;
        RefreshAll(selectAll: wasAll, preferCategoryId: selectedId);
        OnPropertyChanged(nameof(TotalFolderCount));
    }

    private void RefreshAll(bool selectAll, Guid? preferCategoryId = null)
    {
        Categories.Clear();
        var all = new CategoryListItem(null, "全部文件夹", isVirtualAll: true)
        {
            Count = _session.Catalog.Folders.Count,
            IsReorderable = false
        };
        Categories.Add(all);

        foreach (var category in _session.Catalog.Categories)
        {
            Categories.Add(new CategoryListItem(category.Id, category.Name, iconFileName: category.Icon)
            {
                Count = _session.Catalog.GetFoldersInCategory(category.Id).Count,
                IsReorderable = !category.IsUncategorized
            });
        }

        CategoryListItem target;
        if (selectAll || preferCategoryId is null)
            target = all;
        else
            target = Categories.FirstOrDefault(c => c.CategoryId == preferCategoryId) ?? all;

        SelectCategory(target);
    }

    private void RefreshFolders()
    {
        var selectedId = SelectedFolder?.Id;
        Folders.Clear();

        IEnumerable<FolderRecord> source;
        var keyword = SearchText;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            source = _session.Catalog.Search(keyword);
        }
        else if (SelectedCategory is null || SelectedCategory.IsVirtualAll)
        {
            source = _session.Catalog.Folders;
        }
        else if (SelectedCategory.CategoryId is Guid catId)
        {
            source = _session.Catalog.GetFoldersInCategory(catId);
        }
        else
        {
            source = Array.Empty<FolderRecord>();
        }

        var pinnedFolderIds = _session.CardDefinitions.LoadAll()
            .Where(c => c.Enabled)
            .Select(c => c.FolderId)
            .ToHashSet();

        foreach (var folder in source)
        {
            var cats = _session.Catalog.GetCategoriesForFolder(folder.Id);
            var catsLabel = string.Join(" / ", cats.Select(c => c.Name));
            var subtitle = string.IsNullOrEmpty(folder.Alias)
                ? $"文件夹 · {catsLabel}"
                : $"{folder.DisplayName} · {catsLabel}";

            Folders.Add(new FolderListItem(
                folder.Id,
                folder.ListTitle,
                subtitle,
                string.IsNullOrWhiteSpace(folder.Description) ? "暂无用途说明" : folder.Description!,
                folder.Path,
                catsLabel,
                folder.IsPathUnavailable,
                isPinned: pinnedFolderIds.Contains(folder.Id),
                icon: folder.Icon));
        }

        ResultCountText = $"{Folders.Count} 个项目";
        IsEmpty = Folders.Count == 0;

        if (selectedId is Guid id)
        {
            var match = Folders.FirstOrDefault(f => f.Id == id);
            if (match is not null)
                SelectFolder(match);
            else
                SelectedFolder = null;
        }
        else
        {
            SelectedFolder = null;
        }

        foreach (var category in Categories)
        {
            category.Count = category.IsVirtualAll
                ? _session.Catalog.Folders.Count
                : category.CategoryId is Guid cid
                    ? _session.Catalog.GetFoldersInCategory(cid).Count
                    : 0;
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private static bool IsCategoryIndexReorderable(int index, ObservableCollection<CategoryListItem> categories)
    {
        if (index < 0 || index >= categories.Count)
            return false;

        return categories[index].IsReorderable;
    }

    private bool IsCategoryIndexReorderable(int index)
        => IsCategoryIndexReorderable(index, Categories);

    private static void MoveListItem<T>(ObservableCollection<T> items, int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex)
            return;

        var item = items[fromIndex];
        items.RemoveAt(fromIndex);
        var insertIndex = toIndex;
        if (toIndex > fromIndex)
            insertIndex--;
        items.Insert(insertIndex, item);
    }

    private void ApplySettingsSnapshot(MainSettingsSnapshot settings)
    {
        // 先换主题字典再改 UiTheme：主窗 ApplyChrome 依赖 AppTheme.Current 的 panel RGB。
        var theme = ThemeId.Normalize(settings.UiTheme);
        AppTheme.Apply(theme);

        MainPinned = settings.MainPinned;
        MainLocked = settings.MainLocked;
        KeepCardsAfterMainExit = settings.KeepCardsAfterMainExit;
        PanelOpacity = Math.Clamp(settings.MainOpacity, 0.20, 1.0);
        UiTheme = theme;
    }

    private FolderListItem? ResolveFolder(object? parameter)
        => parameter as FolderListItem ?? SelectedFolder;
}
