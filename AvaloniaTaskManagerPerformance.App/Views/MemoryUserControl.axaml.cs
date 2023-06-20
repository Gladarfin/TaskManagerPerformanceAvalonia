using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTaskManagerPerformance.App.ViewModels;

namespace AvaloniaTaskManagerPerformance.App.Views;

public partial class MemoryUserControl : UserControl
{
    public MemoryUserControl()
    {
        InitializeComponent();
        DataContext = new MemoryViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}