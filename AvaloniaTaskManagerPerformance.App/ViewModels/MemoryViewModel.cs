using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class MemoryViewModel  : ObservableObject
{
    #region Properties
    
    [ObservableProperty] private List<ISeries> _series;
    [ObservableProperty] private float _installedMemory;
    [ObservableProperty] private float _totalVisibleMemory;
    [ObservableProperty] private float _memoryInUse;
    [ObservableProperty] private string _freePhysicalMemory;
    [ObservableProperty] private string _committedMemory;
    [ObservableProperty] private float _pagedPool;
    [ObservableProperty] private float _nonPagedPool;
    [ObservableProperty] private string _slots;
    [ObservableProperty] private string _formFactor;
    [ObservableProperty] private float _reservedMemory;
    [ObservableProperty] private string _cachedMemory;
    [ObservableProperty] private int _memorySpeed;
    [ObservableProperty] private string _memoryInfoPreview;
    
    [ObservableProperty] private List<ISeries> _something;
    public Charts Charts { get; } = new();

    private readonly MemoryInfoHelper _mih;

    #region Labels
    
    [ObservableProperty] private string _memoryLabel = "Memory";
    [ObservableProperty] private string _memoryUsageLabel = "Memory usage";
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

    private static SeriesHelper SeriesHelper { get; } = new();
    private const int Index = 62;
    
    public MemoryViewModel()
    {
        _observableValues = new List<ObservablePoint>();
        _mih = new MemoryInfoHelper();
        
        for (var i = 0; i < 62; i++)
        {
            _observableValues.Add(new ObservablePoint(i, -1));
        }

        StartMemoryMeasuring();
    }

    private void StartMemoryMeasuring()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        Slots = _mih.GetSlots();
        InstalledMemory = _mih.GetInstalledMemory();
        //On my laptop it doesn't work, so i just put value from task manager manually in xaml
        FormFactor = _mih.GetFormFactor();
        MemorySpeed = _mih.GetMemorySpeed();

        timer.Tick += (sender, e) =>
        {
            _mih.UpdateMemoryInfoValues();
            TotalVisibleMemory = _mih.TotalVisibleMemory;
            MemoryInUse = _mih.MemoryInUse;
            FreePhysicalMemory = _mih.FreePhysicalMemory;
            CommittedMemory = _mih.CommittedMemory;
            PagedPool = _mih.PagedPool;
            NonPagedPool = _mih.NonPagedPool;
            ReservedMemory = (InstalledMemory - _mih.TotalVisibleMemory) * 1024;
            //slightly different from TM
            CachedMemory = _mih.Cached;
            MemoryInfoPreview = $"{MemoryInUse:F1}/{TotalVisibleMemory:F1} ({MemoryInUse * 100 / TotalVisibleMemory:F0} %)";
        };

        
        Task.Run(GetNextMemoryLoadTrackingValue);
        timer.Start();
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
        var nextValue = (int)(MemoryInUse * 100 / TotalVisibleMemory);
        _observableValues.Add(new ObservablePoint(Index, nextValue));
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
}