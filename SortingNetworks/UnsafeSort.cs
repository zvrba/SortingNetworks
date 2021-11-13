using System;
using System.Collections.Generic;
using System.Text;

namespace SortingNetworks
{
    /// <summary>
    /// Represents an in-place sorting method.
    /// </summary>
    /// <typeparam name="T">Type of array elements.  This must be an integer or floating-point type.</typeparam>
    /// <param name="data">
    /// Pointer to the chunk of data to be sorted.  If this chunk is not of sufficient length, UNDEFINED BEHAVIOUR
    /// occurs (data corruption, crash).
    /// </param>
    public unsafe delegate void Sorter<T>(T* data) where T : unmanaged;

    /// <summary>
    /// Provides complete information about a sorter: element type, array length and the sorting delegate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct UnsafeSort<T> where T : unmanaged
    {
        /// <summary>
        /// Length of the fixed-length array required by <see cref="Sorter"/>.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// Delegate that performs the actual sorting without any error-checking.
        /// </summary>
        public readonly Sorter<T> Sorter;

        private UnsafeSort(int length, Sorter<T> sort) {
            Length = length;
            Sorter = sort;
        }

        public static unsafe UnsafeSort<int> CreateInt(int length) {
            var p = new PeriodicInt2();
            if (length == 4)
                return new UnsafeSort<int>(length, p.Sort4);
            if (length == 8)
                return new UnsafeSort<int>(length, p.Sort8);

            throw new ArgumentOutOfRangeException(nameof(length));
        }
    }
}
