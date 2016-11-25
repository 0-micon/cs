using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class Solution
    {
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

        public static List<HashSet<ulong>> Run(ulong middle_key)
        {
            HashSet<ulong> ring = new HashSet<ulong>();
            ring.Add(middle_key);

            List<HashSet<ulong>> solution = new List<HashSet<ulong>>();
            solution.Add(ring);

            Cube cube = new Cube();
            for (int i = 0; i < 6; i++)
            {
                HashSet<ulong> next_ring = new HashSet<ulong>();
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
                solution.Add(next_ring);
                ring = next_ring;
            }
            return solution;
        }

        public static int TestKey(ulong key, List<HashSet<ulong>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && !solution[i].Contains(key));
            return i;
        }

        public static List<ulong> RunTo(ulong middle_key, List<HashSet<ulong>> r_rings)
        {
            HashSet<ulong> ring = new HashSet<ulong>();
            ring.Add(middle_key);

            List<HashSet<ulong>> l_rings = new List<HashSet<ulong>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            while (true)
            {
                HashSet<ulong> next_ring = new HashSet<ulong>();
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
                                    if (l_rings[l].Contains(prev_key))
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
                                    if (r_rings[r].Contains(prev_key))
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
