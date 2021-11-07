using System;
using System.Collections.Generic;
using System.Text;

namespace SortingNetworks
{
    /// <summary>
    /// Methods for high-performance random number generation.  This interface exists because using <c>System.Random</c>
    /// during benchmarking unveals a bimodal timing distribution, which makes a rather bad baseline.
    /// </summary>
    public interface IUnsafeRandom
    {
        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="seed">
        /// An array consisting of at least one element.  The contents is used in an implementation-specific manner.
        /// </param>
        void Initialize(int[] seed);

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with random 32-bit integers.
        /// </summary>
        /// <param name="data">
        /// Pointer to a memory chunk of at least 4 integers.  Behaviour is UNDEFINED if the allocated
        /// space for the chunk is shorter.
        /// </param>
        public unsafe void Get4(int* data);
    }
}
