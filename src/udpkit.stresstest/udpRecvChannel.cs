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
using System.Text;

namespace UdpKit.stresstest {

    class udpRecvChannel<T> {
        int tail;
        int mask;
        int shift;
        uint next;

        readonly node[] nodes;

        struct node {
            public bool received;
            public uint sequence;
            public T value;
        }

        public udpRecvChannel (int windowBits, int sequenceBits) {
            nodes = new node[1 << windowBits];
            shift = 32 - sequenceBits;
            mask = nodes.Length - 1;
        }

        public bool tryRemove (ref uint sequence, ref T value) {
            node n = nodes[tail];

            if (n.received) {
                value = n.value;
                sequence = n.sequence;
                nodes[tail] = default(node);

                tail += 1;
                tail &= mask;

                next = sequence + 1;
            }

            return n.received;
        }

        public bool tryAdd (uint sequence, T value) {
            int distance = sequenceDistance(sequence, next);
            int index = (tail + distance) & mask;

            if (distance < 0 || distance >= nodes.Length) {
                return false;
            }

            node n = nodes[index];
            if (n.received == false) {
                n.received = true;
                n.sequence = sequence;
                n.value = value;
                nodes[index] = n;
            }

            return true;
        }

        int sequenceDistance (uint from, uint to) {
            from <<= shift;
            to <<= shift;
            return ((int) (from - to)) >> shift;
        }
    }

}