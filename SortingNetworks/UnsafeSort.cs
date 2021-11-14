using System;

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
    public unsafe delegate void FullSorter<T>(T* data) where T : unmanaged;

    /// <summary>
    /// Represents an in-place sorting method.
    /// </summary>
    /// <typeparam name="T">Type of array elements.  This must be an integer or floating-point type.</typeparam>
    /// <param name="data">
    /// Pointer to the chunk of data to be sorted.  If this chunk is does not point to at least <c>c</c> elements,
    /// UNDEFINED BEHAVIOUR occurs (data corruption, crash).
    /// </param>
    /// <param name="c">
    /// Number of elements that should be sorted, starting at <c>data</c>.
    /// </param>
    public unsafe delegate void TruncatedSorter<T>(T* data, int c) where T : unmanaged;

    /// <summary>
    /// Provides complete information about a sorter: element type, maximum supported array length and the sorting delegates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct UnsafeSort<T> where T : unmanaged
    {
        /// <summary>
        /// Maximum array length supported by this sorter.  This length is also REQUIRED on input to <see cref="FullSorter"/>.
        /// </summary>
        public readonly int MaxLength;

        /// <summary>
        /// Delegate that performs the actual sorting.
        /// The input array must have at least <see cref="MaxLength"/> elements.
        /// </summary>
        public readonly FullSorter<T> FullSorter;

        /// <summary>
        /// Delegate that performs the actual sorting.  The input array is allowed to have a length
        /// less than <see cref="MaxLength"/>.  It is also allowed to invoke this method with <see cref="MaxLength"/>
        /// elements, but performance will be worse.
        /// </summary>
        public readonly TruncatedSorter<T> TruncatedSorter;

        private UnsafeSort(int length, FullSorter<T> fsort, TruncatedSorter<T> tsort) {
            MaxLength = length;
            FullSorter = fsort;
            TruncatedSorter = tsort;
        }

        public static unsafe UnsafeSort<int> CreateInt(int length) {
            var p = new PeriodicInt();
            if (length <= 4)
                return new UnsafeSort<int>(length, p.Sort4, p.Sort4);
            if (length <= 8)
                return new UnsafeSort<int>(length, p.Sort8, p.Sort8);
            if (length <= 16)
                return new UnsafeSort<int>(length, p.Sort16, p.Sort16);

            throw new ArgumentOutOfRangeException(nameof(length));
        }
    }
}
