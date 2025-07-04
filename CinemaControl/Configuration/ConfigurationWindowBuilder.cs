using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Win32;

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

    protected static CheckBox AddCheckBox(ConfigurationWindow window, string labelText, bool value = false)
    {
        var label = new Label { Content = labelText };
        var checkBox = new CheckBox { IsChecked = value };
        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        stackPanel.Children.Add(checkBox);
        stackPanel.Children.Add(label);
        window.ConfigurationStack.Children.Add(stackPanel);
        return checkBox;   
    }

    protected static TextBox AddBrowseTextBox(ConfigurationWindow window, string labelText,
        OpenFileDialog openFileDialog, string value = "")
    {
        var label = new Label { Content = labelText };
        var textBox = new TextBox { Text = value, Width = 300 };
        var browseButton = new Button { Content = "Обзор...", Width = 50 };
        browseButton.Click += (_, _) =>
        {
            if (openFileDialog.ShowDialog() == true)
            {
                textBox.Text = openFileDialog.FileName;
            }
        };
        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(browseButton);
        window.ConfigurationStack.Children.Add(label);
        window.ConfigurationStack.Children.Add(stackPanel);
        return textBox;
    }
}