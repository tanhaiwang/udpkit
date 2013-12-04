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

namespace UdpKit {
	public sealed unsafe class 
#if UDPKIT_IOS
	UdpPlatformIOS
#elif UDPKIT_ANDROID
	UdpPlatformAndroid
#elif UDPKIT_WIN32
    UdpPlatformWin32
#endif
	: UdpPlatform {
        IntPtr ptr;

        public override UdpEndPoint EndPoint {
            get { 
                UdpEndPoint ep = default(UdpEndPoint);

                if (UdpNativeInvoke.udpGetEndPoint(ptr, &ep) == UdpNativeInvoke.UDPKIT_SOCKET_OK) {
                    return ep;
                }

                return UdpEndPoint.Any;
            }
        }

        public override UdpSocketInterfaceError Error {
            get { return UdpSocketInterfaceError.Unknown; }
        }
        
        public override long PlatformError {
            get { return UdpNativeInvoke.udpLastError(ptr); }
        }

        public override string PlatformErrorString {
            get { return UdpNativeInvoke.udpGetPlatformErrorString((int)PlatformError);  }
        }
        
#if UDPKIT_IOS || UDPKIT_ANDROID
        public override uint PlatformPrecisionTime {
            get { return UdpNativeInvoke.udpGetHighPrecisionTime(); }
        }
#elif UDPKIT_WIN32
        public override uint PlatformPrecisionTime {
            get { return UdpPrecisionTimer.GetCurrentTime(); }
        }
#endif

        public 
#if UDPKIT_IOS
	UdpPlatformIOS
#elif UDPKIT_ANDROID
	UdpPlatformAndroid
#elif UDPKIT_WIN32
    UdpPlatformWin32
#endif
		() {
            ptr = UdpNativeInvoke.udpCreate();
        }


        public override bool Close () {
            return UdpNativeInvoke.udpClose(ptr) == UdpNativeInvoke.UDPKIT_SOCKET_OK;
        }

        public override bool Bind (UdpEndPoint endpoint) {
            return UdpNativeInvoke.udpBind(ptr, endpoint) == UdpNativeInvoke.UDPKIT_SOCKET_OK;
        }

        public override bool RecvPoll (int timeoutInMs) {
            return UdpNativeInvoke.udpRecvPoll(ptr, timeoutInMs) == UdpNativeInvoke.UDPKIT_SOCKET_OK;
        }

        public override bool RecvFrom (byte[] buffer, int bufferSize, ref int bytesReceived, ref UdpEndPoint remoteEndpoint) {
            UdpEndPoint nativeEndpoint = default(UdpEndPoint);

            fixed (byte* p = buffer) {
                bytesReceived = UdpNativeInvoke.udpRecvFrom(ptr, new IntPtr(p), bufferSize, &nativeEndpoint);
            }

            if (bytesReceived > 0) {
                remoteEndpoint = nativeEndpoint;
                return true;
            }

            return false;
        }

        public override bool SendTo (byte[] buffer, int bytesToSend, UdpEndPoint endpoint, ref int bytesSent) {
            fixed (byte* p = buffer) {
                return bytesToSend == (bytesSent = UdpNativeInvoke.udpSendTo(ptr, new IntPtr(p), bytesToSend, endpoint));
            }
        }
    }
}
