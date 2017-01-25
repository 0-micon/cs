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
        static void Main(string[] args)
        {
            const int max_depth = 7;

            // 1. Generate cube rings
            CubeKey key = FastCube.Identity;
            var cube_solution = new CubeGeneralSolution<CubeKey, FastCube>();

            try
            {
                Console.Write("Loading Moves... ");
                cube_solution.Load(Constants.FnameCubeRings, Constants.ExtensionCubeRings, max_depth,
                    sizeof(ulong) * 2, br => new CubeKey(br.ReadUInt64(), br.ReadUInt64()));
                Console.WriteLine("done!");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                Console.Write("Precomputing Moves... ");
                cube_solution.PrecomputeMoves(key, max_depth, 18, 13, FastCube.NextKeys);
                Console.WriteLine("done!");

                Action<System.IO.BinaryWriter, CubeKey> saver = (bw, k) =>
                {
                    bw.Write(k.corners);
                    bw.Write(k.middles);
                };

                Console.Write("Saving Moves... ");
                cube_solution.Save(Constants.FnameCubeRings, Constants.ExtensionCubeRings, saver);
                Console.WriteLine("done!");
            }


            var middle_solution = new CubeGeneralSolution<ulong, Cross>();
            foreach (var ring in cube_solution)
            {
                List<ulong> middles = new List<ulong>(from k in ring select k.middles);
                middles.DistinctValues();
                middle_solution.Add(middles);

                Console.WriteLine($"{ring.Count}:{middles.Count}");
            }

            // 2. Load xsequences
            Console.Write("Loading saltire sequences... ");
            SaltireAlgorithms xalgorithms = new SaltireAlgorithms();
            xalgorithms.Load(Constants.FnameXAlgorithms);
            Console.WriteLine("done!");
            Console.WriteLine(xalgorithms.Tracks.Count);

            while (true)
            {
            Loop_start:

                DateTime time = DateTime.Now;

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
                    MoveTrack path = middle_solution.SolveCube(cube.Middles, max_depth, x => x, x => x);
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

                    MoveTrack path = cube_solution.SolveCube(cube, max_depth, x => x, x => x);
                    if (path == null)
                    {
                        path = xalgorithms.RunOnce(cube.Corners);
                    }
                    else if (path.Count > 0 && xalgorithms.Add(path) > 0)
                    {
                        Console.Write("New xsequences! ");
                        xalgorithms.Save(Constants.FnameXAlgorithms);
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

                TimeSpan delta = DateTime.Now - time;
                Console.WriteLine("Time: " + delta.ToString(@"mm\:ss\.fff"));
                //break;
            }
            
        }

    }
}
