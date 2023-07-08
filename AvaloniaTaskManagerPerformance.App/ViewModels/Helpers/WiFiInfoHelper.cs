using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
[SupportedOSPlatform("windows")]
public class WiFiInfoHelper
{
    //Maybe this isn't right way to get Network values
    private static PerformanceCounter _dataSentCounter;
    private static PerformanceCounter _dataReceivedCounter;

    private string _networkCard;
    public string Ipv4 { get; private set; }
    public string Ipv6 { get; private set; }
    public string WiFiType { get; private set; }
    public string WiFiDnsName { get; private set; }
    public string WiFiDriver { get; private set; }
    public string Ssid { get; private set; }
    public string ConnectionType { get; private set; }
    public float SendKbytes { get; private set; }
    public float ReceiveKbytes { get; private set; }

    public WiFiInfoHelper()
    {
        GetNetworkCard();
        AssignPerformanceCounter();
        AssignWiFiConstValues();
    }
    
    private void GetNetworkCard()
    {
        var category = new PerformanceCounterCategory("Network Interface");
        var instanceNames = category.GetInstanceNames();

        foreach (var name in instanceNames)
        {
            _networkCard = name;
        }
    }
    private void AssignPerformanceCounter()
    {
        _dataSentCounter = new ("Network Interface", "Bytes Sent/sec", _networkCard);   
        _dataReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", _networkCard);
        _dataSentCounter.NextValue();
        _dataReceivedCounter.NextValue();
    }
    
    private void AssignWiFiConstValues()
    {
        //I don't know why, but I can't get Name from the process even if I change cultureInfo (it's in Cyrillic on my laptop).
        //But I can get it in the console application, so maybe it's Avalonia. I don't know. It's not a big deal, because I need
        //this for the DNS name anyway.
        var adapter = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(x => x is { 
                NetworkInterfaceType: NetworkInterfaceType.Wireless80211,
                OperationalStatus: OperationalStatus.Up 
            });
        
        var strHostName = Dns.GetHostName(); ;
        var ipEntry = Dns.GetHostEntry(strHostName);
        var addr = ipEntry.AddressList;

        Ipv6 = addr[0].ToString();
        Ipv4 = addr[1].ToString();
        
        var p = new Process();
        p.StartInfo.FileName = "netsh.exe";
        p.StartInfo.Arguments = "wlan show interfaces";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();
        var s = p.StandardOutput.ReadToEnd();

        WiFiType = adapter.Name;
        WiFiDnsName = adapter.GetIPProperties().DnsSuffix;
        

        var description = s[s.IndexOf("Description", StringComparison.Ordinal)..];
        var nextIndex = description.IndexOf(":", StringComparison.Ordinal) + 2;
        WiFiDriver = description.Substring(nextIndex, description.IndexOf(Environment.NewLine, StringComparison.Ordinal) - nextIndex).Trim();
        

        var ssid = s[s.IndexOf("SSID", StringComparison.Ordinal)..];
        nextIndex = ssid.IndexOf(":", StringComparison.Ordinal) + 2;
        Ssid = ssid.Substring(nextIndex, ssid.IndexOf(Environment.NewLine, StringComparison.Ordinal) - nextIndex).Trim();

        var radioType = s[s.IndexOf("Radio type", StringComparison.Ordinal)..];
        nextIndex = radioType.IndexOf(":", StringComparison.Ordinal) + 2;
        ConnectionType = radioType.Substring(nextIndex, radioType.IndexOf(Environment.NewLine, StringComparison.Ordinal) - nextIndex).Trim();
    }

    public void GetWiFiValues()
    {
        SendKbytes =  8 * _dataSentCounter.NextValue() / 1000;
        ReceiveKbytes = 8 * _dataReceivedCounter.NextValue() / 1000;
    }
}