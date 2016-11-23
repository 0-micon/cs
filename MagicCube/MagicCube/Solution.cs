using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class Solution
    {
        Cube cube = new Cube();

        public delegate bool ContainsDelegate(ulong key);

        public List<HashSet<ulong>> Run()
        {
            List<HashSet<ulong>> solution = new List<HashSet<ulong>>();

            var ring = FirstIteration();
            solution.Add(ring);

            ContainsDelegate test = key =>
            {
                foreach(var set in solution)
                {
                    if (set.Contains(key))
                    {
                        return true;
                    }
                }
                return false;
            };

            for (int i = 0; i < 5; i++)
            {
                solution.Add(NextIteration(solution.Last(), test));
            }
            return solution;
        }

        public IEnumerable<ulong> Moves()
        {
            for (uint face = 0; face < Cube.FACE_NUM; face++)
            {
                cube.RotateRight(face);
                yield return cube.MiddleKey;

                cube.RotateRight(face);
                yield return cube.MiddleKey;

                cube.RotateRight(face);
                yield return cube.MiddleKey;

                cube.RotateRight(face); // restore
            }
        }

        public HashSet<ulong> FirstIteration()
        {
            HashSet<ulong> next_ring = new HashSet<ulong>();

            foreach (ulong next_key in Moves())
            {
                next_ring.Add(next_key);
            }

            return next_ring;
        }

        public HashSet<ulong> NextIteration(HashSet<ulong> ring, ContainsDelegate test)
        {
            HashSet<ulong> next_ring = new HashSet<ulong>();

            foreach (ulong key in ring)
            {
                cube.MiddleKey = key;
                foreach(ulong next_key in Moves())
                {
                    if (!test(next_key))
                    {
                        next_ring.Add(next_key);
                    }
                }
            }
            
            return next_ring;
        }
    }
}
