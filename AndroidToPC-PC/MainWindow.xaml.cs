using AndroidToPC_PC.Net.Manager;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using AndroidToPC_PC.Net.Protocol;
using System;

namespace AndroidToPC_PC {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private ObservableCollection<DeviceItem> listDatas = new ObservableCollection<DeviceItem>();

        public MainWindow() {
            InitializeComponent();

            UDPReceiveManager.getInstance().startReceiveListen();
            UDPReceiveManager.getInstance().addReceiveCallback("Online", onlineCallback);
            UDPSendManager.sendOnlineMessage(null);

            listView.ItemsSource = listDatas;
        }

        private void refresh_Click(object sender, RoutedEventArgs e) {
            listDatas.Clear();
            UDPSendManager.sendOnlineMessage(null);
        }

        private void onlineCallback(Protocol p) {
            Online online = (Online)p.param;
            listDatas.Add(new DeviceItem() {
                DeviceName = online.deviceName,
                DeviceIp = p.host.Address.ToString(),
                DeviceState = "空闲"
            });

            if (online.onlineType == Online.SELF_ONLINE) {
                UDPSendManager.sendOnlineMessage(p.host);
            }
        }

        protected override void OnClosed(EventArgs e) {
            UDPReceiveManager.getInstance().clearCallback();
            UDPReceiveManager.getInstance().stopReceiveListen();
            base.OnClosed(e);
            System.Environment.Exit(0);
        }

    }

    class DeviceItem {
        public string DeviceName { get; set; }
        public string DeviceIp { get; set; }
        public string DeviceState { get; set; }
    }
}
