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
using System.Diagnostics;
using System.Threading;

namespace UdpKit {
    static class UdpAssert {
        [Conditional("DEBUG")]
        internal static void Assert (bool condition) {
            if (!condition)
                throw new UdpException("assert failed");
        }

        [Conditional("DEBUG")]
        internal static void Assert (bool condition, string message) {
            if (!condition)
                throw new UdpException(String.Concat("assert failed: ", message));
        }

        [Conditional("DEBUG")]
        internal static void Assert (bool condition, string message, params object[] args) {
            if (!condition)
                throw new UdpException(String.Concat("assert failed: ", String.Format(message, args)));
        }

        [Conditional("DEBUG")]
        internal static void AssertThread (Thread thread) {
            Assert(ReferenceEquals(Thread.CurrentThread, thread), "expected thread to be '{0}', but was '{1}'", thread.Name, Thread.CurrentThread.Name);
        }
    }
}
