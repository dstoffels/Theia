using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theia
{
   public struct utils
    {
        public delegate int TotalReducer<T>(T item);
        public static int Sum<T>(IEnumerable enumerable, TotalReducer<T> reducer)
        {
            int total = 0;
            foreach (T item in enumerable)
                total += reducer(item);
            return total;
        }

        public delegate TReturn Reducer<TReturn, T>(T item, TReturn reduced);

        public static TReturn Reduce<TReturn, T>(IEnumerable enumerable, Reducer<TReturn, T> reducer)
        {
            TReturn reduced = default;
            foreach (T item in enumerable)
                reduced = reducer(item, reduced);
            return reduced;
        }
    }
}
