using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaControl.Services.Weekly;
using Microsoft.Playwright;

namespace CinemaControl;

public partial class WeeklyReportView
{
    private readonly IWeeklyReportService _reportService;
    private string? _currentReportFolderPath;

    public WeeklyReportView()
    {
        InitializeComponent();
        _reportService = new CompositeWeeklyReportService([new WeeklyRentalsReportService(ReportProgressBar), new WeeklyCashierReportService(ReportProgressBar), new WeeklyCardReportService(ReportProgressBar)]);
        // Clear placeholder items
        DownloadedFilesListBox.Items.Clear();
        InitializeWebView();
        OpenFolderIcon.Visibility = Visibility.Collapsed;
    }

    private async void InitializeWebView()
    {
        await PdfViewer.EnsureCoreWebView2Async(null);
    }

    private async void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        var startDate = FromDatePicker.SelectedDate;
        var endDate = ToDatePicker.SelectedDate;

        if (startDate == null || endDate == null)
        {
            MessageBox.Show("Пожалуйста, выберите начальную и конечную даты.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (startDate > endDate)
        {
            MessageBox.Show("Начальная дата не может быть позже конечной.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        IsEnabled = false;
        ReportProgressBar.Maximum = _reportService.GetFilesCount(startDate.Value, endDate.Value);
        ProgressPanel.Visibility = Visibility.Visible;
        DownloadedFilesListBox.Items.Clear();
        _currentReportFolderPath = null;
        OpenFolderIcon.Visibility = Visibility.Collapsed;

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();
            _currentReportFolderPath = await _reportService.GenerateReportFiles(startDate.Value, endDate.Value, page);
                
            var filePaths = Directory.EnumerateFiles(_currentReportFolderPath, "*.pdf");

            foreach (var path in filePaths.OrderBy(p => p))
            {
                var item = new ListBoxItem
                {
                    Content = Path.GetFileName(path),
                    Tag = path // Store full path in Tag
                };
                DownloadedFilesListBox.Items.Add(item);
            }

            if (DownloadedFilesListBox.Items.Count > 0)
            {
                OpenFolderIcon.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Произошла ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsEnabled = true;
            ProgressPanel.Visibility = Visibility.Collapsed;
            ReportProgressBar.Value = 0;
            ProgressText.Text = "Загрузка...";
        }
    }
        
    private void OpenReportFolder_Click(object sender, MouseButtonEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentReportFolderPath) && Directory.Exists(_currentReportFolderPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo(_currentReportFolderPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DownloadedFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DownloadedFilesListBox.SelectedItem is ListBoxItem selectedItem)
        {
            var filePath = selectedItem.Tag as string;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath) && PdfViewer?.CoreWebView2 != null)
            {
                PdfViewer.CoreWebView2.Navigate(filePath);
            }
        }
    }

    private void DownloadedFilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DownloadedFilesListBox.SelectedItem is ListBoxItem selectedItem)
        {
            var filePath = selectedItem.Tag as string;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void ReportProgressBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        ProgressText.Text = $"Загрузка {ReportProgressBar.Value}/{ReportProgressBar.Maximum}";
    }
}