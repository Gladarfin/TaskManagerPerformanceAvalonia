using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class DiskViewModel : ObservableObject
{
    #region Properties
    
    [ObservableProperty] private List<ISeries> _series;
    [ObservableProperty] private double _diskCapacity;
    [ObservableProperty] private double _diskFormattedCapacity;
    [ObservableProperty] private string _diskType;
    [ObservableProperty] private string _diskLabel;
    [ObservableProperty] private int _diskActiveTime;
    
    public Charts Charts { get; } = new();

    private readonly DiskInfoHelper _dih;
    
    #endregion
    
    private readonly List<ObservablePoint> _observableValues;
    
    private static SeriesHelper SeriesHelper { get; } = new();
    private const int Index = 62;
    
    
    #region Labels
    
    
    [ObservableProperty] private string _diskModel = "Disk Model Placeholder";
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


    public DiskViewModel()
    {
        _observableValues = new List<ObservablePoint>();
        
        for (var i = 0; i < 62; i++)
        {
            _observableValues.Add(new ObservablePoint(i, -1));
        }

        _dih = new DiskInfoHelper();
        _dih.GetDiskConstValues();
        StartDiskMeasuring();
    }
    
    private void StartDiskMeasuring()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        DiskLabel = $"Disk 0 ({_dih.DiskLetters})";
        DiskType = _dih.DiskType;
        DiskCapacity = _dih.Capacity;
        DiskFormattedCapacity = _dih.Capacity;
        DiskModel = _dih.Model;
        
        timer.Tick += (sender, e) =>
        {
            _dih.GetDiskInfo();
            DiskActiveTime = _dih.DiskActiveTime;
        };
        
        //Task.Run(GetNextDiskLoadTrackingValue);
        timer.Start();
    }

    private async Task GetNextDiskLoadTrackingValue()
    {
        while (true)
        {
            RemoveFirstDiskLoadValue();
            AddNextDiskLoadValue();
            UpdateSeriesValues();
            await Task.Delay(1000);
        }
    }

    private void RemoveFirstDiskLoadValue()
    {
        _observableValues.RemoveAt(0);
        foreach (var elem in _observableValues)
        {
            elem.X--;
        }
    }

    private void AddNextDiskLoadValue()
    {
        
    }

    private void UpdateSeriesValues()
    {
        Series = SeriesHelper.SetSeriesValues(
            new SKColor(239, 247, 233), 
            new SKColor(77, 166, 12),
            _observableValues);
        Charts.SeriesPreview = SeriesHelper.SetSeriesValues(
            new SKColor(239, 247, 233),
            new SKColor(77, 166, 12),
            _observableValues);
    }
}