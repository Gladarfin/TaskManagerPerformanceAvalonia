using System;
using System.Diagnostics;
using System.Management;
using System.Text;
using AvaloniaTaskManagerPerformance.App.Models;

namespace AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;

public class DiskInfoHelper
{
    public string DiskLabel{ get; set; }
    public double DiskCapacity { get; set; }
    public double DiskFormattedCapacity{ get; set; }
    public string DiskType{ get; set; }
    public string DiskModel { get; set; }
    public float DiskAvgResponseTime { get; set; }
    public string ReadSpeed { get; set; }
    public string WriteSpeed { get; set; }
    public  int ActiveTime { get; set; }
    public float Read;
    public float Write;

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

    public DiskInfoHelper()
    {
        //first call returns 0, so we skip it - https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecounter.nextvalue?view=dotnet-plat-ext-7.0&redirectedfrom=MSDN#System_Diagnostics_PerformanceCounter_NextValue
        _idleDiskCounter.NextValue();
        AssignDiskConstValues();
    }

    public void GetDiskValues()
    {
        ActiveTime = 100 - (int)_idleDiskCounter.NextValue();
        DiskAvgResponseTime = (_avgDiskRead.NextValue() + _avgDiskWrite.NextValue()) * 1000;
        Read = _readCounter.NextValue() / 1000;
        ReadSpeed = GetSpeedValue(Read);
        Write = _writeCounter.NextValue() / 1000;
        WriteSpeed = GetSpeedValue(Write);
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
    
    private string GetSpeedValue(float val)
    {
        return val > 1002 ? $"{val / 1000:F1} MB/s" : $"{val:F1} KB/s";  
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
    
}