using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FolderHub.App.Dialogs;

public partial class DonationDialog : Window
{
    public DonationDialog()
    {
        InitializeComponent();
        WeixinImg.Source = TryLoadDonationImage("weixin.jpg");
        AlipayImg.Source = TryLoadDonationImage("alipay.jpg");
        ContentRendered += (_, _) => CenterOnOwner();
    }

    private void CenterOnOwner()
    {
        if (Owner is not Window owner)
            return;

        UpdateLayout();
        var w = ActualWidth > 0 ? ActualWidth : Width;
        var h = ActualHeight > 0 ? ActualHeight : Height;
        Left = owner.Left + (owner.Width - w) / 2;
        Top = owner.Top + (owner.Height - h) / 2;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private static BitmapImage? TryLoadDonationImage(string fileName)
    {
        var full = Path.Combine(AppContext.BaseDirectory, "donation", fileName);
        if (!File.Exists(full))
            return null;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(full, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}