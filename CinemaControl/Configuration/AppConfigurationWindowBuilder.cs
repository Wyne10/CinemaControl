using System.Windows.Controls;

namespace CinemaControl.Configuration;

public class AppConfigurationWindowBuilder(ConfigurationService configuration) : ConfigurationWindowBuilder
{
    private TextBox? _apiTokenTextBox;
    private CheckBox? _debugCheckBox;
    
    public override void BuildWindow(ConfigurationWindow window)
    {
        _apiTokenTextBox = AddTextBox(window, "Api Token", configuration.AppConfiguration.ApiToken);
        _debugCheckBox = AddCheckBox(window, "Debug", configuration.AppConfiguration.Debug);
    }

    public override void SaveConfiguration()
    {
        var jsonSettings = GetConfiguration();

        if (!jsonSettings.ContainsKey("App"))
            jsonSettings["App"] = new Dictionary<string, object>();
        jsonSettings["App"]["ApiToken"] = _apiTokenTextBox?.Text ?? "";
        jsonSettings["App"]["Debug"] = _debugCheckBox?.IsChecked ?? false;

        WriteConfiguration(jsonSettings);
    }
}