using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaControl.Services;
using Microsoft.Playwright;
using ClosedXML.Excel;
using Xceed.Words.NET;
using System.Data;

namespace CinemaControl;

public partial class ReportView
{
    private readonly IReportService _reportService;
    private string? _currentReportFolderPath;

    public ReportView(IReportService reportService)
    {
        InitializeComponent();
        _reportService = reportService;
        // Clear placeholder items
        DownloadedFilesListBox.Items.Clear();
        InitializeWebView();
        OpenFolderIcon.Visibility = Visibility.Collapsed;
    }

    private async void InitializeWebView()
    {
        await WebView.EnsureCoreWebView2Async(null);
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
        DownloadedFilesListBox.Items.Clear();
        _currentReportFolderPath = null;
        OpenFolderIcon.Visibility = Visibility.Collapsed;

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
            var page = await browser.NewPageAsync();
            _currentReportFolderPath = await _reportService.GenerateReportFiles(startDate.Value, endDate.Value, page);
                
            var filePaths = Directory.EnumerateFiles(_currentReportFolderPath);

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

    private void HideAllPreviewers()
    {
        WebView.Visibility = Visibility.Collapsed;
        ExcelDataGrid.Visibility = Visibility.Collapsed;
        WordScrollViewer.Visibility = Visibility.Collapsed;
    }

    private void DownloadedFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        HideAllPreviewers();
        if (DownloadedFilesListBox.SelectedItem is not ListBoxItem selectedItem) return;

        var filePath = selectedItem.Tag as string;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        try
        {
            switch (extension)
            {
                case ".pdf":
                    if (WebView?.CoreWebView2 != null)
                    {
                        WebView.Visibility = Visibility.Visible;
                        WebView.CoreWebView2.Navigate(filePath);
                    }
                    break;
                case ".docx":
                    var doc = DocX.Load(filePath);
                    WordTextBlock.Text = doc.Text;
                    WordScrollViewer.Visibility = Visibility.Visible;
                    break;
                case ".xlsx":
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null) return;

                        var dt = new DataTable();
                        // Создание колонок
                        foreach (var cell in worksheet.FirstRow().Cells())
                        {
                            dt.Columns.Add(cell.Value.ToString());
                        }

                        // Добавление строк
                        foreach (var row in worksheet.Rows().Skip(1))
                        {
                            var newRow = dt.NewRow();
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                newRow[i] = row.Cell(i + 1).Value.ToString();
                            }
                            dt.Rows.Add(newRow);
                        }
                        ExcelDataGrid.ItemsSource = dt.DefaultView;
                    }
                    ExcelDataGrid.Visibility = Visibility.Visible;
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить предпросмотр файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
}