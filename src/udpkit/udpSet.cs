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
    class UdpSet<T> {
        public int Count {
            get { return set.Count; }
        }

        public bool Remove (T value) {
            return set.Remove(value);
        }

#if HAS_HASHSET
        readonly HashSet<T> set;

        public UdpSet (IEqualityComparer<T> comparer) {
            set = new HashSet<T>(comparer);
        }

        public bool Add (T value) {
            return set.Add(value);
        }

        public bool Contains (T value) {
            return set.Contains(value);
        }
#else
        readonly Dictionary<T, object> set;

        public UdpSet (IEqualityComparer<T> comparer) {
            set = new Dictionary<T, object>(comparer);
        }

        public bool Add (T value) {
            if (set.ContainsKey(value))
                return false;

            set.Add(value, null);
            return true;
        }

        public bool Contains (T value) {
            return set.ContainsKey(value);
        }
#endif
    }
}
