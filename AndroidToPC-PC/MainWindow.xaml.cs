using AndroidToPC_PC.Net.Manager;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using AndroidToPC_PC.Net.Protocol;
using System;
using System.Threading;

namespace AndroidToPC_PC {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private ObservableCollection<DeviceItem> listDatas = new ObservableCollection<DeviceItem>();
        private Thread connectThread;
        private string connectIp;
        private bool connectAccess;
        private int connectTimes;
        private const int CONNECT_TIMES = 5;

        public MainWindow() {
            InitializeComponent();

            UDPReceiveManager.getInstance().startReceiveListen();
            UDPReceiveManager.getInstance().addReceiveCallback(ReceiveType.Online, onlineCallback);
            UDPReceiveManager.getInstance().addReceiveCallback(ReceiveType.Offline, offlineCallback);
            UDPReceiveManager.getInstance().addReceiveCallback(ReceiveType.Connect, connectCallback);
            UDPReceiveManager.getInstance().addReceiveCallback(ReceiveType.ConnectFeedback, connectFeedbackCallback);
            UDPReceiveManager.getInstance().addReceiveCallback(ReceiveType.MoveCursor, moveCursorCallback);
            UDPReceiveManager.getInstance().addReceiveCallback(ReceiveType.Click, clickCallback);
            UDPSendManager.sendOnlineMessage(null);

            listView.ItemsSource = listDatas;
        }

        private void refresh_Click(object sender, RoutedEventArgs e) {
            listDatas.Clear();
            UDPSendManager.sendOnlineMessage(null);
        }

        private void onlineCallback(Protocol p) {
            Online online = (Online)p.param;
            if (online.onlineType == Online.SELF_ONLINE) {
                UDPSendManager.sendOnlineMessage(p.host);
            }

            ThreadPool.QueueUserWorkItem(delegate {
                this.Dispatcher.Invoke(new Action(() => {
                    DeviceItem item = new DeviceItem() {
                        DeviceName = online.deviceName,
                        DeviceIp = p.host.Address.ToString(),
                        DeviceState = "空闲"
                    };
                    if (!listDatas.Contains(item)) {
                        listDatas.Add(item);
                    }
                    if (p.param is Connect) {
                        item.DeviceState = "配对中...";
                    }
                }), null);
            });
        }

        private void offlineCallback(Protocol p) {
            ThreadPool.QueueUserWorkItem(delegate {
                this.Dispatcher.Invoke(new Action(() => {
                    foreach (DeviceItem item in listDatas) {
                        if (item.DeviceIp.Equals(p.host.Address.ToString())) {
                            listDatas.Remove(item);
                            break;
                        }
                    }
                }), null);
            });
        }

        private void connectCallback(Protocol p) {
            if (connectThread != null && connectThread.IsAlive) return;

            onlineCallback(p);

            Connect connect = (Connect)p.param;
            MessageBoxResult result = MessageBox.Show("\"" + connect.deviceName + "\"请求与你配对,是否同意？",
                "配对请求", MessageBoxButton.YesNo);
            connectIp = p.host.Address.ToString();
            connectAccess = MessageBoxResult.Yes.Equals(result);
            connectThread = new Thread(connectResponse);
            connectThread.Start();
        }

        private void connectResponse() {
            if (connectAccess)
                connectTimes = 0;
            else
                connectTimes = CONNECT_TIMES - 1;

            while (true) {
                UDPSendManager.sendConnectResponse(connectIp, connectAccess);

                if (++connectTimes > CONNECT_TIMES) {
                    showConnectResult(false);
                    break;
                }
                try {
                    Thread.Sleep(1000);
                } catch (Exception ex) {
                    System.Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        private void connectFeedbackCallback(Protocol p) {
            if (connectThread != null && connectThread.IsAlive) {
                connectThread.Abort();
            }

            showConnectResult(connectAccess);
        }

        private void showConnectResult(bool success) {
            ThreadPool.QueueUserWorkItem(delegate {
                this.Dispatcher.Invoke(new Action(() => {
                    foreach (DeviceItem item in listDatas) {
                        if (item.DeviceIp.Equals(connectIp)) {
                            item.DeviceState = success ? "配对成功" : "空闲";
                        }
                    }
                }), null);
            });
        }

        private void moveCursorCallback(Protocol p) {
            MoveCursor mc = (MoveCursor)p.param;
            if (mc.passwrod != null && !mc.passwrod.Equals("") &&
                mc.passwrod.Equals(AndroidToPC_PC.Net.Protocol.Cursor.CONNECTED_PASSWORD)) {
            }
        }

        private void clickCallback(Protocol p) {
            Click click = (Click)p.param;
            if (click.passwrod != null && !click.passwrod.Equals("") &&
                click.passwrod.Equals(AndroidToPC_PC.Net.Protocol.Cursor.CONNECTED_PASSWORD)) {
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            UDPReceiveManager.getInstance().clearCallback();
            UDPReceiveManager.getInstance().stopReceiveListen();
            UDPSendManager.sendOfflineMessage();

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            System.Environment.Exit(0);
        }

    }

    class DeviceItem {
        public string DeviceName { get; set; }
        public string DeviceIp { get; set; }
        public string DeviceState { get; set; }

        public override bool Equals(object obj) {
            if (obj != null && obj is DeviceItem) {
                DeviceItem other = (DeviceItem)obj;
                return DeviceIp.Equals(other.DeviceIp);
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
