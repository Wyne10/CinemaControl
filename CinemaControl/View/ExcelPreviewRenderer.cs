using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;

namespace CinemaControl.View;

public class ExcelPreviewRenderer(DataGrid excelDataGrid) : IPreviewRenderer
{
    public void Render(string filePath)
    {
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
            excelDataGrid.ItemsSource = dt.DefaultView;
        }
        excelDataGrid.Visibility = Visibility.Visible; 
    }

    public void Hide()
    {
        excelDataGrid.Visibility = Visibility.Collapsed;
    }
}