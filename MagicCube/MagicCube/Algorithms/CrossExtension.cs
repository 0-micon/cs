﻿using System;
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

        public static MoveTrack Solve(this Cross cube, List<List<ulong>> r_rings, int depth)
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
                            path = next_cube.PathTo(l_rings);
                            path = path.Reverse();
                            path += next_cube.PathTo(r_rings, pos);
                            return path;
                        }
                        else
                        {
                            if (middles > solved_middles)
                            {
                                path = next_cube.PathTo(l_rings);
                                path = path.Reverse();

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
    }
}
