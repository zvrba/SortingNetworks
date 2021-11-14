using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    sealed class Int4Sorter : UnsafeSort<int>
    {
        internal Int4Sorter(PeriodicInt periodicInt) : base(periodicInt, 4) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort4(data);

        public override unsafe void Sort(int* data, int c) {
            throw new NotImplementedException();
        }
    }

    sealed class Int8Sorter : UnsafeSort<int>
    {
        internal Int8Sorter(PeriodicInt periodicInt) : base(periodicInt, 8) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort8(data);

        public override unsafe void Sort(int* data, int c) {
            throw new NotImplementedException();
        }
    }

    sealed class Int16Sorter : UnsafeSort<int>
    {
        internal Int16Sorter(PeriodicInt periodicInt) : base(periodicInt, 16) { }

        public override unsafe void Sort(int* data) => PeriodicInt.Sort16(data);

        public override unsafe void Sort(int* data, int c) {
            throw new NotImplementedException();
        }
    }

    sealed class Int32Sorter : UnsafeSort<int>
    {
        internal Int32Sorter(PeriodicInt periodicInt) : base(periodicInt, 32) { }

        public override unsafe void Sort(int* data) {
            throw new NotImplementedException();
        }

        public override unsafe void Sort(int* data, int c) {
            throw new NotImplementedException();
        }
    }
}
