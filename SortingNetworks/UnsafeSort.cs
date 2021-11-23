using System;

namespace SortingNetworks
{
    /// <summary>
    /// Represents an in-place sorting method with possibly limited bounds on the valid values of <paramref name="c"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements being sorted.</typeparam>
    /// <param name="data">Pointer to the beginning of the range to sort.</param>
    /// <param name="c">Number of elements in the range.</param>
    public unsafe delegate void Sorter<T>(T* data, int c) where T : unmanaged;

    /// <summary>
    /// Provides methods for sorting arrays of ints or floats using a periodic sorting network.
    /// </summary>
    /// <typeparam name="T">The type of array elements.</typeparam>
    /// <remarks>
    /// WARNING! All methods taking pointer arguments require that the allocated size is correct wrt. the implied or specified
    /// length. Also, the input length must conform to <see cref="MinLength"/> and <see cref="MaxLength"/> limits.  Otherwise
    /// UNDEFINED BEHAVIOR occurs: incorrect result, data corruption or crash.
    /// </remarks>
    public abstract class UnsafeSort<T> where T : unmanaged
    {
        /// <summary>
        /// Creates an instance of <c>UnsafeSort{T}</c>.
        /// </summary>
        /// <param name="maxLength">
        /// Maximum array length supported by the sorter.  Sorters for sizes of up to 16 are more efficent than the general-length
        /// sorters and should therefore be used for small arrays.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="maxLength"/> exceeds <c>2^24</c>, which is the maximum supported value. - OR - <typeparamref name="T"/>
        /// is not <c>int</c> or <c>float</c>.
        /// </exception>
        public static UnsafeSort<T> Create(int maxLength) {
            object ret = null;

            if (typeof(T) == typeof(int))
                ret = new IntSorter(maxLength);
            if (typeof(T) == typeof(float))
                ret = new FloatSorter(maxLength);

            if (ret == null)
                throw new ArgumentOutOfRangeException("Unsupported element type: " + typeof(T).Name);
            return (UnsafeSort<T>)ret;
        }

        // This base is derivable only in this assembly.
        private protected UnsafeSort()
        { }

        /// <summary>
        /// Minimum array length supported by this sorter.
        /// </summary>
        public int MinLength { get; protected set; }

        /// <summary>
        /// Maximum array length supported by this sorter.
        /// </summary>
        public int MaxLength { get; protected set; }

        /// <summary>
        /// Delegate that performs the actual sorting.  WARNING!  The count argument given to the delegate must be between
        /// <see cref="MinLength"/> and <see cref="MaxLength"/> (inclusive).  No bounds are checked.
        /// </summary>
        public Sorter<T> Sorter { get; protected set; }

        /// <summary>
        /// Convenience overload for use in "safe" code.  Checks preconditions and then invokes <see cref="Sort(T*, int)"/>.
        /// </summary>
        /// <param name="data">Array to sort.</param>
        /// <exception cref="ArgumentOutOfRangeException">The array length is invalid.</exception>
        public unsafe void Sort(T[] data) {
            if (data.Length < MinLength || data.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(data), $"Invalid array length ({data.Length}).");
            fixed (T* p = data)
                Sorter(p, data.Length);
        }
    }
}
