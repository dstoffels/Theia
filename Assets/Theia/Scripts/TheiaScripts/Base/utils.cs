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
        public delegate int Reducer<T>(T element);
        public static int Sum<T>(IEnumerable enumerable, Reducer<T> reducer)
        {
            int total = 0;
            foreach (T item in enumerable)
                total += reducer(item);
            return total;
        }
    }
}
