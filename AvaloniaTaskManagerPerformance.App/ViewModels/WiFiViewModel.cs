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
    
    private static WiFiInfoHelper WiFiHelper { get; set; }
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

        WiFiHelper = new WiFiInfoHelper();
        AssignConstValues();
        StartWiFiMeasuring();
    }

    private void AssignConstValues()
    {
        Ipv4 = WiFiHelper.Ipv4;
        Ipv6 = WiFiHelper.Ipv6;
        WiFiTypeLabel = WiFiHelper.WiFiType;
        WiFiDnsName = WiFiHelper.WiFiDnsName;
        WiFiDriverLabel = WiFiHelper.WiFiDriver;
        Ssid = WiFiHelper.Ssid;
        ConnectionType = WiFiHelper.ConnectionType;
    }

    private void StartWiFiMeasuring()
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        timer.Tick += (sender, e) =>
        {
            WiFiHelper.GetWiFiValues();
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

        for (var i = 0; i < _receiveValues.Count; i++)
        {
            _receiveValues[i].X--;
            _sendValues[i].X--;
        }
    }

    private void AddNextWiFiLoadValue()
    {
        Send = WiFiHelper.SendKbytes > 1001 ? $"{ WiFiHelper.SendKbytes / 1000:F1} Mbps" : $"{WiFiHelper.SendKbytes:F1} Kbps";
        Receive = WiFiHelper.ReceiveKbytes > 1001 ? $"{ WiFiHelper.ReceiveKbytes / 1000:F1} Mbps" : $"{WiFiHelper.ReceiveKbytes:F1} Kbps";
        SendAndReceive = $"S: {WiFiHelper.SendKbytes:F1} R: {WiFiHelper.ReceiveKbytes:F1} Kbps";
        _receiveValues.Add(new ObservablePoint(Index, (int)WiFiHelper.ReceiveKbytes));
        _sendValues.Add(new ObservablePoint(Index,(int)WiFiHelper.SendKbytes));
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