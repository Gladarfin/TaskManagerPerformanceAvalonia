using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class CpuViewModel : ObservableObject
{
    #region Properties

    #region Values

    [ObservableProperty] private string _timer = "00:00:00:00";
    [ObservableProperty] private int _processes;
    [ObservableProperty] private int _threads;
    [ObservableProperty] private int _handles;
    [ObservableProperty] private string _cpuCounter;
    [ObservableProperty] private double _cpuSpeed;
    [ObservableProperty] private List<ISeries> _series;
    [ObservableProperty] private List<ISeries> _seriesPreview;
    public static ProcessorInfo Info { get; private set; } = new();
    public Charts Charts { get; } = new();

    #endregion

    #region Labels
    
    [ObservableProperty] private string _cpuLabel = "CPU";
    [ObservableProperty] private string _utilLabel = "% Utilization";
    [ObservableProperty] private string _maxPercent = "100%";
    [ObservableProperty] private string _maxTime = "60 seconds";
    [ObservableProperty] private string _minTime = "0";
    [ObservableProperty] private string _baseSpeed = "Base speed:";
    [ObservableProperty] private string _sockets = "Sockets:";
    [ObservableProperty] private string _cores = "Cores:";
    [ObservableProperty] private string _logicalProcessors = "Logical processors:";
    [ObservableProperty] private string _virtualization = "Virtualization:";
    [ObservableProperty] private string _l1CacheLabel = "L1 cache:";
    [ObservableProperty] private string _l2CacheLabel = "L2 cache:";
    [ObservableProperty] private string _l3CacheLabel = "L3 cache:";
    [ObservableProperty] private string _utilizationLabel = "Utilization";
    [ObservableProperty] private string _speedLabel = "Speed";
    [ObservableProperty] private string _processesLabel = "Processes";
    [ObservableProperty] private string _threadsLabel = "Threads";
    [ObservableProperty] private string _handlesLabel = "Handles";
    [ObservableProperty] private string _uptimeLabel = "Up time";
    
    #endregion
    
    #endregion
    
    private readonly List<ObservablePoint> _observableValues;
    private const int Index = 62;
    private int tmpCpu;
    private static SeriesHelper SeriesHelper { get; } = new();
    
    private readonly PerformanceCounter _cpuLoad = new("Processor Information", "% Processor Utility", "_Total");
    private readonly PerformanceCounter _threadsCount = new("System", "Threads");
    private readonly PerformanceCounter _handlesCount = new("Process", "Handle Count", "_Total");
    private static readonly PerformanceCounter _cpuPerformance = new ("Processor Information", "% Processor Performance", "_Total");
    public CpuViewModel()
    {
        _observableValues = new List<ObservablePoint>();

        for (var i = 0; i < 62; i++)
        {
            _observableValues.Add(new ObservablePoint(i, -1));
        }
        StartCpuMeasuring();
    }
    
    private void StartCpuMeasuring()
    {
        Info = GetProcessorInfo();
    
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        timer.Tick += (sender, e) =>
        {
            Timer = TimeSpan.FromMilliseconds(Environment.TickCount & int.MaxValue).ToString(@"dd\:hh\:mm\:ss");
            Processes = Process.GetProcesses().Length;
            Threads = (int)_threadsCount.NextValue();
            Handles = (int)_handlesCount.NextValue();
            tmpCpu = Math.Min((int)_cpuLoad.NextValue(), 100);
            RemoveFirstCpuLoadValue();
            AddNextCpuLoadValue(tmpCpu);
            UpdateSeriesValues();
            Task.Run(GetSpeed);
        };
        
        timer.Start();
    }

    private static ProcessorInfo GetProcessorInfo()
    {
        var result = new ProcessorInfo();
        var mgt = new ManagementClass("Win32_Processor");
        var procs = mgt.GetInstances();
        foreach (var item in procs)
        {
            result.Name = item.Properties["Name"].Value.ToString();
            result.ThreadCount = (uint)item.Properties["ThreadCount"].Value;
            result.NumberOfCores = (uint)item.Properties["NumberOfCores"].Value;
            result.NumberOfLogicalProcessors = (uint)item.Properties["NumberOfLogicalProcessors"].Value;
            result.L2Cache = (uint)item.Properties["L2CacheSize"].Value / 1024.0;
            result.L3Cache = (uint)item.Properties["L3CacheSize"].Value / 1024.0;
            result.MaxClockSpeed = (uint)item.Properties["MaxClockSpeed"].Value / 1000.0;
            result.Virtualization = (bool)item.Properties["VirtualizationFirmwareEnabled"].Value
                ? "Enabled"
                : "Disabled";
        }
        
        result.L1Cache = GetCacheL1();
        return result;
    }

    private static int GetCacheL1()
    {
        using var searcher = new ManagementObjectSearcher("Select * from Win32_CacheMemory");
        var cacheSize = 0;

        foreach (var cache in searcher.Get())
        {
            var level = (ushort)cache["Level"];
            if (level == 3)
            {
                cacheSize += Convert.ToInt32(cache["MaxCacheSize"]);
            }
        }

        return cacheSize;
    }

    private void AddNextCpuLoadValue(int cpuLoadValue)
    {
        // Ok, i don't know why (probably because UI elements from MainWindow rendering earlier than from UserControls
        // but values in views (on utilization labels) slightly different). Maybe I'll return to it later.
        CpuCounter = $"{cpuLoadValue}%";
        _observableValues.Add(new ObservablePoint(Index, cpuLoadValue));
    }

    private void UpdateSeriesValues()
    {
        Series = SeriesHelper.SetSeriesValues(
            new SKColor(241,246,250), 
            new SKColor(156, 200, 226), 
            _observableValues);
        SeriesPreview = SeriesHelper.SetSeriesValues(
            new SKColor(241, 246, 250), 
            new SKColor(17, 125, 187), 
            _observableValues);
    }

    private void RemoveFirstCpuLoadValue()
     {
         _observableValues.RemoveAt(0);
         foreach (var elem in _observableValues)
         {
             elem.X--;
         }
     }
    
    private async Task GetSpeed()
    {
        var maxSpeed = 0.0;
        using var searcher = new ManagementObjectSearcher("SELECT *, Name FROM Win32_Processor");
        foreach (var obj in searcher.Get())
        {
            maxSpeed = Convert.ToDouble(obj["MaxClockSpeed"]) / 1000;
            await Task.Delay(1);
            break;
        }
        CpuSpeed = maxSpeed * _cpuPerformance.NextValue() / 100;
    }

}