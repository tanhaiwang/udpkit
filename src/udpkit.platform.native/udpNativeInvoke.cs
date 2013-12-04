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
using System.Runtime.InteropServices;
using System.Security;

namespace UdpKit {
    internal static unsafe class UdpNativeInvoke {

        public const string DLL_NAME = 

#if UDPKIT_IOS
		"__Internal";
#elif UDPKIT_ANDROID
        "updkit_android";
#elif UDPKIT_WIN32
        "libudpkit_win32";
#endif

        public const int UDPKIT_SOCKET_OK = 0;
        public const int UDPKIT_SOCKET_ERROR = -1;
        public const int UDPKIT_SOCKET_NOTVALID = -2;
        public const int UDPKIT_SOCKET_NODATA = -3;

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr udpCreate ();

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpBind (IntPtr socket, UdpEndPoint addr);

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpSendTo (IntPtr socket, IntPtr buffer, int size, UdpEndPoint addr);

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpRecvFrom (IntPtr socket, IntPtr buffer, int size, UdpEndPoint* addr);

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpRecvPoll (IntPtr socket, int timeout);

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpLastError (IntPtr socket);

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpGetEndPoint (IntPtr socket, UdpEndPoint* addr);

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern Int32 udpClose (IntPtr socket);
        
		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        static extern IntPtr udpPlatform ();
        public static string udpGetPlatform () { return Marshal.PtrToStringAnsi(udpPlatform()); }
        
		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        static extern IntPtr udpPlatformErrorString (int code);
        public static string udpGetPlatformErrorString (int code) { return Marshal.PtrToStringAnsi(udpPlatformErrorString(code)); }

		[DllImport(DLL_NAME, ExactSpelling = true)]
        [SuppressUnmanagedCodeSecurity]
        public static extern UInt32 udpGetHighPrecisionTime();
    }
}
