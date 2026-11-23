using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FolderHub.CardHost.Services;

namespace FolderHub.CardHost.Views;

public partial class CardWindow : Window
{
    private readonly CardHostController _controller;
    private Point _dragStart;
    private Point _windowOrigin;
    private bool _dragging;
    private bool _moved;

    public CardWindow(CardHostController controller, CardModel model)
    {
        _controller = controller;
        Model = model;
        InitializeComponent();
        ApplyModel();
        Left = model.Left;
        Top = model.Top;
        ContextMenu = (ContextMenu)FindResource("CardMenu");
    }

    public CardModel Model { get; }

    public void ApplyModel()
    {
        TitleText.Text = Model.Title;
        DescText.Text = string.IsNullOrWhiteSpace(Model.Description) ? "暂无用途说明" : Model.Description;
        HintText.Text = Model.Hint;
        LockShacklePath.Data = Geometry.Parse(Model.Locked
            ? "M5,7.25 L5,5 A3,3 0 0 1 11,5 L11,7.25"
            : "M5,7.25 L5,5 A3,3 0 0 1 10.7,3.65");
        LockButton.ToolTip = Model.Locked ? "解锁位置" : "锁定位置";
        LockButton.SetResourceReference(Control.ForegroundProperty,
            Model.Locked ? "AccentBrush" : "InkSoftBrush");
        RootBorder.Cursor = Model.Locked ? Cursors.Arrow : Cursors.SizeAll;
        if (Model.IsPathUnavailable)
            HintText.Text = "路径异常 · 请在主程序中重新指定";

        IconToneBorder.Background = Model.IconToneBrush;
        IconGlyphText.Text = Model.IconGlyph;
        if (Model.IconImage is { } image)
        {
            CustomIconImage.Source = image;
            CustomIconImage.Visibility = Visibility.Visible;
            IconGlyphText.Visibility = Visibility.Collapsed;
        }
        else
        {
            CustomIconImage.Source = null;
            CustomIconImage.Visibility = Visibility.Collapsed;
            IconGlyphText.Visibility = Visibility.Visible;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.Button)
            return;
        if (Model.Locked)
            return;

        _dragging = true;
        _moved = false;
        _dragStart = PointToScreen(e.GetPosition(this));
        _windowOrigin = new Point(Left, Top);
        CaptureMouse();
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging || Model.Locked)
            return;

        var current = PointToScreen(e.GetPosition(this));
        var dx = current.X - _dragStart.X;
        var dy = current.Y - _dragStart.Y;
        if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2)
            _moved = true;

        // 允许负虚拟屏坐标（多显示器左侧/上方扩展屏）。
        Left = _windowOrigin.X + dx;
        Top = _windowOrigin.Y + dy;
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
            return;

        _dragging = false;
        ReleaseMouseCapture();
        if (_moved)
            _controller.PersistPresentation(this);
    }

    private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_moved)
            return;
        _controller.OpenFolder(Model);
    }

    private void Window_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    {
        // 使用 XAML 资源菜单。
    }

    private void Open_Click(object sender, RoutedEventArgs e)
        => _controller.OpenFolder(Model);

    private void Lock_Click(object sender, RoutedEventArgs e)
    {
        Model.Locked = !Model.Locked;
        ApplyModel();
        _controller.PersistPresentation(this);
    }

    private void CloseCard_Click(object sender, RoutedEventArgs e)
        => _controller.RequestUnpin(Model.CardId);
}
