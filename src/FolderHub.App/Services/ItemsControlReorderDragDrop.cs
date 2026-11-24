using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FolderHub.App.Services;

/// <summary>
/// ItemsControl 内项的拖拽重排（按住左键拖动，越过阈值后启动拖放）。
/// </summary>
public sealed class ItemsControlReorderDragDrop
{
    public const string CategoryPayloadFormat = "FolderHub.CategoryReorder";
    public const string FolderPayloadFormat = "FolderHub.FolderReorder";

    private readonly ItemsControl _itemsControl;
    private readonly string _payloadFormat;
    private readonly Func<int, bool> _canDragIndex;
    private readonly Func<int, bool> _canDropIndex;
    private readonly Action<int, int> _onReordered;
    private readonly Func<int, int>? _normalizeDropIndex;

    private Point _pressPoint;
    private int _sourceIndex = -1;
    private bool _dragStarted;

    public ItemsControlReorderDragDrop(
        ItemsControl itemsControl,
        string payloadFormat,
        Func<int, bool> canDragIndex,
        Func<int, bool> canDropIndex,
        Action<int, int> onReordered,
        Func<int, int>? normalizeDropIndex = null)
    {
        _itemsControl = itemsControl;
        _payloadFormat = payloadFormat;
        _canDragIndex = canDragIndex;
        _canDropIndex = canDropIndex;
        _onReordered = onReordered;
        _normalizeDropIndex = normalizeDropIndex;

        _itemsControl.AllowDrop = true;
        _itemsControl.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        // handledEventsToo：在分类行 Button 上移动时仍能检测拖动阈值，且不在按下时 CaptureMouse（避免抢走点击选中）
        _itemsControl.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(OnPreviewMouseMove), true);
        _itemsControl.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        _itemsControl.DragOver += OnDragOver;
        _itemsControl.Drop += OnDrop;
        _itemsControl.DragLeave += OnDragLeave;
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var index = IndexFromSource(e.OriginalSource as DependencyObject);
        if (index < 0 || !_canDragIndex(index))
            return;

        if (IsNestedInteractiveControl(e.OriginalSource as DependencyObject, index))
            return;

        _sourceIndex = index;
        _pressPoint = e.GetPosition(_itemsControl);
        _dragStarted = false;
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _sourceIndex < 0 || _dragStarted)
            return;

        var pos = e.GetPosition(_itemsControl);
        if (!ExceedsDragThreshold(_pressPoint, pos))
            return;

        _dragStarted = true;
        var data = new DataObject(_payloadFormat, _sourceIndex);
        try
        {
            DragDrop.DoDragDrop(_itemsControl, data, DragDropEffects.Move);
        }
        finally
        {
            _sourceIndex = -1;
            _dragStarted = false;
            ClearDropAdorners();
        }
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _sourceIndex = -1;
        _dragStarted = false;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(_payloadFormat))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        if (e.Data.GetData(_payloadFormat) is not int sourceIndex)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var targetIndex = ResolveTargetIndex(e.GetPosition(_itemsControl));
        if (targetIndex < 0 || !_canDropIndex(targetIndex) || sourceIndex == targetIndex)
        {
            e.Effects = DragDropEffects.None;
            ClearDropAdorners();
        }
        else
        {
            e.Effects = DragDropEffects.Move;
            HighlightDropTarget(targetIndex);
        }

        e.Handled = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        ClearDropAdorners();
        if (!e.Data.GetDataPresent(_payloadFormat)
            || e.Data.GetData(_payloadFormat) is not int sourceIndex)
        {
            return;
        }

        var targetIndex = ResolveTargetIndex(e.GetPosition(_itemsControl));
        if (targetIndex < 0 || !_canDropIndex(targetIndex) || sourceIndex == targetIndex)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        _onReordered(sourceIndex, targetIndex);
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e)
        => ClearDropAdorners();

    private int IndexFromSource(DependencyObject? source)
    {
        if (source is null)
            return -1;

        var container = ItemsControl.ContainerFromElement(_itemsControl, source);
        if (container is null)
            return -1;

        var index = _itemsControl.ItemContainerGenerator.IndexFromContainer(container);
        return index >= 0 && index < _itemsControl.Items.Count ? index : -1;
    }

    private int ResolveTargetIndex(Point position)
    {
        var index = TargetIndex(position);
        return _normalizeDropIndex is null ? index : _normalizeDropIndex(index);
    }

    private int TargetIndex(Point position)
    {
        for (var i = 0; i < _itemsControl.Items.Count; i++)
        {
            var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as UIElement;
            if (container is null)
                continue;

            var topLeft = container.TranslatePoint(new Point(0, 0), _itemsControl);
            var bounds = new Rect(topLeft, container.RenderSize);
            if (position.Y < bounds.Bottom)
                return position.Y < bounds.Top + bounds.Height / 2 ? i : Math.Min(i + 1, _itemsControl.Items.Count - 1);
        }

        return _itemsControl.Items.Count - 1;
    }

    private static bool ExceedsDragThreshold(Point start, Point current)
    {
        var min = Math.Max(SystemParameters.MinimumHorizontalDragDistance, SystemParameters.MinimumVerticalDragDistance);
        return Math.Abs(current.X - start.X) >= min || Math.Abs(current.Y - start.Y) >= min;
    }

    /// <summary>
    /// 跳过项内嵌套的交互控件（如卡片上的固定/菜单按钮），允许项根上的 Button（分类行）。
    /// </summary>
    private bool IsNestedInteractiveControl(DependencyObject? source, int itemIndex)
    {
        if (source is null)
            return false;

        var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(itemIndex);
        if (container is null)
            return false;

        DependencyObject? templateRoot = container;
        if (container is ContentPresenter && VisualTreeHelper.GetChildrenCount(container) > 0)
            templateRoot = VisualTreeHelper.GetChild(container, 0);

        while (source is not null && source != container)
        {
            if (source is Button or TextBox or ScrollBar or Thumb)
                return source != templateRoot;

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private void HighlightDropTarget(int index)
    {
        ClearDropAdorners();
        var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
        if (container is null)
            return;

        container.Opacity = 0.55;
        container.Tag = "folderhub-reorder-drop";
    }

    private void ClearDropAdorners()
    {
        for (var i = 0; i < _itemsControl.Items.Count; i++)
        {
            var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
            if (container is null || !Equals(container.Tag, "folderhub-reorder-drop"))
                continue;

            container.Opacity = 1;
            container.ClearValue(UIElement.OpacityProperty);
            container.Tag = null;
        }
    }
}