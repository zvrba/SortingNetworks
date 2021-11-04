using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks
{
    using V = System.Runtime.Intrinsics.Vector256<int>;

    /// <summary>
    /// Builds an expression for periodic sorting network, TAOCP3, section 5.3.4, exercise 53.
    /// The network is hard-coded to 16 elements.
    /// </summary>
    class Periodic16Expr
    {

        static readonly Type TAVX = typeof(Avx);
        static readonly Type TAVX2 = typeof(Avx2);
        static readonly Type TV = typeof(V);

        // All zeros
        static readonly V Zero = Vector256.Create(0);
        // All ones
        static readonly V Complement = Avx2.CompareEqual(Zero, Zero);
        // FF00FF00 (1 digit = 32 bits)
        static readonly V AlternatingMaskHi64 = Avx2.Xor(Complement, Avx2.ShiftRightLogical128BitLane(Complement, 8));
        // F0F0F0F0
        static readonly V AlternatingMaskLo32 = Avx2.Xor(
            Complement.AsInt64(),
            Avx2.ShiftLeftLogical(Complement.AsInt64(), 32)
        ).AsInt32();

        static readonly MethodInfo LoadVector256 = TAVX.GetMethod("LoadVector256", new Type[] { typeof(int*) });
        static readonly MethodInfo Shuffle = TAVX2.GetMethod("Shuffle", new Type[] { TV, typeof(byte) });
        static readonly MethodInfo BlendVariable = TAVX2.GetMethod("BlendVariable", new Type[] { TV, TV, TV });
        static readonly MethodInfo Permute2x128 = TAVX2.GetMethod("Permute2x128", new Type[] { TV, typeof(byte) });
        static readonly MethodInfo CompareGreaterThan = TAVX2.GetMethod("CompareGreaterThan", new Type[] { TV, TV });
        static readonly MethodInfo UnpackLow = TAVX2.GetMethod("UnpackLow", new Type[] { TV, TV });
        static readonly MethodInfo UnpackHigh = TAVX2.GetMethod("UnpackHigh", new Type[] { TV, TV });

        readonly ParameterExpression data;
        readonly ParameterExpression lo;
        readonly ParameterExpression hi;
        readonly ParameterExpression tmp1;
        readonly ParameterExpression tmp2;
        readonly ParameterExpression tmp3;

        public unsafe delegate void Sorter(int* data);
        public Sorter Sort { get; private set; }

        public Periodic16Expr() {
            data = Expression.Parameter(typeof(int*), "data");
            lo = Expression.Parameter(TV, "lo");
            hi = Expression.Parameter(TV, "hi");
            tmp1 = Expression.Parameter(TV, "tmp1");
            tmp2 = Expression.Parameter(TV, "tmp2");
            tmp3 = Expression.Parameter(TV, "tmp3");
            var step = Step();

        }

        private BlockExpression Step() {
            var es = new List<Expression>();

            es.AddRange(new Expression[] { 
                Expression.Assign(lo, Expression.Call(LoadVector256, data)),
                Expression.Assign(hi, Expression.Call(LoadVector256, Expression.Add(data, Expression.Constant(8))))
            });

            // STAGE1

            es.AddRange(new Expression[] {
                Expression.Assign(tmp1, Expression.Call(Shuffle, Expression.Constant((byte)0x1B))),
                Expression.Assign(hi, Expression.Call(Permute2x128, tmp1, tmp1, Expression.Constant((byte)1))),
                Expression.Assign(tmp1, Expression.Call(CompareGreaterThan, hi, lo))
            });
            es.AddRange(Swap(lo, hi, tmp1));

            // STAGE2

            es.AddRange(new Expression[] {
                Expression.Assign(tmp1, Expression.Call(Permute2x128, lo, hi, Expression.Constant((byte)0x31))),
                Expression.Assign(tmp1, Expression.Call(Shuffle, tmp1, Expression.Constant((byte)0x1B))),
                Expression.Assign(lo, Expression.Call(Permute2x128, lo, tmp1, Expression.Constant((byte)0x30))),
                Expression.Assign(hi, Expression.Call(Permute2x128, hi, tmp1, Expression.Constant((byte)0x02))),
                Expression.Assign(tmp1, Expression.Call(CompareGreaterThan, hi, lo))
            });
            es.AddRange(Swap(lo, hi, tmp1));

            // STAGE3

            es.AddRange(Swap(lo, hi, Expression.Constant(AlternatingMaskHi64)));
            es.AddRange(new Expression[] {
                Expression.Assign(lo, Expression.Call(Shuffle, lo, Expression.Constant((byte)0b01001011))),
                Expression.Assign(hi, Expression.Call(Shuffle, hi, Expression.Constant((byte)0b10110100))),
                Expression.Assign(tmp1, Expression.Call(CompareGreaterThan, hi, lo))
            });
            es.AddRange(Swap(lo, hi, tmp1));

            // STAGE4

            es.AddRange(Swap(lo, hi, Expression.Constant(AlternatingMaskLo32)));
            es.AddRange(new Expression[] {
                Expression.Assign(hi, Expression.Call(Shuffle, hi, Expression.Constant((byte)0b10110001))),
                Expression.Assign(tmp1, Expression.Call(CompareGreaterThan, hi, lo))
            });
            es.AddRange(Swap(lo, hi, tmp1));

            // RESTORE ORDER.
            es.AddRange(new Expression[] {
                Expression.Assign(tmp1, Expression.Call(UnpackLow, lo, hi)),
                Expression.Assign(tmp2, Expression.Call(UnpackHigh, lo, hi)),
                Expression.Assign(lo, Expression.Call(Permute2x128, tmp1, tmp2, Expression.Constant((byte)0x20))),
                Expression.Assign(hi, Expression.Call(Permute2x128, tmp1, tmp2, Expression.Constant((byte)0x31)))
            });

            return Expression.Block(new ParameterExpression[] { tmp1, tmp2, tmp3 }, es);
        }

        private Expression[] Swap(ParameterExpression lo, ParameterExpression hi, Expression mask) {
            return new Expression[] {
                Expression.Assign(tmp3, Expression.Call(BlendVariable, lo, hi, mask)),
                Expression.Assign(lo, Expression.Call(BlendVariable, hi, lo, mask)),
                Expression.Assign(hi, tmp3)
            };
        }
    }
}
