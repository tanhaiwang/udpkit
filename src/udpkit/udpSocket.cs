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
using System.Collections.Generic;
using System.Threading;

namespace UdpKit {

    enum udpSocketState : int {
        None = 0,
        Created = 1,
        Running = 2,
        Shutdown = 3
    }

    public class UdpSocket {
        readonly internal UdpConfig Config;

        volatile int frame;
        volatile udpSocketState state;

        readonly Random random;
        readonly Thread threadSocket;
        readonly byte[] objectBuffer;
        readonly byte[] receiveBuffer;
        readonly UdpPlatform platform;
        readonly Queue<UdpEvent> eventQueueIn;
        readonly Queue<UdpEvent> eventQueueOut;
        readonly UdpSerializerFactory serializerFactory;
        readonly List<UdpConnection> connList = new List<UdpConnection>();
        readonly UdpSet<UdpEndPoint> pendingConnections = new UdpSet<UdpEndPoint>(new UdpEndPointComparer());
        readonly Dictionary<UdpEndPoint, UdpConnection> connLookup = new Dictionary<UdpEndPoint, UdpConnection>(new UdpEndPointComparer());

        /// <summary>
        /// Current amount of connections
        /// </summary>
        public int ConnectionCount {
            get { return connLookup.Count; }
        }

        /// <summary>
        /// Local endpoint of this socket
        /// </summary>
        public UdpEndPoint EndPoint {
            get { return platform.EndPoint; }
        }

        public UdpSocket (UdpPlatform platform, UdpSerializerFactory serializerFactory)
            : this(platform, serializerFactory, new UdpConfig()) {
        }

        public UdpSocket (UdpPlatform platform, UdpSerializerFactory serializerFactory, UdpConfig config) {
            this.platform = platform;
            this.serializerFactory = serializerFactory;
            this.Config = config.Duplicate();

            random = new Random(500);
            state = udpSocketState.Created;
            receiveBuffer = new byte[config.MtuMax * 2];
            objectBuffer = new byte[config.MtuMax * 2];

            eventQueueIn = new Queue<UdpEvent>(config.InitialEventQueueSize);
            eventQueueOut = new Queue<UdpEvent>(config.InitialEventQueueSize);

            threadSocket = new Thread(NetworkLoop);
            threadSocket.Name = "udpkit thread";
            threadSocket.IsBackground = true;
            threadSocket.Start();
        }

        /// <summary>
        /// Start this socket
        /// </summary>
        /// <param name="endpoint">The endpoint to bind to</param>
        public void Start (UdpEndPoint endpoint) {
            Raise(UdpEvent.INTERNAL_START, endpoint);
        }

        /// <summary>
        /// Close this socket
        /// </summary>
        public void Close () {
            Raise(UdpEvent.INTERNAL_CLOSE);
        }

        /// <summary>
        /// Connect to remote endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint to connect to</param>
        public void Connect (UdpEndPoint endpoint) {
            Raise(UdpEvent.INTERNAL_CONNECT, endpoint);
        }

        /// <summary>
        /// Accept a connection request from a remote endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint to accept</param>
        public void Accept (UdpEndPoint endpoint) {
            Raise(UdpEvent.INTERNAL_ACCEPT, endpoint);
        }

        /// <summary>
        /// Refuse a connection request from a remote endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint to refuse</param>
        public void Refuse (UdpEndPoint endpoint) {
            Raise(UdpEvent.INTERNAL_REFUSE, endpoint);
        }

