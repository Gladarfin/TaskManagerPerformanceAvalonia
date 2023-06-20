namespace AvaloniaTaskManagerPerformance.App.Models;

public class DiskInfo
{
    public uint Capacity { get; set; }
    public uint Formatted { get; set; }
    public bool SystemDisk { get; set; }
    public bool PageFile { get; set; }
    public string Type { get; set; }
    
}