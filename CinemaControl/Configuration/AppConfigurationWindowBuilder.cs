using System.Windows.Controls;
using Microsoft.Extensions.Options;

namespace CinemaControl.Configuration;

public class AppConfigurationWindowBuilder(IOptions<AppConfiguration> configuration) : ConfigurationWindowBuilder
{
    private TextBox? _apiTokenTextBox;
    
    public override void BuildWindow(ConfigurationWindow window)
    {
        _apiTokenTextBox = AddTextBox(window, "Api Token", configuration.Value.ApiToken);
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