using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class CubeGeneralSolution<TCube> : GeneralSolution<TCube> where TCube : Faces.IRotatable, IComparable<TCube>
    {
        public MoveTrack PathTo(TCube cube, int count) => new MoveTrack(PathTo(cube, count, Faces.NextCubes));

        public MoveTrack SolveCube(TCube src, int depth)
        {
            var ring = new List<TCube>();
            ring.Add(src);

            var l_rings = new CubeGeneralSolution<TCube>();
            l_rings.Add(ring);

            int reserved = 18;
            while (true)
            {
                List<TCube> next_ring = null;
                if (--depth > 0)
                {
                    next_ring = new List<TCube>(reserved);
                }

                foreach (var cube in ring)
                {
                    foreach (var dst_cube in Faces.NextCubes(cube))
                    {
                        int pos = this.FindRow(dst_cube);
                        if (pos >= 0)
                        {
                            return l_rings.PathTo(dst_cube, l_rings.Count).Reverse + PathTo(dst_cube, pos);
                        }
                        else if (next_ring != null && l_rings.FindRow(dst_cube) < 0)
                        {
                            next_ring.Add(dst_cube);
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

        public IEnumerable<MoveTrack> AllSolutions(TCube cube, int depth)
        {
            var solutions = new HashSet<MoveTrack>();

            Func<IEnumerable<int>, IEnumerable<int>, bool> on_solved = (l_path, r_path) =>
            {
                var a = new MoveTrack(l_path);
                var b = new MoveTrack(r_path);
                solutions.Add(a.Reverse + b);
                return false;
            };

            ForAllSolutions(cube, depth, 18, 13, Faces.NextCubes, on_solved);

            return solutions;
        }
    }
}
