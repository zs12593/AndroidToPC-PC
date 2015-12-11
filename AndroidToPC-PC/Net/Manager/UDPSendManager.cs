using System.Net;

namespace AndroidToPC_PC.Net.Manager {
    public class UDPSendManager {
        public const int SEND_PORT = 10250;

        public static void sendOnlineMessage(IPEndPoint host) {
            bool self = false;
            if (host == null) {
                self = true;
                host = new IPEndPoint(IPAddress.Parse("255.255.255.255"),
                    UDPSendManager.SEND_PORT);
            }
            Protocol.Protocol p = new Protocol.Protocol(host,
                new Protocol.Online(
                    Protocol.Online.DEVICE_PC,
                    self ? Protocol.Online.SELF_ONLINE : Protocol.Online.ONLINE_FEEDBACK,
                    Dns.GetHostName()));
            p.send();
        }

    }
}
