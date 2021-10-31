using System;
using System.Linq.Expressions;

namespace SortingNetworks
{
    class Builder
    {


        public static Action<int[], int, int> Swap() {
            var i1 = Expression.Parameter(typeof(int), "i1");
            var i2 = Expression.Parameter(typeof(int), "i2");
            var a = Expression.Parameter(typeof(int[]), "a");
            var t = Expression.Parameter(typeof(int), "t");

            var swap = Expression.Block(
                new[] { t },
                Expression.Assign(t, Expression.ArrayAccess(a, i1)),
                Expression.Assign(Expression.ArrayAccess(a, i1), Expression.ArrayAccess(a, i2)),
                Expression.Assign(Expression.ArrayAccess(a, i2), t));

            return Expression.Lambda<Action<int[], int, int>>(swap, a, i1, i2).Compile();
        }
    }
}
