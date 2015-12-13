using AndroidToPC_PC.Net.Manager;
using System.Windows;
using System.ComponentModel;
using AndroidToPC_PC.Net.Protocol;
using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace AndroidToPC_PC {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private BindingList<DeviceItem> listDatas = new BindingList<DeviceItem>();
        // 请求连接
        private Thread connectThread;
        private string connectIp;
        private bool connectAccess;
        private int connectTimes;
        private string connectMsg;
        private const int CONNECT_TIMES = 5;
        private bool isConnected = false;

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
            if (isConnected || (connectThread != null && connectThread.IsAlive)) {
                connectMsg = "已和其它设备配对...";
                return;
            }

            connectMsg = "";
            isConnected = false;
            connectAccess = false;
            connectIp = p.host.Address.ToString();

            ThreadPool.QueueUserWorkItem(delegate {
                this.Dispatcher.Invoke(new Action(() => {
                    Connect connect = (Connect)p.param;

                    DeviceItem item = new DeviceItem() {
                        DeviceName = connect.deviceName,
                        DeviceIp = p.host.Address.ToString(),
                        DeviceState = "配对中..."
                    };
                    if (!listDatas.Contains(item)) {
                        listDatas.Add(item);
                    }

                    RequestDialog dialog = new RequestDialog();
                    dialog.reqDevicename = connect.deviceName;
                    dialog.ShowDialog();
                    connectAccess = dialog.Access;
                    connectThread = new Thread(connectResponse);
                    connectThread.Start();

                }), null);
            });
        }

        private void connectResponse() {
            if (!connectAccess)
                connectTimes = 0;
            else
                connectTimes = CONNECT_TIMES - 1;

            while (true) {
                UDPSendManager.sendConnectResponse(connectIp, connectAccess, connectMsg);

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
            isConnected = success;

            ThreadPool.QueueUserWorkItem(delegate {
                this.Dispatcher.Invoke(new Action(() => {
                    refresh.IsEnabled = !isConnected;

                    foreach (DeviceItem item in listDatas) {
                        if (item.DeviceIp.Equals(connectIp)) {
                            item.DeviceState = success ? "配对成功" : "空闲";
                            break;
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

    class DeviceItem : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        private string deviceName = String.Empty;
        private string deviceIp = String.Empty;
        private string deviceState = String.Empty;

        public string DeviceName {
            get { return deviceName; }
            set {
                if (value != this.deviceName) {
                    this.deviceName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string DeviceIp {
            get { return deviceIp; }
            set {
                if (value != this.deviceIp) {
                    this.deviceIp = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string DeviceState {
            get { return deviceState; }
            set {
                if (value != this.deviceState) {
                    this.deviceState = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
