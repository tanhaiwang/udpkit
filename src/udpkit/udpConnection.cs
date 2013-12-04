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

using System;
namespace UdpKit {
    enum UdpConnectionState : int {
        None = 0,
        Connecting = 1,
        Connected = 2,
        Disconnected = 3,
        Destroy = 4
    }

    enum UdpConnectionError : int {
        None = 0,
        SequenceOutOfBounds = 1,
        IncorrectCommand = 2,
        SendWindowFull = 3,
    }

    enum UdpConnectionMode : int {
        Client = 1,
        Server = 2
    }

    public enum UdpConnectionOption : int {
        MtuSize,
        AlwaysSendMtu
    }

    public class UdpConnection {

        /// <summary>
        /// A user-assignable object
        /// </summary>
        public object UserToken {
            get;
            set;
        }

        /// <summary>
        /// The round-trip time of the network layer, excluding processing delays and ack time
        /// </summary>
        public float NetworkPing {
            get { return networkRtt; }
        }

        /// <summary>
        /// The total round-trip time, including processing delays and ack
        /// </summary>
        public float AliasedPing {
            get { return aliasedRtt; }
        }

        /// <summary>
        /// If this connection is a client
        /// </summary>
        public bool IsClient {
            get { return mode == UdpConnectionMode.Client; }
        }

        /// <summary>
        /// IF this connections a server
        /// </summary>
        public bool IsServer {
            get { return mode == UdpConnectionMode.Server; }
        }

        /// <summary>
        /// If we are connected
        /// </summary>
        public bool IsConnected {
            get { return state == UdpConnectionState.Connected; }
        }

        /// <summary>
        /// If we are disconnected (not connected)
        /// </summary>
        public bool IsDisconnected {
            get { return state != UdpConnectionState.Connected; }
        }

        /// <summary>
        /// The remote end point
        /// </summary>
        public UdpEndPoint RemoteEndPoint {
            get { return endpoint; }
        }

        /// <summary>
        /// Socket object which owns this connection
        /// </summary>
        public UdpSocket Socket {
            get { return socket; }
        }

        /// <summary>
        /// The maximum transmission unit of this connection, see: http://en.wikipedia.org/wiki/Maximum_transmission_unit
        /// </summary>
        public int Mtu {
            get { return mtu; }
        }

        /// <summary>
        /// How much of the current outgoing packet window is waiting for acks
        /// </summary>
        public float WindowFillRatio {
            get { return sendWindow.FillRatio; }
        }

        /// <summary>
        /// Stats for this specific connection
        /// </summary>
        public UdpConnectionStats Stats {
            get { return stats; }
        }

        int mtu;
        float networkRtt = 0.1f;
        float aliasedRtt = 0.1f;
        bool alwaysSendMtu;
        UdpSerializer serializer;
        UdpConnectionMode mode;
        UdpEndPoint endpoint;
        UdpConnectionStats stats;
        uint sendTime;
        ushort sendSequence;
        UdpRingBuffer<UdpHandle> sendWindow;
        uint recvTime;
        ushort recvSequence;
        ulong recvHistory;
        uint recvSinceLastSend;
        uint connectTimeout;
        uint connectAttempts;

        internal UdpSocket socket;
        internal UdpConnectionState state;

        internal UdpConnection (UdpSocket sock, UdpConnectionMode m, UdpEndPoint ep) {
            socket = sock;
            mode = m;
            endpoint = ep;
            networkRtt = socket.Config.DefaultNetworkPing;
            aliasedRtt = socket.Config.DefaultAliasedPing;
            mtu = sock.Config.DefaultMtu;
            alwaysSendMtu = sock.Config.DefaultAlwaysSendMtu;
            serializer = sock.CreateSerializer();
            state = UdpConnectionState.Connecting;
            recvTime = socket.GetCurrentTime();
            sendTime = recvTime;
            stats = new UdpConnectionStats();
            sendWindow = new UdpRingBuffer<UdpHandle>(sock.Config.PacketWindow);
        }

        /// <summary>
        /// Send an object on this connection
        /// </summary>
        /// <param name="obj">The object to send</param>
        public void Send (object obj) {
            socket.Raise(UdpEvent.INTERNAL_SEND, this, obj);
        }

        /// <summary>
        /// Disconnect this connection forcefully
        /// </summary>
        public void Disconnect () {
            socket.Raise(UdpEvent.INTERNAL_DISCONNECT, this);
        }

        /// <summary>
        /// Set option with a bool value on this connection
        /// </summary>
        /// <param name="option">The option to set</param>
        /// <param name="value">The value</param>
        public void SetOption (UdpConnectionOption option, bool value) {
            switch (option) {
                case UdpConnectionOption.AlwaysSendMtu:
                    socket.Raise(UdpEvent.INTERNAL_CONNECTION_OPTION, this, option, value ? 1 : 0);
                    break;
            }
        }

