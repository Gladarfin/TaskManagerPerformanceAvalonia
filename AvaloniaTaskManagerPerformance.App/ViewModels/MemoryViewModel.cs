using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class MemoryViewModel  : ObservableObject
{
    #region Properties
    

    #endregion

    #region Labels

    #region Memory

    [ObservableProperty] private string _memoryLabel = "Memory";
    [ObservableProperty] private string _memoryUsageLabel = "Memory usage";
    [ObservableProperty] private string _memoryMaxLabel = "7,8 GB";
    [ObservableProperty] private string _maxTime = "60 seconds";
    [ObservableProperty] private string _minTime = "0";
    [ObservableProperty] private string _memoryCompositionLabel = "Memory composition";
    [ObservableProperty] private string _inUseLabel = "In use (Compressed)";
    [ObservableProperty] private string _memoryAvailableLabel = "Available";
    [ObservableProperty] private string _committedLabel = "Committed";
    [ObservableProperty] private string _cachedLabel = "Cached";
    [ObservableProperty] private string _pagedPoolLabel = "Paged pool";
    [ObservableProperty] private string _nonPagedPoolLabel = "Non-paged pool";
    [ObservableProperty] private string _speedLabel = "Speed:";
    [ObservableProperty] private string _slotsUsedLabel = "Slots used:";
    [ObservableProperty] private string _formFactorLabel = "Form factor:";
    [ObservableProperty] private string _hardwareReservedLabel = "Hardware reserved:";

    #endregion

    #endregion
}