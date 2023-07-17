using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTaskManagerPerformance.App.ViewModels;

namespace AvaloniaTaskManagerPerformance.App.Views;
[SupportedOSPlatform("windows")]
public partial class WiFiUserControl : UserControl
{
    public WiFiUserControl()
    {
        InitializeComponent();
        DataContext = new WiFiViewModel();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}