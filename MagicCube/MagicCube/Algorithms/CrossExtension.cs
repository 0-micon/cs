using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube.Algorithms
{
    public static class CrossExtension
    {
        public static MoveTrack PathTo(this Cross cube, List<List<ulong>> rings, int begin = -1)
        {
            MoveTrack path = new MoveTrack();
            if (begin < 0)
            {
                begin = rings.Count;
            }

            for (int i = begin; i-- > 0;)
            {
                if (rings[i].BinarySearch(cube) >= 0)
                {
                    continue;
                }

                foreach (var pair in Faces.NextMoves(cube))
                {
                    ulong dst_key = pair.Value;
                    if (rings[i].BinarySearch(dst_key) >= 0)
                    {
                        cube = dst_key;
                        path.Add(pair.Key);
                        break;
                    }
                }
            }
            return path;
        }

        public static MoveTrack Solve(this Cross cube, List<List<ulong>> r_rings, int depth, Predicate<MoveTrack> on_solved = null)
        {
            var ring = new List<ulong>();
            ring.Add(cube);

            var l_rings = new List<List<ulong>>();
            l_rings.Add(ring);

            int solved_middles = cube.CountSolvedCubelets;
            int reserved = 18;
            MoveTrack path = null;

            while (true)
            {
                List<ulong> next_ring = null;
                if (--depth > 0)
                {
                    next_ring = new List<ulong>(reserved);
                }

                foreach (var key in ring)
                {
                    cube = key;

                    foreach (var next_cube in Faces.NextCubes(cube))
                    {
                        int middles = next_cube.CountSolvedCubelets;

                        int pos = r_rings.FindRow(next_cube);
                        if (pos >= 0)
                        {
                            path = next_cube.PathTo(l_rings).Reverse + next_cube.PathTo(r_rings, pos);
                            if (on_solved == null || on_solved(path))
                            {
                                return path; 
                            }
                            // no need in next_ring anymore?
                            //next_ring = null;
                        }
                        else
                        {
                            if (middles > solved_middles)
                            {
                                path = next_cube.PathTo(l_rings);
                                path = path.Reverse;

                                solved_middles = middles;
                            }

                            if (next_ring != null && l_rings.FindRow(next_cube) < 0)
                            {
                                next_ring.Add(next_cube);
                            }
                        }
                    }
                }

                if (next_ring == null)
                {
                    return path;
                }

                reserved = next_ring.Count * 13;
                next_ring.DistinctValues();
                l_rings.Add(next_ring);
                ring = next_ring;
            }
        }

        public static IEnumerable<MoveTrack> AllSolutions(this Cross cube, List<List<ulong>> r_rings, int depth)
        {
            var l_rings = new GeneralSolution<ulong>();
            l_rings.PrecomputeMoves(cube, depth, 18, 13, Cross.NextKeys);

            // for each intersection
            for (int r = r_rings.Count; r-- > 0;)
            {
                var dst = r_rings[r];
                for(int l = l_rings.Count; l-- > 0;)
                {
                    var src = l_rings[l];

                    foreach(Cross c in dst.Intersection(src))
                    {
                        yield return c.PathTo(l_rings, l).Reverse + c.PathTo(r_rings, r);
                    }
                }
            }
        }
    }
}
