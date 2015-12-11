using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace AndroidToPC_PC.Net.Protocol {

    [DataContract]
    public class ProtocolParam {
        [DataMember(Order = 0)]
        public long id { set; get; }
        [DataMember(Order = 1)]
        public String protocolName { set; get; }

        public ProtocolParam() {
            string[] names = this.GetType().FullName.Split('.');
            this.protocolName = names[names.Length - 1];
        }

        public override string ToString() {
            using (var ms = new MemoryStream()) {
                new DataContractJsonSerializer(this.GetType()).WriteObject(ms, this);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }

    [DataContract]
    public class Online : ProtocolParam {
        public const int DEVICE_ANDROID = 0;
        public const int DEVICE_PC = 1;

        public const int SELF_ONLINE = 0;
        public const int ONLINE_FEEDBACK = 1;

        [DataMember(Order = 2)]
        public int deviceType { set; get; } // 0 Android 1 PC
        [DataMember(Order = 3)]
        public int onlineType { set; get; } // SELF_ONLINE 主动上线  ONLINE_FEEDBACK 上线回文
        [DataMember(Order = 4)]
        public String deviceName { set; get; }

        public Online(int deviceType, int onlineType, String deviceName) {
            this.deviceType = deviceType;
            this.onlineType = onlineType;
            this.deviceName = deviceName;
        }
    }
}
