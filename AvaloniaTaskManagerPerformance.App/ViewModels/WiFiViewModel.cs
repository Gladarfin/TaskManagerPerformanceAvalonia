using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaTaskManagerPerformance.App.Models;
using AvaloniaTaskManagerPerformance.App.ViewModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using SkiaSharp;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class WiFiViewModel : ObservableObject
{
    #region Properties

    [ObservableProperty] private List<ISeries> _series;
    [ObservableProperty] private List<ISeries> _seriesPreview;
    

    public Charts Charts { get; } = new();

    #endregion
    
    private readonly List<ObservablePoint> _observableValues;
    
    
    private static SeriesHelper SeriesHelper { get; } = new();
    
    #region Labels

    [ObservableProperty] private string _wiFiLabel = "Wi-Fi";
    [ObservableProperty] private string _wiFiDriverLabel = "Driver Placeholder";
    [ObservableProperty] private string _wiFiTypeLabel = "Беспроводная сеть";
    [ObservableProperty] private string _wiFiThroughputLabel = "Throughput";
    [ObservableProperty] private string _wiFiMaxThroughputLabel = "100 Kbps";
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
        _observableValues = new List<ObservablePoint>();
            
        for (var i = 0; i < 62; i++)
        {
            _observableValues.Add(new ObservablePoint(i, -1));
        }
        
        StartDiskMeasuring();
    }
        
        private void StartDiskMeasuring()
        {

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            
            timer.Tick += (sender, e) =>
            {
                
            };
            
            Task.Run(GetNextWiFiLoadTrackingValue);
            timer.Start();
        }

        private async Task GetNextWiFiLoadTrackingValue()
        {
            while (true)
            {
                RemoveFirstWiFiLoadValue();
                AddNextWiFiLoadValue();
                UpdateSeriesValues();
                await Task.Delay(1000);
            }
        }
    
        private void RemoveFirstWiFiLoadValue()
        {
            _observableValues.RemoveAt(0);
            foreach (var elem in _observableValues)
            {
                elem.X--;
            }
        }
    
        private void AddNextWiFiLoadValue()
        {
            
        }
    
        private void UpdateSeriesValues()
        {
            Series = SeriesHelper.SetSeriesValues(
                new SKColor(252, 243, 235), 
                new SKColor(169, 79, 1), 
                _observableValues);
            SeriesPreview = SeriesHelper.SetSeriesValues(
                new SKColor(252, 243, 235), 
                new SKColor(169, 79, 1),
                _observableValues);
        }
}