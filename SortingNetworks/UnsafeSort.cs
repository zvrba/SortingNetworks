﻿using System;

namespace SortingNetworks
{
    /// <summary>
    /// Factory methods for creating instances of <see cref="UnsafeSort{T}"/>.
    /// </summary>
    public static class UnsafeSort
    {
        // We want to access static data as infrequently as possible.
        static private readonly PeriodicInt PeriodicInt = new PeriodicInt();

        /// <summary>
        /// Creates an instance of <c>UnsafeSort{int}</c>.
        /// </summary>
        /// <param name="length">
        /// Maximum array length supported by the sorter.  Note that a "big" sorter cannot sort arbitrarily small arrays;
        /// <see cref="UnsafeSort{T}.MinLength"/>.  If length is greater than 32, length will be truncated to the NEXT HIGHER
        /// power of two.
        /// </param>
        /// <seealso cref="UnsafeSort{T}"/>
        public static UnsafeSort<int> CreateInt(int length) {
            if (length <= 4)
                return new Int4Sorter(PeriodicInt);
            if (length <= 8)
                return new Int8Sorter(PeriodicInt);
            if (length <= 16)
                return new Int16Sorter(PeriodicInt);
            if (length <= 32)
                return new Int32Sorter(PeriodicInt);
            if (length <= 1 << 20) {
                length = 1 << (int)Math.Ceiling(Math.Log2(length - 1));
                return new IntBigSorter(PeriodicInt, length);
            }
            throw new ArgumentOutOfRangeException(nameof(length));
        }
    }

    /// <summary>
    /// Provides methods for sorting "small" arrays of ints or floats.  NB! All methods taking pointer arguments require
    /// that the allocated size is correct wrt. the implied or specified length.  Also, the input length must conform to
    /// <see cref="MinLength"/> and <see cref="MaxLength"/> limits.  Otherwise UNDEFINED BEHAVIOR occurs: data corruption
    /// or crash.
    /// </summary>
    /// <typeparam name="T">The type of array elements.</typeparam>
    /// <remarks>
    /// </remarks>
    public abstract class UnsafeSort<T> where T : unmanaged
    {
        private protected UnsafeSort(PeriodicInt periodicInt, int minLength, int maxLength) {
            PeriodicInt = periodicInt;
            MinLength = minLength;
            MaxLength = maxLength;
        }

        private protected readonly PeriodicInt PeriodicInt;

        /// <summary>
        /// Minimum array length supported by this sorter.  NB! Shorter arrays WILL BE SORTED INCORRECTLY!
        /// </summary>
        public int MinLength { get; }

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
        /// Passing less than <see cref="MinLength"/> will return an INCORRECT RESULT!
        /// </param>
        abstract public unsafe void Sort(T* data, int c);
    }
}
