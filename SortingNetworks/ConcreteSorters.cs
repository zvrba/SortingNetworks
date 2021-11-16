using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = Vector256<int>;
    
    sealed class Int4Sorter : UnsafeSort<int>
    {
        internal Int4Sorter(PeriodicInt periodicInt) : base(periodicInt, 1, 4) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort4(data);
        public override unsafe void Sort(int* data, int c) => PeriodicInt.Sort4(data, c);
    }

    sealed class Int8Sorter : UnsafeSort<int>
    {
        internal Int8Sorter(PeriodicInt periodicInt) : base(periodicInt, 1, 8) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort8(data);
        public override unsafe void Sort(int* data, int c) => PeriodicInt.Sort8(data, c);
    }

    sealed class Int16Sorter : UnsafeSort<int>
    {
        internal Int16Sorter(PeriodicInt periodicInt) : base(periodicInt, 9, 16) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort16(data);
        public override unsafe void Sort(int* data, int c) => PeriodicInt.Sort16(data, c);
    }

    sealed class Int32Sorter : UnsafeSort<int>
    {
        internal Int32Sorter(PeriodicInt periodicInt) : base(periodicInt, 17, 32) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort32(data);
        public override unsafe void Sort(int* data, int c) => PeriodicInt.Sort32(data, c);
    }


    // TODO: for truncated sorters: many comparisons can be omitted as the result is given ("low part" remains there).
    sealed class IntBigSorter : UnsafeSort<int>
    {
        readonly int logLength;
        
        internal IntBigSorter(PeriodicInt periodicInt, int logLength) : base(periodicInt, 1 << logLength, 1 << logLength) {
            this.logLength = logLength;
        }

        public override unsafe void Sort(int* data) {
            for (int i = 0; i < logLength; ++i)
                Block(Math.Min(2 + i, logLength), data, data + MaxLength);
        }
        public override unsafe void Sort(int* data, int c) => throw new NotImplementedException();

        // A block has logN phases, only the first p of which are executed
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        unsafe void Block(int p, int* b, int* e) {
            int size = (int)(e - b);
            int split = 1;
            for (;  p > 0; --p, ++split, size /= 2) {
                for (int i = 0; i < split; ++i)
                    Phase(p, b, b + i * size);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        unsafe void Phase(int p, int* b, int* e) {
            if (e - b >= 32) {
                for (; e > b; b += 32, e -= 32)
                    PeriodicInt.Phase_N_32(b, e);
            }
            else {
                var v0 = Avx.LoadVector256(b);
                var v1 = Avx.LoadVector256(b + 8);
                PeriodicInt.Block_16_1(p, ref v0, ref v1);
                Avx.Store(b, v0);
                Avx.Store(b, v1);
            }
        }
    }
}
