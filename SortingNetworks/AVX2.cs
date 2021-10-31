using System;
using System.Reflection;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = System.Runtime.Intrinsics.Vector256<int>;

    /// <summary>
    /// Helpers for generating call expressions.
    /// </summary>
    static class AVX2
    {
        static readonly Type TAVX = typeof(Avx);
        static readonly Type TAVX2 = typeof(Avx2);
        
        public static readonly MethodInfo LoadVector256 = TAVX.GetMethod("LoadVector256", new Type[] { typeof(int*) });
        public static readonly MethodInfo Shuffle = TAVX2.GetMethod("Shuffle", new Type[] { typeof(V), typeof(byte) });
        public static readonly MethodInfo Perm2x128 = TAVX2.GetMethod("Permute2x128", new Type[] { typeof(V), typeof(V), typeof(byte) });
        public static readonly MethodInfo Max = TAVX2.GetMethod("Max", new Type[] { typeof(V), typeof(V) });
        public static readonly MethodInfo Min = TAVX2.GetMethod("Min", new Type[] { typeof(V), typeof(V) });
    }
}
