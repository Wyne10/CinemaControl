using System.Windows.Controls;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace CinemaControl.Configuration;

public class MonthlyReportConfigurationWindowBuilder(IOptions<MonthlyReportConfiguration> configuration) : ConfigurationWindowBuilder
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
            configuration.Value.TemplatePath);
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