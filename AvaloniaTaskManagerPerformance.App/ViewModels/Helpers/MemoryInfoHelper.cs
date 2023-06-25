using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using AvaloniaTaskManagerPerformance.App.Models;

namespace AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;

public class MemoryInfoHelper
{
    public float TotalVisibleMemory{ get; set; }
    public float MemoryInUse{ get; set; }
    public string FreePhysicalMemory{ get; set; }
    public uint PagedPool{ get; set; }
    public uint NonPagedPool{ get; set; }
    public string CommittedMemory{ get; set; }
    public float TotalVirtualMemory{ get; set; }
    public float FreeVirtualMemory{ get; set; }
    public float VirtualMemoryInUse{ get; set; }
    public string Cached { get; set; }

    private float _freePhysMemory;
    private float _cachedMemory;
        
    private static readonly ManagementObjectSearcher PhysicalMemorySearcher = new ("SELECT * FROM Win32_PhysicalMemory");
    private static readonly PerformanceCounter PagedPoolCounter = new ("Memory", "Pool Paged Bytes");
    private static readonly PerformanceCounter NonPagedPoolCounter = new ("Memory", "Pool Nonpaged Bytes");
    private static readonly PerformanceCounter CacheBytesCounter = new ("Memory", "Cache Bytes");
    private static readonly PerformanceCounter ModifiedPageListBytesCounter = new ("Memory", "Modified Page List Bytes");
    private static readonly PerformanceCounter StandbyCacheNormalCounter = new ("Memory", "Standby Cache Normal Priority Bytes");
    private static readonly PerformanceCounter StandbyCacheCoreCounter = new ("Memory", "Standby Cache Core Bytes");
    
    public string GetSlots()
    {
        var totalSlots = 0;
        var usedSlots = 0;

        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemoryArray");
        foreach (var obj in searcher.Get())
        {
            totalSlots += Convert.ToInt32(obj["MemoryDevices"]);
        }

        searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
        foreach (var obj in searcher.Get())
        {
            if (obj["DeviceLocator"] != null && obj["DeviceLocator"].ToString() != "")
            {
                usedSlots++;
            }
        }

        return $"{usedSlots} of {totalSlots}";
    }
    
    public long GetInstalledMemory()
    {
        return new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory")
            .Get()
            .Cast<ManagementObject>()
            .Sum(x => Convert.ToInt64(x.Properties["Capacity"].Value)) / 1024 / 1024 / 1024;
    }

    public string GetFormFactor()
    {
        var formFactor = "";
        foreach (ManagementObject obj in PhysicalMemorySearcher.Get())
        {
            formFactor = Enum.GetName(typeof(FormFactorEnum), obj["FormFactor"]);
            break;
        }

        return formFactor;
    }

    public int GetMemorySpeed()
    {
        var memorySpeed = 0;
        foreach (ManagementObject obj in PhysicalMemorySearcher.Get())
        {
            memorySpeed = Convert.ToInt32(obj["Speed"]);
            break;
        }

        return memorySpeed;
    }

    public void UpdateMemoryInfoValues()
    {
        var wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
        var searcher = new ManagementObjectSearcher(wql);
        var results = searcher.Get();

        foreach (var result in results)
        {
            TotalVisibleMemory = ConvertValue(result["TotalVisibleMemorySize"].ToString());
            _freePhysMemory = ConvertValue(result["FreePhysicalMemory"].ToString());
            MemoryInUse = TotalVisibleMemory - _freePhysMemory;
            FreePhysicalMemory = _freePhysMemory * 1024 > 1032 ? $"{_freePhysMemory :F1} GB" : $"{_freePhysMemory * 1024:F0} MB";
            TotalVirtualMemory = ConvertValue(result["TotalVirtualMemorySize"].ToString());
            FreeVirtualMemory = ConvertValue(result["FreeVirtualMemory"].ToString());
            VirtualMemoryInUse = TotalVirtualMemory - FreeVirtualMemory;
            CommittedMemory = $"{VirtualMemoryInUse:F1}/{TotalVirtualMemory:F1} GB";
            _cachedMemory = ConvertValue((CacheBytesCounter.NextSample().RawValue + 
                                  StandbyCacheCoreCounter.NextSample().RawValue + 
                                  ModifiedPageListBytesCounter.NextSample().RawValue +
                                  StandbyCacheNormalCounter.NextSample().RawValue).ToString());
            Cached = _cachedMemory > 1032 ? $"{_cachedMemory / 1024 :F1} GB" : $"{_cachedMemory:F0} MB";
        }

        UpdatePagedPoolValues();
    }
    
    private float ConvertValue(string str)
    {
        var res = float.Parse(str);
        return res / 1024 / 1024;
    }

    private void UpdatePagedPoolValues()
    {
        PagedPool = (uint)ConvertValue(PagedPoolCounter.NextValue().ToString());
        NonPagedPool = (uint)ConvertValue(NonPagedPoolCounter.NextValue().ToString());
    }
}