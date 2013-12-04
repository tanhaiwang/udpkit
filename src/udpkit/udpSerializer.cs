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

namespace UdpKit {
    public delegate UdpSerializer UdpSerializerFactory ();

    public abstract class UdpSerializer {
        readonly Queue<object> sendQueue = new Queue<object>();

        internal bool HasQueuedObjects {
            get { return sendQueue.Count > 0; }
        }

        internal object NextObject () {
            return sendQueue.Dequeue();
        }

        /// <summary>
        /// The connection which owns this serializer
        /// </summary>
        public UdpConnection Connection {
            get;
            internal set;
        }

        /// <summary>
        /// Queue an object for immediate sending after the current object has been packed
        /// </summary>
        /// <param name="o">The object to send</param>
        public void SendNext (object o) {
            sendQueue.Enqueue(o);
        }

        /// <summary>
        /// Reject an object from being sent, sending it back to the event-thread
        /// </summary>
        /// <param name="o">The object to reject</param>
        public void Reject (object o) {
            Connection.socket.Raise(UdpEvent.PUBLIC_OBJECT_REJECTED, Connection, o);
        }

        public abstract bool Pack (ref UdpBitStream buffer, ref object o);
        public abstract bool Unpack (ref UdpBitStream buffer, ref object o);

    }
}
