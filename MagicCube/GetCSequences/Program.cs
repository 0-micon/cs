using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicCube;

namespace GetCSequences
{
    class Program
    {
        public static void Main(string[] args)
        {
            const int max_depth = 7;
            int max_count = 0;

            // 1. Generate cube rings
            var cube_solution = new CubeGeneralSolution<FastCube>();

            try
            {
                Console.Write("Loading Moves... ");
                cube_solution.Load(Constants.FnameCubeRings, Constants.ExtensionCubeRings, max_depth,
                    sizeof(ulong) * 2, br => new FastCube(br.ReadUInt64(), br.ReadUInt64()));
                Console.WriteLine("done!");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                Console.Write("Precomputing Moves... ");
                cube_solution.PrecomputeMoves(FastCube.Identity, max_depth, 18, 13, Faces.NextCubes);
                Console.WriteLine("done!");

                Action<BinaryWriter, FastCube> saver = (bw, k) =>
                {
                    CubeKey key = k;
                    bw.Write(key.corners);
                    bw.Write(key.middles);
                };

                Console.Write("Saving Moves... ");
                cube_solution.Save(Constants.FnameCubeRings, Constants.ExtensionCubeRings, saver);
                Console.WriteLine("done!");
            }

            var salt_solution = new CubeGeneralSolution<Saltire>();
            salt_solution.PrecomputeMoves(Saltire.IDENTITY, 7, 18, 13, Faces.NextCubes);

            // 2. Load csequences
            Console.Write("Loading Cross sequences... ");
            var calgorithms = new CrossAlgorithms();
            try
            {
                calgorithms.Load(Constants.FnameCAlgorithms);
                Console.WriteLine("done!");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);

                calgorithms.AddShortAlgorithms(cube_solution);
                calgorithms.Save(Constants.FnameCAlgorithms);
            }
            Console.WriteLine(calgorithms.Tracks.Count);
            calgorithms.Purge(from pair in calgorithms.Tracks where pair.Value.Count <= 7 select pair.Key);
            calgorithms.Save(Constants.FnameCAlgorithms);
            Console.WriteLine(calgorithms.Tracks.Count);

            while (true)
            {
            Loop_start:
                MoveTrack total = null;

                if (Console.ReadLine().ToUpper().StartsWith("Q"))
                {
                    break;
                }

                // 3. Randomize
                FastCube cube = FastCube.Identity.Random();
                Console.WriteLine($"\nCube (0): {cube}");

                DateTime time = DateTime.Now;

                int solved_corners = cube.Corners.CountSolvedCubelets;
                int solved_middles = cube.Middles.CountSolvedCubelets;

                // 4. Solving corners
                while (solved_corners < Saltire.CUBELET_NUM)
                {
                    Console.Write("Solving corners... ");

                    int solved = -1;
                    MoveTrack path = null;
                    foreach (MoveTrack p in salt_solution.AllSolutions(cube.Corners, 7))
                    {
                        int x = cube.Middles.PlayForward(p).CountSolvedCubelets;
                        if (x > solved)
                        {
                            solved = x;
                            path = p;
                        }
                        else if (x == solved && p.Count < path.Count)
                        {
                            path = p;
                        }
                    }

                    if (path == null)
                    {
                        Console.WriteLine("error!");
                        goto Loop_start;
                    }

                    Console.WriteLine($"done! {path.Count}: {path}");
                    cube = cube.PlayForward(path);
                    Console.WriteLine($"Cube (1): {cube}");
                    solved_corners = cube.Corners.CountSolvedCubelets;
                    solved_middles = cube.Middles.CountSolvedCubelets;
                    total = path;
                }

                // 5. Solve middles
                while (solved_middles < Cross.CUBELET_NUM)
                {
                    Console.Write("Solving middles... ");

                    MoveTrack path = calgorithms.Solve(cube.Middles, 12);
                    if (path == null)
                    {
                        //path = cube_solution.SolveCube(cube, max_depth, x => x, x => x);
                        //if (path != null && path.Count > 0 && xalgorithms.Add(path) > 0)
                        //{
                        //    Console.Write("New xsequences! ");
                        //    xalgorithms.Save(Constants.FnameXAlgorithms);
                        //}

                        //if (path == null)
                        //{
                        //    path = xalgorithms.RunOnce(cube.Corners);
                        //}
                    }

                    if (path == null || path.Count == 0)
                    {
                        Console.WriteLine("error!");
                        goto Loop_start;
                    }

                    Console.WriteLine($"done! {path.Count}: {path}");
                    cube = cube.PlayForward(path);
                    Console.WriteLine($"Cube (2): {cube}");
                    total += path;
                    solved_middles = cube.Middles.CountSolvedCubelets;
                    solved_corners = cube.Corners.CountSolvedCubelets;
                }

                if (max_count < total.Count)
                {
                    max_count = total.Count;
                }

                Console.WriteLine($"Total {total.Count} ({max_count}): {total}");

                TimeSpan delta = DateTime.Now - time;
                Console.WriteLine("Time: " + delta.ToString(@"mm\:ss\.fff"));
            }
        }
    }
}
