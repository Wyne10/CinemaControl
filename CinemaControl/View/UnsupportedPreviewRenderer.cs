using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace CinemaControl.View;

public class UnsupportedPreviewRenderer(Label label) : IPreviewRenderer
{
    public void Render(string filePath)
    {
        label.Visibility = Visibility.Visible;
    }

    public void Hide()
    {
        label.Visibility =  Visibility.Collapsed;
    }
}