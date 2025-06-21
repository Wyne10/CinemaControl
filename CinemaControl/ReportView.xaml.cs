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
using Microsoft.Extensions.Logging;

namespace CinemaControl;

public partial class ReportView : INotifyPropertyChanged
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportView> _logger;
    private readonly ImmutableDictionary<string, IPreviewRenderer> _previewRenderers;

    #region Properties

    private DateTime? _from;
    public DateTime? From
    {
        get => _from;
        set
        {
            _from = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _to;
    public DateTime? To
    {
        get => _to;
        set
        {
            _to = value;
            OnPropertyChanged();
        }
    }
    
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
    
    private ListBoxItem? _selectedReport;
    public ListBoxItem? SelectedReport
    {
        get => _selectedReport;
        set
        {
            _selectedReport = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    public ReportView(IReportService reportService, ILogger<ReportView> logger)
    {
        InitializeComponent();
        DataContext = this;
        _reportService = reportService;
        _logger = logger;
        _previewRenderers = new Dictionary<string, IPreviewRenderer>
        {
            {".pdf", new PdfPreviewRenderer(WebView) },
            {".xlsx", new ExcelPreviewRenderer(ExcelDataGrid) }
        }.ToImmutableDictionary();
        InitializeWebView();
        DownloadedFilesListBox.Items.Clear();
    }
    
    private async void InitializeWebView()
    {
        await WebView.EnsureCoreWebView2Async(null);
    }

    private List<ListBoxItem> GetCurrentReports()
    {
        if (From == null || To == null) return [];
        var reportsPath = _reportService.GetSessionPath(From.Value, To.Value);
        if (!Directory.Exists(reportsPath)) return [];
        var filePaths = Directory.EnumerateFiles(reportsPath);
        var items = filePaths.OrderBy(p => p).Select(path => new ListBoxItem { Content = Path.GetFileName(path), Tag = path }).ToList();
        return items;
    }
    
    private void OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            Reports = new ObservableCollection<ListBoxItem>(GetCurrentReports());
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message);
            throw;
        }
    }
    
    private void OpenReportDirectory(object sender, MouseButtonEventArgs e)
    {
        var reportsPathParent = Path.Combine(Path.GetTempPath(), ReportService.ReportsRootPath);
        if (From != null && To != null)
        {
            var reportsPath = _reportService.GetSessionPath(From.Value, To.Value);
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
            _logger.LogError(ex, "Не удалость открыть папку");
        }
    }
    
    private void PreviewReport(object sender, SelectionChangedEventArgs e)
    {
        foreach (var previewRenderer in _previewRenderers.Values) previewRenderer.Hide();

        var filePath = SelectedReport?.Tag as string;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        var extension = Path.GetExtension(filePath).ToLower();
        try
        {
            _previewRenderers[extension].Render(filePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить предпросмотр файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            _logger.LogError(ex, "Не удалость загрузить предпросмотр файла");
        }
    }

    private void OpenReport(object sender, MouseButtonEventArgs e)
    {
        var filePath = SelectedReport?.Tag as string;
        OpenFile(filePath);
    }

    private void OpenFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        try
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            _logger.LogError(ex, "Не удалость открыть файл");
        }
    }

    private async void GenerateReport(object sender, RoutedEventArgs e)
    {
        if (From == null || To == null)
        {
            MessageBox.Show("Пожалуйста, выберите начальную и конечную даты.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (From > To)
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
            await _reportService.GenerateReportFiles(From.Value, To.Value, page);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Произошла ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            _logger.LogError(ex, "Произошла ошибка при формировании отчета");
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