using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public static class Utils
    {
        public static void Save<T>(this IEnumerable<T> list, string fname, Action<BinaryWriter, T> writer)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fname, FileMode.Create)))
            {
                foreach (T val in list)
                {
                    writer(bw, val);
                }
            }
        }

        public static IEnumerable<T> Load<T>(string fname, int element_size, Func<BinaryReader, T> reader)
        {
            using (BinaryReader br = new BinaryReader(File.Open(fname, FileMode.Open)))
            {
                long flength = (new FileInfo(fname)).Length;
                for(int count = (int)(flength / element_size); count-- > 0;)
                {
                    yield return reader(br);
                }
            }
        }

        public static void Load<T>(this List<T> list, string fname, int element_size, Func<BinaryReader, T> reader)
        {
            using (BinaryReader br = new BinaryReader(File.Open(fname, FileMode.Open)))
            {
                long flength = (new FileInfo(fname)).Length;
                int count = (int)(flength / element_size);
                list.Capacity = list.Count + count;

                while (count-- > 0)
                {
                    list.Add(reader(br));
                }
            }
        }

        public static int FindRow<T>(this List<List<T>> lists, T key)
        {
            int i = lists.Count;
            while (i-- > 0 && lists[i].BinarySearch(key) < 0);
            return i;
        }

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

        public static bool FirstIntersection<T>(this IEnumerable<T> a, IEnumerable<T> b, out T item) where T : IComparable<T>
        {
            foreach(T i in a.Intersection(b))
            {
                item = i;
                return true; 
            }
            item = default(T);
            return false;
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

        public static IEnumerable<KeyValuePair<int, T>> Index<T>(IEnumerable<T> e)
        {
            int idx = 0;
            foreach(T val in e)
            {
                yield return new KeyValuePair<int, T>(idx++, val);
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

        public static IEnumerable<KeyValuePair<int, int>> GetRange(int start, int end, int length)
        {
            for(int i = 0; i < end - length; i++)
            {
                yield return new KeyValuePair<int, int>(i, i + length);
            }
        }
    }

    public class SimpleEnumerator<T> : IEnumerator<T>
    {
        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int _position = -1;
        T[] _array;

        public SimpleEnumerator(T[] array)
        {
            _array = array;
        }

        public bool MoveNext() => ++_position < _array.Length;

        public void Reset()
        {
            _position = -1;
        }

        public void Dispose()
        {
        }

        public T Current => _array[_position];
        object IEnumerator.Current => _array[_position];
    }
}
