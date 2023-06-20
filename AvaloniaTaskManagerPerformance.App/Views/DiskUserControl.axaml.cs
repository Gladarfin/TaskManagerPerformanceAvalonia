using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTaskManagerPerformance.App.ViewModels;

namespace AvaloniaTaskManagerPerformance.App.Views;

public partial class DiskUserControl : UserControl
{
    public DiskUserControl()
    {
        InitializeComponent();
        DataContext = new DiskViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}