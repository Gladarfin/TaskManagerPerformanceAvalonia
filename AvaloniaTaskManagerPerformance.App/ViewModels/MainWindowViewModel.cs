using System.Runtime.Versioning;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTaskManagerPerformance.App.ViewModels
{
    [SupportedOSPlatform("windows")]
    public class MainWindowViewModel : ObservableObject
    {
        public CpuViewModel CpuVM { get; }
        public MemoryViewModel MemoryVM { get; }
        public DiskViewModel DiskVM { get; }
        public WiFiViewModel WiFiVM { get; }
        
        public MainWindowViewModel()
        {
            CpuVM = new CpuViewModel();
            MemoryVM = new MemoryViewModel();
            DiskVM = new DiskViewModel();
            WiFiVM = new WiFiViewModel();
        }
    }
}