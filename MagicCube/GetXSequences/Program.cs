using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MagicCube;

namespace GetXSequences
{
    class Program
    {
        const string _fname_xalgorithms = @"..\..\..\_xsequences.txt";

        static void Main(string[] args)
        {
            // 1. Generate cube rings
            CubeKey key = FastCube.Identity;

            Console.Write("Precomputing Moves... ");
            var cube_rings = Solution.PrecomputeMoves(key, 7, 18, 13, FastCube.NextKeys);
            Console.WriteLine("done!");

            var middle_rings = new List<List<ulong>>(cube_rings.Count);
            foreach (var ring in cube_rings)
            {
                List<ulong> middles = new List<ulong>(from k in ring select k.middles);
                middles.DistinctValues();
                middle_rings.Add(middles);

                Console.WriteLine($"{ring.Count}:{middles.Count}");
            }

            // 2. Load xsequences
            Console.Write("Loading saltire sequences... ");
            SaltireAlgorithms xalgorithms = new SaltireAlgorithms();
            xalgorithms.Load(_fname_xalgorithms);
            Console.WriteLine("done!");
            Console.WriteLine(xalgorithms.Tracks.Count);

            while (true)
            {
            Loop_start:
                Console.WriteLine("");
                key = Faces.RandomKey<CubeKey, FastCube>(key, x => x, x => x);

                FastCube cube = key;

                int solved_corners = cube.Corners.CountSolvedCubelets;
                int solved_middles = cube.Middles.CountSolvedCubelets;

                // 3. Randomize
                Console.WriteLine($"Cube (0): key={key}, corners={solved_corners}, middles={solved_middles}");

                MoveTrack total = new MoveTrack();

                // 4. Solve middles
                while (solved_middles < Cross.CUBELET_NUM)
                {
                    Console.Write("Solving middles... ");
                    MoveTrack path = Solution.SolveMiddle(cube.Middles, 7, middle_rings);
                    if (path != null)
                    {
                        Console.WriteLine($"done! {path.Count}: {path}");
                        cube = cube.PlayForward(path);
                        total += path;
                        solved_middles = cube.Middles.CountSolvedCubelets;
                        solved_corners = cube.Corners.CountSolvedCubelets;
                    }
                    else
                    {
                        Console.WriteLine("error!");
                        goto Loop_start;
                    }
                }

                key = cube;
                Console.WriteLine($"Cube (1): key={key}, corners={solved_corners}, middles={solved_middles}");

                // 5. Solve corners
                while (solved_middles == Cross.CUBELET_NUM && solved_corners != Saltire.CUBELET_NUM)
                {
                    Console.Write("Solving corners... ");


                    MoveTrack path = Solution.SolveCube(cube, 7, cube_rings);
                    if (path == null)
                    {
                        path = xalgorithms.RunOnce(cube.Corners);
                    }
                    else if (path.Count > 0 && xalgorithms.Add(path) > 0)
                    {
                        Console.Write("New xsequences! ");
                        xalgorithms.Save(_fname_xalgorithms);
                    }

                    if (path == null || path.Count == 0)
                    {
                        Console.WriteLine("error!");
                        goto Loop_start;
                    }

                    Console.WriteLine($"done! {path.Count}: {path}");
                    cube = cube.PlayForward(path);
                    total += path;
                    solved_middles = cube.Middles.CountSolvedCubelets;
                    solved_corners = cube.Corners.CountSolvedCubelets;
                }

                key = cube;
                Console.WriteLine($"Cube (2): key={key}, corners={solved_corners}, middles={solved_middles}");
                Console.WriteLine($"Total {total.Count}: {total}");
                //break;
            }
            
        }

    }
}