        /// <summary>
        /// Set option with a int value on this connection
        /// </summary>
        /// <param name="option">The option to set</param>
        /// <param name="value">The value</param>
        public void SetOption (UdpConnectionOption option, int value) {
            switch (option) {
                case UdpConnectionOption.MtuSize:
                    socket.Raise(UdpEvent.INTERNAL_CONNECTION_OPTION, this, option, value);
                    break;
            }
        }

        internal void ProcessConnectingTimeouts (uint now) {
            switch (mode) {
                case UdpConnectionMode.Client:
                    if (connectTimeout < now && !SendConnectRequest()) {
                        socket.Raise(UdpEvent.PUBLIC_CONNECT_FAILED, endpoint);

                        // destroy this connection on next timeout check
                        ChangeState(UdpConnectionState.Destroy);
                    }
                    break;
            }
        }

        internal void ProcessConnectedTimeouts (uint now) {
            if ((recvTime + socket.Config.ConnectionTimeout) < now) {
                UdpLog.Debug("disconnecting due to timeout from {0}, recvTime: {1}, now: {2}", endpoint.ToString(), recvTime.ToString(), now.ToString());
                ChangeState(UdpConnectionState.Disconnected);
            }

            if (CheckState(UdpConnectionState.Connected)) {
                if (sendTime + socket.Config.PingTimeout < now || recvSinceLastSend >= socket.Config.RecvWithoutAckLimit) {
                    SendCommand(UdpCommandType.Ping);
                }
            }
        }

        internal void OnEventConnectionOption (UdpEvent ev) {
            switch (ev.Option) {
                case UdpConnectionOption.AlwaysSendMtu:
                    alwaysSendMtu = ev.OptionIntValue == 1;
                    break;

                case UdpConnectionOption.MtuSize:
                    mtu = UdpMath.Clamp(ev.OptionIntValue, socket.Config.MtuMin, socket.Config.MtuMax);
                    break;
            }
        }

        internal void ChangeState (UdpConnectionState newState) {
            if (newState == state)
                return;

            UdpConnectionState oldState = state;

            switch (state = newState) {
                case UdpConnectionState.Connected:
                    OnStateConnected(oldState);
                    break;

                case UdpConnectionState.Disconnected:
                    OnStateDisconnected(oldState);
                    break;
            }
        }

        internal void OnPacket (UdpBitStream buffer) {
            recvTime = socket.GetCurrentTime();

            if ((buffer.Data[0] & 1) == 1) {
                OnObjectReceived(buffer);
            } else {
                OnCommandReceived(buffer);
            }
        }

        void OnCommandReceived (UdpBitStream buffer) {
            if (ParseHeader(buffer)) {
                stats.CommandsReceived += 1;

                buffer.Ptr = UdpHeader.GetSize(socket);
                UdpCommandType cmd = (UdpCommandType) buffer.ReadByte(8);

                switch (cmd) {
                    case UdpCommandType.Connect: OnCommandConnect(buffer); break;
                    case UdpCommandType.Accepted: OnCommandAccepted(buffer); break;
                    case UdpCommandType.Refused: OnCommandRefused(buffer); break;
                    case UdpCommandType.Disconnected: OnCommandDisconnected(buffer); break;
                    case UdpCommandType.Ping: OnCommandPing(buffer); break;
                    default: ConnectionError(UdpConnectionError.IncorrectCommand); break;
                }
            }
        }

        internal bool CheckState (UdpConnectionState stateValue) {
            return state == stateValue;
        }

        internal void SendObject (object o) {
            serializer.SendNext(o);

            while (serializer.HasQueuedObjects) {
                UdpSendFailReason reason = CheckCanSend(false);

                if (reason != UdpSendFailReason.None) {
                    while (serializer.HasQueuedObjects) {
                        socket.Raise(UdpEvent.PUBLIC_OBJECT_SEND_FAILED, this, serializer.NextObject(), reason);
                    }

                    break;
                }

                UdpBitStream stream = new UdpBitStream(socket.GetWriteBuffer(), mtu, UdpHeader.GetSize(socket));
                object obj = serializer.NextObject();

                if (serializer.Pack(ref stream, ref obj)) {
                    if (stream.Overflowing && (socket.Config.AllowPacketOverflow == false)) {
                        UdpLog.Error("stream to {0} is overflowing, not sending", endpoint.ToString());
                        socket.Raise(UdpEvent.PUBLIC_OBJECT_SEND_FAILED, this, obj, UdpSendFailReason.StreamOverflow);
                        return;
                    }

                    UdpHeader header = MakeHeader(true);
                    header.Pack(new UdpBitStream(stream.Data, mtu, 0), socket);

                    UdpHandle handle = MakeHandle(ref header);
                    handle.Object = obj;

                    if (SendStream(stream, handle, alwaysSendMtu) == false) {
                        socket.Raise(UdpEvent.PUBLIC_OBJECT_SEND_FAILED, this, obj, UdpSendFailReason.SocketError);
                    } else {
                        stats.PacketSent();
                    }
                }
            }
        }

