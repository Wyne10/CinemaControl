using System.Windows.Controls;
using Microsoft.Win32;

namespace CinemaControl.Configuration;

public class QuarterlyReportConfigurationWindowBuilder(ConfigurationService configuration) : ConfigurationWindowBuilder
{
    private TextBox? _quarterlyReportTemplateTextBox;
    
    public override void BuildWindow(ConfigurationWindow window)
    {
        _quarterlyReportTemplateTextBox = AddBrowseTextBox(
            window,
            "Шаблон ежеквартального отчета (.xlsx)", 
            new OpenFileDialog
            {
                Filter = "Excel Documents (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Выберите шаблон ежеквартального отчета"
            },
            configuration.QuarterlyReportConfiguration.TemplatePath);
    }

    public override void SaveConfiguration()
    {
        var jsonSettings = GetConfiguration();

        if (!jsonSettings.ContainsKey("QuarterlyReport"))
            jsonSettings["QuarterlyReport"] = new Dictionary<string, object>();
        jsonSettings["QuarterlyReport"]["TemplatePath"] = _quarterlyReportTemplateTextBox?.Text ?? "";

        WriteConfiguration(jsonSettings);
    }
}