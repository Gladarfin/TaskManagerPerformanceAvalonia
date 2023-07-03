namespace AvaloniaTaskManagerPerformance.App.Models;

public class ProcessorInfo
{
    public string Name { get; set; }
    public uint ThreadCount { get; set; }
    public uint NumberOfCores { get; set; }
    public uint NumberOfLogicalProcessors { get; set; }
    public int L1Cache { get; set; }
    public double L2Cache { get; set; }
    public double L3Cache { get; set; }
    public double MaxClockSpeed { get; set; }
    public string Virtualization { get; set; }
    
    
}