        internal void SendCommand (UdpCommandType cmd) {
            if (CheckCanSend(true) == UdpSendFailReason.None) {
                UdpBitStream stream = new UdpBitStream(socket.GetWriteBuffer(), mtu, UdpHeader.GetSize(socket));
                stream.WriteByte((byte) cmd, 8);

                UdpHeader header = MakeHeader(false);
                header.Pack(new UdpBitStream(stream.Data, mtu, 0), socket);

                UdpHandle handle = MakeHandle(ref header);
                handle.Object = null;

                if (SendStream(stream, handle, false) == false) {
                    // do something here?
                }

                stats.CommandSent += 1;
            }
        }

        bool SendStream (UdpBitStream stream, UdpHandle handle, bool expandToMtu) {
            int bytesToSend = UdpMath.BytesRequired(stream.Ptr);
            if (bytesToSend < mtu && expandToMtu) {
                bytesToSend = mtu;
            }

            sendTime = handle.SendTime;
            sendSequence = handle.ObjSequence;
            sendWindow.Enqueue(handle);
            recvSinceLastSend = 0;

            return socket.Send(endpoint, stream.Data, bytesToSend);
        }

        UdpHandle MakeHandle (ref UdpHeader header) {
            UdpHandle handle = new UdpHandle();
            handle.IsObject = header.IsObject;
            handle.ObjSequence = header.ObjSequence;
            handle.SendTime = header.Now;
            return handle;
        }

        UdpHeader MakeHeader (bool isObject) {
            UdpHeader header = new UdpHeader();
            header.IsObject = isObject;
            header.AckHistory = recvHistory;
            header.AckSequence = recvSequence;
            header.ObjSequence = UdpMath.SeqNext(sendSequence, UdpHeader.SEQ_MASK);
            header.Now = socket.GetCurrentTime();

            if (recvTime > 0)
                header.AckTime = (ushort) UdpMath.Clamp(header.Now - recvTime, 0, socket.Config.MaxPing);

            return header;
        }

        UdpSendFailReason CheckCanSend (bool sendingCommand) {
            if (CheckState(UdpConnectionState.Connected) == false || sendWindow.Full) {
                if (sendingCommand && IsClient && CheckState(UdpConnectionState.Connecting)) {
                    return UdpSendFailReason.None;
                }

                if (sendWindow.Full) {
                    ConnectionError(UdpConnectionError.SendWindowFull);
                    return UdpSendFailReason.PacketWindowFull;
                }

                return UdpSendFailReason.NotConnected;
            }

            return UdpSendFailReason.None;
        }

        bool ParseHeader (UdpBitStream buffer) {
            UdpHeader header = new UdpHeader();
            header.Unpack(buffer, socket);

            int seqDistance = UdpMath.SeqDistance(header.ObjSequence, recvSequence, UdpHeader.SEQ_PADD);

            // we have to be within window size
            if (seqDistance > socket.Config.PacketWindow || seqDistance < -socket.Config.PacketWindow) {
                ConnectionError(UdpConnectionError.SequenceOutOfBounds);
                return false;
            }

            // this is an old packet
            if (seqDistance <= 0)
                return false;

            // update receive history
            if (seqDistance >= socket.Config.AckRedundancy) {
                recvHistory = 1UL;
            } else {
                recvHistory = (recvHistory << seqDistance) | 1UL;
            }

            // update our receive stats
            recvSequence = header.ObjSequence;
            recvSinceLastSend += 1;

            // ack sent objects
            AckHandles(header, true);

            return true;
        }

        void OnObjectReceived (UdpBitStream buffer) {
            EnsureClientIsConnected();

            if (CheckState(UdpConnectionState.Connected) == false)
                return;

            if (ParseHeader(buffer)) {
                object obj = null;
                buffer.Ptr = UdpHeader.GetSize(socket);

                if (serializer.Unpack(ref buffer, ref obj)) {
                    socket.Raise(UdpEvent.PUBLIC_OBJECT_RECEIVED, this, obj);
                }

                stats.PacketReceived();
            }
        }

        void OnStateConnected (UdpConnectionState oldState) {
            if (oldState == UdpConnectionState.Connecting) {
                if (IsServer) {
                    SendCommand(UdpCommandType.Accepted);
                }

                socket.Raise(UdpEvent.PUBLIC_CONNECTED, this);
            }
        }

