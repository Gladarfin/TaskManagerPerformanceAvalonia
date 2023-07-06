using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class DiskViewModel : ObservableObject
{
    #region Properties
    
    [ObservableProperty] private List<ISeries> _series;
    [ObservableProperty] private List<ISeries> _seriesPreview;
    [ObservableProperty] private List<ISeries> _seriesWriteRead;
    [ObservableProperty] private static List<Axis> _yAxisMultiplySeries;
    

    [ObservableProperty] private double _diskCapacity;
    [ObservableProperty] private double _diskFormattedCapacity;
    [ObservableProperty] private string _diskType;
    [ObservableProperty] private string _diskLabel;
    [ObservableProperty] private string _diskActiveTime;
    //I don't know how exactly it's computed in Windows, but I assume it's the sum of write and read.
    [ObservableProperty] private float _diskAvgResponseTime;
    [ObservableProperty] private string _readSpeed;
    [ObservableProperty] private string _writeSpeed;

    
    public Charts Charts { get; } = new();

    #endregion
    
    private readonly List<ObservablePoint> _observableValues;
    private readonly List<ObservablePoint> _writeSpeedValues;
    private readonly List<ObservablePoint> _readSpeedValues;

    private static SeriesHelper SeriesHelper { get; } = new();
    private const int Index = 62;
    private static DiskInfoHelper DiskHelper { get; set; }
    
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
    [ObservableProperty] private string _isSystemDisk = "Yes";
    [ObservableProperty] private string _pageFileLabel = "Page file:";
    [ObservableProperty] private string _isPageFile = "Yes";
    [ObservableProperty] private string _typeLabel = "Type:";

    #endregion

                                                                                                                                     
    
    private readonly DashEffect _effect = new(new float[]{ 3, 2 });

    public DiskViewModel()
    {
        _observableValues = new List<ObservablePoint>();
        _writeSpeedValues = new List<ObservablePoint>();
        _readSpeedValues = new List<ObservablePoint>();
        for (var i = 0; i < 62; i++)
        {
            _observableValues.Add(new ObservablePoint(i, -1));
            _readSpeedValues.Add(new ObservablePoint(i, -1));
            _writeSpeedValues.Add(new ObservablePoint(i, -1));
        }
        
        YAxisMultiplySeries = new List<Axis>
        {
            new Axis
            {
                MaxLimit = 10000,
                MinLimit = 0,
                MinStep = 500,
                IsVisible = false
            }
        };
        DiskHelper = new DiskInfoHelper();
        AssignConstantValues();
        StartDiskMeasuring();
    }

    private void AssignConstantValues()
    {
        DiskLabel = DiskHelper.DiskLabel;
        DiskCapacity  = DiskHelper.DiskCapacity;
        DiskFormattedCapacity = DiskHelper.DiskFormattedCapacity;
        DiskType = DiskHelper.DiskType;
        DiskModel = DiskHelper.DiskModel;
    }
    
    private void StartDiskMeasuring()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        
        
        timer.Tick += (sender, e) =>
        {
            DiskHelper.GetDiskValues();
            DiskAvgResponseTime = DiskHelper.DiskAvgResponseTime;
            ReadSpeed = DiskHelper.ReadSpeed;
            WriteSpeed = DiskHelper.WriteSpeed;
            RemoveFirstDiskLoadValue(); 
            AddNextDiskLoadValue();
            UpdateSeriesValues();
        };
        timer.Start();
    }

    private void RemoveFirstDiskLoadValue()
    {
        _observableValues.RemoveAt(0);
        _writeSpeedValues.RemoveAt(0);
        _readSpeedValues.RemoveAt(0);
        for (var i = 0; i < _observableValues.Count; i++)
        {
            _observableValues[i].X--;
            _writeSpeedValues[i].X--;
            _readSpeedValues[i].X--;
        }
    }
    private void AddNextDiskLoadValue()
    {
        DiskActiveTime = $"{DiskHelper.ActiveTime}%";
        _observableValues.Add(new ObservablePoint(Index, DiskHelper.ActiveTime));
        _writeSpeedValues.Add(new ObservablePoint(Index, DiskHelper.Write));
        _readSpeedValues.Add(new ObservablePoint(Index, DiskHelper.Read));
    }

    private void UpdateSeriesValues()
    {
        Series = SeriesHelper.SetSeriesValues(
            new SKColor(239, 247, 233),
            new SKColor(77, 166, 12),
            _observableValues);
        SeriesPreview = SeriesHelper.SetSeriesValues(
            new SKColor(239, 247, 233),
            new SKColor(77, 166, 12),
            _observableValues);
        SeriesWriteRead = SeriesHelper.SetMultiplySeriesValues(
            new SKColor(239, 247, 233),
            new SKColor(77, 166, 12),
            _readSpeedValues,
            _writeSpeedValues,
            _effect);
    }
    
}