using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace CinemaControl.View;

public class PdfPreviewRenderer(WebView2 webView) : IPreviewRenderer
{
    public void Render(string filePath)
    {
        webView.Visibility = Visibility.Visible;
        webView.CoreWebView2.Navigate(filePath);
    }

    public void Hide()
    {
        webView.Visibility =  Visibility.Collapsed;
    }
}