using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    // GeneralSolution is a very general problem-solving technique that
    // consists of systematically enumerating all possible candidates for the
    // solution and checking whether each candidate satisfies the problem's statement.
    public class GeneralSolution<K> : List<List<K>> where K : IComparable<K>
    {
        public void PrecomputeMoves(K start_key, int max_depth, int reserved, int growth_rate, Func<K, IEnumerable<K>> next)
        {
            List<K> src_ring = new List<K>(1);
            src_ring.Add(start_key);

            Clear();
            Capacity = max_depth;
            Add(src_ring);

            while (Count < max_depth)
            {
                List<K> next_ring = new List<K>(reserved);
                foreach (K src_key in src_ring)
                {
                    foreach (K dst_key in next(src_key))
                    {
                        if (this.FindRow(dst_key) < 0)
                        {
                            next_ring.Add(dst_key);
                        }
                    }
                }

                reserved = next_ring.Count * growth_rate;
                // sort and remove duplicates
                next_ring.DistinctValues();

                Add(next_ring);
                src_ring = next_ring;
            }
        }

        public IEnumerable<int> PathTo(K start_key, int count, Func<K, IEnumerable<K>> next)
        {
            for (int i = count; i-- > 0;)
            {
                if (this[i].BinarySearch(start_key) >= 0)
                {
                    continue;
                }

                foreach (var pair in Utils.Index(next(start_key)))
                {
                    if (this[i].BinarySearch(pair.Value) >= 0)
                    {
                        start_key = pair.Value;
                        yield return pair.Key;
                        break;
                    }
                }
            }
        }

        public void ForAllSolutions(K start_key, int max_depth, int reserved, int growth_rate, Func<K, IEnumerable<K>> next,
            Func<IEnumerable<int>, IEnumerable<int>, bool> on_solved)
        {
            var l_rings = new GeneralSolution<K>();
            l_rings.PrecomputeMoves(start_key, max_depth, reserved, growth_rate, next);

            // for each intersection
            for (int r = Count; r-- > 0;)
            {
                for (int l = l_rings.Count; l-- > 0;)
                {
                    foreach (K c in this[r].Intersection(l_rings[l]))
                    {
                        if (on_solved(l_rings.PathTo(c, l, next), PathTo(c, r, next)))
                        {
                            return;
                        }
                    }
                }
            }

            /*//
            var last = l_rings.Last();
            foreach(K k in last)
            {
                foreach(K c in next(k))
                {
                    int pos = this.FindRow(c);
                    if (pos >= 0 && l_rings.FindRow(c) < 0)
                    {
                        if (on_solved(l_rings.PathTo(c, l_rings.Count, next), PathTo(c, pos, next)))
                        {
                            return;
                        }
                    }
                }
            }
            //*/
        }

        public void Save(string fname, string extension, Action<BinaryWriter, K> saver)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Save(fname + $"{i}." + extension, saver);
            }
        }

        public void Load(string fname, string extension, int capacity, int element_size, Func<BinaryReader, K> reader)
        {
            Clear();
            Capacity = capacity;

            for (int i = 0; i < capacity; i++)
            {
                var list = new List<K>();
                list.Load(fname + $"{i}." + extension, element_size, reader);
                Add(list);
            }
        }
    }
}
