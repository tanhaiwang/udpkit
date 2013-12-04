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

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UdpKit {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UdpEndPoint {
        public static readonly UdpEndPoint Any = new UdpEndPoint(UdpIPv4Address.Any, 0);
        public readonly UdpIPv4Address Address;
        public readonly ushort Port;

        public UdpEndPoint (string endpoint) {
            string[] parts = endpoint.Split(':');
            this.Address = new UdpIPv4Address(parts[0]);
            this.Port = ushort.Parse(parts[1]);
        }

        public UdpEndPoint (UdpIPv4Address address, ushort port) {
            this.Address = address;
            this.Port = port;
        }

        public override int GetHashCode () {
            return (int) (Address.Packet ^ Port);
        }

        public override bool Equals (object obj) {
            if (obj is UdpEndPoint) {
                return Compare(this, (UdpEndPoint) obj) == 0;
            }

            return false;
        }

        public override string ToString () {
            return string.Format("{0}.{1}.{2}.{3}:{4}", Address.Byte3, Address.Byte2, Address.Byte1, Address.Byte0, Port);
        }

        public static bool operator == (UdpEndPoint x, UdpEndPoint y) {
            return Compare(x, y) == 0;
        }

        public static bool operator != (UdpEndPoint x, UdpEndPoint y) {
            return Compare(x, y) != 0;
        }

        internal static int Compare (UdpEndPoint x, UdpEndPoint y) {
            if (x.Address.Packet > y.Address.Packet) return 1;
            if (x.Address.Packet < y.Address.Packet) return -1;

            if (x.Port > y.Port) return 1;
            if (x.Port < y.Port) return -1;

            return 0;
        }
    }

    public class UdpEndPointComparer : IEqualityComparer<UdpEndPoint> {
        bool IEqualityComparer<UdpEndPoint>.Equals (UdpEndPoint x, UdpEndPoint y) {
            return UdpEndPoint.Compare(x, y) == 0;
        }

        int IEqualityComparer<UdpEndPoint>.GetHashCode (UdpEndPoint obj) {
            return obj.GetHashCode();
        }
    }
}
