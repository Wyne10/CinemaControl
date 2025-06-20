using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CinemaControl.Services;
using Microsoft.Playwright;

namespace CinemaControl.ViewModel;

public class ReportViewModel(IReportService reportService) : INotifyPropertyChanged
{
    private bool _downloading;
    public bool Active
    {
        get => !_downloading;
        set
        {
            _downloading = !value;
            OnPropertyChanged();
        }
    }
    
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

    private RelayCommand? _updateReportsCommand;
    public RelayCommand UpdateReportsCommand
    {
        get
        {
            return _updateReportsCommand ??= new RelayCommand(_ =>
            {
                Reports = new ObservableCollection<ListBoxItem>(GetCurrentReports());
            });
        }
    }

    private RelayCommand? _openReportsDirectoryCommand;
    public RelayCommand OpenReportsDirectoryCommand
    {
        get
        {
            return _openReportsDirectoryCommand ??= new RelayCommand(_ =>
            {
                OpenReportsDirectory();
            });
        }
    }
    
    private RelayCommand? _openReportCommand;
    public RelayCommand OpenReportCommand
    {
        get
        {
            return _openReportCommand ??= new RelayCommand(_ =>
            {
                OpenReport();
            });
        }
    }
    
    private RelayCommand? _generateReportCommand;
    public RelayCommand GenerateReportCommand
    {
        get
        {
            return _generateReportCommand ??= new RelayCommand(async void (_) =>
            {
                await GenerateReport();
            });
        }
    }

    private async Task GenerateReport()
    {
        if (_from == null || _to == null)
        {
            MessageBox.Show("Пожалуйста, выберите начальную и конечную даты.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_from > _to)
        {
            MessageBox.Show("Начальная дата не может быть позже конечной.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        //Active = false;

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();
            reportService.OnDownloadProgress += () =>
            {
                MessageBox.Show("Test");
                Reports = new ObservableCollection<ListBoxItem>(GetCurrentReports());
            };
            await reportService.GenerateReportFiles(_from.Value, _to.Value, page);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Произошла ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Active = true;
        }
    }

    private List<ListBoxItem> GetCurrentReports()
    {
        if (_from == null || _to == null) return [];
        var reportsPath = reportService.GetSessionPath(_from.Value, _to.Value);
        if (!Directory.Exists(reportsPath)) return [];
        var filePaths = Directory.EnumerateFiles(reportsPath);
        var items = filePaths.OrderBy(p => p).Select(path => new ListBoxItem { Content = Path.GetFileName(path), Tag = path }).ToList();
        return items;
    }

    private void OpenReportsDirectory()
    {
        var reportsPathParent = Path.Combine(Path.GetTempPath(), ReportService.ReportsRootPath);
        if (_from != null && _to != null)
        {
            var reportsPath = reportService.GetSessionPath(_from.Value, _to.Value);
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

    private void OpenReport()
    {
        if (SelectedReport?.Tag is not string filePath) return;
        try
        {
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        } 
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}