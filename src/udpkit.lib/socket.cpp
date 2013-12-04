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

#include "socket.h"

static int udpStartCalls = 0;

static bool udpStart() {
	bool ok = true;

#if UDPKIT_WIN
	if (udpStartCalls == 0) {
		WSADATA wsaData = { 0 };
		ok = WSAStartup(MAKEWORD(2, 2), &wsaData) == 0;
	}
#endif

	udpStartCalls += 1;
	return ok;
}

static void udpShutdown() {
	udpStartCalls -= 1;

#ifdef UDPKIT_WIN
	if (udpStartCalls == 0) {
		WSACleanup();
	}
#endif
}

static S32 udpLastError() {
#ifdef UDPKIT_WIN
	return WSAGetLastError();
#else
    return errno;
#endif
}

EXPORT_API udpSocket* udpCreate() {
	if (udpStart()) {
		udpSocket* socket = new udpSocket();
		socket->endPoint = udpEndPoint();
		socket->lastError = 0;
		socket->recvBufferSize = 1 << 15;
		socket->sendBufferSize = 1 << 15;
		socket->nativeSocket = INVALID_SOCKET;
		socket->nativeSocket = ::socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

		if (socket->nativeSocket == INVALID_SOCKET) {
			return NULL;
		}

		return socket;
	}
	else {
		return NULL;
	}
}

EXPORT_API S32 udpBind(udpSocket* socket, udpEndPoint addr) {
	S32 result = 0;

	UDPKIT_CHECK_VALIDSOCKET(socket);

	sockaddr_in bind;
	bind.sin_family = AF_INET;
	bind.sin_addr.s_addr = htonl(addr.address);
	bind.sin_port = htons(addr.port);

#if UDPKIT_WIN
#else
	//U32 reuseaddr = 1;
	//result = setsockopt(socket->nativeSocket, SOL_SOCKET, SO_REUSEADDR, (char*)&reuseaddr, sizeof(reuseaddr));
	//UDPKIT_CHECK_RESULT(result);
#endif

	result = ::bind(socket->nativeSocket, (sockaddr*)&bind, sizeof(bind));
	UDPKIT_CHECK_RESULT(result);

	result = setsockopt(socket->nativeSocket, SOL_SOCKET, SO_RCVBUF, (char *)&socket->recvBufferSize, sizeof(socket->recvBufferSize));
	UDPKIT_CHECK_RESULT(result);

	result = setsockopt(socket->nativeSocket, SOL_SOCKET, SO_SNDBUF, (char *)&socket->sendBufferSize, sizeof(socket->sendBufferSize));
	UDPKIT_CHECK_RESULT(result);

#if UDPKIT_WIN
	DWORD nonblock = 1;
	result = ioctlsocket(socket->nativeSocket, FIONBIO, &nonblock);
	UDPKIT_CHECK_RESULT(result);
#else
	U32 notblock = 1;
	result = ioctl(socket->nativeSocket, FIONBIO, &notblock);
	UDPKIT_CHECK_RESULT(result);
#endif

	sockaddr address;
	socklen_t addressSize = sizeof(address);
	getsockname(socket->nativeSocket, (sockaddr*)&address, &addressSize);
	socket->endPoint.address = ntohl(((sockaddr_in*)&address)->sin_addr.s_addr);
	socket->endPoint.port = ntohs(((sockaddr_in*)&address)->sin_port);

	return UDPKIT_SOCKET_OK;
}

EXPORT_API S32 udpSendTo(udpSocket* socket, char* buffer, S32 size, udpEndPoint addr) {
	UDPKIT_CHECK_VALIDSOCKET(socket);

	sockaddr_in to;
	to.sin_family = AF_INET;
	to.sin_addr.s_addr = htonl(addr.address);
	to.sin_port = htons(addr.port);

	S32 result = ::sendto(socket->nativeSocket, buffer, size, 0, (sockaddr*)&to, sizeof(to));
	UDPKIT_CHECK_RESULT(result);

	return result;
}

EXPORT_API S32 udpRecvFrom(udpSocket* socket, char* buffer, S32 size, udpEndPoint* addr) {
	UDPKIT_CHECK_VALIDSOCKET(socket);

	sockaddr_in from;
	socklen_t fromSize = sizeof(from);

	S32 result = ::recvfrom(socket->nativeSocket, buffer, size, 0, (sockaddr*)&from, &fromSize);
	UDPKIT_CHECK_RESULT(result);

	addr->port = ntohs(from.sin_port);
	addr->address = ntohl(from.sin_addr.s_addr);

	return result;
}

EXPORT_API S32 udpRecvPoll(udpSocket* socket, S32 timeoutMs) {
	UDPKIT_CHECK_VALIDSOCKET(socket);

#if defined(UDPKIT_WIN) && defined(UDPKIT_WIN_WSAPOLL)
	WSAPOLLFD poll;
	poll.fd = socket->nativeSocket;
	poll.events = POLLRDNORM;

	S32 result = ::WSAPoll(&poll, 1, timeoutMs);
	UDPKIT_CHECK_RESULT(result);

	if (poll.revents & POLLRDNORM) {
		return UDPKIT_SOCKET_OK;
	}

	return UDPKIT_SOCKET_NODATA;
#else
	fd_set set;
	timeval tv;

	FD_ZERO(&set);
	FD_SET(socket->nativeSocket, &set);

	tv.tv_sec = 0;
	tv.tv_usec = timeoutMs * 1000;

	S32 result = select(1, &set, NULL, NULL, &tv);
	if (result == SOCKET_ERROR) {
		return UDPKIT_SOCKET_ERROR;
	}

	if (FD_ISSET(socket->nativeSocket, &set)) {
		return UDPKIT_SOCKET_OK;
	}

	return UDPKIT_SOCKET_NODATA;
#endif
}

EXPORT_API S32 udpLastError(udpSocket* socket) {
	return socket->lastError;
}

EXPORT_API S32 udpGetEndPoint(udpSocket* socket, udpEndPoint* endPoint) {
	*endPoint = udpEndPoint();
	UDPKIT_CHECK_VALIDSOCKET(socket);
	*endPoint = socket->endPoint;
	return UDPKIT_SOCKET_OK;
}

EXPORT_API S32 udpClose(udpSocket* socket) {
	UDPKIT_CHECK_VALIDSOCKET(socket);

#ifdef UDPKIT_WIN
	S32 result = closesocket(socket->nativeSocket);
#else
    S32 result = close(socket->nativeSocket);
#endif
    
	// delete socket object
	delete socket;

	if (result == SOCKET_ERROR) {
		S32 error = udpLastError();
		udpShutdown();
		return error;
	}
	else {
		udpShutdown();
		return UDPKIT_SOCKET_OK;
	}
}
