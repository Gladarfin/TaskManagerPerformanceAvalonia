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
    private int _activeTime;
    private float _read;
    private float _write;
    
    public Charts Charts { get; } = new();

    #endregion
    
    private readonly List<ObservablePoint> _observableValues;
    private readonly List<ObservablePoint> _writeSpeedValues;
    private readonly List<ObservablePoint> _readSpeedValues;

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
    [ObservableProperty] private string _isSystemDisk = "Yes";
    [ObservableProperty] private string _pageFileLabel = "Page file:";
    [ObservableProperty] private string _isPageFile = "Yes";
    [ObservableProperty] private string _typeLabel = "Type:";

    #endregion

                                                                                                                                     
    private readonly WqlObjectQuery _diskDriveLogicalDiskObjectQuery = new ("SELECT * FROM Win32_LogicalDisk");                      
    private readonly ManagementScope _scope = new (@"\\.\root\microsoft\windows\storage");                                           
    private readonly ManagementObjectSearcher _mediaType = new ("SELECT * FROM MSFT_PhysicalDisk");                                  
    private readonly ManagementObjectSearcher _allPartitionsSize = new("SELECT * FROM MSFT_Partition");                              
    private readonly WqlObjectQuery _diskDriveObjectQuery = new ("SELECT * FROM Win32_DiskDrive");                                   
    private readonly PerformanceCounter _idleDiskCounter = new("PhysicalDisk", "% Idle Time", "_Total");          
    private readonly PerformanceCounter _avgDiskRead = new("PhysicalDisk", "Avg. Disk sec/Read", "_Total");        
    private readonly PerformanceCounter _avgDiskWrite = new("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
    private readonly PerformanceCounter _readCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
    private readonly PerformanceCounter _writeCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
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
        
        
        AssignDiskConstValues();
        StartDiskMeasuring();
    }
    
    private void StartDiskMeasuring()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        //first call returns 0, so we skip it - https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecounter.nextvalue?view=dotnet-plat-ext-7.0&redirectedfrom=MSDN#System_Diagnostics_PerformanceCounter_NextValue
        _idleDiskCounter.NextValue();
        
        timer.Tick += (sender, e) =>
        {
            _activeTime = 100 - (int)_idleDiskCounter.NextValue();
            DiskAvgResponseTime = (_avgDiskRead.NextValue() + _avgDiskWrite.NextValue()) * 1000;
            _read = _readCounter.NextValue() / 1000;
            ReadSpeed = GetSpeedValue(_read);
            _write = _writeCounter.NextValue() / 1000;
            WriteSpeed = GetSpeedValue(_write);
            RemoveFirstDiskLoadValue();
            AddNextDiskLoadValue();
            UpdateSeriesValues();
        };
        timer.Start();
    }

    private void AssignDiskConstValues()
    {
        var diskLetters = GetDiskLetters();
        DiskLabel = $"Disk 0 ({diskLetters})";
        DiskCapacity = ConvertFromBytesToGigabytes(GetDiskSizeWithHiddenPartitions());
        DiskFormattedCapacity = DiskCapacity;
        DiskType = GetDiskType();
        DiskModel = GetDiskModel();
    }

    private string GetDiskLetters()
    {
        using var res = new ManagementObjectSearcher(_diskDriveLogicalDiskObjectQuery);
        var sb = new StringBuilder();
        foreach (var disk in res.Get())
        {
            sb.Append(disk["DeviceID"]);
            sb.Append(' ');
            //If we use a query from LogicalDisk to calculate the size, it will be different because it doesn't append hidden partitions.
            //Capacity += (ulong)disk["Size"];
        }

        sb.Length--;
        return sb.ToString();
    }

    private double GetDiskSizeWithHiddenPartitions()
    {
        _scope.Connect();
        _allPartitionsSize.Scope = _scope;
        var result = 0.0;

        foreach(var part in _allPartitionsSize.Get())
        {
            result += (ulong)part["Size"];
        }
        
        _allPartitionsSize.Dispose();
        return result;
    }

    private string GetDiskType()
    {
        var result = "";
        _scope.Connect();
        _mediaType.Scope = _scope;
        foreach (var queryObj in _mediaType.Get())
        {
            var val = Convert.ToInt16(queryObj["MediaType"]);
            result = (val is > 2 and < 6 ? Enum.GetName(typeof(DiskTypeEnum), val) : "Unspecified") ?? string.Empty;
        }
        _mediaType.Dispose();

        return result;
    }

    private static double ConvertFromBytesToGigabytes(double val)
    {
        return val / 1024 / 1024 / 1024;
    }

    private string GetDiskModel()
    {
        var result = "";
        using var res = new ManagementObjectSearcher(_diskDriveObjectQuery);
        foreach (var o in res.Get())
        {
            result = o["Model"].ToString();
        }
        return result;
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
        DiskActiveTime = $"{_activeTime}%";
        _observableValues.Add(new ObservablePoint(Index, _activeTime));
        _writeSpeedValues.Add(new ObservablePoint(Index, _write));
        _readSpeedValues.Add(new ObservablePoint(Index, _read));
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

    private string GetSpeedValue(float val)
    {
        return val > 1002 ? $"{val / 1000:F1} MB/s" : $"{val:F1} KB/s";  
    }
}