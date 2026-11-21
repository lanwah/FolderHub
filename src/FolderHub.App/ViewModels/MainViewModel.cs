using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FolderHub.App.Services;
using FolderHub.Domain.Entities;
using FolderHub.UseCases.Persistence;

namespace FolderHub.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly AppSession _session;
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

    public MainViewModel(AppSession session)
    {
        _session = session;
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
        PinFolderCommand = new RelayCommand(_ =>
            StatusMessage = "固定到桌面卡片将在 WP7 实现");
        AddCategoryCommand = new RelayCommand(_ => RequestAddCategory?.Invoke());
        RenameCategoryCommand = new RelayCommand(
            _ => RequestRenameCategory?.Invoke(SelectedCategory!),
            _ => SelectedCategory is { IsVirtualAll: false, CategoryId: not null }
                 && SelectedCategory.CategoryId != _session.Catalog.Uncategorized.Id);
        DeleteCategoryCommand = new RelayCommand(
            _ => DeleteSelectedCategory(),
            _ => SelectedCategory is { IsVirtualAll: false, CategoryId: not null }
                 && SelectedCategory.CategoryId != _session.Catalog.Uncategorized.Id);
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

    public event Action? RequestAddCategory;
    public event Action<CategoryListItem>? RequestRenameCategory;
    public event Action<Guid?>? RequestEditFolder;
    public event Action? RequestOpenSettings;
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

    public MainSettingsSnapshot CurrentSettings => _session.Settings;

    public IReadOnlyList<Category> GetEditableCategories()
        => _session.Catalog.Categories.ToList();

    public FolderRecord? GetFolder(Guid id)
        => _session.Catalog.Folders.FirstOrDefault(f => f.Id == id);

    public IReadOnlyList<Guid> GetFolderCategoryIds(Guid folderId)
        => _session.Catalog.GetCategoriesForFolder(folderId).Select(c => c.Id).ToList();

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

    public bool TryAddCategory(string name, out string error)
    {
        error = string.Empty;
        try
        {
            _session.Catalog.AddCategory(name);
            PersistCatalog($"已创建分类「{name.Trim()}」");
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public bool TryRenameCategory(Guid categoryId, string name, out string error)
    {
        error = string.Empty;
        try
        {
            _session.Catalog.RenameCategory(categoryId, name);
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
                        description);
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
                _session.Catalog.UpdateFolder(existingId.Value, alias, description);
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
        Rect bounds)
    {
        opacity = Math.Clamp(opacity, 0.20, 1.0);
        var next = new MainSettingsSnapshot(
            KeepCardsAfterMainExit: keepCards,
            MainPinned: mainPinned,
            MainLeft: bounds.Left,
            MainTop: bounds.Top,
            MainWidth: Math.Max(360, bounds.Width),
            MainHeight: Math.Max(420, bounds.Height),
            MainLocked: mainLocked,
            MainOpacity: opacity,
            StartupEnabled: _session.Settings.StartupEnabled);

        _session.SaveSettings(next);
        ApplySettingsSnapshot(next);
        StatusMessage = mainPinned
            ? "设置已保存（固定到桌面将在 WP9 壁纸层实现）"
            : "设置已保存";
    }

    public void PersistWindowBounds(Rect bounds)
    {
        var s = _session.Settings;
        if (s.MainLocked)
            return;

        var next = s with
        {
            MainLeft = bounds.Left,
            MainTop = bounds.Top,
            MainWidth = Math.Max(360, bounds.Width),
            MainHeight = Math.Max(420, bounds.Height)
        };
        _session.SaveSettings(next);
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

    private void DeleteSelectedCategory()
    {
        if (SelectedCategory?.CategoryId is not Guid id)
            return;

        var name = SelectedCategory.Name;
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
            _session.Catalog.DeleteFolder(item.Id);
            PersistCatalog("记录已删除，磁盘目录不受影响");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "无法删除", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            Count = _session.Catalog.Folders.Count
        };
        Categories.Add(all);

        foreach (var category in _session.Catalog.Categories)
        {
            Categories.Add(new CategoryListItem(category.Id, category.Name)
            {
                Count = _session.Catalog.GetFoldersInCategory(category.Id).Count
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
                folder.IsPathUnavailable));
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

    private void ApplySettingsSnapshot(MainSettingsSnapshot settings)
    {
        MainPinned = settings.MainPinned;
        MainLocked = settings.MainLocked;
        KeepCardsAfterMainExit = settings.KeepCardsAfterMainExit;
        PanelOpacity = Math.Clamp(settings.MainOpacity, 0.20, 1.0);
    }

    private FolderListItem? ResolveFolder(object? parameter)
        => parameter as FolderListItem ?? SelectedFolder;
}
