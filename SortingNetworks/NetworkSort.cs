using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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

        readonly PeriodicInt periodicInt = new PeriodicInt();

        private NetworkSort() {

        }


    }
}
