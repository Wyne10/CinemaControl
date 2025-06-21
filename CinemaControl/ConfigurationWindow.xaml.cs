using System.Windows;
using CinemaControl.Configuration;

namespace CinemaControl;

public partial class ConfigurationWindow
{
    private readonly ConfigurationWindowBuilder _windowBuilder;
    
    public ConfigurationWindow(ConfigurationWindowBuilder windowBuilder)
    {
        InitializeComponent();
        _windowBuilder = windowBuilder;
        _windowBuilder.BuildWindow(this);
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        _windowBuilder.SaveConfiguration();
    }
}