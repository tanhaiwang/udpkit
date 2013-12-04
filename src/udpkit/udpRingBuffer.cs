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
    class UdpRingBuffer<T> where T : struct {
        int head;
        int tail;
        int count;

        readonly T[] array;

        public bool Full {
            get { return count == array.Length; }
        }

        public bool Empty {
            get { return count == 0; }
        }

        public float FillRatio {
            get { return UdpMath.Clamp((float) count / (float) array.Length, 0f, 1f); }
        }

        public UdpRingBuffer (int size) {
            array = new T[size];
        }

        public void Enqueue (T item) {
            if (count == array.Length)
                throw new UdpException("buffer is full");

            array[head] = item;
            head = (head + 1) % array.Length;
            count += 1;
        }

        public T Dequeue () {
            if (count == 0)
                throw new UdpException("buffer is empty");

            T item = array[tail];
            array[tail] = default(T);
            tail = (tail + 1) % array.Length;
            count -= 1;
            return item;
        }

        public T Peek () {
            if (count == 0)
                throw new UdpException("buffer is empty");

            return array[tail];
        }
    }
}
