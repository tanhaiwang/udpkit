using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace UdpKit.stresstest {
    struct pair {
        public uint seq;
        public uint val;

        public pair (uint seq, uint val) {
            this.seq = seq;
            this.val = val;
        }
    }

    class ConnectionObject {
        bool done = false;
        public uint recvNext = 0;
        public uint fromNumber = 0;
        public uint toNumber = 1 << 11;
        public UdpConnection connection = null;
        public udpSendChannel<uint> sendChan = new udpSendChannel<uint>(6, 8);
        public udpRecvChannel<uint> recvChan = new udpRecvChannel<uint>(6, 8);

        public bool QueueNext () {
            if (fromNumber < toNumber && sendChan.tryInsert(fromNumber)) {
                fromNumber += 1u;
                return true;
            }

            return false;
        }

        public void Ack (pair p) {
            if (sendChan.tryAck(p.seq)) {
                uint seq = 0, val = 0;

                while (sendChan.tryRemoveAcked(ref seq, ref val)) {
                    UdpLog.User("delivered {0} (seq: {1})", val, seq);
                }
            }
        }

        public void Nack (pair p) {
            UdpLog.User("lost {0} (seq: {1})", p.val, p.seq);
            sendChan.tryNack(p.seq);
        }

        public void Send () {
            uint seq = 0, val = 0;

            while (sendChan.tryNextSend(ref seq, ref val)) {
                connection.Send(new pair(seq, val));
            }
        }

        public void Recv (pair p) {
            recvChan.tryAdd(p.seq, p.val);
        }

        public void DequeueReceived () {
            uint seq = 0, val = 0;

            while (recvChan.tryRemove(ref seq, ref val)) {
                if (val != recvNext) {
                    throw new Exception();
                }

                UdpLog.User("received {0} (seq: {1})", val, seq);
                recvNext = val + 1u;

                if (recvNext == toNumber) {
                    done = true;
                }
            }
        }
    }

    class Serializer : UdpSerializer {
        public override bool Pack (ref UdpBitStream buffer, ref object o) {
            pair p = (pair) o;
            buffer.WriteUInt(p.seq, 32);
            buffer.WriteUInt(p.val, 32);

            UdpLog.User("sending {0} (seq: {1})", p.val, p.seq);
            return true;
        }

        public override bool Unpack (ref UdpBitStream buffer, ref object o) {
            pair p = new pair();
            p.seq = buffer.ReadUInt(32);
            p.val = buffer.ReadUInt(32);
            o = p;
            return true;
        }
    }

    abstract class SocketObject {
        protected UdpSocket socket;

        public List<UdpConnection> connections = new List<UdpConnection>(256);

        public abstract ushort Port { get; }
        public abstract void Started ();

        public SocketObject () {
            socket = UdpSocket.Create<UdpPlatformManaged, Serializer>(new UdpConfig { SimulatedLoss = 0.25f, ConnectionTimeout = 100000000, PingTimeout = 10, ConnectionLimit = -1 });
        }

        public void Start () {
            socket.Start(new UdpEndPoint(UdpIPv4Address.Localhost, Port));
            Started();
        }

        public bool Process () {
            UdpEvent ev = default(UdpEvent);
            ConnectionObject co = null;
            pair val = default(pair);

            if (socket.Poll(ref ev)) {
                switch (ev.EventType) {
                    case UdpEventType.Connected:
                        ev.Connection.UserToken = co = new ConnectionObject();
                        co.connection = ev.Connection;
                        connections.Add(ev.Connection);

                        for (int i = 0; i < 16; ++i) {
                            co.QueueNext();
                            co.Send();
                        }

                        break;

                    case UdpEventType.ObjectLost:
                        val = (pair) ev.Object;
                        co = (ConnectionObject) ev.Connection.UserToken;
                        co.Nack(val);
                        co.Send();
                        break;

                    case UdpEventType.ObjectDelivered:
                        val = (pair) ev.Object;
                        co = (ConnectionObject) ev.Connection.UserToken;
                        co.Ack(val);
                        co.QueueNext();
                        co.Send();
                        break;

                    case UdpEventType.ObjectReceived:
                        val = (pair) ev.Object;
                        co = (ConnectionObject) ev.Connection.UserToken;
                        co.Recv(val);
                        co.DequeueReceived();
                        break;

                    case UdpEventType.ObjectRejected:
                        Console.WriteLine("REJECTED");
                        break;

                    case UdpEventType.ObjectSendFailed:
                        Console.WriteLine("SENDFAILED");
                        break;
                }

                return true;
            }

            return false;
        }
    }

    class ClientObject : SocketObject {
        public override ushort Port {
            get { return 0; }
        }

        public override void Started () {
            socket.Connect(new UdpEndPoint(UdpIPv4Address.Localhost, 14000));
        }
    }

    class ServerObject : SocketObject {
        public override ushort Port {
            get { return 14000; }
        }

        public override void Started () {
        }
    }


    class Program {

        static void ProcThread () {
            while (true) {
                bool any = false;

                for (int i = 0; i < sockets.Length; ++i) {
                    any = any || sockets[i].Process();
                }

                if (!any) {
                    Thread.Sleep(1);
                }
            }
        }

        const int ClientCount = 32;

        static SocketObject[] sockets;
        static DateTime start;
        public static int connectionsCreated = 0;
        public static int ConnectionCount = ClientCount * 2;

        static void connectionStats (UdpConnection c) {
            UdpConnectionStats s = c.Stats;
            Console.WriteLine("{0}: {1} / {2} / {3} / {4}", c.RemoteEndPoint, s.PacketsSent, s.PacketsReceived, s.CommandSent, s.CommandsReceived);
        }

        static void Main (string[] args) {
            if(File.Exists("log.txt"))
                File.Delete("log.txt");

            var fs = File.AppendText("log.txt");
            sockets = new SocketObject[ClientCount + 1];
            Thread th = new Thread(ProcThread);
            UdpLog.SetWriter(s => { fs.WriteLine(s); });

            sockets[0] = new ServerObject();

            for (int i = 1; i < sockets.Length; ++i) {
                sockets[i] = new ClientObject();
            }

            start = DateTime.Now;
            th.Start();

            for (int i = 0; i < sockets.Length; ++i) {
                sockets[i].Start();
            }

            while (true) {
                Console.Clear();

                for (int i = 0; i < sockets[0].connections.Count; ++i) {
                    connectionStats(sockets[0].connections[i]);
                }


                Console.WriteLine("-");

                for (int i = 1; i < sockets.Length; ++i) {
                    if (sockets[i].connections.Count > 0) {
                        connectionStats(sockets[i].connections[0]);
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
