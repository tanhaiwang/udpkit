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
using System.Threading;
using UdpKit;

namespace udpkitdev {
    class main {
        static Thread serverThread;
        static Thread clientThread;

        class serializer : UdpSerializer {
            public override bool Pack (ref UdpBitStream buffer, ref object o) {
                buffer.WriteUInt((uint) o, 32);
                return true;
            }

            public override bool Unpack (ref UdpBitStream buffer, ref object o) {
                o = buffer.ReadUInt(32);
                return true;
            }

            public static serializer factory () {
                return new serializer();
            }
        }

        static void EventLoop (UdpSocket socket) {
            UdpConnection c = null;

            while (true) {
                UdpEvent ev = default(UdpEvent);

                if (socket.Poll(ref ev)) {
                    UdpLog.User(ev.EventType.ToString());

                    switch (ev.EventType) {
                        case UdpEventType.ConnectRequest:
                            socket.Accept(ev.EndPoint);
                            break;

                        case UdpEventType.Connected:
                            c = ev.Connection;
                            break;

                    }
                }

                if (c != null) {
                    c.Send(10u);
                }

                Thread.Sleep(100);
            }
        }

        static void Server () {
            UdpSocket socket = new UdpSocket(new UdpPlatformManaged(), serializer.factory);
            socket.Start(new UdpEndPoint(UdpIPv4Address.Localhost, 14000));
            EventLoop(socket);
        }

        static void Client () {
            UdpSocket socket = new UdpSocket(new UdpPlatformManaged(), serializer.factory);
            socket.Start(new UdpEndPoint(UdpIPv4Address.Localhost, 0));
            socket.Connect(new UdpEndPoint(UdpIPv4Address.Localhost, 14000));
            EventLoop(socket);
        }

        static void Main (string[] args) {
            Console.BufferHeight = 5000;

            UdpLog.SetWriter(Console.WriteLine);

            serverThread = new Thread(Server);
            serverThread.IsBackground = true;
            serverThread.Name = "server";
            serverThread.Start();

            clientThread = new Thread(Client);
            clientThread.IsBackground = true;
            clientThread.Name = "client";
            clientThread.Start();

            Console.ReadLine();
        }
    }
}
