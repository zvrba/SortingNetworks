using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SortingNetworks
{
    using V = System.Runtime.Intrinsics.Vector256<int>;

    /// <summary>
    /// Builds an expression for periodic sorting network, TAOCP3, section 5.3.4, exercise 53.
    /// The network is hard-coded to 16 elements.
    /// </summary>
    class Periodic16
    {
        public static unsafe void Direct(int* data) {

        }

        static Expression Compare1x8(ParameterExpression lo, ParameterExpression hi, ParameterExpression tmp) {
            Debug.Assert(lo.Type == typeof(V));
            Debug.Assert(hi.Type == typeof(V));
            Debug.Assert(tmp.Type == typeof(V));

            return Expression.Block(
                Expression.Assign(tmp, Reverse8(hi)),
                Expression.Assign(lo, Expression.Call(IExpr.Min, lo, tmp)),
                Expression.Assign(hi, Expression.Call(IExpr.Max, hi, tmp)),
                Expression.Assign(hi, Reverse8(hi))
            );
        }

        static Expression Reverse8(ParameterExpression p) {
            Debug.Assert(p.Type == typeof(V));
            var rev4 = Expression.Call(IExpr.Shuffle, p, Expression.Constant(0x1B, typeof(int)));        // Reversed within lanes
            return Expression.Call(IExpr.Perm2x128, rev4, rev4, Expression.Constant(1, typeof(int)));    // Lanes swapped
        }
        
    }
}
