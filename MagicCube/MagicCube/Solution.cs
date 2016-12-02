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

        List<List<ulong>> r_rings;
        
        public Solution()
        {
            r_rings = LoadRings(7, "middle_key_ring_", "ulong");
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
            foreach(ulong next_key in cube.Moves())
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

        public static void PrecomputeMoves(int level)
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
                    foreach (ulong next_key in cube.Moves())
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

        public static int TestKey(ulong key, List<HashSet<ulong>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && !solution[i].Contains(key));
            return i;
        }

        public static int TestKey(ulong key, List<List<ulong>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && solution[i].BinarySearch(key) < 0);
            return i;
        }

        public List<ulong> SolveMiddle(ulong middle_key)
        {
            //List<List<ulong>> r_rings = LoadRings(7, "middle_key_ring_", "ulong");

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
                    foreach (ulong next_key in cube.Moves())
                    {
                        int i = TestKey(next_key, r_rings);
                        if (i >= 0)
                        {
                            List<ulong> solution = new List<ulong>();
                            solution.Add(next_key);

                            for(int l = l_rings.Count; l-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.Moves(solution.Last()))
                                {
                                    if (l_rings[l].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }

                            solution.Reverse();
                            for(int r = i; r-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.Moves(solution.Last()))
                                {
                                    if (r_rings[r].BinarySearch(prev_key) >= 0)
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
            foreach (ulong next_key in cube.Moves())
            {
                ring.Add(next_key);
            }

            return ring;
        }
    }
}
