using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CinemaControl.Services;

namespace CinemaControl.ViewModel;

public class ReportViewModel(IReportService reportService) : INotifyPropertyChanged
{
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
    public RelayCommand? OpenReportsDirectoryCommand
    {
        get
        {
            return _openReportsDirectoryCommand ??= new RelayCommand(_ =>
            {
                OpenReportsDirectory();
            });
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

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}