using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
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
    [ObservableProperty] private List<ISeries> _series;
    [ObservableProperty] private List<ISeries> _seriesPreview;
    [ObservableProperty]
    private List<Axis> _xAxis = new List<Axis>
    {
        new Axis
        {
            MaxLimit = 62,
            MinLimit = 2,
            MinStep = 1,
            IsVisible = false,
        }
    };
    [ObservableProperty]
    private List<Axis> _yAxis = new List<Axis>
    {
        new Axis
        {
            MaxLimit = 100,
            MinLimit = 0,
            MinStep = 10,
            IsVisible = false
        }
    };

    public static ProcessorInfo Info { get; private set; } = new();

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
    
    #endregion
    
    #endregion
    
    private List<ObservablePoint> _observableValues;
    private const int Index = 62;
    
    private readonly PerformanceCounter _cpuLoad = new("Processor Information", "% Processor Utility", "_Total");
    private readonly PerformanceCounter _threadsCount = new("System", "Threads");
    private readonly PerformanceCounter _handlesCount = new("Process", "Handle Count", "_Total");
    
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
    
        //все обновляемые значения в окне
        timer.Tick += (sender, e) =>
        {
            Timer = TimeSpan.FromMilliseconds(Environment.TickCount & int.MaxValue).ToString(@"dd\:hh\:mm\:ss");
            Processes = Process.GetProcesses().Length;
            Threads = (int)_threadsCount.NextValue();
            Handles = (int)_handlesCount.NextValue();
            var tmpCpu = Math.Min((int)_cpuLoad.NextValue(), 100);
            CpuCounter = $"{tmpCpu.ToString()}%";
        };
        
        timer.Start();
        Task.Run(GetNextCpuLoadTrackingValue);
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
    
        return result;
    }

    private async Task GetNextCpuLoadTrackingValue()
    {
        while (true)
        {
            RemoveFirstItem();
            AddNextCpuLoadValue();
            await Task.Delay(1000);
        }
    }

    private void AddNextCpuLoadValue()
    {
        _observableValues.Add(new ObservablePoint(Index, (int)_cpuLoad.NextValue()));
        Series = SetSeriesValues(new SKColor(241,246,250), new SKColor(156, 200, 226));
        SeriesPreview = SetSeriesValues(new SKColor(241, 246, 250), new SKColor(17, 125, 187));
    }

    private List<ISeries> SetSeriesValues(SKColor fillColor, SKColor strokeColor)
    {
        return new List<ISeries>
        {
            new LineSeries<ObservablePoint>
            {
                Values = _observableValues,
                Fill = new SolidColorPaint(fillColor),
                Stroke = new SolidColorPaint(strokeColor){StrokeThickness = 1},
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0
            }
        };
    }
     
     private void RemoveFirstItem()
     {
         _observableValues.RemoveAt(0);
         //костыль, чтобы можно было добавить по оси Х ограничения с 0 до 60 => нужно уменьшать все индексы после удаления элемента,
         //иначе не будет отображаться (но это сделано только для того, чтобы выглядело, как в TaskManager)
         foreach (var elem in _observableValues)
         {
             elem.X--;
         }
     }

}