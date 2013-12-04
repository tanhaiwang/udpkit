//#define DISABLE_AUTO_ACCEPT
//#define ENABLE_MANUAL_ACCEPT

using System;
using System.Threading;
using UdpKit;

namespace UdpKit.Examples.Simple {
    class DummySerializer : UdpSerializer {
        public override bool Pack (ref UdpBitStream stream, ref object o) {
            throw new NotImplementedException();
        }

        public override bool Unpack (ref UdpBitStream stream, ref object o) {
            throw new NotImplementedException();
        }
    }

    class Program {
        static void Client () {
            UdpSocket client = UdpSocket.Create<UdpPlatformManaged, DummySerializer>();
            client.Start(UdpEndPoint.Any);
            client.Connect(new UdpEndPoint(UdpIPv4Address.Localhost, 14000));

            while (true) {
                UdpEvent ev = default(UdpEvent);

                while (client.Poll(ref ev)) {
                    UdpLog.User("Event raised {0}", ev.EventType);

                    switch (ev.EventType) {
                        case UdpEventType.Connected:
                            UdpLog.User("Connected to server at {0}", ev.Connection.RemoteEndPoint);
                            break;

#if DISABLE_AUTO_ACCEPT
                        case UdpEventType.ConnectFailed:
                            UdpLog.User("Connection to {0} failed", ev.EndPoint);
                            break;
#endif
                    }
                }
                
                // Simulate ~60fps game loop
                Thread.Sleep(16);
            }
        }

        static void Server () {
#if DISABLE_AUTO_ACCEPT
            UdpConfig config = new UdpConfig();
            config.AutoAcceptIncommingConnections = false;
#else
            UdpConfig config = new UdpConfig();
#endif
            UdpSocket server = UdpSocket.Create<UdpPlatformManaged, DummySerializer>(config);
            server.Start(new UdpEndPoint(UdpIPv4Address.Localhost, 14000));

            while (true) {
                UdpEvent ev = default(UdpEvent);

                while (server.Poll(ref ev)) {
                    UdpLog.User("Event raised {0}", ev.EventType);

                    switch (ev.EventType) {
                        case UdpEventType.Connected:
                            UdpLog.User("Client connected from {0}, total clients connected: {1}", ev.Connection.RemoteEndPoint, server.ConnectionCount);
                            break;

#if ENABLE_MANUAL_ACCEPT
                        case UdpEventType.ConnectRequest:
                            UdpLog.User("Connection requested from {0}", ev.EndPoint);
                            server.Accept(ev.EndPoint);
                            break;
#endif
                    }
                }
                
                // Simulate ~60fps game loop
                Thread.Sleep(16);
            }
        }

        static void Main (string[] args) {
            Console.WriteLine("Example: Simple");
            Console.WriteLine("Press [S] to start server");
            Console.WriteLine("Press [C] to start client");
            Console.Write("... ");

            UdpLog.SetWriter(Console.WriteLine);

            switch (Console.ReadKey(true).Key) {
                case ConsoleKey.S:
                    Console.WriteLine("Server");
                    Server();
                    break;

                case ConsoleKey.C:
                    Console.WriteLine("Client");
                    Client();
                    break;

                default:
                    Main(args);
                    break;
            }
        }
    }
}
