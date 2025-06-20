using System.Windows.Controls;

namespace CinemaControl.Configuration;

public class AppConfigurationWindowBuilder(AppConfiguration configuration) : ConfigurationWindowBuilder
{
    private TextBox? _apiTokenTextBox;
    
    public override void BuildWindow(ConfigurationWindow window)
    {
        _apiTokenTextBox = AddTextBox(window, "Api Token", configuration.ApiToken);
    }

    public override void SaveConfiguration()
    {
        var jsonSettings = GetConfiguration();

        if (!jsonSettings.ContainsKey("App"))
            jsonSettings["App"] = new Dictionary<string, object>();
        jsonSettings["App"]["ApiToken"] = _apiTokenTextBox?.Text ?? "";

        WriteConfiguration(jsonSettings);
    }
}