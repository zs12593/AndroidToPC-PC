using System.Windows;

namespace AndroidToPC_PC {
    /// <summary>
    /// RequestDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RequestDialog : Window {

        public string reqDevicename { get; set; }
        private bool access;
        public bool Access { get { return access; } }

        public RequestDialog() {
            InitializeComponent();
            access = false;
        }

        private void yesBtn_Click(object sender, RoutedEventArgs e) {
            access = true;
            this.Close();
        }

        private void noBtn_Click(object sender, RoutedEventArgs e) {
            access = false;
            this.Close();
        }
    }
}
