using System.Diagnostics;

namespace udpkit {
    class udpPrecisionTimer {
        static readonly long start = Stopwatch.GetTimestamp();
        static readonly double freq = 1.0 / (double) Stopwatch.Frequency;

        internal static uint getCurrentTime () {
            long diff = Stopwatch.GetTimestamp() - start;
            double seconds = (double) diff * freq;
            return (uint) (seconds * 1000.0);
        }
    }
}
