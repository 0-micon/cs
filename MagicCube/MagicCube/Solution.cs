using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;


namespace MagicCube
{
    public class Solution
    {

        internal List<List<ulong>> middle_rings;
        internal List<List<ulong>> corner_rings;
        internal List<List<CubeKey>> cube_rings;


        public Solution()
        {
            middle_rings = LoadRings(7/* + 1*/, "_middle_key_ring_", "ulong");
            corner_rings = LoadRings(7/* + 1*/, "corner_key_ring_", "ulong");
            cube_rings = LoadKeyRings(6 + 1, "key_ring_", "ulong");

            // SaveShortSequences("short_sequences.txt");
            //SaveNextSequences("next_sequences.txt");
            //SaveLongSequences("6_sequences.txt", "long_sequences.txt");
            /*//
            Algorithm test = new Algorithm();
            test.Load("long_sequences.txt");

            test.Save("long_sequences_new.txt");

            Algorithm test2 = new Algorithm();
            test2.Load("long_sequences_new.txt");

            
            Algorithm test_7 = new Algorithm();
            test_7.Load("7_sequences.txt");

            foreach(Key key in test_7.tracks.Keys)
            {
                if (test.tracks.ContainsKey(key))
                {
                    test.tracks.Remove(key);
                }
            }

            test.Save("long_sequences_new.txt");
            //*/
            /*//
            var test_rings = LoadRings(7, "_middle_key_ring_", "ulong");
            foreach(var ring in test_rings)
            {
                var set = new HashSet<ulong>();

                foreach(ulong key in ring)
                {
                    ulong[] key_array = new ulong[Faces.Count];
                    key_array[0] = key;

                    Cross a = key;
                    // move up to front
                    a.RotateDU();
                    a.RotateLR();
                    key_array[1] = a;   // up

                    a.RotateLR();
                    a.RotateFB();
                    key_array[2] = a;   // right

                    a.RotateDU();
                    a.RotateLR();
                    key_array[3] = a;   // back

                    a.RotateLR();
                    a.RotateFB();
                    key_array[4] = a;   // down

                    a.RotateDU();
                    a.RotateLR();
                    key_array[5] = a;   // left

                    foreach(ulong k in key_array)
                    {
                        Debug.Assert(ring.BinarySearch(k) >= 0);
                    }

                    ulong min_key = key_array.Min();
                    set.Add(min_key);
                    //Cross b = key;
                    //b.RotateLR();
                    //b.RotateFB();
                    //b.RotateFB();
                    //b.RotateFB();

                    //ulong key_a = a;
                    //ulong key_b = b;

                    //Debug.Assert(ring.BinarySearch(key_a) >= 0);
                    //Debug.Assert(key_b == key_a);
                }
                if(ring.Count > 6)
                {
                    Debug.Assert(set.Count < ring.Count);
                }
            }
            //*/
        }

