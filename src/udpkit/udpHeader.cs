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

namespace UdpKit {
    struct UdpHeader {
        public const int SEQ_BITS = 15;
        public const int SEQ_PADD = 16 - SEQ_BITS;
        public const int SEQ_MASK = (1 << SEQ_BITS) - 1;

        public ushort ObjSequence;
        public ushort AckSequence;
        public ulong AckHistory;
        public ushort AckTime;
        public bool IsObject;
        public ushort BitSize;
        public uint Now;

        public void Pack (UdpBitStream buffer, UdpSocket socket) {
            buffer.WriteUShort(PadSequence(ObjSequence), 16);
            buffer.WriteUShort(PadSequence(AckSequence), 16);
            buffer.WriteULong(AckHistory, socket.Config.AckRedundancy);

            if (socket.Config.CalculateNetworkPing) {
                buffer.WriteUShort(AckTime, 16);
            }

            if (socket.Config.WritePacketBitSize) {
                buffer.WriteUShort(BitSize, 16);
            }
        }

        public void Unpack (UdpBitStream buffer, UdpSocket socket) {
            ObjSequence = TrimSequence(buffer.ReadUShort(16));
            AckSequence = TrimSequence(buffer.ReadUShort(16));
            AckHistory = buffer.ReadULong(socket.Config.AckRedundancy);

            if (socket.Config.CalculateNetworkPing) {
                AckTime = buffer.ReadUShort(16);
            }

            if (socket.Config.WritePacketBitSize) {
                BitSize = buffer.ReadUShort(16);
            }
        }

        public static int GetSize (UdpSocket socket) {
            return 16 + 16 + socket.Config.AckRedundancy + (socket.Config.CalculateNetworkPing ? 16 : 0) + (socket.Config.WritePacketBitSize ? 16 : 0);
        }

        ushort PadSequence (ushort sequence) {
            sequence <<= SEQ_PADD;

            if (IsObject)
                sequence |= ((1 << SEQ_PADD) - 1);

            return sequence;
        }

        ushort TrimSequence (ushort sequence) {
            sequence >>= SEQ_PADD;
            return sequence;
        }
    }
}
