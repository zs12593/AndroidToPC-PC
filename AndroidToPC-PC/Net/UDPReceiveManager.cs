using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace AndroidToPC_PC.Net {
    class UDPReceiveManager {
        private const int RECEIVE_SIZE = 1024;
        private const int PORT = 10250;
        private static UDPReceiveManager instance;
        private Thread receiveThread;
        private UdpClient reveiceClient;

        private UDPReceiveManager() { }
        public static UDPReceiveManager getInstance() {
            if (instance == null) {
                instance = new UDPReceiveManager();
            }
            return instance;
        }

        public void startReceiveListen() {
            receiveThread = new Thread(receiveMessage);
            receiveThread.Start();
        }

        public void stopReceiveListen() {
        }

        public void receiveMessage(object obj) {
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);
            while (true) {
            }
        }

    }
}
