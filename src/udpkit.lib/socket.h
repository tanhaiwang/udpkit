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

#include "common.h"

#define UDPKIT_SOCKET_OK 0
#define UDPKIT_SOCKET_ERROR -1
#define UDPKIT_SOCKET_NOTVALID -2
#define UDPKIT_SOCKET_NODATA -3
#define UDPKIT_CHECK_RESULT(result) \
if (result == SOCKET_ERROR) { \
	socket->lastError = udpLastError(); \
	return UDPKIT_SOCKET_ERROR; \
} \

#define UDPKIT_CHECK_VALIDSOCKET(s) \
if (s == NULL || s->nativeSocket == INVALID_SOCKET) { \
	return UDPKIT_SOCKET_NOTVALID; \
} \

#if UDPKIT_WIN
	#ifndef _WIN32_WINNT_WINXP 
		#define UDPKIT_WIN_WSAPOLL
	#endif

	#include <WinSock2.h>
	#include <WS2tcpip.h>
	#include <stdio.h>
	#pragma comment(lib, "Ws2_32.lib")
#else
    #include <unistd.h>
    #include <sys/types.h>
    #include <sys/socket.h>
    #include <sys/poll.h>
    #include <arpa/inet.h>
    #include <netdb.h>
    #include <netinet/in.h>
    #include <errno.h>
    #include <sys/ioctl.h>

    #define INVALID_SOCKET -1
    #define SOCKET_ERROR -1
#endif

struct udpEndPoint {
	U32 address;
	U16 port;
};

struct udpSocket {
	S32 nativeSocket;
	S32 lastError;
	U32 sendBufferSize;
	U32 recvBufferSize;
	udpEndPoint endPoint;
};

EXPORT_API udpSocket* udpCreate();
EXPORT_API S32 udpBind(udpSocket* socket, udpEndPoint addr);
EXPORT_API S32 udpSendTo(udpSocket* socket, char* buffer, S32 size, udpEndPoint addr);
EXPORT_API S32 udpRecvFrom(udpSocket* socket, char* buffer, S32 size, udpEndPoint* addr);
EXPORT_API S32 udpRecvPoll(udpSocket* socket, S32 timeoutMs);
EXPORT_API S32 udpLastError(udpSocket* socket);
EXPORT_API S32 udpClose(udpSocket* socket);
EXPORT_API S32 udpGetEndPoint(udpSocket* socket, udpEndPoint* endPoint);
