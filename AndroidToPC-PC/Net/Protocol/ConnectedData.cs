
namespace AndroidToPC_PC.Net.Protocol {
    class ConnectedData {
        public static string ip { get; set; }
        public static string password { get; set; }

        public static bool isAccess(Protocol p) {
            string hostIp = p.host.Address.ToString();
            if (hostIp != null && hostIp.Equals(ip)) {
                string pw = "";
                if (p.param is UnConnect) {
                    UnConnect uc = (UnConnect)p.param;
                    pw = uc.password;
                } else if (p.param is Cursor) {
                    Cursor cur = (Cursor)p.param;
                    pw = cur.password;
                }
                if (pw != null && pw.Equals(password)) {
                    return true;
                }
            }

            return false;
        }
    }
}
