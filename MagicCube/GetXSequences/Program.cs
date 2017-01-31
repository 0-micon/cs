using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MagicCube;

namespace GetXSequences
{
    class FastCubeClass : Faces.IRotatable, Faces.IConvertible<CubeKey>
    {
        public FastCube _cube;

        public FastCubeClass()
        {
            _cube = FastCube.Identity;
        }

        public CubeKey Key
        {
            get
            {
                return _cube.Key;
            }

            set
            {
                _cube.Key = value;
            }
        }

        public void RotateFace(uint face)
        {
            _cube.RotateFace(face);
        }
    }

    class Program
    {
        static IEnumerable<MoveTrack> Load(string fname)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(fname))
            {
                for (string buf; (buf = file.ReadLine()) != null;)
                {
                    string[] arr = buf.Split(';');
                    if (arr.Length > 1)
                    {
                        yield return new MoveTrack(arr[1], false);
                    }
                }
            }
        }

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

            /*//
            var test = from x in cube_solution.Last() where (new FastCube(x.corners, x.middles)).CountSolvedCubelets >= 12 select x;
            var list = new List<CubeKey>(test);
            Console.WriteLine(list.Count);

            var alg = new Algorithms<CubeKey, FastCubeClass>();
            foreach(var c in list)
            {
                alg.Add(cube_solution.PathTo(c, x => x, x => x));
            }
            alg.Save(Constants.FnameAlgorithms);
            //*/

#if GET_SHORT_XSEQUENCES
#else
            var middle_solution = new CubeGeneralSolution<ulong, Cross>();
            foreach (var ring in cube_solution)
            {
                List<ulong> middles = new List<ulong>(from k in ring select k.middles);
                middles.DistinctValues();
                middle_solution.Add(middles);

                Console.WriteLine($"{ring.Count}:{middles.Count}");
            }
