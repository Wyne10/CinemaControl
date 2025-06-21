using System.Windows.Controls;
using Microsoft.Win32;

namespace CinemaControl.Configuration;

public class WeeklyReportConfigurationWindowBuilder(WeeklyReportConfiguration configuration) : ConfigurationWindowBuilder
{
    public override void BuildWindow(ConfigurationWindow window)
    {
    }

    public override void SaveConfiguration()
    {
        var jsonSettings = GetConfiguration();
        WriteConfiguration(jsonSettings);
    }
}