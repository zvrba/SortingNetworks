using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    /// <summary>
    /// Random number generation using AES-NI instructions.  NB! Single round, as in this implementation, seems to "mix" badly.
    /// Code adapted from https://github.com/dragontamer/AESRand/blob/master/AESRand/AESRand/AESRand.cpp
    /// </summary>
    struct AESRand : IUnsafeRandom {
        static readonly Vector128<ulong> PRIME_INCREMENT = Vector128.Create(
            0x2f, 0x2b, 0x29, 0x25, 0x1f, 0x1d, 0x17, 0x13,
            0x11, 0x0D, 0x0B, 0x07, 0x05, 0x03, 0x02, 0x01).AsUInt64();

        Vector128<ulong> state;

        /// <summary>
        /// Initializes the AES state.  Default is 0; any other number is broadcast to 4 lanes.
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(int[] state) {
            this.state = Vector128.Create(state[0]).AsUInt64();
        }

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with random 32-bit integers.
        /// </summary>
        /// <param name="data">
        /// Array of length at least 4.  Behaviour is UNDEFINED if the array is shorter.
        /// </param>
        public unsafe void Get4(int* data) {
            state = Sse2.Add(state, PRIME_INCREMENT);
            var tmp = Aes.Encrypt(state.AsByte(), PRIME_INCREMENT.AsByte()).AsInt32();
            Sse2.Store(data, tmp.AsInt32());
        }
    }
}
