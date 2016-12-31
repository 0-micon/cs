using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public static class Utils
    {
        public static void DistinctValues<T>(this List<T> list)
        {
            list.Sort();

            int src = 0;
            int dst = 0;
            while (src < list.Count)
            {
                var val = list[src];
                list[dst] = val;

                ++dst;
                while (++src < list.Count && list[src].Equals(val)) ;
            }
            if (dst < list.Count)
            {
                list.RemoveRange(dst, list.Count - dst);
            }
        }

        // Summary: Compare two sorted collections.
        public static IEnumerable<T> Intersection<T>(this IEnumerable<T> a, IEnumerable<T> b) where T : IComparable<T>
        {
            using (var a_enum = a.GetEnumerator())
            {
                using(var b_enum = b.GetEnumerator())
                {
                    // We must call the MoveNext method to advance the enumerator to the first element of the collection before reading the value of Current.
                    for (bool ok = a_enum.MoveNext() && b_enum.MoveNext(); ok;)
                    {
                        int result = a_enum.Current.CompareTo(b_enum.Current);
                        if(result > 0)
                        {
                            ok = b_enum.MoveNext();
                        }
                        else if(result < 0)
                        {
                            ok = a_enum.MoveNext();
                        }
                        else
                        {
                            yield return a_enum.Current;
                            ok = a_enum.MoveNext() && b_enum.MoveNext();
                        }
                    }
                }
            }
        }

        public static T Min<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) < 0 ? a : b;
        }

        public static T Max<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) > 0 ? a : b;
        }
    }
}
