using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    /// <summary>
    /// Random number generation using AES-NI instructions.
    /// Code adapted from https://github.com/dragontamer/AESRand/blob/master/AESRand/AESRand/AESRand.cpp
    /// </summary>
    public sealed class AESRand : UnsafeRandom {
        static readonly Vector128<ulong> PRIME_INCREMENT = Vector128.Create(
            0x2f, 0x2b, 0x29, 0x25, 0x1f, 0x1d, 0x17, 0x13,
            0x11, 0x0D, 0x0B, 0x07, 0x05, 0x03, 0x02, 0x01).AsUInt64();

        Vector128<ulong> state;

        public AESRand(int[] seed) {
            if (seed.Length != 4)
                throw new ArgumentException("Seed must contain exactly 4 elements.", nameof(seed));
            this.state = Vector128.Create(seed[0], seed[1], seed[2], seed[3]).AsUInt64();
        }

        /// <summary>
        /// Overwrites the initial 4 elements of <paramref name="data"/> with random 32-bit integers.
        /// </summary>
        /// <param name="data">
        /// Array of length at least 4.  Behaviour is UNDEFINED if the array is shorter.
        /// </param>
        public override Vector128<int> Get4() {
            state = Sse2.Add(state, PRIME_INCREMENT);
            var r1 = Aes.Encrypt(state.AsByte(), PRIME_INCREMENT.AsByte());
            var r2 = Aes.Encrypt(r1, PRIME_INCREMENT.AsByte()).AsInt32();
            return r2;
        }
    }
}