#endif
            // 2. Load xsequences
            Console.Write("Loading saltire sequences... ");
            SaltireAlgorithms xalgorithms = new SaltireAlgorithms();
            try
            {
                xalgorithms.Load(Constants.FnameXAlgorithms);
                //foreach(var path in Load(Constants.FnameXAlgorithms))
                //{
                //    xalgorithms.Add(cube_solution.SolveCube(FastCube.Identity.PlayForward(path), max_depth, x => x, x => x));
                //}
                Console.WriteLine("done!");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine(xalgorithms.Tracks.Count);
            //xalgorithms.Save(Constants.FnameXAlgorithms);

            /*//
            Console.Write("Loading common sequences... ");
            var algorithms = new Algorithms<CubeKey, FastCubeClass>();
            try
            {
                algorithms.Load(Constants.FnameAlgorithms);
                Console.WriteLine("done!");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine(algorithms.Tracks.Count);
            //*/
#if GET_SHORT_XSEQUENCES
            List<CubeKey> last = cube_solution.Last();
            int count = last.Count;

            var dict = new Dictionary<ulong, List<ulong>>();

            for (int i = 1; i < cube_solution.Count; i++)
            {
                var list = cube_solution[i];
                foreach(var src in list)
                {
                    if (!dict.ContainsKey(src.middles))
                    {
                        dict.Add(src.middles, new List<ulong>());
                    }
                    dict[src.middles].Add(src.corners);
                }
            }

            if (dict.ContainsKey(Cross.IDENTITY))
            {
                dict.Remove(Cross.IDENTITY);
            }

            var less_than_two = new List<ulong>(from p in dict where p.Value.Count < 2 select p.Key);
            foreach(var src in less_than_two)
            {
                dict.Remove(src);
            }
            less_than_two = null;

            //var two_or_more = from p in dict where p.Value.Count >= 2 select p;
            foreach (var pair in dict)
            {
                ulong middles = pair.Key;
                var corners_list = pair.Value;
                for(int i = 0; i < corners_list.Count; i++)
                {
                    CubeKey src = new CubeKey(corners_list[i], middles);

                    for (int j = i + 1; j < corners_list.Count; j++)
                    {
                        CubeKey dst = new CubeKey(corners_list[j], middles);

                        MoveTrack back = cube_solution.PathTo(dst, x => x, x => x);
                        MoveTrack head = cube_solution.PathTo(src, x => x, x => x);
                        MoveTrack path = head.Reverse() + back;

                        if(xalgorithms.Add(path) > 0)
                        {
                            Console.WriteLine(xalgorithms.Tracks.Count);
                        }
                    }
                }
            }

            xalgorithms.Save(Constants.FnameXAlgorithms);

            /*//
            int step = count / 100;
            for (int i = 0; i < count; i++)
            {
                if (i == step)
                {
                    int ratio = (step + 1) * 100 / count;
                    Console.WriteLine($"done {ratio}%");
                    step = (ratio + 1) * count / 100;
                }

                var src = last[i];
                foreach (var corners in from c in dict[src.middles] where c != src.corners select c)
                {
                    CubeKey dst = new CubeKey(corners, src.middles);
                    MoveTrack back = cube_solution.PathTo(dst, x => x, x => x);
                    MoveTrack head = cube_solution.PathTo(src, x => x, x => x);
                    MoveTrack path = head.Reverse() + back;

                    int result = xalgorithms.Add(path);
                    if (result > 0)
                    {
                        FastCube cube = FastCube.Identity.PlayForward(path);
                        path = cube_solution.SolveCube(cube, max_depth, x => x, x => x);
                        xalgorithms.Add(path);

                        Console.WriteLine($"New xsequences! {path.Count}: {path}");
                        xalgorithms.Save(Constants.FnameXAlgorithms);
                    }
                }
                //*/

                /*//
                FastCube cube_src = last[i];
                foreach (var move_0 in Faces.NextMoves(cube_src))
                {
                    foreach (var move_1 in Faces.NextMoves(move_0.Value))
                    {
                        foreach (var move_2 in Faces.NextMoves(move_1.Value))
                        {
                            if (move_2.Value.Middles.CountSolvedCubelets == Cross.CUBELET_NUM)
                            {
                                MoveTrack path = cube_solution.PathTo(move_0.Value, x => x, x => x);
                                path = path.Reverse();
                                path.Add(move_1.Key);
                                path.Add(move_2.Key);
                                path.Trim();

                                int result = xalgorithms.Add(path);
                                if (result > 0)
                                {
                                    Console.WriteLine($"New xsequences! {path.Count}: {path}");
                                    xalgorithms.Save(Constants.FnameXAlgorithms);
                                }
                            }
                        }
                    }
                }
                //*/
                /*//
            }
            //*/
#else
            while (true)
            {
            Loop_start:

                if (Console.ReadLine().ToUpper().StartsWith("Q"))
                {
                    break;
                }

                FastCube cube = FastCube.Identity.Random();
                Console.WriteLine("");

                DateTime time = DateTime.Now;

                int solved_corners = cube.Corners.CountSolvedCubelets;
                int solved_middles = cube.Middles.CountSolvedCubelets;
                key = cube; // Faces.RandomKey<CubeKey, FastCube>(key, x => x, x => x);

                // 3. Randomize
                Console.WriteLine($"Cube (0): key={key}, corners={solved_corners}, middles={solved_middles}");

                MoveTrack total = null;
                //total = algorithms.Solve(cube.Key, FastCube.Identity, 50, c => c._cube.CountSolvedCubelets);
                if (total == null)
                {
                    total = new MoveTrack();
                }

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

                    MoveTrack path = xalgorithms.Solve(cube.Corners, 10);
                    if (path == null)
                    {
                        path = cube_solution.SolveCube(cube, max_depth, x => x, x => x);
                        if (path != null && path.Count > 0 && xalgorithms.Add(path) > 0)
                        {
                            Console.Write("New xsequences! ");
                            xalgorithms.Save(Constants.FnameXAlgorithms);
                        }

                        if (path == null)
                        {
                            path = xalgorithms.RunOnce(cube.Corners);
                        }
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
#endif            
        }
    }
}
