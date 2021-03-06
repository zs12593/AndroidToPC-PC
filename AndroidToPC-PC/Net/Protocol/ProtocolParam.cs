﻿using System;
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

    [DataContract]
    public class Offline : ProtocolParam {
    }

    [DataContract]
    public class Connect : Online {
        public Connect(int deviceType, int onlineType, String deviceName) :
            base(deviceType, onlineType, deviceName) {
        }
    }

    [DataContract]
    public class ConnectResponse : ProtocolParam {
        [DataMember(Order = 2)]
        public bool access { get; set; }
        [DataMember(Order = 3)]
        public string password { get; set; }
        [DataMember(Order = 4)]
        public string message;

        public ConnectResponse(bool access, string password, string message) {
            this.access = access;
            this.password = password;
            this.message = message;
        }
    }

    [DataContract]
    public class ConnectFeedback : ProtocolParam {
    }

    [DataContract]
    public class UnConnect : ProtocolParam {
        [DataMember(Order = 2)]
        public string password { get; set; }

        public UnConnect(string passwro) {
            this.password = password;
        }
    }


    [DataContract]
    public class UnConnectResponse : ProtocolParam {
    }

    [DataContract]
    public class Cursor : ProtocolParam {
        [DataMember(Order = 2)]
        public string password { get; set; }

        public Cursor(string password) {
            this.password = password;
        }
    }

    [DataContract]
    public class MoveCursor : Cursor {
        [DataMember(Order = 3)]
        public int x { get; set; }
        [DataMember(Order = 4)]
        public int y { get; set; }

        public MoveCursor(string password, int x, int y) : base(password) {
            this.x = x;
            this.y = y;
        }
    }

    [DataContract]
    public class Click : Cursor {
        public const int LEFT_BUTTON = 0;
        public const int RIGHT_BUTTON = 1;

        public const int STATE_DOWN = 0;
        public const int STATE_UP = 1;
        public const int STATE_DOWN_UP = 2;

        [DataMember(Order = 3)]
        public int button { get; set; }

        [DataMember(Order = 4)]
        public int state { get; set; }

        public Click(string password, int button, int state) : base(password) {
            this.button = button;
            this.state = state;
        }
    }
}
