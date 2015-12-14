using AndroidToPC_PC.Net.Protocol;
using System.Net;

namespace AndroidToPC_PC.Net.Manager {
    public class UDPSendManager {
        private const int SEND_PORT = 10251;

        public static void sendOnlineMessage(IPEndPoint host) {
            bool self = false;
            if (host == null) {
                self = true;
                host = new IPEndPoint(IPAddress.Parse("255.255.255.255"), SEND_PORT);
            } else {
                host.Port = SEND_PORT;
            }
            Protocol.Protocol p = new Protocol.Protocol(host,
                new Protocol.Online(
                    Protocol.Online.DEVICE_PC,
                    self ? Protocol.Online.SELF_ONLINE : Protocol.Online.ONLINE_FEEDBACK,
                    Dns.GetHostName()));
            p.send();
        }

        public static void sendOfflineMessage() {
            IPEndPoint host = new IPEndPoint(IPAddress.Parse("255.255.255.255"), SEND_PORT);
            Protocol.Protocol p = new Protocol.Protocol(host, new Protocol.Offline());
            p.send();
        }

        public static void sendConnectResponse(string ip, bool access, string message) {
            IPEndPoint host = new IPEndPoint(IPAddress.Parse("255.255.255.255"), SEND_PORT);
            if (access) {
                ConnectedData.ip = ip;
                ConnectedData.password = generatePassword(access);
            }
            Protocol.Protocol p = new Protocol.Protocol(
                host, new Protocol.ConnectResponse(access, ConnectedData.password, message));
            p.send();
        }

        public static void sendUnConnectResponse(string ip) {
            IPEndPoint host = new IPEndPoint(IPAddress.Parse("255.255.255.255"), SEND_PORT);
            Protocol.Protocol p = new Protocol.Protocol(host, new Protocol.UnConnectResponse());
            p.send();
        }

        private static string generatePassword(bool access) {
            string pw = "";
            if (access) {
                return "12345678";
            }
            return pw;
        }

    }
}
