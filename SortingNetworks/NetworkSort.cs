using System;
using System.Collections.Generic;
using System.Text;

namespace SortingNetworks
{
    /// <summary>
    /// Main entry point for network-sorting algorithms.  The (only) instance is accessible through <see cref="Instance"/>.
    /// All instance methods are thread-safe.
    /// </summary>
    public class NetworkSort
    {
        /// <summary>
        /// Refers to the singleton instance of this class.  Should be cached in a local variable when used in tight loops.
        /// </summary>
        public static readonly NetworkSort Instance = new NetworkSort();

        private NetworkSort() {

        }


        public unsafe void Sort16(int* data) {

        }
    }
}
