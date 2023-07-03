using System;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using AvaloniaTaskManagerPerformance.App.Models;

namespace AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;

public class DiskInfoHelper
{
    
    public string DiskType { get; set; }
    public double Capacity { get; set; }
    public string Model { get; set; }
    
    public int DiskActiveTime { get; set; }
    
    public string DiskLetters { get; set; }

    private const string instanceName = "0 C: D:";
    WqlObjectQuery diskDriveLogicalDiskObjectQuery = new ("SELECT * FROM Win32_LogicalDisk");
    ManagementScope scope = new (@"\\.\root\microsoft\windows\storage");
    ManagementObjectSearcher mediaType = new ("SELECT * FROM MSFT_PhysicalDisk");
    ManagementObjectSearcher allPartitionsSize = new("SELECT * FROM MSFT_Partition");
    WqlObjectQuery diskDriveObjectQuery = new ("SELECT * FROM Win32_DiskDrive");
    WqlObjectQuery diskActiveTimeQuery = new ($"SELECT * FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk WHERE Name = '{instanceName}'");

    public void GetDiskInfo()
    {
        //DiskActiveTime = GetDiskActiveTime();


    }

    public void GetDiskConstValues()
    {
        
        DiskLetters = GetDiskLetters();
        Capacity = ConvertFromBytesToGigabytes(GetDiskSizeWithHiddenPartitions());
        DiskType = GetDiskType();
        Model = GetDiskModel();
    }

    private string GetDiskLetters()
    {
        using var res = new ManagementObjectSearcher(diskDriveLogicalDiskObjectQuery);
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
        scope.Connect();
        allPartitionsSize.Scope = scope;
        var result = 0.0;

        foreach(var part in allPartitionsSize.Get())
        {
            result += (ulong)part["Size"];
        }
        
        allPartitionsSize.Dispose();
        return result;
    }

    private string GetDiskType()
    {
        var result = "";
        scope.Connect();
        mediaType.Scope = scope;
        foreach (var queryObj in mediaType.Get())
        {
            var val = Convert.ToInt16(queryObj["MediaType"]);
            result = (val is > 2 and < 6 ? Enum.GetName(typeof(DiskTypeEnum), val) : "Unspecified") ?? string.Empty;
        }
        mediaType.Dispose();

        return result;
    }

    private static double ConvertFromBytesToGigabytes(double val)
    {
        return val / 1024 / 1024 / 1024;
    }

    private string GetDiskModel()
    {
        var result = "";
        using var res = new ManagementObjectSearcher(diskDriveObjectQuery);
        foreach (var o in res.Get())
        {
            result = o["Model"].ToString();
        }
        return result;
    }

    private int GetDiskActiveTime()
    {
        var result = 0;
        using var res = new ManagementObjectSearcher(diskActiveTimeQuery);
        foreach (var mbo in res.Get())
        {
            result = 100 - Convert.ToInt32(mbo["PercentIdleTime"]);
        }

        return result;
    }
    
}