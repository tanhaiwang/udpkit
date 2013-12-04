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
    public static class UdpLog {
        public delegate void Writer (string msg);

        public const uint INFO = 1;
        public const uint USER = 2;
        public const uint DEBUG = 4;
        public const uint WARN = 8;

        static uint enabled = INFO | USER | DEBUG | WARN;
        static Writer writer = null;
        static readonly object sync = new object();

        static void Write (string message) {
            lock (UdpLog.sync) {
                Writer callback = UdpLog.writer;

                if (callback != null)
                    callback(message);
            }
        }

        static string Time () {
            return DateTime.Now.ToString("H:mm:ss:fff");
        }

        static string ThreadName () {
            return " | thread #" + Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(3, '0');
        }

        static internal void Info (string format, params object[] args) {
            if (UdpMath.IsSet(UdpLog.enabled, UdpLog.INFO))
                Write(String.Concat(Time(), ThreadName(), " | info  | ", String.Format(format, args)));
        }

        static public void User (string format, params object[] args) {
            if (UdpMath.IsSet(UdpLog.enabled, UdpLog.USER))
                Write(String.Concat(Time(), ThreadName(), " | user  | ", String.Format(format, args)));
        }

        [Conditional("DEBUG")]
        static internal void Debug (string format, params object[] args) {
#if DEBUG
            if (UdpMath.IsSet(UdpLog.enabled, UdpLog.DEBUG))
                Write(String.Concat(Time(), ThreadName(), " | debug | ", String.Format(format, args), "\r\n", Environment.StackTrace));
#endif
        }

        static internal void Warn (string format, params object[] args) {

            if (UdpMath.IsSet(UdpLog.enabled, UdpLog.WARN)) {
#if DEBUG
                Write(String.Concat(Time(), ThreadName(), " | warn  | ", String.Format(format, args), "\r\n", Environment.StackTrace));
#else
                write(String.Concat(timePadded(), threadName(), " | warn  | ", String.Format(format, args)));
#endif
            }
        }

        static internal void Error (string format, params object[] args) {
#if DEBUG
            Write(String.Concat(Time(), ThreadName(), " | error | ", String.Format(format, args), "\r\n", Environment.StackTrace));
#else
            write(String.Concat(timePadded(), threadName(), " | error | ", String.Format(format, args)));
#endif
        }

        static public void SetWriter (UdpLog.Writer callback) {
            UdpLog.writer = callback;
        }

        static public void Disable (uint flag) {
            enabled &= ~flag;
        }

        static public void Enable (uint flag) {
            enabled |= flag;
        }
    }
}