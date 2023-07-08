using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTaskManagerPerformance.App.ViewModels;

namespace AvaloniaTaskManagerPerformance.App.Views;
[SupportedOSPlatform("windows")]
public partial class CpuUserControl : UserControl
{
    public CpuUserControl()
    {
        InitializeComponent();
        DataContext = new CpuViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}