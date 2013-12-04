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

#include "timers.h"

#ifdef UDPKIT_ANDROID
EXPORT_API U32 udpGetHighPrecisionTime()
{
	timespec spec = timespec();
	clock_gettime(CLOCK_MONOTONIC, &spec);

	return (spec.tv_sec * 1000) + (spec.tv_nsec / 1000 / 1000);
}
#elif UDPKIT_IOS

double mach_clock()
{
    static bool init = 0;
    static mach_timebase_info_data_t tbInfo;
    static double conversionFactor;
    
    if (!init)
    {
        init = 1 ;
        mach_timebase_info(&tbInfo);
        conversionFactor = tbInfo.numer / (1e9 * tbInfo.denom);
    }
    
    return mach_absolute_time() * conversionFactor;
}

double mach_clock_diff()
{
    static double lastTime = 0;
    double currentTime = mach_clock();
    double diff = currentTime - lastTime;
    lastTime = currentTime;
    return diff;
}

EXPORT_API U32 udpGetHighPrecisionTime()
{
    return (U32) (mach_clock_diff() * 1000.0);
}
#endif