using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class WiFiViewModel : ObservableObject
{
    #region Properties
    
    [ObservableProperty] private string _send;
    [ObservableProperty] private string _receive;
    [ObservableProperty] private string _sendAndReceive;
    [ObservableProperty] private string _ssid;
    [ObservableProperty] private string _connectionType;
    [ObservableProperty] private string _ipv4;
    [ObservableProperty] private string _ipv6;

    [ObservableProperty] private List<ISeries> _multiplySeries;
    [ObservableProperty] private List<ISeries> _multiplySeriesPreview;
    [ObservableProperty] private static List<Axis> _yAxisMultiplySeries;
    public Charts Charts { get; } = new();

    #endregion
    
    private readonly List<ObservablePoint> _receiveValues;
    private readonly List<ObservablePoint> _sendValues;
    private readonly object _dataLock = new object();

    //Maybe this isn't right way to get Network values
    private static PerformanceCounter _dataSentCounter;
    private static PerformanceCounter _dataReceivedCounter;
    private string _networkCard = "";
    private float _sendKbytes;
    private float _receiveKbytes;
    private const int Index = 62;
    private readonly DashEffect _effect = new(new float[]{ 3, 2 });
    private static SeriesHelper SeriesHelper { get; } = new();
    
    #region Labels

    [ObservableProperty] private string _wiFiLabel = "Wi-Fi";
    [ObservableProperty] private string _wiFiDriverLabel = "";
    [ObservableProperty] private string _wiFiTypeLabel = "";
    [ObservableProperty] private string _wiFiDnsName = "";
    [ObservableProperty] private string _wiFiThroughputLabel = "Throughput";
    [ObservableProperty] private string _wiFiMaxThroughputLabel = "20 Mbps";
    [ObservableProperty] private string _maxTime = "60 seconds";
    [ObservableProperty] private string _minTime = "0";
    [ObservableProperty] private string _sendLabel = "Send";
    [ObservableProperty] private string _receiveLabel = "Receive";
    [ObservableProperty] private string _adapterNameLabel = "Adapter name:";
    [ObservableProperty] private string _ssidLabel = "SSID:";
    [ObservableProperty] private string _dnsNameLabel = "DNS name:";
    [ObservableProperty] private string _connectionTypeLabel = "Connection type:";
    [ObservableProperty] private string _ipv4AddressLabel = "IPv4 address:";
    [ObservableProperty] private string _ipv6AddressLabel = "IPv6 address:";

    #endregion

    public WiFiViewModel()
    {
        _sendValues = new List<ObservablePoint>();
        _receiveValues = new List<ObservablePoint>();

        for (var i = 0; i < 62; i++)
        {
            _sendValues.Add(new ObservablePoint(i, -1));
            _receiveValues.Add(new ObservablePoint(i, -1));
        }
        
        YAxisMultiplySeries = new List<Axis>
        {
            new Axis
            {
                MaxLimit = 20000,
                MinLimit = 0,
                MinStep = 10,
                IsVisible = false
            }
        };

        GetNetworkCard();
        AssignPerformanceCounter();
        AssignWiFiConstValues();
        StartWiFiMeasuring();
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

        WiFiTypeLabel = adapter.Name;
        WiFiDnsName = adapter.GetIPProperties().DnsSuffix;
        

        var description = s[s.IndexOf("Description")..];
        var nextIndex = description.IndexOf(":") + 2;
        WiFiDriverLabel = description.Substring(nextIndex, description.IndexOf(Environment.NewLine) - nextIndex).Trim();
        

        var ssid = s[s.IndexOf("SSID")..];
        nextIndex = ssid.IndexOf(":") + 2;
        Ssid = ssid.Substring(nextIndex, ssid.IndexOf(Environment.NewLine) - nextIndex).Trim();

        var radioType = s[s.IndexOf("Radio type")..];
        nextIndex = radioType.IndexOf(":") + 2;
        ConnectionType = radioType.Substring(nextIndex, radioType.IndexOf(Environment.NewLine) - nextIndex).Trim();
    }

    private void StartWiFiMeasuring()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        timer.Tick += (sender, e) =>
        {
            lock (_dataLock)
            {
                _sendKbytes =  8 * _dataSentCounter.NextValue() / 1000;
                _receiveKbytes = 8 * _dataReceivedCounter.NextValue() / 1000;
            }

            RemoveFirstWiFiLoadValue();
            AddNextWiFiLoadValue();
            UpdateSeriesValues();
        };
        timer.Start();
    }

    private void RemoveFirstWiFiLoadValue()
    {
        _receiveValues.RemoveAt(0);
        _sendValues.RemoveAt(0);

        foreach (var point in _receiveValues)
        {
            point.X--;
        }

        foreach (var point in _sendValues)
        {
            point.X--;
        }
    }

    private void AddNextWiFiLoadValue()
    {
        lock (_dataLock)
        {
            Send = _sendKbytes > 1001 ? $"{ _sendKbytes / 1000:F1} Mbps" : $"{_sendKbytes:F1} Kbps";
            Receive = _receiveKbytes > 1001 ? $"{ _receiveKbytes / 1000:F1} Mbps" : $"{_receiveKbytes:F1} Kbps";
            SendAndReceive = $"S: {Send[..Send.IndexOf(" ")]} R: {Receive}";
            _receiveValues.Add(new ObservablePoint(Index, (int)_receiveKbytes));
            _sendValues.Add(new ObservablePoint(Index,(int)_sendKbytes));
        }
        
    }
    
    private void UpdateSeriesValues()
    {
        MultiplySeries = SeriesHelper.SetMultiplySeriesValues(
            new SKColor(252, 243, 235),
            new SKColor(167, 79, 1), 
            _receiveValues, 
            _sendValues, 
            _effect);
        MultiplySeriesPreview = SeriesHelper.SetMultiplySeriesValues(
            new SKColor(252, 243, 235),
            new SKColor(167, 79, 1), 
            _receiveValues, 
            _sendValues, 
            _effect);
    }
}