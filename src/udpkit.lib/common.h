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

#if _WIN64
	#define UDPKIT_WIN64 1
	#define UDPKIT_WIN 1
	#define UDPKIT_SUFFIX win64_
#elif _WIN32
	#define UDPKIT_WIN32 1
	#define UDPKIT_WIN 1
	#define UDPKIT_SUFFIX win32_
#elif __APPLE__
	#include "TargetConditionals.h"
	#if TARGET_OS_IPHONE && TARGET_IPHONE_SIMULATOR
		#define UDPKIT_IOS 1
		#define UDPKIT_IOS_SIMULATOR 1
		#define UDPKIT_SUFFIX ios_
	#elif TARGET_OS_IPHONE
		#define UDPKIT_IOS 1
		#define UDPKIT_SUFFIX ios_
	#else
		#define UDPKIT_OSX 1
		#define UDPKIT_SUFFIX osx_
	#endif
#elif __ANDROID__
	#define UDPKIT_ANDROID 1
	#define UDPKIT_SUFFIX android_
#elif __linux
	#define UDPKIT_LINUX 1
	#define UDPKIT_SUFFIX linux_
#endif

#if defined(_DEBUG) || defined(DEBUG)
	#define UDPKIT_DEBUG
#endif

#ifndef UNICODE
	#define UNICODE
#endif

#if UDPKIT_WIN
	#define WIN32_LEAN_AND_MEAN
	#define EXPORT_API extern "C" __declspec(dllexport)
#else
	#define EXPORT_API extern "C" 
#endif

typedef signed char S8;
typedef unsigned char U8;
typedef signed short S16;
typedef unsigned short U16;
typedef signed int S32;
typedef unsigned int U32;
typedef signed long long S64;
typedef unsigned long long U64;

#ifndef UDPKIT_ANDROID
static_assert(sizeof(U8) == 1 && sizeof(S8) == 1, "U8 and S8 must be 1 byte");
static_assert(sizeof(U16) == 2 && sizeof(S16) == 2, "U16 and S16 must be 2 bytes");
static_assert(sizeof(U32) == 4 && sizeof(S32) == 4, "U32 and S32 must be 4 bytes");
static_assert(sizeof(U64) == 8 && sizeof(S64) == 8, "U64 and S64 must be 8 bytes");
#endif

#include <errno.h>
#include <string.h>

EXPORT_API const char* udpPlatform();
EXPORT_API const char* udpPlatformErrorString(int errorCode);