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

    class udpSendChannel<T> {
        int tail;
        int mask;
        int shift;
        int count;
        node[] nodes;
        udpSequenceGenerator generator;

        enum state {
            empty,
            queued,
            pending,
            delivered
        }

        struct node {
            public state state;
            public uint sequence;
            public T value;
        }

        public bool isFull {
            get { return count == nodes.Length; }
        }

        public bool isEmpty {
            get { return count == 0; }
        }

        public int inQueue {
            get { return count; }
        }

        public udpSendChannel (int windowBits, int sequenceBits) {
            nodes = new node[1 << windowBits];
            shift = 32 - sequenceBits;
            mask = nodes.Length - 1;
            generator = new udpSequenceGenerator(sequenceBits, uint.MaxValue);
        }

        public bool tryNack (uint sequence) {
            return tryChangeState(sequence, state.queued);
        }

        public bool tryAck (uint sequence) {
            return tryChangeState(sequence, state.delivered);
        }

        bool tryChangeState (uint sequence, state state) {
            if (count == 0) {
                return false;
            }

            int distance = sequenceDistance(sequence, nodes[tail].sequence);
            if (distance < 0 || distance >= count) {
                return false;
            }

            nodes[(tail + distance) & mask].state = state;
            return true;
        }

        public bool tryNextSend (ref uint sequence, ref T value) {
            if (count == 0)
                return false;

            for (int i = 0; i < count; ++i) {
                int index = (tail + i) & mask;

                if (nodes[index].state == state.queued) {
                    sequence = nodes[index].sequence;
                    value = nodes[index].value;
                    nodes[index].state = state.pending;
                    return true;
                }
            }

            return false;
        }

        public bool tryRemoveAcked (ref uint sequence, ref T value) {
            if (count > 0 && nodes[tail].state == state.delivered) {
                sequence = nodes[tail].sequence;
                value = nodes[tail].value;
                nodes[tail] = new node();

                tail += 1;
                tail &= mask;

                count -= 1;
                return true;
            }

            return false;
        }

        public bool tryInsert (T value) {
            if (count == 0) {
                nodes[tail].sequence = generator.next();
                nodes[tail].state = state.queued;
                nodes[tail].value = value;
            } else {
                if (count == nodes.Length) {
                    return false;
                }

                int index = (tail + count) & mask;
                nodes[index].state = state.queued;
                nodes[index].sequence = generator.next();
                nodes[index].value = value;
            }

            count += 1;
            return true;
        }

        int sequenceDistance (uint from, uint to) {
            from <<= shift;
            to <<= shift;
            return ((int) (from - to)) >> shift;
        }
    }
}