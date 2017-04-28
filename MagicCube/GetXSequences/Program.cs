using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MagicCube;
using MagicCube.Algorithms;

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

        static IEnumerable<FastCube> NextCubes(CubeGeneralSolution<FastCube> solution)
        {
            var ring = solution.Last();
            for (int i = 0; i < ring.Count; i++)
            {
                FastCube src_cube = ring[i];
                foreach (var dst_cube in Faces.NextCubes(src_cube))
                {
                    if (solution.FindRow(dst_cube) < 0)
                    {
                        yield return dst_cube;
                    }
                }
            }
        }

        static void MakeXSequences(CubeGeneralSolution<FastCube> cube_solution, CubeGeneralSolution<Cross> cross_solution,
            SaltireAlgorithms xalg)
        {
            SaltireAlgorithms alg = new SaltireAlgorithms();

            foreach (var dst_cube in NextCubes(cube_solution))
            {
                MoveTrack path_a = cube_solution.PathTo(dst_cube, cube_solution.Count);
                MoveTrack path_b = cross_solution.SolveCube(dst_cube.Middles, path_a.Count);
                if (path_a.Count > path_b.Count)
                {
                    ulong key = dst_cube.PlayForward(path_b).Corners.Transform;
                    if (!xalg.Tracks.ContainsKey(key) && !alg.Tracks.ContainsKey(key))
                    {
                        MoveTrack path = path_a.Reverse + path_b;
                        alg.Add(path);
                    }
                }
            }
            alg.Save("_test.txt");
        }

        static bool Increment(char[] path)
        {
            char ch_min = 'a';
            char ch_max = (char)('a' + 18 - 1);

            int i = path.Length;
            while (i-- > 0)
            {
                if (path[i] == ch_max)
                {
                    path[i] = ch_min;
                }
                else
                {
                    path[i]++;
                    return true;
                }
            }
            return false;
        }

        static void MakeReplaces(CubeGeneralSolution<FastCube> cube_solution)
        {
            char[] path = { 'a', 'a', 'a', 'a', 'a', 'a', 'a' };

            SearchTree substitute = new SearchTree();

            do
            {
                MoveTrack src_track = new MoveTrack(new string(path), false);
                src_track.Trim();

                if (src_track.Count == path.Length)
                {
                    CubeKey key = FastCube.Identity.PlayForward(src_track).Key;

                    int pos = cube_solution.FindRow(key);
                    if (pos >= 0)
                    {
                        MoveTrack dst_track = cube_solution.PathTo(key, pos);
                        dst_track = dst_track.Reverse;

                        int delta = src_track.Count - dst_track.Count;
                        if(delta > 2)
                        {
                            int beg = 0;                 // left trim count
                            int end = dst_track.Count;   // dst_track.Count - right trim count

                            while (beg < end && dst_track.Track[beg] == src_track.Track[beg])
                            {
                                beg++;
                            }

                            while (end > beg && dst_track.Track[end - 1] == src_track.Track[end - 1 + delta])
                            {
                                end--;
                            }

                            MoveTrack dst = dst_track.SubTrack(beg, end - beg);
                            MoveTrack src = src_track.SubTrack(beg, end - beg + delta);

                            if (!substitute.ContainsKey(src.Track))
                            {
                                substitute.Add(src.Track, dst.Track);
                            }
                            //break;
                        }
                    }
                }
            }
            while (Increment(path));

            substitute.Save("subs.txt");
        }

        static IEnumerable<MoveTrack> Optimize(MoveTrack path, CubeGeneralSolution<FastCube> cube_solution)
        {
            int min_length = 11;
            for (int i = 0; i < path.Count - min_length; i++)
            {
                for (int length = path.Count - i; length > min_length; length--)
                {
                    MoveTrack src = path.SubTrack(i, length);

                    FastCube cube = FastCube.Identity.PlayBackward(src);

                    MoveTrack dst = cube_solution.SolveCube(cube, min_length - cube_solution.Count + 1);
                    if(dst != null)
                    {
                        dst = path.SubTrack(0, i) + dst + path.SubTrack(length, path.Count - length);
                        yield return dst;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            const int max_depth = 7;

            // 1. Generate cube rings
            var cube_solution = new CubeGeneralSolution<FastCube>();

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
                cube_solution.PrecomputeMoves(FastCube.Identity, max_depth, 18, 13, Faces.NextCubes);
                Console.WriteLine("done!");

                Action<System.IO.BinaryWriter, FastCube> saver = (bw, k) =>
                {
                    bw.Write(k.Corners);
                    bw.Write(k.Middles);
                };

                Console.Write("Saving Moves... ");
                cube_solution.Save(Constants.FnameCubeRings, Constants.ExtensionCubeRings, saver);
                Console.WriteLine("done!");
            }

            //MakeReplaces(cube_solution);

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
            var middle_solution = new CrossSolution(); //new CubeGeneralSolution<Cross>();
            foreach (var ring in cube_solution)
            {
                var middles = new List<Cross>(from k in ring select k.Middles);
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

            //Console.Write("Loading additional saltire sequences... ");
            //var xalg = new SaltireAlgorithms();
            try
            {
                /*
                xalg.Load("_test_13_2.txt");
                xalg.Load("_test_13_3.txt");
                xalg.Load("_test_13_4.txt");
                xalg.Load("_test_13_5.txt");
                xalg.Load("_test_13_6.txt");
                xalg.Load("_test_13_7.txt");
                xalg.Load("_test_13_8.txt");
                */
                //xalgorithms.Load("_xsequences.13.new.txt");
               // Console.WriteLine("done!");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Console.WriteLine(xalgorithms.Tracks.Count);

            //Console.Write("Purging saltire sequences... ");
            //xalgorithms.Purge(from pair in xalgorithms.Tracks where pair.Value.Count <= 11 select pair.Key);
            //Console.WriteLine("done!");
            //xalgorithms.Save(Constants.FnameXAlgorithms);
            //Console.WriteLine(xalgorithms.Tracks.Count);

            Console.Write("Loading additional substitutes... ");
            //var substitutes = new SearchTree();
            try
            {
                MoveTrack._substitute.Load("subs.txt");
                Console.WriteLine("done!");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            //*/
            /*//
            var short_tracks = new List<MoveTrack>(from track in xalgorithms.Tracks.Values where track.Count < 12 select track);
            foreach(var track_a in short_tracks)
            {
                foreach (var track_b in short_tracks)
                {
                    MoveTrack path = track_a + track_b;
                    if (path.Count <= 12)
                    {
                        var key = FastCube.Identity.Corners.PlayForward(path).Transform;
                        if (xalgorithms.Tracks.ContainsKey(key))
                        {
                            //MoveTrack path_13 = xalg.Tracks[key];
                            if (xalgorithms.Tracks[key].Count >= path.Count)
                            {
                                xalgorithms.Tracks.Remove(key);
                            }

                        }
                    }
                }
                Console.WriteLine(xalgorithms.Tracks.Count);
            }

            xalgorithms.Save("_xsequences.new.txt");
            //*/
            //xalgorithms.Save(Constants.FnameXAlgorithms);
            //MakeXSequences(cube_solution, middle_solution, xalgorithms);

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
            int max_count = 0;
            //var long_algs = new SaltireAlgorithms();
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

                // 3. Randomize
                Console.WriteLine($"Cube (0): {cube}");

                MoveTrack total = null;
                //total = algorithms.Solve(cube.Key, FastCube.Identity, 50, c => c._cube.CountSolvedCubelets);
                //if (total == null)
                //{
                //    total = new MoveTrack();
                //}

                /*//
                // Solve for max elements
                //while (true)
                {
                    Console.Write("Solving ... ");
                    //MoveTrack path = cube_solution.SolveCube(cube, max_depth - 1, x => x, x => x, x => x.CountSolvedCubelets);
                    MoveTrack path = cube.Solve(cube_solution, max_depth - 1);
                    if (path != null)
                    {
                        Console.WriteLine($"done! {path.Count}: {path}");
                        cube = cube.PlayForward(path);
                        total += path;
                        solved_middles = cube.Middles.CountSolvedCubelets;
                        solved_corners = cube.Corners.CountSolvedCubelets;

                        Console.WriteLine($"Cube (1): {cube}");
                    }
                    else
                    {
                        //Console.WriteLine("error!");
                        //break;
                    }
                }
                //*/

                // 4. Solve middles
                while (solved_middles < Cross.CUBELET_NUM)
                {
                    //Console.Write("Solving middles... ");

                    Predicate<MoveTrack> on_solved = (midd_path) =>
                    {
                        bool happy = false;
                        MoveTrack corn_path = xalgorithms.Solve(cube.Corners.PlayForward(midd_path), midd_path);

                        if (corn_path != null)
                        {
                            MoveTrack full_path = midd_path + corn_path;
                            if (total == null || full_path.Count < total.Count)
                            {
                                total = full_path;
                                Console.WriteLine($"{total.Count}:{total}\n\t({midd_path.Count}.{corn_path.Count})");
                            }

                            if (total.Count < 26)
                            {
                                happy = true;
                            }

                            if (midd_path.Count > 12)
                            {
                                happy = true;
                            }

                            if (midd_path.Count >= 12 && total.Count < 30)
                            {
                                happy = true;
                            }

                            if (midd_path.Count >= 11 && total.Count < 28)
                            {
                                happy = true;
                            }
                        }
                        return happy;
                    };

                    //middle_solution.SolveCube(cube.Middles, max_depth, on_solved);

                    foreach(MoveTrack midd_tr in middle_solution.AllSolutions(cube.Middles, max_depth))
                    {
                        MoveTrack corn_tr = xalgorithms.Solve(cube.Corners.PlayForward(midd_tr), midd_tr);

                        if (corn_tr != null)
                        {
                            MoveTrack full_path = midd_tr + corn_tr;
                            if (total == null || full_path.Count < total.Count)
                            {
                                total = full_path;
                                Console.WriteLine($"{total.Count}:{total}\n\t({midd_tr.Count}.{corn_tr.Count})");
                            }
                        }
                    }

                    MoveTrack path = total;

                    //Cross.NextKeys(cube.Middles);

                    /*//
                    int corners = 0;
                    foreach(MoveTrack p in middle_solution.AllSolutions(cube.Middles, max_depth))
                    {
                        int csc = cube.Corners.PlayForward(p).CountSolvedCubelets;
                        if (path == null || corners < csc || (corners == csc && path.Count > p.Count))
                        {
                            path = p;
                            corners = csc;
                        }
                    }
                    //*/

                    //DateTime t0 = DateTime.Now;

                    //Console.WriteLine($"{path.Count}:{path}");
                    //TimeSpan dt = DateTime.Now - time;
                    //Console.WriteLine("Time: " + dt.ToString(@"mm\:ss\.fff"));

                    //t0 = DateTime.Now;
                    //path = middle_solution.SolveCube(cube.Middles, max_depth, x => x, x => x);
                    //dt = DateTime.Now - time;
                    //Console.WriteLine("Time: " + dt.ToString(@"mm\:ss\.fff"));

                    if (path != null)
                    {
                        //Console.WriteLine($"done! {path.Count}: {path}");
                        cube = cube.PlayForward(path);
                        //total += path;
                        solved_middles = cube.Middles.CountSolvedCubelets;
                        solved_corners = cube.Corners.CountSolvedCubelets;
                    }
                    else
                    {
                        Console.WriteLine("error!");
                        goto Loop_start;
                    }
                }

                Console.WriteLine($"Cube (1): {cube}");

                // 5. Solve corners
                while (solved_middles == Cross.CUBELET_NUM && solved_corners != Saltire.CUBELET_NUM)
                {
                    Console.Write("Solving corners... ");

                    MoveTrack path = xalgorithms.Solve(cube.Corners, total);
                    //MoveTrack path = xalgorithms.Solve(cube.Corners, 12);
                    if (path == null)
                    {
                        path = cube_solution.SolveCube(cube, max_depth);
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

                //Console.WriteLine($"Cube (2): {cube}");

                if (max_count < total.Count)
                {
                    max_count = total.Count;
                    //long_algs.Tracks.Clear();
                }

                //if (max_count == total.Count)
                //{
                //    long_algs.Add(path);
                //}

                Console.WriteLine($"Total {total.Count} ({max_count}): {total}");

                TimeSpan delta = DateTime.Now - time;
                Console.WriteLine("Time: " + delta.ToString(@"mm\:ss\.fff"));

               // MoveTrack test = new MoveTrack(substitutes.Replace(total.Track), false);
               // if(test.Count < total.Count)
               // {
               //     Console.WriteLine($"Total substitute {test.Count} ({max_count}): {test}");
               // }
               //foreach(var dst in Optimize(total, cube_solution))
               //{
               //     Console.WriteLine($"Optimized {dst.Count} ({max_count}): {dst}");
               //}
            }

            //long_algs.Save("_long_sequences.txt");
#endif            
        }
    }
}
