using System;

namespace SortingNetworks
{
    /// <summary>
    /// Factory methods for creating instances of <see cref="UnsafeSort{T}"/>.
    /// </summary>
    public static class UnsafeSort
    {
        public static UnsafeSort<int> CreateInt(int length) {
#if false
            var p = new PeriodicInt();
            if (length <= 4)
                return new UnsafeSort<int>(length, p.Sort4, p.Sort4);
            if (length <= 8)
                return new UnsafeSort<int>(length, p.Sort8, p.Sort8);
            if (length <= 16)
                return new UnsafeSort<int>(length, p.Sort16, p.Sort16);
#endif
            throw new ArgumentOutOfRangeException(nameof(length));
        }

    }

    /// <summary>
    /// Provides methods for sorting "small" arrays of ints or floats.  NB! All methods taking pointer arguments require
    /// that the allocated size is correct wrt. the implied or specified length.  Otherwise UNDEFINED BEHAVIOR occurs: data
    /// corruption or crash.
    /// </summary>
    /// <typeparam name="T">The type of array elements.</typeparam>
    public abstract class UnsafeSort<T> where T : unmanaged
    {
        // We want to access static data as infrequently as possible.
        static private readonly PeriodicInt PeriodicInt = new PeriodicInt();

        private protected UnsafeSort(int maxLength) {
            MaxLength = maxLength;
        }

        /// <summary>
        /// Maximum array length supported by this sorter.  This is also the length that is REQUIRED on input to <see cref="Sort(T*)"/>.
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// In-place sorts <see cref="MaxLength"/> elements starting at <paramref name="data"/>.
        /// </summary>
        /// <param name="data">Pointer to the chunk of data to be sorted.</param>
        abstract public unsafe void Sort(T* data);

        /// <summary>
        /// In-place sorts <paramref name="c"/> elements starting at <paramref name="data"/>.
        /// </summary>
        /// <param name="data">Pointer to the chunk of data to be sorted.</param>
        /// <param name="c">
        /// Number of elements to sort; unchecked!  It should be between 2 and <see cref="MaxLength"/>.  Passing
        /// <see cref="MaxLength"/> is allowed, but performance will be worse than calling <see cref="Sort(T*)"/>.
        /// </param>
        abstract public unsafe void Sort(T* data, int c);
    }
}