        /// <summary>
        /// Poll socket for any events
        /// </summary>
        /// <param name="ev">The current event on this socket</param>
        /// <returns>True if a new event is available, False otherwise</returns>
        public bool Poll (ref UdpEvent ev) {
            lock (eventQueueOut) {
                if (eventQueueOut.Count > 0) {
                    ev = eventQueueOut.Dequeue();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Abort the socket thread for this socket, this will halt all processing but not close the socket
        /// </summary>
        public void Abort () {
            threadSocket.Abort();
        }

        internal void Raise (int eventType) {
            UdpEvent ev = new UdpEvent();
            ev.Type = eventType;
            Raise(ev);
        }

        internal void Raise (int eventType, UdpEndPoint endpoint) {
            UdpEvent ev = new UdpEvent();
            ev.Type = eventType;
            ev.EndPoint = endpoint;
            Raise(ev);
        }

        internal void Raise (int eventType, UdpConnection connection) {
            UdpEvent ev = new UdpEvent();
            ev.Type = eventType;
            ev.Connection = connection;
            Raise(ev);
        }

        internal void Raise (int eventType, UdpConnection connection, object obj) {
            UdpEvent ev = new UdpEvent();
            ev.Type = eventType;
            ev.Connection = connection;
            ev.Object = obj;
            Raise(ev);
        }
        
        internal void Raise (int eventType, UdpConnection connection, object obj, UdpSendFailReason reason) {
            UdpEvent ev = new UdpEvent();
            ev.Type = eventType;
            ev.Connection = connection;
            ev.FailedReason = reason;
            ev.Object = obj;
            Raise(ev);
        }

        internal void Raise (int eventType, UdpConnection connection, UdpConnectionOption option, int value) {
            UdpEvent ev = new UdpEvent();
            ev.Type = eventType;
            ev.Connection = connection;
            ev.Option = option;
            ev.OptionIntValue = value;
            Raise(ev);
        }

        internal bool Send (UdpEndPoint endpoint, byte[] buffer, int length) {
            if (state == udpSocketState.Running || state == udpSocketState.Created) {
                int bytesSent = 0;
                return platform.SendTo(buffer, length, endpoint, ref bytesSent);
            }

            return false;
        }

        internal float RandomFloat () {
            return (float) random.NextDouble();
        }

        internal UdpSerializer CreateSerializer () {
            return serializerFactory();
        }

        internal byte[] GetWriteBuffer () {
            Array.Clear(objectBuffer, 0, objectBuffer.Length);
            return objectBuffer;
        }

        internal uint GetCurrentTime () {
            return platform.PlatformPrecisionTime;
        }

        void Raise (UdpEvent ev) {
            if (ev.IsInternal) {
                lock (eventQueueIn) {
                    eventQueueIn.Enqueue(ev);
                }
            } else {
                lock (eventQueueOut) {
                    eventQueueOut.Enqueue(ev);
                }
            }
        }

        void SendRefusedCommand (UdpEndPoint endpoint) {
            UdpBitStream stream = new UdpBitStream(GetWriteBuffer(), Config.DefaultMtu, UdpHeader.GetSize(this));
            stream.WriteByte((byte) UdpCommandType.Refused, 8);

            UdpHeader header = new UdpHeader();
            header.IsObject = false;
            header.AckHistory = 0;
            header.AckSequence = 1;
            header.ObjSequence = 1;
            header.Now = 0;
            header.Pack(new UdpBitStream(stream.Data, Config.DefaultMtu, 0), this);

            if (Send(endpoint, stream.Data, UdpMath.BytesRequired(stream.Ptr)) == false) {
                // do something here?
            }
        }

        bool ChangeState (udpSocketState from, udpSocketState to) {
            if (CheckState(from)) {
                state = to;
                return true;
            }

            return false;
        }

        bool CheckState (udpSocketState s) {
            if (state != s) {
                return false;
            }

            return true;
        }

        UdpConnection CreateConnection (UdpEndPoint endpoint, UdpConnectionMode mode) {
            if (connLookup.ContainsKey(endpoint)) {
                UdpLog.Warn("connection for {0} already exists", endpoint);
                return default(UdpConnection);
            }

            UdpConnection cn = new UdpConnection(this, mode, endpoint);
            connLookup.Add(endpoint, cn);
            connList.Add(cn);

            return cn;
        }

        bool DestroyConnect (UdpConnection cn) {
            for (int i = 0; i < connList.Count; ++i) {
                if (connList[i] == cn) {
                    connList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        void NetworkLoop () {
            UdpLog.Info("socket created");
            while (state == udpSocketState.Created) {
                ProcessIncommingEvents();
                Thread.Sleep(1);
            }

            UdpLog.Info("socket started");
            while (state == udpSocketState.Running) {
                RecvNetworkData();
                ProcessTimeouts();
                ProcessIncommingEvents();
                frame += 1;
            }

            UdpLog.Info("socket closing");
        }

        void ProcessIncommingEvents () {
            while (true) {
                UdpEvent ev = default(UdpEvent);

                lock (eventQueueIn) {
                    if (eventQueueIn.Count > 0) {
                        ev = eventQueueIn.Dequeue();
                    }
                }

                if (ev.Type == 0) {
                    return;
                }

                switch (ev.Type) {
                    case UdpEvent.INTERNAL_START: OnEventStart(ev); break;
                    case UdpEvent.INTERNAL_CONNECT: OnEventConnect(ev); break;
                    case UdpEvent.INTERNAL_ACCEPT: OnEventAccept(ev); break;
                    case UdpEvent.INTERNAL_REFUSE: OnEventRefuse(ev); break;
                    case UdpEvent.INTERNAL_DISCONNECT: OnEventDisconect(ev); break;
                    case UdpEvent.INTERNAL_CLOSE: OnEventClose(ev); break;
                    case UdpEvent.INTERNAL_SEND: OnEventSend(ev); break;
                    case UdpEvent.INTERNAL_CONNECTION_OPTION: OnEventConnectionOption(ev); break;
                }
            }
        }

        void OnEventStart (UdpEvent ev) {
            if (ChangeState(udpSocketState.Created, udpSocketState.Running)) {
                if (platform.Bind(ev.EndPoint)) {
                    UdpLog.Info("socket bound to {0}", platform.EndPoint.ToString());
                } else {
                    UdpLog.Error("could not bind socket, platform code: {0}, platform error: {1}", platform.PlatformError.ToString(), platform.PlatformErrorString);
                }
            }
        }

        void OnEventConnect (UdpEvent ev) {
            if (CheckState(udpSocketState.Running)) {
                UdpConnection cn = CreateConnection(ev.EndPoint, UdpConnectionMode.Client);

                if (cn == null) {
                    UdpLog.Error("could not create connection for endpoint {0}", ev.EndPoint.ToString());
                }
            }
        }

        void OnEventAccept (UdpEvent ev) {
            if (pendingConnections.Remove(ev.EndPoint)) {
                AcceptConnection(ev.EndPoint);
            }
        }

        void OnEventRefuse (UdpEvent ev) {
            if (pendingConnections.Remove(ev.EndPoint)) {
                SendRefusedCommand(ev.EndPoint);
            }
        }

        void OnEventDisconect (UdpEvent ev) {
            if (ev.Connection.CheckState(UdpConnectionState.Connected)) {
                ev.Connection.SendCommand(UdpCommandType.Disconnected);
                ev.Connection.ChangeState(UdpConnectionState.Disconnected);
            }
        }

        void OnEventClose (UdpEvent ev) {
            if (ChangeState(udpSocketState.Running, udpSocketState.Shutdown)) {
                if (platform.Close() == false) {
                    UdpLog.Error("failed to shutdown socket interface, platform code: {0}", platform.PlatformError.ToString());
                }
            }
        }

        void OnEventSend (UdpEvent ev) {
            ev.Connection.SendObject(ev.Object);
        }

        void OnEventConnectionOption (UdpEvent ev) {
            ev.Connection.OnEventConnectionOption(ev);
        }

        void AcceptConnection (UdpEndPoint ep) {
            UdpConnection cn = CreateConnection(ep, UdpConnectionMode.Server);
            cn.ChangeState(UdpConnectionState.Connected);
        }

        void ProcessTimeouts () {
            if ((frame & 3) == 3) {
                uint now = GetCurrentTime();

                for (int i = 0; i < connList.Count; ++i) {
                    UdpConnection cn = connList[i];

                    switch (cn.state) {
                        case UdpConnectionState.Connecting:
                            cn.ProcessConnectingTimeouts(now);
                            break;

                        case UdpConnectionState.Connected:
                            cn.ProcessConnectedTimeouts(now);
                            break;

                        case UdpConnectionState.Disconnected:
                            cn.ChangeState(UdpConnectionState.Destroy);
                            break;

                        case UdpConnectionState.Destroy:
                            if (DestroyConnect(cn)) {
                                --i;
                            }
                            break;
                    }
                }
            }
        }

        void RecvNetworkData () {
            if (platform.RecvPoll(1)) {
                int byteReceived = 0;
                UdpEndPoint ep = UdpEndPoint.Any;

                if (platform.RecvFrom(receiveBuffer, receiveBuffer.Length, ref byteReceived, ref ep)) {
#if DEBUG
                    if (random.NextDouble() < Config.SimulatedLoss) {
                        UdpLog.Info("simulated loss of packet from {0}", ep.ToString());
                        return;
                    }
#endif
                    UdpConnection cn;

                    if (connLookup.TryGetValue(ep, out cn)) {
                        cn.OnPacket(new UdpBitStream(receiveBuffer, byteReceived));
                    } else {
                        RecvUnconnectedPacket(new UdpBitStream(receiveBuffer, byteReceived), ep);
                    }
                }
            }
        }

        void RecvUnconnectedPacket (UdpBitStream buff, UdpEndPoint ep) {
            buff.Ptr = UdpHeader.GetSize(this);

            if (buff.ReadByte(8) == (byte) UdpCommandType.Connect) {
                if (Config.AllowIncommingConnections && ((connLookup.Count + pendingConnections.Count) < Config.ConnectionLimit || Config.ConnectionLimit == -1)) {
                    if (Config.AutoAcceptIncommingConnections) {
                        AcceptConnection(ep);
                    } else {
                        if (pendingConnections.Add(ep)) {
                            Raise(UdpEvent.PUBLIC_CONNECT_REQUEST, ep);
                        }
                    }
                } else {
                    SendRefusedCommand(ep);
                }
            } else {
                UdpLog.Debug("received invalid header byte in unconnected packet from {0}", ep.ToString());
            }
        }
    }
}
