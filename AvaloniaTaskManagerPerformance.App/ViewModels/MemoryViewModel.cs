using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class MemoryViewModel  : ObservableObject
{
    #region Properties

    [ObservableProperty] private double _memoryUsage;
    [ObservableProperty] private double _installedMemory;
    
    [ObservableProperty] private List<ISeries> _series;

    public MemoryInfo Info { get; set; }
    public Charts Charts { get; } = new();

    #region Labels
    
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
    
    private readonly List<ObservablePoint> _observableValues;
    
    private readonly PerformanceCounter _availableMemory = new PerformanceCounter("Memory", "Available MBytes");
    private static SeriesHelper SeriesHelper { get; } = new();
    
    public MemoryViewModel()
    {
        _observableValues = new List<ObservablePoint>();
        
        for (var i = 0; i < 62; i++)
        {
            _observableValues.Add(new ObservablePoint(i, -1));
        }

        StartMemoryMeasuring();
    }

    private void StartMemoryMeasuring()
    {
        Info = GetMemoryInfo();

        InstalledMemory = GetInstalledMemory();
        
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        
        timer.Tick += (sender, e) =>
        {
            
        };
        
        Task.Run(GetNextMemoryLoadTrackingValue);
        timer.Start();
    }

    private static MemoryInfo GetMemoryInfo()
    {
        var result = new MemoryInfo();

        return result;
    }

    private async Task GetNextMemoryLoadTrackingValue()
    {
        while (true)
        {
            RemoveFirstMemoryLoadValue();
            AddNextMemoryLoadValue();
            UpdateSeriesValues();
            await Task.Delay(1000);
        }
    }

    private void RemoveFirstMemoryLoadValue()
    {
        _observableValues.RemoveAt(0);
        foreach (var elem in _observableValues)
        {
            elem.X--;
        }
    }

    private void AddNextMemoryLoadValue()
    {
        
    }

    private void UpdateSeriesValues()
    {
        Series = SeriesHelper.SetSeriesValues(
            new SKColor(244, 244, 244), 
            new SKColor(139, 18, 174), 
            _observableValues);
        Charts.SeriesPreview = SeriesHelper.SetSeriesValues(
            new SKColor(244, 244, 244), 
            new SKColor(139, 18, 174),
            _observableValues);
    }

    private double GetInstalledMemory()
    {
        long memKB;
        GetPhysicallyInstalledSystemMemory(out memKB);
        return memKB / 1024 / 1024;
    }
    
    
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

}