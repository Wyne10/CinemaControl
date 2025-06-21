using System.Windows.Controls;
using Microsoft.Win32;

namespace CinemaControl.Configuration;

public class MonthlyReportConfigurationWindowBuilder(MonthlyReportConfiguration configuration) : ConfigurationWindowBuilder
{
    private TextBox? _monthlyReportTemplateTextBox;
    
    public override void BuildWindow(ConfigurationWindow window)
    {
        _monthlyReportTemplateTextBox = AddBrowseTextBox(
            window,
            "Шаблон ежемесячного отчета (.docx)", 
            new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx|All files (*.*)|*.*",
                Title = "Выберите шаблон ежемесячного отчета"
            },
            configuration.TemplatePath);
    }

    public override void SaveConfiguration()
    {
        var jsonSettings = GetConfiguration();

        if (!jsonSettings.ContainsKey("MonthlyReport"))
            jsonSettings["MonthlyReport"] = new Dictionary<string, object>();
        jsonSettings["MonthlyReport"]["TemplatePath"] = _monthlyReportTemplateTextBox?.Text ?? "";

        WriteConfiguration(jsonSettings);
    }
}