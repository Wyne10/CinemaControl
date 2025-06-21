using System.IO;
using System.Text.Json;
using System.Windows.Controls;

namespace CinemaControl.Configuration;

public abstract class ConfigurationWindowBuilder
{
    public abstract void BuildWindow(ConfigurationWindow window);
    public abstract void SaveConfiguration();

    protected static Dictionary<string, Dictionary<string, object>> GetConfiguration()
    {
        var configFile = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(configFile);
        var jsonSettings = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
        return jsonSettings ?? new Dictionary<string, Dictionary<string, object>>();
    }

    protected static void WriteConfiguration(Dictionary<string, Dictionary<string, object>> configuration)
    {
        var configFile = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configFile, json);
    }

    protected static TextBox AddTextBox(ConfigurationWindow window, string labelText, string value = "")
    {
        var label = new Label { Content = labelText };
        var textBox = new TextBox { Text = value };
        window.ConfigurationStack.Children.Add(label);
        window.ConfigurationStack.Children.Add(textBox);
        return textBox;
    }
}