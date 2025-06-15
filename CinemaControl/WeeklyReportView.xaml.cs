using CinemaControl.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CinemaControl
{
    public partial class WeeklyReportView : UserControl
    {
        private readonly IWeeklyReportService _reportService;

        public WeeklyReportView()
        {
            InitializeComponent();
            _reportService = new RealWeeklyReportService();
            // Clear placeholder items
            DownloadedFilesListBox.Items.Clear();
            InitializeWebView();
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

            try
            {
                // Показываем индикатор загрузки
                this.IsEnabled = false;
                var filePaths = await _reportService.GetReportFilesAsync(startDate.Value, endDate.Value);
                DownloadedFilesListBox.Items.Clear();
                foreach (var path in filePaths)
                {
                    var item = new ListBoxItem
                    {
                        Content = Path.GetFileName(path),
                        Tag = path // Store full path in Tag
                    };
                    DownloadedFilesListBox.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Убираем индикатор загрузки
                this.IsEnabled = true;
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
    }
} 