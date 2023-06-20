using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTaskManagerPerformance.App.ViewModels;

public partial class WiFiViewModel : ObservableObject
{
    #region Properties

    

    #endregion
    
    #region Labels

    [ObservableProperty] private string _wifiLabel = "Wi-Fi";
    [ObservableProperty] private string _wifiTypeLabel = "Беспроводная сеть";
    [ObservableProperty] private string _wifiThroughputLabel = "Throughput";
    [ObservableProperty] private string _wifiMaxThroughputLabel = "10 Kbps";
    [ObservableProperty] private string _sendLabel = "Send";
    [ObservableProperty] private string _receiveLabel = "Receive";
    [ObservableProperty] private string _adapterNameLabel = "Adapter name:";
    [ObservableProperty] private string _ssidLabel = "SSID:";
    [ObservableProperty] private string _dnsNameLabel = "DNS name:";
    [ObservableProperty] private string _connectionTypeLabel = "Connection type:";
    [ObservableProperty] private string _ipv4AddressLabel = "IPv4 address:";
    [ObservableProperty] private string _ipv6AddressLabel = "IPv6 address:";

    #endregion
}