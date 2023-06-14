using System;
using System.Diagnostics;
using System.Management;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTaskManagerPerformance.App.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty] private string _timer = "00:00:00:00";
        [ObservableProperty] private int _processes;
        [ObservableProperty] private int _threads;
        [ObservableProperty] private int _handles;
        [ObservableProperty] private string _cpuCounter;

        private readonly PerformanceCounter _cpuLoad = new("Processor Information", "% Processor Utility", "_Total");
        private readonly PerformanceCounter _threadsCount = new("System", "Threads");
        private readonly PerformanceCounter _handlesCount = new("Process", "Handle Count", "_Total");

        public static ProcessorInfo Info { get; set; }
        

        public MainWindowViewModel()
        {
            Info = GetProccessorInfo();
            
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            //все обновляемые значения в окне
            timer.Tick += (sender, e) =>
            {
                Timer = TimeSpan.FromMilliseconds(Environment.TickCount & int.MaxValue).ToString(@"dd\:hh\:mm\:ss");
                Processes = Process.GetProcesses().Length;
                
                Threads =(int)_threadsCount.NextValue();
                Handles = (int)_handlesCount.NextValue();
                CpuCounter = $"{((int)_cpuLoad.NextValue()).ToString()}%";

            };
            
            timer.Start();
            
        }

        private static ProcessorInfo GetProccessorInfo()
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
        
    }
}