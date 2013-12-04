/*
* The MIT License (MIT)
* 
* Copyright (c) 2012-2013 Fredrik Holmstrom (fredrik.johan.holmstrom@gmail.com)
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.Runtime.InteropServices;

namespace UdpKit {
    public enum UdpSendFailReason {
        None, 
        StreamOverflow, 
        NotConnected, 
        PacketWindowFull, 
        SocketError,
        SerializerReturnedFalse
    }

    public enum UdpEventType {
        ConnectRequest = UdpEvent.PUBLIC_CONNECT_REQUEST,
        ConnectFailed = UdpEvent.PUBLIC_CONNECT_FAILED,
        ConnectRefused = UdpEvent.PUBLIC_CONNECT_REFUSED,
        Connected = UdpEvent.PUBLIC_CONNECTED,
        Disconnected = UdpEvent.PUBLIC_DISCONNECTED,
        ObjectSendFailed = UdpEvent.PUBLIC_OBJECT_SEND_FAILED,
        ObjectRejected = UdpEvent.PUBLIC_OBJECT_REJECTED,
        ObjectDelivered = UdpEvent.PUBLIC_OBJECT_DELIVERED,
        ObjectLost = UdpEvent.PUBLIC_OBJECT_LOST,
        ObjectReceived = UdpEvent.PUBLIC_OBJECT_RECEIVED
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct UdpEvent {

        struct UdpEventReferenceObjects {
            public UdpConnection Connection;
            public object Object;
        }

        internal const int INTERNAL_START = 1;
        internal const int INTERNAL_CONNECT = 3;
        internal const int INTERNAL_ACCEPT = 5;
        internal const int INTERNAL_REFUSE = 7;
        internal const int INTERNAL_DISCONNECT = 9;
        internal const int INTERNAL_CLOSE = 11;
        internal const int INTERNAL_SEND = 13;
        internal const int INTERNAL_CONNECTION_OPTION = 15;

        internal const int PUBLIC_CONNECT_REQUEST = 2;
        internal const int PUBLIC_CONNECT_FAILED = 4;
        internal const int PUBLIC_CONNECT_REFUSED = 6;
        internal const int PUBLIC_CONNECTED = 8;
        internal const int PUBLIC_DISCONNECTED = 10;
        internal const int PUBLIC_OBJECT_SEND_FAILED = 12;
        internal const int PUBLIC_OBJECT_REJECTED = 14;
        internal const int PUBLIC_OBJECT_DELIVERED = 16;
        internal const int PUBLIC_OBJECT_LOST = 18;
        internal const int PUBLIC_OBJECT_RECEIVED = 20;

        [FieldOffset(0)]
        internal int Type;

        [FieldOffset(4)]
        UdpEndPoint endPoint;

        [FieldOffset(4)]
        internal UdpConnectionOption Option;

        [FieldOffset(4)]
        UdpSendFailReason failReason;

        [FieldOffset(8)]
        internal int OptionIntValue;

        [FieldOffset(8)]
        internal int OptionFloatValue;

        [FieldOffset(16)]
        UdpEventReferenceObjects Refs;

        internal bool IsInternal {
            get { return (Type & 1) == 1; }
        }

        public UdpEventType EventType {
            get { return (UdpEventType) Type; }
        }

        public UdpEndPoint EndPoint {
            get { return endPoint; }
            internal set { endPoint = value; }
        }

        public UdpSendFailReason FailedReason {
            get { return failReason; }
            internal set { failReason = value; }
        }

        public UdpConnection Connection {
            get { return Refs.Connection; }
            internal set { Refs.Connection = value; }
        }

        public object Object {
            get { return Refs.Object; }
            internal set { Refs.Object = value; }
        }
    }
}
