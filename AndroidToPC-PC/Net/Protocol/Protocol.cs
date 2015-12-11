using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace AndroidToPC_PC.Net.Protocol {
    public class Protocol {
        public ProtocolParam param { get; set; }
        public IPEndPoint host { get; set; }
        public int state { get; set; } // 0 可发送 1 不可发送

        public Protocol(IPEndPoint host, ProtocolParam p) {
            this.host = host;
            this.param = p;
            this.state = 0;
        }

        public Protocol(IPEndPoint host, String data) {
            this.param = createProtocolParam(data);
            this.host = host;
            this.state = 1;
        }

        public void send() {
            if (state == 1) return;
            new Thread(sendMessage).Start();
        }

        private void sendMessage(object obj) {
            byte[] sendbytes = Encoding.UTF8.GetBytes(param.ToString());
            UdpClient sendClient = new UdpClient();
            sendClient.Send(sendbytes, sendbytes.Length, host);
            sendClient.Close();

            System.Console.WriteLine("==== Message Send Finished");
        }

        private ProtocolParam createProtocolParam(string json) {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProtocolParam));
                ProtocolParam obj = (ProtocolParam)serializer.ReadObject(ms);
                ms.Seek(0, SeekOrigin.Begin);
                Type t = Type.GetType("AndroidToPC_PC.Net.Protocol." + obj.protocolName);
                serializer = new DataContractJsonSerializer(t);
                obj = (ProtocolParam)serializer.ReadObject(ms);
                return obj;
            }
        }
    }
}
