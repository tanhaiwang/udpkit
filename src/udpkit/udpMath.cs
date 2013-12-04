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
    internal static class UdpMath {
        internal static bool IsSet (uint mask, uint flag) {
            return (mask & flag) == flag;
        }

        internal static uint NextPow2 (uint v) {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        internal static int HighBit (uint v) {
            if (v == 0)
                return 0;

            int r = 0;

            do {
                r += 1;
            } while ((v >>= 1) > 0);

            return r;
        }

        internal static int BytesRequired (int bits) {
            return (bits + 7) >> 3;
        }

        internal static int SeqDistance (ushort from, ushort to, int shift) {
            from <<= shift;
            to <<= shift;
            return ((short) (from - to)) >> shift;
        }

        internal static ushort SeqNext (ushort seq, ushort mask) {
            seq += 1;
            seq &= mask;
            return seq;
        }

        internal static ushort SeqPrev (ushort seq, ushort mask) {
            seq -= 1;
            seq &= mask;
            return seq;
        }
        
        internal static ushort Clamp (ushort value, ushort min, ushort max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static float Clamp (float value, float min, float max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
        
        internal static int Clamp (int value, int min, int max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static uint Clamp (uint value, uint min, uint max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        internal static byte Clamp (byte value, byte min, byte max) {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
