using System.IO;
using System.Text.Json;
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
        var configFile = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(configFile);
        var jsonSettings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);

        // Update App section
        if (!jsonSettings.ContainsKey("App"))
            jsonSettings["App"] = new Dictionary<string, object>();
        jsonSettings["App"]["ApiToken"] = _apiTokenTextBox.Text;

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(configFile, JsonSerializer.Serialize(jsonSettings, options));
    }
}