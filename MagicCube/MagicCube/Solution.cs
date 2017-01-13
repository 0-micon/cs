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
            middle_rings = LoadRings(7, "middle_key_ring_", "ulong");
            corner_rings = LoadRings(7, "corner_key_ring_", "ulong");
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
            List<List<ulong>> rings = new List<List<ulong>>(capacity);
            ElementReader<ulong> reader = (br) => { return br.ReadUInt64(); };

            for(int i = 0; i < capacity; i++)
            {
                rings.Add(LoadList(fname + i + "." + extension, reader, sizeof(ulong)));
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
                    foreach (uint next_move in cube.Moves())
                    {
                        //Key next_key = new Key(cube);
                        CubeKey x = new CubeKey(cube);
                        cube.Shift2(); cube.Shift2();
                        CubeKey y = new CubeKey(cube);
                        cube.Shift2(); cube.Shift2();
                        CubeKey z = new CubeKey(cube);
                        cube.Shift2(); cube.Shift2();
                        CubeKey next_key = Utils.Min(Utils.Min(x, y), z);

                        if (TestKey(next_key, solution) < 0)
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
                    foreach (uint next_move in cube.Moves())
                    {
                        CubeKey next_key = cube.Key;

                        if (TestKey(next_key, solution) < 0)
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

        public static void PrecomputeMiddleMoves(int level)
        {
            Cube cube = new Cube();

            List<ulong> ring = new List<ulong>(1);
            ring.Add(cube.MiddleKey);

            List<List<ulong>> solution = new List<List<ulong>>(level);
            solution.Add(ring);

            int reserved = 18;
            for (int i = 0; i < level; i++)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.MiddleKey = key;
                    foreach (ulong next_key in cube.NextMiddleKeys())
                    {
                        if (TestKey(next_key, solution) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                //next_ring.Sort();
                //next_ring = next_ring.Distinct().ToList();
                reserved = next_ring.Count * 13;
                next_ring.DistinctValues();

                solution.Add(next_ring);
                ring = next_ring;
            }

            for(int i = 0; i < solution.Count; i++)
            {
                SaveRing(solution[i], "middle_key_ring_" + i + ".ulong");
            }
        }

        public static void PrecomputeCornerMoves(int level)
        {
            Cube cube = new Cube();

            List<ulong> ring = new List<ulong>(1);
            ring.Add(cube.CornerKey);

            List<List<ulong>> solution = new List<List<ulong>>(level);
            solution.Add(ring);

            int reserved = 18;
            for (int i = 0; i < level; i++)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.CornerKey = key;
                    foreach (ulong next_key in cube.NextCornerKeys())
                    {
                        if (TestKey(next_key, solution) < 0)
                        {
                            next_ring.Add(next_key);
                        }
                    }
                }

                // sort and remove duplicates
                reserved = next_ring.Count * 13;
                next_ring.DistinctValues();

                solution.Add(next_ring);
                ring = next_ring;
            }

            for (int i = 0; i < solution.Count; i++)
            {
                SaveRing(solution[i], "corner_key_ring_" + i + ".ulong");
            }
        }

        public static int TestKey(ulong key, List<HashSet<ulong>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && !solution[i].Contains(key));
            return i;
        }

        public static int TestKey<T>(T key, List<List<T>> solution)
        {
            int i = solution.Count;
            while (i-- > 0 && solution[i].BinarySearch(key) < 0);
            return i;
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
                    foreach (uint move in cube.Moves())
                    {
                        ulong next_key = cube.CornerKey;
                        int pos = TestKey(next_key, corner_rings);
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
                        else if (TestKey(next_key, l_rings) < 0)
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

        public List<ulong> SolveMiddle(ulong middle_key)
        {
            List<ulong> ring = new List<ulong>();
            ring.Add(middle_key);

            List<List<ulong>> l_rings = new List<List<ulong>>();
            l_rings.Add(ring);

            Cube cube = new Cube();

            int reserved = 18;
            while (true)
            {
                List<ulong> next_ring = new List<ulong>(reserved);
                foreach (ulong key in ring)
                {
                    cube.MiddleKey = key;
                    foreach (ulong next_key in cube.NextMiddleKeys())
                    {
                        int pos = TestKey(next_key, middle_rings);
                        if (pos >= 0)
                        {
                            List<ulong> solution = new List<ulong>();
                            solution.Add(next_key);

                            for(int l = l_rings.Count; l-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextMiddleKeys(solution.Last()))
                                {
                                    if (l_rings[l].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }

                            solution.Reverse();
                            for(int r = pos; r-- > 0;)
                            {
                                foreach (ulong prev_key in Cube.NextMiddleKeys(solution.Last()))
                                {
                                    if (middle_rings[r].BinarySearch(prev_key) >= 0)
                                    {
                                        solution.Add(prev_key);
                                        break;
                                    }
                                }
                            }
                            return solution;
                        }
                        else if (TestKey(next_key, l_rings) < 0)
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

        public List<CubeKey> Solve(CubeKey start_key)
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
            while (true)
            {
                List<CubeKey> next_ring = new List<CubeKey>(reserved);
                foreach (CubeKey key in ring)
                {
                    cube.MiddleKey = key.middles;
                    cube.CornerKey = key.corners;

                    if(middle_count < 0)
                    {
                        middle_count = cube.CountSolvedMiddles;
                    }

                    foreach (uint next_move in cube.Moves())
                    {
                        CubeKey next_key = new CubeKey(cube);

                        int pos = TestKey(next_key, cube_rings);
                        if (pos >= 0 ||
                            (next_key.corners == corner_key && TestKey(next_key, l_rings) < 0 && middle_count < cube.CountSolvedMiddles))
                        {
                            List<CubeKey> solution = new List<CubeKey>();
                            solution.Add(next_key);

                            for (int l = l_rings.Count; l-- > 0;)
                            {
                                Cube c = new Cube();
                                c.MiddleKey = solution.Last().middles;
                                c.CornerKey = solution.Last().corners;
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
                                Cube c = new Cube();
                                c.MiddleKey = solution.Last().middles;
                                c.CornerKey = solution.Last().corners;
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
                        else if (TestKey(next_key, l_rings) < 0)
                        {
                            next_ring.Add(next_key);
                            if(next_ring.Count >= max_reserved)
                            {
                                goto NextStep;
                            }
                        }
                    }
                }

            NextStep:
                reserved = Math.Min(next_ring.Count * 13, max_reserved);
                next_ring.DistinctValues();
                l_rings.Add(next_ring);
                ring = next_ring;

                if(reserved > 10000000 || l_rings.Count > 100)
                {
                    return null;
                }
            }
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
                    cube.MiddleKey = key.middles;
                    cube.CornerKey = key.corners;

                    foreach (uint next_move in cube.Moves())
                    {
                        CubeKey next_key = new CubeKey(cube);

                        int pos = TestKey(next_key, cube_rings);
                        if (pos >= 0)
                        {
                            MoveTrack solution = new MoveTrack();
                            GetMoveTrack(next_key, solution, l_rings, l_rings.Count - 1, -1);
                            solution = solution.Reverse();
                            GetMoveTrack(next_key, solution, cube_rings, cube_rings.Count - 1, -1);

                            return solution;
                        }
                        else if (TestKey(next_key, l_rings) < 0)
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
                    if (src_key.corners == corner_key && TestKey(src_key, cube_rings) < 0)
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
                mta.PlayForward(cube);
                foreach(MoveTrack mtb in alg_src.Tracks)
                {
                    mtb.PlayForward(cube);

                    CubeKey key = new CubeKey(cube);

                    if (TestKey(key, cube_rings) < 0)
                    {
                        //Debug.Assert(!alg_src.tracks.ContainsKey(key));
                        keyset.Add(key);
                    }

                    mtb.PlayBackward(cube);
                }
                mta.PlayBackward(cube);
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

        public HashSet<ulong> GetMoveKeys(ulong middle_key)
        {
            HashSet<ulong> ring = new HashSet<ulong>();
            Cube cube = new Cube();
            cube.MiddleKey = middle_key;
            foreach (ulong next_key in cube.NextMiddleKeys())
            {
                ring.Add(next_key);
            }

            return ring;
        }

        public MoveTrack Analyze2(MoveTrack path, int depth)
        {
            for(int length = path.Count; length >= depth + cube_rings.Count; length--)
            {
                for (int i = 0; i <= path.Count - length; i++)
                {
                    MoveTrack test = path.SubTrack(i, length);

                    Cube cube = new Cube();
                    test.PlayBackward(cube);

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
            Cube cube = new Cube();
            path.PlayBackward(cube);

            int count = path.Count;
            List<List<CubeKey>>[] rings = new List<List<CubeKey>>[count + 1];
            for (int i = 0; i < count; i++)
            {
                rings[i] = ComputeMoves(depth - 1, cube.Key);
                path.PlayForward(cube, i);
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

                        Cube cube_a = new Cube();
                        Cube cube_b = new Cube();
                        path.PlayForward(cube_a);
                        result.PlayForward(cube_b);
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
                        track.PlayForward(cube_a);
                        Debug.Assert(cube_a.Key == ring_b[0][0]);
                        
                        return track;
                    }
                }
            }
            return null;
        }
    }
}
