using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class CubeGeneralSolution<K, T> : GeneralSolution<K> where T : Faces.IRotatable
    {
        public MoveTrack PathTo(K key, Func<K, T> KtoT, Func<T, K> TtoK)
        {
            MoveTrack path = new MoveTrack();
            for (int i = Count; i-- > 0;)
            {
                foreach (var pair in Faces.NextMoves(KtoT(key)))
                {
                    K dst_key = TtoK(pair.Value);
                    if (this[i].BinarySearch(dst_key) >= 0)
                    {
                        key = dst_key;
                        path.Add(pair.Key);
                        break;
                    }
                }
            }
            return path;
        }

        public MoveTrack SolveCube(K start_key, int depth, Func<K, T> KtoT, Func<T, K> TtoK)
        {
            List<K> ring = new List<K>();
            ring.Add(start_key);

            CubeGeneralSolution<K, T> l_rings = new CubeGeneralSolution<K, T>();
            l_rings.Add(ring);

            int reserved = 18;
            while (true)
            {
                List<K> next_ring = null;
                if (--depth > 0)
                {
                    next_ring = new List<K>(reserved);
                }

                foreach (K key in ring)
                {
                    foreach (K next_key in Faces.NextKeys(key, KtoT, TtoK))
                    {
                        int pos = this.FindRow(next_key);
                        if (pos >= 0)
                        {
                            MoveTrack path = l_rings.PathTo(next_key, KtoT, TtoK);
                            path = path.Reverse();
                            path += PathTo(next_key, KtoT, TtoK);
                            return path;
                        }
                        else if (next_ring != null && l_rings.FindRow(next_key) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                if (next_ring == null)
                {
                    return null;
                }

                reserved = next_ring.Count * 13;
                next_ring.DistinctValues();
                l_rings.Add(next_ring);
                ring = next_ring;
            }
        }
    }
}
