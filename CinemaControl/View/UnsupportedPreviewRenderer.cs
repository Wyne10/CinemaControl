using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace CinemaControl.View;

public class UnsupportedPreviewRenderer(TextBox textBox) : IPreviewRenderer
{
    public void Render(string filePath)
    {
        textBox.Visibility = Visibility.Visible;
    }

    public void Hide()
    {
        textBox.Visibility =  Visibility.Collapsed;
    }
}