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

using System.Net;
using System.Runtime.InteropServices;

namespace UdpKit {
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct UdpIPv4Address {
        public static readonly UdpIPv4Address Any = new UdpIPv4Address();
        public static readonly UdpIPv4Address Localhost = new UdpIPv4Address(127, 0, 0, 1);

        [FieldOffset(0)]
        public readonly uint Packet;
        [FieldOffset(0)]
        public readonly byte Byte0;
        [FieldOffset(1)]
        public readonly byte Byte1;
        [FieldOffset(2)]
        public readonly byte Byte2;
        [FieldOffset(3)]
        public readonly byte Byte3;

        public UdpIPv4Address (long addr) {
            Byte0 = Byte1 = Byte2 = Byte3 = 0;
            Packet = (uint) IPAddress.NetworkToHostOrder((int) addr);
        }

        public UdpIPv4Address (string ip) {
            string[] parts = ip.Split('.');
            Packet = 0;
            Byte0 = byte.Parse(parts[3]);
            Byte1 = byte.Parse(parts[2]);
            Byte2 = byte.Parse(parts[1]);
            Byte3 = byte.Parse(parts[0]);
        }

        public UdpIPv4Address (byte a, byte b, byte c, byte d) {
            Packet = 0;
            Byte0 = d;
            Byte1 = c;
            Byte2 = b;
            Byte3 = a;
        }

        public override int GetHashCode () {
            return (int) Packet;
        }

        public override bool Equals (object obj) {
            if (obj is UdpIPv4Address) {
                return Compare(this, (UdpIPv4Address) obj) == 0;
            }

            return false;
        }

        public override string ToString () {
            return string.Format("{0}.{1}.{2}.{3}", Byte3, Byte2, Byte1, Byte0);
        }

        public static bool operator == (UdpIPv4Address x, UdpIPv4Address y) {
            return Compare(x, y) == 0;
        }

        public static bool operator != (UdpIPv4Address x, UdpIPv4Address y) {
            return Compare(x, y) != 0;
        }

        static int Compare (UdpIPv4Address x, UdpIPv4Address y) {
            if (x.Packet > y.Packet) return 1;
            if (x.Packet < y.Packet) return -1;
            return 0;
        }

    }
}