        public static void SaveRing(IEnumerable<ulong> ring, string fname)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fname, FileMode.Create)))
            {
                foreach(ulong key in ring)
                {
                    bw.Write(key);
                }
            }
        }

        public static void SaveRing(IEnumerable<CubeKey> ring, string fname)
        {
            using (BinaryWriter bw = new BinaryWriter(File.Open(fname, FileMode.Create)))
            {
                foreach (CubeKey key in ring)
                {
                    bw.Write(key.corners);
                    bw.Write(key.middles);
                }
            }
        }

        public delegate T ElementWriter<T>(BinaryWriter bw);
        public delegate T ElementReader<T>(BinaryReader br);

        public static List<T> LoadList<T>(string fname, ElementReader<T> reader, int element_size)
        {
            List<T> list = null;
            using (BinaryReader br = new BinaryReader(File.Open(fname, FileMode.Open)))
            {
                long flength = (new FileInfo(fname)).Length;
                int count = (int)(flength / element_size);
                list = new List<T>(count);
                while (count-- > 0)
                {
                    list.Add(reader(br));
                }
            }
            return list;
        }

        public static List<List<ulong>> LoadRings(int capacity, string fname, string extension)
        {
            var rings = new List<List<ulong>>(capacity);

            for(int i = 0; i < capacity; i++)
            {
                var list = new List<ulong>();
                list.Load(fname + $"{i}." + extension, sizeof(ulong), br => br.ReadUInt64());
                rings.Add(list);
            }
            return rings;
        }

        public static List<List<CubeKey>> LoadKeyRings(int capacity, string fname, string extension)
        {
            List<List<CubeKey>> rings = new List<List<CubeKey>>(capacity);
            ElementReader<CubeKey> reader = (br) => { return new CubeKey { corners = br.ReadUInt64(), middles = br.ReadUInt64() }; };

            for (int i = 0; i < capacity; i++)
            {
                rings.Add(LoadList(fname + i + "." + extension, reader, sizeof(ulong) * 2));
            }
            return rings;
        }

        public static List<ulong> ToList(IEnumerable<ulong> ring)
        {
            List<ulong> list = new List<ulong>(ring.Count());
            foreach(ulong key in ring)
            {
                list.Add(key);
            }
            list.Sort();
            return list;
        }

        public static void PrecomputeMoves(int level)
        {
            Cube cube = new Cube();

            List<CubeKey> ring = new List<CubeKey>(1);
            ring.Add(new CubeKey(cube));

            List<List<CubeKey>> solution = new List<List<CubeKey>>(level);
            solution.Add(ring);

            int reserved = 18;
            for (int i = 0; i < level; i++)
            {
                List<CubeKey> next_ring = new List<CubeKey>(reserved);
                foreach (CubeKey key in ring)
                {
                    cube.CornerKey = key.corners;
                    cube.MiddleKey = key.middles;
                    foreach (var c in Faces.NextCubes(cube))
                    {
                        //Key next_key = new Key(cube);
                        CubeKey x = new CubeKey(cube);
                        cube.Shift2(); cube.Shift2();
                        CubeKey y = new CubeKey(cube);
                        cube.Shift2(); cube.Shift2();
                        CubeKey z = new CubeKey(cube);
                        cube.Shift2(); cube.Shift2();
                        CubeKey next_key = Utils.Min(Utils.Min(x, y), z);

                        if (solution.FindRow(next_key) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                reserved = next_ring.Count * 14;
                next_ring.DistinctValues();

                solution.Add(next_ring);
                ring = next_ring;
            }

            for (int i = 0; i < solution.Count; i++)
            {
                SaveRing(solution[i], "new_key_ring_" + i + ".ulong");
            }
        }

        public static List<List<CubeKey>> ComputeMoves(int level, CubeKey src_key)
        {
            List<CubeKey> ring = new List<CubeKey>(1);
            ring.Add(src_key);

            List<List<CubeKey>> solution = new List<List<CubeKey>>(level);
            solution.Add(ring);

            int reserved = 18;
            Cube cube = new Cube();
            for (int i = 0; i < level; i++)
            {
                List<CubeKey> next_ring = new List<CubeKey>(reserved);
                foreach (CubeKey key in ring)
                {
                    cube.Key = key;
                    foreach (var c in Faces.NextCubes(cube))
                    {
                        CubeKey next_key = cube.Key;

                        if (solution.FindRow(next_key) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                reserved = next_ring.Count * 14;
                next_ring.DistinctValues();

                solution.Add(next_ring);
                ring = next_ring;
            }

            return solution;
        }


        public static void Save<K>(List<List<K>> solution, Action<BinaryWriter, K> saver,
            string fname, string extension)
        {
            //string extension = typeof(K).Name;
            for (int i = 0; i < solution.Count; i++)
            {
                solution[i].Save(fname + $"{i}." + extension, saver);
            }
        }

        public static List<List<K>> PrecomputeMoves<K>(K start_key, int max_depth, int reserved, int growth_rate,
            Func<K, IEnumerable<K>> next)
        {
            List<K> src_ring = new List<K>(1);
            src_ring.Add(start_key);

            List<List<K>> solution = new List<List<K>>(max_depth);
            solution.Add(src_ring);

            while (solution.Count < max_depth)
            {
                List<K> next_ring = new List<K>(reserved);
                foreach (K src_key in src_ring)
                {
                    foreach (K dst_key in next(src_key))
                    {
                        if (solution.FindRow(dst_key) < 0)
                        {
                            next_ring.Add(dst_key);
                        }
                    }
                }

                // sort and remove duplicates
                reserved = next_ring.Count * growth_rate;
                next_ring.DistinctValues();

                solution.Add(next_ring);
                src_ring = next_ring;
            }
            return solution;
        }

        public static void PrecomputeMiddleMoves(int max_depth)
        {
            var solution = PrecomputeMoves(Cross.IDENTITY, max_depth, 18, 13, Cross.NextKeys);
            Save(solution, (b, k) => b.Write(k),
                "_middle_key_ring_", "ulong");
        }

        public static void PrecomputeCornerMoves(int max_depth)
        {
            var solution = PrecomputeMoves(Saltire.IDENTITY, max_depth, 18, 13, Saltire.NextKeys);
            Save(solution, (b, k) => b.Write(k),
                "_corner_key_ring_", "ulong");
        }

        public List<ulong> SolveCorners(ulong corner_key)
        {
            List<ulong> ring = new List<ulong>();
            ring.Add(corner_key);

            List<List<ulong>> l_rings = new List<List<ulong>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            int reserved = 18;
            while (true)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.CornerKey = key;
                    foreach (var c in Faces.NextCubes(cube))
                    {
                        ulong next_key = cube.CornerKey;
                        int pos = corner_rings.FindRow(next_key);
                        if (pos >= 0)
                        {
                            List<ulong> solution = new List<ulong>();
                            solution.Add(next_key);

                            for (int l = l_rings.Count; l-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextCornerKeys(solution.Last()))
                                {
                                    if (l_rings[l].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }

                            solution.Reverse();
                            for (int r = pos; r-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextCornerKeys(solution.Last()))
                                {
                                    if (corner_rings[r].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }
                            return solution;
                        }
                        else if (l_rings.FindRow(next_key) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }
                reserved = next_ring.Count * 13;
                next_ring.DistinctValues();
                l_rings.Add(next_ring);
                ring = next_ring;
            }
        }

        public MoveTrack SolveMiddle(ulong middle_key, int depth)
        {
            List<ulong> ring = new List<ulong>();
            ring.Add(middle_key);

            List<List<ulong>> l_rings = new List<List<ulong>>();
            l_rings.Add(ring);

            int solved_count = (new Cross(middle_key)).CountSolvedCubelets;
            MoveTrack solution = null;

            int reserved = 18;
            while (--depth > 0)
            {
                List<ulong> next_ring = null;
                if (depth > 1)
                {
                    next_ring = new List<ulong>(reserved);
                }

                foreach (ulong src_key in ring)
                {
                    foreach (ulong dst_key in Cross.NextKeys(src_key))
                    {
                        Cross dst_cube = dst_key;
                        int pos = middle_rings.FindRow(dst_key);
                        if (pos >= 0 || dst_cube.CountSolvedCubelets > solved_count)
                        {
                            solved_count = dst_cube.CountSolvedCubelets;
                            solution = new MoveTrack();
                            for (int l = l_rings.Count; l-- > 0;)
                            {
                                foreach (var pair in Faces.NextMoves(dst_cube))
                                {
                                    if (l_rings[l].BinarySearch(pair.Value) >= 0)
                                    {
                                        solution.Add(pair.Key);
                                        dst_cube = pair.Value;
                                        break;
                                    }
                                }
                            }

                            solution = solution.Reverse();
                            dst_cube = dst_key;
                            for (int r = pos; r-- > 0;)
                            {
                                foreach (var pair in Faces.NextMoves(dst_cube))
                                {
                                    if (middle_rings[r].BinarySearch(pair.Value) >= 0)
                                    {
                                        solution.Add(pair.Key);
                                        dst_cube = pair.Value;
                                        break;
                                    }
                                }
                            }
                            solution.Trim();

                            if (pos >= 0)
                            {
                                return solution;
                            }
                        }
                        else if (next_ring != null && l_rings.FindRow(dst_key) < 0)
                        {
                            next_ring.Add(dst_key);
                        }
                    }
                }

                if(next_ring != null)
                {
                    reserved = next_ring.Count * 13;
                    next_ring.DistinctValues();
                    l_rings.Add(next_ring);
                    ring = next_ring;
                }
            }
            return solution;
        }

        public List<CubeKey> Solve(CubeKey start_key, int max_depth)
        {
            List<CubeKey> ring = new List<CubeKey>();
            ring.Add(start_key);

            List<List<CubeKey>> l_rings = new List<List<CubeKey>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            ulong corner_key = cube.CornerKey;
            int middle_count = -1;

            int reserved = 18;
            const int max_reserved = 10000000;
            
            for(int depth = 0; depth < max_depth; depth++)
            {
                List<CubeKey> next_ring = null;
                if (depth + 1 < max_depth)
                {
                    next_ring = new List<CubeKey>(reserved);
                }
                
                foreach (CubeKey key in ring)
                {
                    cube.Key = key;

                    if(middle_count < 0)
                    {
                        middle_count = cube.CountSolvedMiddles;
                    }

                    foreach (var cc in Faces.NextCubes(cube))
                    {
                        CubeKey next_key = new CubeKey(cube);

                        int pos = cube_rings.FindRow(next_key);
                        if (pos >= 0 ||
                            (next_key.corners == corner_key && l_rings.FindRow(next_key) < 0 && middle_count < cube.CountSolvedMiddles))
                        {
                            List<CubeKey> solution = new List<CubeKey>();
                            solution.Add(next_key);

                            for (int l = l_rings.Count; l-- > 0;)
                            {
                                Cube c = new Cube(solution.Last());
                                foreach (uint prev_move in c.Moves())
                                {
                                    CubeKey prev_key = new CubeKey(c);
                                    if (l_rings[l].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }

                            solution.Reverse();
                            for (int r = pos; r-- > 0;)
                            {
                                Cube c = new Cube(solution.Last());
                                foreach (uint prev_move in c.Moves())
                                {
                                    CubeKey prev_key = new CubeKey(c);
                                    if (cube_rings[r].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }
                            return solution;
                        }
                        else if (next_ring != null && l_rings.FindRow(next_key) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                if (next_ring != null)
                {
                    reserved = Math.Min(next_ring.Count * 13, max_reserved);
                    next_ring.DistinctValues();
                    l_rings.Add(next_ring);
                    ring = next_ring;
                }
            }
            return null;
        }

        public MoveTrack Solve2(CubeKey start_key, int max_level)
        {
            List<CubeKey> ring = new List<CubeKey>();
            ring.Add(start_key);

            List<List<CubeKey>> l_rings = new List<List<CubeKey>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            int reserved = 18;
            while (true)
            {
                List<CubeKey> next_ring = new List<CubeKey>(reserved);
                foreach (CubeKey key in ring)
                {
                    cube.Key = key;

                    foreach (uint next_move in cube.Moves())
                    {
                        CubeKey next_key = cube.Key;

                        int pos = cube_rings.FindRow(next_key);
                        if (pos >= 0)
                        {
                            MoveTrack solution = new MoveTrack();
                            GetMoveTrack(next_key, solution, l_rings, l_rings.Count - 1, -1);
                            solution = solution.Reverse();
                            GetMoveTrack(next_key, solution, cube_rings, cube_rings.Count - 1, -1);

                            return solution;
                        }
                        else if (l_rings.FindRow(next_key) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                reserved = next_ring.Count * 13;
                next_ring.DistinctValues();
                l_rings.Add(next_ring);
                ring = next_ring;

                if (reserved > 10000000 || l_rings.Count + cube_rings.Count >= max_level)
                {
                    return null;
                }
            }
        }

        public void SaveShortSequences(string fname)
        {
            Algorithm alg = new Algorithm();
            ulong corner_key = (new Cube()).CornerKey;

            for(int i = 1; i < cube_rings.Count; i++)
            {
                foreach(CubeKey key in cube_rings[i])
                {
                    if(key.corners == corner_key)
                    {
                        MoveTrack track = new MoveTrack();

                        CubeKey src_key = key;
                        for(int j = i; j-- > 0;)
                        {
                            Cube cube = new Cube();
                            cube.MiddleKey = src_key.middles;
                            cube.CornerKey = src_key.corners;
                            foreach(uint move in cube.Moves())
                            {
                                CubeKey dst_key = new CubeKey(cube);
                                if (cube_rings[j].BinarySearch(dst_key) >= 0)
                                {
                                    src_key = dst_key;
                                    track.Add(new Move(move));

                                    break; // foreach move
                                }
                            }
                        }

                        alg.Add(track);
                        alg.Add(track.Reverse());
                    }
                }
            }

            alg.Save(fname);
        }

        public void SaveShortMiddleSequences(string fname)
        {
            Algorithm alg = new Algorithm();
            ulong middle_key = (new Cube()).MiddleKey;

            for (int i = 1; i < cube_rings.Count; i++)
            {
                foreach (CubeKey key in cube_rings[i])
                {
                    if (key.middles == middle_key)
                    {
                        MoveTrack track = new MoveTrack();

                        CubeKey src_key = key;
                        for (int j = i; j-- > 0;)
                        {
                            Cube cube = new Cube(src_key);
                            foreach (uint move in cube.Moves())
                            {
                                CubeKey dst_key = cube.Key;
                                if (cube_rings[j].BinarySearch(dst_key) >= 0)
                                {
                                    src_key = dst_key;
                                    track.Add(new Move(move));

                                    break; // foreach move
                                }
                            }
                        }

                        alg.Add(track);
                    }
                }
            }

            alg.Save(fname);
        }

        public void SaveNextSequences(string fname)
        {
            Algorithm alg = new Algorithm();

            Cube cube = new Cube();
            ulong corner_key = cube.CornerKey;
            
            foreach (CubeKey key in cube_rings.Last())
            {
                cube.CornerKey = key.corners;
                cube.MiddleKey = key.middles;

                foreach(uint index in cube.Moves())
                {
                    CubeKey src_key = new CubeKey(cube);
                    if (src_key.corners == corner_key && cube_rings.FindRow(src_key) < 0)
                    {
                        MoveTrack track = new MoveTrack();
                        GetMoveTrack(src_key, track, cube_rings, cube_rings.Count - 1, -1);
                        alg.Add(track);
                        alg.Add(track.Reverse());
                        break; // foreach move
                    }
                }
            }

            alg.Save(fname);
        }

        public void SaveLongSequences(string fname_src, string fname_dst)
        {
            Algorithm alg_src = new Algorithm();
            Algorithm alg_dst = new Algorithm();

            alg_src.Load(fname_src);

            Cube cube = new Cube();

            HashSet<CubeKey> keyset = new HashSet<CubeKey>();

            foreach (MoveTrack mta in alg_src.Tracks)
            {
                cube.PlayForward(mta);
                foreach(MoveTrack mtb in alg_src.Tracks)
                {
                    cube.PlayForward(mtb);

                    CubeKey key = new CubeKey(cube);

                    if (cube_rings.FindRow(key) < 0)
                    {
                        //Debug.Assert(!alg_src.tracks.ContainsKey(key));
                        keyset.Add(key);
                    }

                    cube.PlayBackward(mtb);
                }
                cube.PlayBackward(mta);
            }

            int count = keyset.Count;

            /*
            foreach(CubeKey key in keyset)
            {
                if((--count % 100) == 0)
                {
                    System.Console.Out.Write("         \r" + count.ToString());
                }

                if (!alg_dst.tracks.ContainsKey(key))
                {
                    MoveTrack mt = Solve2(key, 12);
                    if (mt != null)
                    {
                        if (mt.Count < 12)
                        {
                            alg_dst.Add(mt);

                            //if (alg_dst.tracks.Count > 2000)
                            //{
                            //    alg_dst.Save(fname_dst);
                            //    return;
                            //}
                        }
                    }
                }
            }
            */

            alg_dst.Save(fname_dst);
        }

        public static void GetMoveTrack(CubeKey key, MoveTrack track, List<List<CubeKey>> rings, int start_ring, int stop_ring)
        {
            Cube cube = new Cube();
            for (int i = start_ring; i != stop_ring; i += (i > stop_ring ? -1 : 1))
            {
                cube.MiddleKey = key.middles;
                cube.CornerKey = key.corners;
                foreach (uint move in cube.Moves())
                {
                    CubeKey dst_key = new CubeKey(cube);
                    if (rings[i].BinarySearch(dst_key) >= 0)
                    {
                        key = dst_key;
                        track.Add(new Move(move));
                        break; // foreach move
                    }
                }
            }
        }

        public MoveTrack Analyze2(MoveTrack path, int depth)
        {
            //for(int length = path.Count; length >= depth + cube_rings.Count; length--)
            for (int length = depth + cube_rings.Count; length <= path.Count; length++)
            {
                for (int i = 0; i <= path.Count - length; i++)
                //for (int i = path.Count - length; i >= 0; i--)
                {
                    MoveTrack test = path.SubTrack(i, length);

                    Cube cube = (new Cube()).PlayBackward(test);

                    MoveTrack result = Solve2(cube.Key, depth + cube_rings.Count);
                    if(result != null)
                    {
                        Debug.Assert(result.Count < test.Count);

                        MoveTrack path_a = path.SubTrack(0, i);
                        MoveTrack path_b = path.SubTrack(i + length, path.Count - i - length);

                        MoveTrack track = path_a + result + path_b;

                        Debug.Assert(track.Count < path.Count);
                        return track;
                    }
                }
            }
            return path;
        }

        public MoveTrack Analyze(MoveTrack path, int depth)
        {
            Cube cube = (new Cube()).PlayBackward(path);

            int count = path.Count;
            List<List<CubeKey>>[] rings = new List<List<CubeKey>>[count + 1];
            for (int i = 0; i < count; i++)
            {
                rings[i] = ComputeMoves(depth - 1, cube.Key);
                cube.MoveForward(path[i]);
            }
            rings[count] = cube_rings.GetRange(0, depth);

            for(int i = 0; i < count - depth; i++)
            {
                for(int j = count; --j > i + depth;)
                {
                    MoveTrack track = Intersect(rings[i], rings[j]);
                    if(track != null)
                    {
                        MoveTrack path_a = path.SubTrack(0, i);
                        MoveTrack path_b = path.SubTrack(j, path.Count - j);
                        MoveTrack result = path_a + track + path_b;
                        Debug.Assert(path.Count == path_a.Count + path_b.Count + j - i);

                        Cube cube_a = (new Cube()).PlayForward(path);
                        Cube cube_b = (new Cube()).PlayForward(result);

                        Debug.Assert(cube_a.Key == cube_b.Key);

                        return result;
                    }
                }
            }
            return path;
        }

        public static MoveTrack Intersect(List<List<CubeKey>> ring_a, List<List<CubeKey>> ring_b)
        {
            for(int i = 0; i < ring_a.Count; i++)
            {
                for(int j = 0; j < ring_b.Count; j++)
                {
                    CubeKey key;
                    if (ring_a[i].FirstIntersection(ring_b[j], out key))
                    {
                        MoveTrack track = new MoveTrack();
                        GetMoveTrack(key, track, ring_a, i - 1, -1);
                        track = track.Reverse();
                        GetMoveTrack(key, track, ring_b, j - 1, -1);

                        Cube cube_a = new Cube(ring_a[0][0]);
                        cube_a.PlayForward(track);
                        Debug.Assert(cube_a.Key == ring_b[0][0]);
                        
                        return track;
                    }
                }
            }
            return null;
        }
    }
}
