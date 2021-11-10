using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace SortingNetworks.Attic
{
    using V = System.Runtime.Intrinsics.Vector256<int>;

    /// <summary>
    /// Builds an expression for periodic sorting network as compiled lambda.
    /// The network is hard-coded to 16 elements.
    /// </summary>
    public class Periodic16Expr
    {
        unsafe delegate void RegisterSort(ref V lo, ref V hi);

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

        static readonly MethodInfo Shuffle = TAVX2.GetMethod("Shuffle", new Type[] { TV, typeof(byte) });
        static readonly MethodInfo BlendVariable = TAVX2.GetMethod("BlendVariable", new Type[] { TV, TV, TV });
        static readonly MethodInfo Permute2x128 = TAVX2.GetMethod("Permute2x128", new Type[] { TV, TV, typeof(byte) });
        static readonly MethodInfo CompareGreaterThan = TAVX2.GetMethod("CompareGreaterThan", new Type[] { TV, TV });
        static readonly MethodInfo UnpackLow = TAVX2.GetMethod("UnpackLow", new Type[] { TV, TV });
        static readonly MethodInfo UnpackHigh = TAVX2.GetMethod("UnpackHigh", new Type[] { TV, TV });

        readonly ParameterExpression lo;
        readonly ParameterExpression hi;
        readonly ParameterExpression tmp1;
        readonly ParameterExpression tmp2;
        readonly ParameterExpression tmp3;
        readonly RegisterSort sort;

        public Periodic16Expr() {
            lo = Expression.Parameter(TV.MakeByRefType(), "lo");
            hi = Expression.Parameter(TV.MakeByRefType(), "hi");
            tmp1 = Expression.Variable(TV, "tmp1");
            tmp2 = Expression.Variable(TV, "tmp2");
            tmp3 = Expression.Variable(TV, "tmp3");

            var steps = new List<Expression>();
            for (int i = 0; i < 4; ++i)
                steps.AddRange(Step());

            var l = Expression.Lambda<RegisterSort>(
                Expression.Block(new ParameterExpression[] { tmp1, tmp2, tmp3 }, steps),
                new ParameterExpression[] { lo, hi });
            sort = l.Compile(false);
        }

        public unsafe void Sort(int* data) {
            var lo = Avx.LoadVector256(data);
            var hi = Avx.LoadVector256(data + 8);
            sort(ref lo, ref hi);
            Avx.Store(data, lo);
            Avx.Store(data + 8, hi);
        }

        private List<Expression> Step() {
            var es = new List<Expression>();

            // STAGE1

            es.AddRange(new Expression[] {
                Expression.Assign(tmp1, Expression.Call(Shuffle, hi, Expression.Constant((byte)0x1B))),
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

            return es;
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
