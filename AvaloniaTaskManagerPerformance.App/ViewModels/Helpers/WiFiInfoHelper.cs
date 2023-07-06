using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;

public class WiFiInfoHelper
{
    //Maybe this isn't right way to get Network values
    private static PerformanceCounter _dataSentCounter;
    private static PerformanceCounter _dataReceivedCounter;

    private string _networkCard;
    public string Ipv4 { get; set; }
    public string Ipv6 { get; set; }
    public string WiFiType { get; set; }
    public string WiFiDnsName { get; set; }
    public string WiFiDriver { get; set; }
    public string Ssid { get; set; }
    public string ConnectionType { get; set; }
    public float SendKbytes { get; set; }
    public float ReceiveKbytes { get; set; }

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
        var adapter = NetworkInterface.GetAllNetworkInterfaces()
            .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && x.OperationalStatus == OperationalStatus.Up)
            .FirstOrDefault();
        
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
        

        var description = s[s.IndexOf("Description")..];
        var nextIndex = description.IndexOf(":") + 2;
        WiFiDriver = description.Substring(nextIndex, description.IndexOf(Environment.NewLine) - nextIndex).Trim();
        

        var ssid = s[s.IndexOf("SSID")..];
        nextIndex = ssid.IndexOf(":") + 2;
        Ssid = ssid.Substring(nextIndex, ssid.IndexOf(Environment.NewLine) - nextIndex).Trim();

        var radioType = s[s.IndexOf("Radio type")..];
        nextIndex = radioType.IndexOf(":") + 2;
        ConnectionType = radioType.Substring(nextIndex, radioType.IndexOf(Environment.NewLine) - nextIndex).Trim();
    }

    public void GetWiFiValues()
    {
        SendKbytes =  8 * _dataSentCounter.NextValue() / 1000;
        ReceiveKbytes = 8 * _dataReceivedCounter.NextValue() / 1000;
    }
}