using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace MagicCube
{
    class Solution
    {

        List<List<ulong>> middle_rings;
        List<List<ulong>> corner_rings;

        public Solution()
        {
            middle_rings = LoadRings(7, "middle_key_ring_", "ulong");
            corner_rings = LoadRings(7, "corner_key_ring_", "ulong");
        }
        
        public struct Move
        {
            public uint face;
            public int count;
            public ulong key;
            public Move(uint face, int count, ulong key)
            {
                this.face = face;
                this.count = count;
                this.key = key;
            }
        }


        public Move? FindMove(HashSet<ulong> ring, ulong middle_key)
        {
            Cube cube = new Cube();
            cube.MiddleKey = middle_key;
            int count = 0;
            foreach(ulong next_key in cube.NextMiddleKeys())
            {
                if (ring.Contains(next_key))
                {
                    uint face = (uint)count / 3;
                    return new Move(face, count % 3 + 1, next_key);
                }
                count++;
            }
            return null;
        }

        public List<Move> Path(List<HashSet<ulong>> rings, ulong middle_key)
        {
            List<Move> path = new List<Move>();

            return path;
        }

        public static void SaveRing(IEnumerable<ulong> ring, string fname)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fname, FileMode.Create)))
            {
                foreach(ulong key in ring)
                {
                    bw.Write(key);
                }
            }
        }

        public static void SaveRing(IEnumerable<Key> ring, string fname)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fname, FileMode.Create)))
            {
                foreach (Key key in ring)
                {
                    bw.Write(key.corners);
                    bw.Write(key.middles);
                }
            }
        }

        public static List<ulong> LoadList(string fname)
        {
            List<ulong> list = null;
            using (BinaryReader br = new BinaryReader(File.Open(fname, FileMode.Open)))
            {
                long flength = (new FileInfo(fname)).Length;
                int count = (int)(flength / sizeof(ulong));
                list = new List<ulong>(count);
                while(count-- > 0)
                {
                    list.Add(br.ReadUInt64());
                }
            }
            return list;
        }

        public static List<List<ulong>> LoadRings(int capacity, string fname, string extension)
        {
            List<List<ulong>> rings = new List<List<ulong>>(capacity);
            for(int i = 0; i < capacity; i++)
            {
                rings.Add(LoadList(fname + i + "." + extension));
            }
            return rings;
        }

        public static List<ulong> ToList(IEnumerable<ulong> ring)
        {
            List<ulong> list = new List<ulong>(ring.Count());
            foreach(ulong key in ring)
            {
                list.Add(key);
            }
            list.Sort();
            return list;
        }

        public static void DistinctValues<T>(List<T> list)
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

        public struct Key : IComparable<Key>
        {
            public ulong corners;
            public ulong middles;

            public Key(ulong corners, ulong middles)
            {
                this.corners = corners;
                this.middles = middles;
            }

            public Key(Cube cube) : this(cube.CornerKey, cube.MiddleKey)
            {
            }

            public bool Equals(Key other)
            {
                return (corners == other.corners) && (middles == other.middles);
            }

            // The == and != operators cannot operate on a struct unless the struct explicitly overloads them.
            public static bool operator ==(Key a, Key b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Key a, Key b)
            {
                return !(a.Equals(b));
            }

            public override bool Equals(object obj)
            {
                if (obj is Key)
                {
                    return Equals((Key)obj);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (int)((corners ^ middles) / 99991);
            }

            public int CompareTo(Key other)
            {
                if(corners == other.corners)
                {
                    if(middles == other.middles)
                    {
                        return 0;
                    }
                    else
                    {
                        return (middles < other.middles) ? -1 : 1;
                    }
                }
                else
                {
                    return (corners < other.corners) ? -1 : 1; 
                }
            }
        }

        public static void PrecomputeMoves(int level)
        {
            Cube cube = new Cube();

            List<Key> ring = new List<Key>(1);
            ring.Add(new Key(cube));

            List<List<Key>> solution = new List<List<Key>>(level);
            solution.Add(ring);

            int reserved = 18;
            for (int i = 0; i < level; i++)
            {
                List<Key> next_ring = new List<Key>(reserved);
                foreach (Key key in ring)
                {
                    cube.CornerKey = key.corners;
                    cube.MiddleKey = key.middles;
                    foreach (uint next_move in cube.Moves())
                    {
                        Key next_key = new Key(cube);
                        if (TestKey(next_key, solution) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                reserved = next_ring.Count * 14;
                DistinctValues(next_ring);

                solution.Add(next_ring);
                ring = next_ring;
            }

            for (int i = 0; i < solution.Count; i++)
            {
                SaveRing(solution[i], "key_ring_" + i + ".ulong");
            }
        }

        public static void PrecomputeMiddleMoves(int level)
        {
            Cube cube = new Cube();

            List<ulong> ring = new List<ulong>(1);
            ring.Add(cube.MiddleKey);

            List<List<ulong>> solution = new List<List<ulong>>(level);
            solution.Add(ring);

            int reserved = 18;
            for (int i = 0; i < level; i++)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.MiddleKey = key;
                    foreach (ulong next_key in cube.NextMiddleKeys())
                    {
                        if (TestKey(next_key, solution) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                //next_ring.Sort();
                //next_ring = next_ring.Distinct().ToList();
                reserved = next_ring.Count * 13;
                DistinctValues(next_ring);

                solution.Add(next_ring);
                ring = next_ring;
            }

            for(int i = 0; i < solution.Count; i++)
            {
                SaveRing(solution[i], "middle_key_ring_" + i + ".ulong");
            }
        }

        public static void PrecomputeCornerMoves(int level)
        {
            Cube cube = new Cube();

            List<ulong> ring = new List<ulong>(1);
            ring.Add(cube.CornerKey);

            List<List<ulong>> solution = new List<List<ulong>>(level);
            solution.Add(ring);

            int reserved = 18;
            for (int i = 0; i < level; i++)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.CornerKey = key;
                    foreach (ulong next_key in cube.NextCornerKeys())
                    {
                        if (TestKey(next_key, solution) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                reserved = next_ring.Count * 13;
                DistinctValues(next_ring);

                solution.Add(next_ring);
                ring = next_ring;
            }

            for (int i = 0; i < solution.Count; i++)
            {
                SaveRing(solution[i], "corner_key_ring_" + i + ".ulong");
            }
        }

        public static int TestKey(ulong key, List<HashSet<ulong>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && !solution[i].Contains(key));
            return i;
        }

        public static int TestKey<T>(T key, List<List<T>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && solution[i].BinarySearch(key) < 0);
            return i;
        }

        public List<ulong> SolveCorners(ulong corner_key)
        {
            List<ulong> ring = new List<ulong>();
            ring.Add(corner_key);

            List<List<ulong>> l_rings = new List<List<ulong>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            int reserved = 18;
            while (true)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.CornerKey = key;
                    foreach (uint move in cube.Moves())
                    {
                        ulong next_key = cube.CornerKey;
                        int pos = TestKey(next_key, corner_rings);
                        if (pos >= 0)
                        {
                            List<ulong> solution = new List<ulong>();
                            solution.Add(next_key);

                            for (int l = l_rings.Count; l-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextCornerKeys(solution.Last()))
                                {
                                    if (l_rings[l].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }

                            solution.Reverse();
                            for (int r = pos; r-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextCornerKeys(solution.Last()))
                                {
                                    if (corner_rings[r].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }
                            return solution;
                        }
                        else if (TestKey(next_key, l_rings) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }
                reserved = next_ring.Count * 13;
                DistinctValues(next_ring);
                l_rings.Add(next_ring);
                ring = next_ring;
            }
        }

        public List<ulong> SolveMiddle(ulong middle_key)
        {
            List<ulong> ring = new List<ulong>();
            ring.Add(middle_key);

            List<List<ulong>> l_rings = new List<List<ulong>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            int reserved = 18;
            while (true)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.MiddleKey = key;
                    foreach (ulong next_key in cube.NextMiddleKeys())
                    {
                        int pos = TestKey(next_key, middle_rings);
                        if (pos >= 0)
                        {
                            List<ulong> solution = new List<ulong>();
                            solution.Add(next_key);

                            for(int l = l_rings.Count; l-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextMiddleKeys(solution.Last()))
                                {
                                    if (l_rings[l].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }

                            solution.Reverse();
                            for(int r = pos; r-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextMiddleKeys(solution.Last()))
                                {
                                    if (middle_rings[r].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }
                            return solution;
                        }
                        else if (TestKey(next_key, l_rings) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }
                reserved = next_ring.Count * 13;
                DistinctValues(next_ring);
                l_rings.Add(next_ring);
                ring = next_ring;
            }        
        }

        public HashSet<ulong> GetMoveKeys(ulong middle_key)
        {
            HashSet<ulong> ring = new HashSet<ulong>();
            Cube cube = new Cube();
            cube.MiddleKey = middle_key;
            foreach (ulong next_key in cube.NextMiddleKeys())
            {
                ring.Add(next_key);
            }

            return ring;
        }
    }
}
