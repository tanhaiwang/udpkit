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
using System.Net;
using System.Net.Sockets;

namespace UdpKit {
    public sealed class UdpPlatformManaged : UdpPlatform {
        Socket socket;
        EndPoint recvEndPoint;
        IPAddress convertAddress;
        IPEndPoint convertEndPoint;
        SocketError socketError;

        public UdpPlatformManaged () {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;

            SetConnReset(socket);

            recvEndPoint = new IPEndPoint(IPAddress.Any, 0);
            convertEndPoint = new IPEndPoint(IPAddress.Any, 0);
            convertAddress = new IPAddress(0L);
        }

        public override UdpSocketInterfaceError Error {
            get {
                switch (socketError) {
                    case SocketError.WouldBlock:
                        return UdpSocketInterfaceError.WouldBlock;

                    default:
                        return UdpSocketInterfaceError.Unknown;
                }
            }
        }

        public override UdpEndPoint EndPoint {
            get { return ConvertEndPoint((IPEndPoint) socket.LocalEndPoint); }
        }

        public override long PlatformError {
            get { return (long) socketError; }
        }

        public override string PlatformErrorString {
            get { return socketError.ToString(); }
        }

        public override uint PlatformPrecisionTime {
            get { return UdpPrecisionTimer.GetCurrentTime(); }
        }

        public override bool Close () {
            try {
                socket.Close();
                return true;
            } catch (SocketException exn) {
                socketError = exn.SocketErrorCode;
                return false;
            }
        }

        public override bool Bind (UdpEndPoint endpoint) {
            try {
                socket.Bind(ConvertEndPoint(endpoint));
                return true;
            } catch (SocketException exn) {
                socketError = exn.SocketErrorCode;
                return false;
            }
        }

        public override bool RecvPoll (int timeoutInMs) {
            try {
                return socket.Poll(timeoutInMs * 1000, SelectMode.SelectRead);
            } catch (SocketException exn) {
                socketError = exn.SocketErrorCode;
                return false;
            }
        }

        public override bool RecvFrom (byte[] buffer, int bufferSize, ref int bytesReceived, ref UdpEndPoint remoteEndpoint) {
            try {
                bytesReceived = socket.ReceiveFrom(buffer, 0, bufferSize, SocketFlags.None, ref recvEndPoint);

                if (bytesReceived > 0) {
                    remoteEndpoint = ConvertEndPoint((IPEndPoint) recvEndPoint);
                    return true;
                } else {
                    return false;
                }
            } catch (SocketException exn) {
                socketError = exn.SocketErrorCode;
                return false;
            }
        }

        public override bool SendTo (byte[] buffer, int bytesToSend, UdpEndPoint endpoint, ref int bytesSent) {
            try {
                bytesSent = socket.SendTo(buffer, 0, bytesToSend, SocketFlags.None, ConvertEndPoint(endpoint));
                return bytesSent == bytesToSend;
            } catch (SocketException exn) {
                socketError = exn.SocketErrorCode;
                return false;
            }
        }

#pragma warning disable 618
        UdpEndPoint ConvertEndPoint (IPEndPoint endpoint) {
            return new UdpEndPoint(new UdpIPv4Address(endpoint.Address.Address), (ushort) endpoint.Port);
        }

        IPEndPoint ConvertEndPoint (UdpEndPoint endpoint) {
            long netOrder = IPAddress.HostToNetworkOrder((int) endpoint.Address.Packet);
            convertAddress.Address = netOrder;
            convertEndPoint.Address = convertAddress;
            convertEndPoint.Port = endpoint.Port;
            return convertEndPoint;
        }
#pragma warning restore 618

        void SetConnReset (Socket s) {
            try {
                const uint IOC_IN      = 0x80000000;
                const uint IOC_VENDOR  = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                s.IOControl((int) SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            } catch { }
        }
    }
}
