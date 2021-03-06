﻿using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace AndroidToPC_PC.Net.Manager {
    public delegate void ReceiveCallback(Protocol.Protocol protocol);

    public enum ReceiveType {
        Online, Offline, Connect, UnConnect, ConnectFeedback, MoveCursor, Click
    }

    class UDPReceiveManager {
        private const int RECEIVE_PORT = 10250;
        private const int RECEIVE_SIZE = 512;
        private static UDPReceiveManager instance;
        private Thread receiveThread;
        private bool isActive { get; set; }
        private Dictionary<ReceiveType, ReceiveCallback> receiveCallbacks = new Dictionary<ReceiveType, ReceiveCallback>();

        private UDPReceiveManager() { }
        public static UDPReceiveManager getInstance() {
            if (instance == null) {
                instance = new UDPReceiveManager();
            }
            return instance;
        }

        public void startReceiveListen() {
            if (isActive) return;
            isActive = true;
            receiveThread = new Thread(receiveMessage);
            receiveThread.Start();
        }

        public void stopReceiveListen() {
            isActive = false;
            if (receiveThread != null && receiveThread.IsAlive) {
                receiveThread.Abort();
            }
        }

        private void receiveMessage(object obj) {
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint localIpep = new IPEndPoint(IPAddress.Any, RECEIVE_PORT);
            UdpClient reveiceClient = new UdpClient(localIpep);
            try {
                while (isActive) {
                    byte[] bytRecv = reveiceClient.Receive(ref remoteIpep);
                    string message = Encoding.UTF8.GetString(bytRecv, 0, bytRecv.Length);
                    System.Console.WriteLine("====" + message);
                    Protocol.Protocol p = new Protocol.Protocol(remoteIpep, message);
                    if (p.param != null) {
                        foreach (var callback in receiveCallbacks) {
                            if (callback.Key.ToString().Equals(p.param.protocolName)) {
                                callback.Value(p);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                reveiceClient.Close();
                isActive = false;
            }
        }

        public void addReceiveCallback(ReceiveType type, ReceiveCallback callback) {
            receiveCallbacks.Add(type, callback);
        }

        public void removeReceiveCallback(ReceiveType type) {
            receiveCallbacks.Remove(type);
        }

        public void clearCallback() {
            receiveCallbacks.Clear();
        }

    }

}
