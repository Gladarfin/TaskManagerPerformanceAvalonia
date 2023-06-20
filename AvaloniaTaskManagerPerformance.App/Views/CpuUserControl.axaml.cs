using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTaskManagerPerformance.App.ViewModels;

namespace AvaloniaTaskManagerPerformance.App.Views;

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