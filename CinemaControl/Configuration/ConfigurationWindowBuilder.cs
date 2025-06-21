using System.Windows.Controls;

namespace CinemaControl.Configuration;

public abstract class ConfigurationWindowBuilder
{
    public abstract void BuildWindow(ConfigurationWindow window);
    public abstract void SaveConfiguration();

    private StackPanel GetConfigurationStack(ConfigurationWindow window) => window.ConfigurationStack;
    
    protected TextBox AddTextBox(ConfigurationWindow window, string labelText, string value = "")
    {
        var label = new Label { Content = labelText };
        var textBox = new TextBox { Text = value };
        GetConfigurationStack(window).Children.Add(label);
        GetConfigurationStack(window).Children.Add(textBox);
        return textBox;
    }
}