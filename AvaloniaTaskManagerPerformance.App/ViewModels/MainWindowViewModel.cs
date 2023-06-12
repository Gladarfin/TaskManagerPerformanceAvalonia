using System;
using System.Collections.Generic;
using System.Management;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AvaloniaTaskManagerPerformance.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public List<string> ProcessorInfo { get; set; }

        public MainWindowViewModel()
        {
            ProcessorInfo = GetProccessorInfo();
        }

        private static List<string> GetProccessorInfo()
        {
            var result = new List<string>();
            var mgt = new ManagementClass("Win32_Processor");
            var procs= mgt.GetInstances();
            foreach (var item in procs)
            {
                var l2Cache = (uint) item.Properties["L2CacheSize"].Value / 1024.0;
                var l3Cache = (uint)item.Properties["L3CacheSize"].Value / 1024.0;
                var speed = (uint) item.Properties["MaxClockSpeed"].Value / 1000.0;
                var ticks = TimeSpan.FromMilliseconds(Environment.TickCount & int.MaxValue);
                
                var virtualization = (bool)item.Properties["VirtualizationFirmwareEnabled"].Value
                    ? "Enabled"
                    : "Disabled";
                result.Add(item.Properties["Name"].Value.ToString());
                result.Add(item.Properties["ThreadCount"].Value.ToString());
                result.Add(item.Properties["NumberOfCores"].Value.ToString());
                result.Add(l2Cache.ToString("0.0 MB"));
                result.Add(l3Cache.ToString("0.0 MB"));
                result.Add(virtualization);
                result.Add(item.Properties["NumberOfLogicalProcessors"].Value.ToString());
                result.Add(speed.ToString("0.00 GHz"));
                result.Add(ticks.ToString(@"hh\:mm\:ss"));
            }

            return result;
        }
    }
}