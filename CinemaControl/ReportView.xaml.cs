using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaControl.Services;
using Microsoft.Playwright;
using System.Runtime.CompilerServices;
using CinemaControl.View;

namespace CinemaControl;

public partial class ReportView : INotifyPropertyChanged
{
    private readonly IReportService _reportService;
    private readonly ImmutableDictionary<string, IPreviewRenderer> _previewRenderers;
    
    private ObservableCollection<ListBoxItem>? _reports;
    public ObservableCollection<ListBoxItem>? Reports
    {
        get => _reports;
        set
        {
            _reports = value;
            OnPropertyChanged();
        }
    }
    
    public ReportView(IReportService reportService)
    {
        InitializeComponent();
        DataContext = this;
        _reportService = reportService;
        _previewRenderers = new Dictionary<string, IPreviewRenderer>
        {
            {".pdf", new PdfPreviewRenderer(WebView) },
            {".xlsx", new ExcelPreviewRenderer(ExcelDataGrid) }
        }.ToImmutableDictionary();
        DownloadedFilesListBox.Items.Clear();
    }

    private List<ListBoxItem> GetCurrentReports()
    {
        if (FromDatePicker.SelectedDate == null || ToDatePicker.SelectedDate == null) return [];
        var reportsPath = _reportService.GetSessionPath(FromDatePicker.SelectedDate.Value, ToDatePicker.SelectedDate.Value);
        if (!Directory.Exists(reportsPath)) return [];
        var filePaths = Directory.EnumerateFiles(reportsPath);
        var items = filePaths.OrderBy(p => p).Select(path => new ListBoxItem { Content = Path.GetFileName(path), Tag = path }).ToList();
        return items;
    }
    
    private void OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        Reports = new ObservableCollection<ListBoxItem>(GetCurrentReports());
    }
    
    private void OpenReportDirectory(object sender, MouseButtonEventArgs e)
    {
        var reportsPathParent = Path.Combine(Path.GetTempPath(), ReportService.ReportsRootPath);
        if (FromDatePicker.SelectedDate != null && ToDatePicker.SelectedDate != null)
        {
            var reportsPath = _reportService.GetSessionPath(FromDatePicker.SelectedDate.Value, ToDatePicker.SelectedDate.Value);
            if (Directory.Exists(reportsPath))
                reportsPathParent = reportsPath;
        }
        
        try
        {
            Process.Start(new ProcessStartInfo(reportsPathParent) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось открыть папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void PreviewReport(object sender, SelectionChangedEventArgs e)
    {
        foreach (var previewRenderer in _previewRenderers.Values) previewRenderer.Hide();
        if (DownloadedFilesListBox.SelectedItem is not ListBoxItem selectedItem) return;

        var filePath = selectedItem.Tag as string;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        var extension = Path.GetExtension(filePath).ToLower();
        _previewRenderers[extension].Render(filePath);        
    }

    private void OpenReport(object sender, MouseButtonEventArgs e)
    {
        if (DownloadedFilesListBox.SelectedItem is not ListBoxItem selectedItem) return;
        var filePath = selectedItem.Tag as string;
        OpenFile(filePath);
    }

    private static void OpenFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        try
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void GenerateReport(object sender, RoutedEventArgs e)
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

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();
            _reportService.OnDownloadProgress += () => Reports = new ObservableCollection<ListBoxItem>(GetCurrentReports());
            await _reportService.GenerateReportFiles(startDate.Value, endDate.Value, page);
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
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

}