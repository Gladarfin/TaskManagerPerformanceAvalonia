using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class DiskViewModel : ObservableObject
{
    #region Properties
    
    
    
    #endregion
    
    #region Labels
    
    [ObservableProperty] private string _diskLabel = "Disk 0";
    [ObservableProperty] private string _diskTypeLabel = "SSD";
    [ObservableProperty] private string _activeTimeLabel = "Active time";
    [ObservableProperty] private string _maxPercent = "100%";
    [ObservableProperty] private string _maxTime = "60 seconds";
    [ObservableProperty] private string _minTime = "0";
    [ObservableProperty] private string _diskTransferRateLabel = "Disk transfer rate";
    [ObservableProperty] private string _diskMaxTransferRateLabel = "10 MB/s";
    [ObservableProperty] private string _averageResponseTimeLabel = "Average response time";
    [ObservableProperty] private string _readSpeedLabel = "Read speed";
    [ObservableProperty] private string _writeSpeedLabel = "Write speed";
    [ObservableProperty] private string _capacityLabel = "Capacity:";
    [ObservableProperty] private string _formattedLabel = "Formatted:";
    [ObservableProperty] private string _systemDiskLabel = "System Disk:";
    [ObservableProperty] private string _pageFileLabel = "Page file:";
    [ObservableProperty] private string _typeLabel = "Type:";
    
    #endregion
}