        void OnStateDisconnected (UdpConnectionState oldState) {
            if (oldState == UdpConnectionState.Connected) {
                while (sendWindow.Empty == false) {
                    UdpHandle handle = sendWindow.Dequeue();

                    if (handle.IsObject) {
                        socket.Raise(UdpEvent.PUBLIC_OBJECT_LOST, this, handle.Object);
                    }
                }

                socket.Raise(UdpEvent.PUBLIC_DISCONNECTED, this);
            }
        }

        void OnCommandConnect (UdpBitStream buffer) {
            if (IsServer) {
                if (CheckState(UdpConnectionState.Connected)) {
                    SendCommand(UdpCommandType.Accepted);
                }
            } else {
                ConnectionError(UdpConnectionError.IncorrectCommand);
            }
        }

        void OnCommandAccepted (UdpBitStream buffer) {
            if (IsClient) {
                if (CheckState(UdpConnectionState.Connecting)) {
                    ChangeState(UdpConnectionState.Connected);
                }
            } else {
                ConnectionError(UdpConnectionError.IncorrectCommand);
            }
        }

        void OnCommandRefused (UdpBitStream buffer) {
            if (IsClient) {
                if (CheckState(UdpConnectionState.Connecting)) {
                    socket.Raise(UdpEvent.PUBLIC_CONNECT_REFUSED, endpoint);

                    // destroy this connection on next timeout check
                    ChangeState(UdpConnectionState.Destroy);
                }
            } else {
                ConnectionError(UdpConnectionError.IncorrectCommand);
            }
        }

        void OnCommandDisconnected (UdpBitStream buffer) {
            EnsureClientIsConnected();

            if (CheckState(UdpConnectionState.Connected)) {
                ChangeState(UdpConnectionState.Disconnected);
            }
        }

        void OnCommandPing (UdpBitStream buffer) {
            EnsureClientIsConnected();
        }

        void EnsureClientIsConnected () {
            if (IsClient && state == UdpConnectionState.Connecting && socket.Config.AllowImplicitAccept) {
                ChangeState(UdpConnectionState.Connected);
            }
        }

        void ConnectionError (UdpConnectionError error) {
            UdpLog.Debug("error '{0}' on connection to {1}", error.ToString(), endpoint.ToString());

            switch (error) {
                case UdpConnectionError.SequenceOutOfBounds:
                    ChangeState(UdpConnectionState.Disconnected);
                    break;

                case UdpConnectionError.IncorrectCommand:
                    ChangeState(UdpConnectionState.Disconnected);
                    break;

                case UdpConnectionError.SendWindowFull:
                    ChangeState(UdpConnectionState.Disconnected);
                    break;
            }
        }

        bool SendConnectRequest () {
            if (connectAttempts < socket.Config.ConnectRequestAttempts) {
                SendCommand(UdpCommandType.Connect);

                connectTimeout = socket.GetCurrentTime() + socket.Config.ConnectRequestTimeout;
                connectAttempts += 1u;
                return true;
            }

            return false;
        }

        void AckHandles (UdpHeader header, bool updateRtt) {
            while (!sendWindow.Empty) {
                UdpHandle handle = sendWindow.Peek();

                int seqDistance = UdpMath.SeqDistance(handle.ObjSequence, header.AckSequence, UdpHeader.SEQ_PADD);
                if (seqDistance > 0) {
                    break;
                }

                if (handle.IsObject) {
                    if (seqDistance <= -socket.Config.AckRedundancy) {
                        ObjectLost(handle.Object);
                    }

                    if ((header.AckHistory & (1UL << -seqDistance)) != 0UL) {
                        ObjectDelivered(handle.Object);
                    } else {
                        ObjectLost(handle.Object);
                    }
                }

                if (seqDistance == 0 && header.AckTime > 0) {
                    UpdatePing(recvTime, handle.SendTime, header.AckTime);
                }

                sendWindow.Dequeue();
            }
        }

        void UpdatePing (uint recvTime, uint sendTime, uint ackTime) {
            uint aliased = recvTime - sendTime;
            aliasedRtt = (aliasedRtt * 0.9f) + ((float) aliased / 1000f * 0.1f);

            if (socket.Config.CalculateNetworkPing) {
                uint network = aliased - UdpMath.Clamp(ackTime, 0, aliased);
                networkRtt = (networkRtt * 0.9f) + ((float) network / 1000f * 0.1f);
            }
        }

        void ObjectLost (object o) {
            stats.PacketLost();
            socket.Raise(UdpEvent.PUBLIC_OBJECT_LOST, this, o);
        }

        void ObjectDelivered (object o) {
            stats.PacketDelivered();
            socket.Raise(UdpEvent.PUBLIC_OBJECT_DELIVERED, this, o);
        }
    }
}
