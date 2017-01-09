using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class Algorithm
    {
        public Dictionary<ulong, MoveTrack> tracks = new Dictionary<ulong, MoveTrack>();

        public bool PlayRandom(Cube cube, int length)
        {
            // int count = tracks.Values.Count(tr => tr.Count == length);
            // int count = tracks.Values.Where(tr => tr.Count == length).Count();
            List<MoveTrack> list = new List<MoveTrack>(from tr in tracks.Values where tr.Count == length select tr);

            if (list.Count > 0)
            {
                int i = new Random().Next(list.Count);
                list[i].PlayForward(cube);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<MoveTrack> Tracks
        {
            get
            {
                return tracks.Values;
            }
        }

        public static IEnumerable<KeyValuePair<ulong, MoveTrack>> AllTransforms(MoveTrack track)
        {
            foreach(MoveTrack dst_track in track.AllTransforms())
            {
                yield return new KeyValuePair<ulong, MoveTrack>(
                    dst_track.PlayForward(Cross.IDENTITY).Transform,
                    dst_track);
            }
        }

        public int Add(MoveTrack track)
        {
            int result = 0;
            foreach(KeyValuePair<ulong, MoveTrack> pair in AllTransforms(track))
            {
                if (tracks.ContainsKey(pair.Key))
                {
                    if(pair.Value.Count < tracks[pair.Key].Count)
                    {
                        tracks[pair.Key] = pair.Value;
                        result++;
                    }
                }
                else
                {
                    tracks.Add(pair.Key, pair.Value);
                    result++;

//                    if(pair.Key.corners != 731796345686735UL)
//                    {
//                        System.Console.Out.WriteLine(pair.Key.ToString());
//                        System.Console.Out.WriteLine(pair.Value.ToString());
//                    }
                }
            }
            return result;
        }

        public void Save(string fname)
        {
            using(System.IO.StreamWriter file = new System.IO.StreamWriter(fname))
            {
                HashSet<ulong> done = new HashSet<ulong>();
                foreach (KeyValuePair<ulong, MoveTrack> pair in tracks)
                {
                    if (!done.Contains(pair.Key))
                    {
                        done.Add(pair.Key);

                        Cross cross = new Cross(Cross.IDENTITY);
                        cross.Transform = pair.Key;

                        int m_count = 12 - cross.CountSolvedMiddles;
                        //int c_count = 12 - cube.CountSolvedCorners;

                        file.Write(m_count);
                        //file.Write('.');
                        //file.Write(c_count);
                        file.Write(';');
                        file.Write(pair.Value.ToString());
                        file.Write(';');
                        file.Write(pair.Value.Count);
                        file.Write(';');
                        file.Write(pair.Key);
                        file.Write('\n');

                        foreach(var child in AllTransforms(pair.Value))
                        {
                            done.Add(child.Key);
                        }
                    }
                }
            }
        }

        public void Load(string fname)
        {
            using(System.IO.StreamReader file = new System.IO.StreamReader(fname))
            {
                for(string buf; (buf = file.ReadLine()) != null;)
                {
                    string[] arr = buf.Split(';');
                    if(arr.Length > 1)
                    {
                        Add(new MoveTrack(arr[1]));
                    }
                }
            }
        }

        public void Solve(CubeKey key, MoveTrack current, ref MoveTrack result)
        {
            Cube cube = new Cube(key);
            int src_count = cube.CountSolvedMiddles;

            foreach (MoveTrack track in tracks.Values)
            {
                if(current.Count + track.Count < 40)
                {
                    if (result == null || result.Count > current.Count + track.Count)
                    {
                        cube.Key = key;
                        track.PlayForward(cube);

                        int dst_count = cube.CountSolvedMiddles;
                        if (dst_count == 12)
                        {
                            result = current + track;
                        }
                        else if (dst_count > src_count)
                        {
                            Solve(new CubeKey(cube), current + track, ref result);
                        }
                    }
                }
            }
        }

        public MoveTrack RunOnce(Cross cross)
        {
            MoveTrack dst_track = null;
            int solved_middles = cross.CountSolvedMiddles;

            foreach (var pair in tracks)
            {
                Cross dst_cross = new Cross(cross);
                dst_cross.Transform = pair.Key; // pair.Value.PlayForward(cross);

                int i = dst_cross.CountSolvedMiddles;
                if(i > solved_middles)
                {
                    solved_middles = i;
                    dst_track = pair.Value;
                }
                else if(i == solved_middles && dst_track != null)
                {
                    if(pair.Value.Count < dst_track.Count)
                    {
                        dst_track = pair.Value;
                    }
                }
            }

            return dst_track;
        }


        public void RunFirst(ulong cross, Dictionary<ulong, CrossEntry> done)
        {
            foreach (var pair in tracks)
            {
                Cross dst_cross = new Cross(cross);
                dst_cross.Transform = pair.Key;

                ulong dst_key = dst_cross;
                MoveTrack dst_path = pair.Value;

                if (!done.ContainsKey(dst_key))
                {
                    done[dst_key] = new CrossEntry(dst_key, dst_cross.CountSolvedMiddles, dst_path);
                }
                else if (done[dst_key].path.Count > dst_path.Count)
                {
                    done[dst_key].path = dst_path;
                }
            }
        }

        public void RunNext(ulong cross, MoveTrack path, Dictionary<ulong, CrossEntry> done, int threshold)
        {
            foreach (var pair in tracks)
            {
                if (pair.Value.Count + path.Count >= threshold)
                {
                    continue;
                }

                Cross dst_cross = new Cross(cross);
                dst_cross.Transform = pair.Key;

                ulong dst_key = dst_cross;
                MoveTrack dst_path = path + pair.Value;

                if (!done.ContainsKey(dst_key))
                {
                    done[dst_key] = new CrossEntry(dst_key, dst_cross.CountSolvedMiddles, dst_path);
                }
                else if (done[dst_key].path.Count > dst_path.Count)
                {
                    done[dst_key].path = dst_path;
                    done[dst_key].handled = false;
                }
            }
        }

        public MoveTrack Run(Cube cube)
        {
            MoveTrack track = null;
            int threshold = 100;

            List<Entry> result = new List<Entry>();
            result.Add(new Entry(new CubeKey(cube), cube.CountSolvedMiddles, new MoveTrack()));

            for (int try_count = 0; result.Count > 0 && try_count < 100; try_count++)
            {
                List<Entry> next_result = new List<Entry>();

                foreach(Entry entry in result)
                {
                    Test(entry, next_result, threshold);
                }

                next_result.Sort();
                int i = next_result.Count;
                while (i-- > 0 && next_result[i].solved_middles == 12)
                {
                    Entry entry = next_result[i];
                    if(track == null || track.Count > next_result[i].path.Count)
                    {
                        track = next_result[i].path;
                        threshold = track.Count;
                    }
                }
                next_result.RemoveRange(i + 1, next_result.Count - i - 1);

                next_result.Reverse();
                if(next_result.Count > 50)
                {
                    int pos = 15;
                    int solved_middles = Math.Max(next_result[pos].solved_middles, 1);
                    while (++pos < next_result.Count && solved_middles == next_result[pos].solved_middles) ;

                    next_result.RemoveRange(pos, next_result.Count - pos);
                }

                result = next_result;
            }

            return track;
        }

        public void Test(Entry entry, List<Entry> result, int threshold)
        {
            Cube cube = new Cube();
            threshold -= entry.path.Count;

            foreach (MoveTrack track in tracks.Values)
            {
                if(track.Count < threshold)
                {
                    cube.Key = entry.dst_key;

                    track.PlayForward(cube);
                    result.Add(new Entry(new CubeKey(cube), cube.CountSolvedMiddles, entry.path + track));
                }
            }
        }

        public class Entry : IComparable<Entry>
        {
            public CubeKey dst_key;
            public int solved_middles;
            public MoveTrack path;

            public Entry(CubeKey dst_key, int solved_middles, MoveTrack path)
            {
                this.dst_key = dst_key;
                this.solved_middles = solved_middles;
                this.path = path;
            }

            public int CompareTo(Entry other)
            {
                int result = solved_middles - other.solved_middles;
                if(result == 0)
                {
                    result = other.path.Count - path.Count;
                    if(result == 0)
                    {
                        result = dst_key.CompareTo(other.dst_key);
                    }
                }
                return result;
            }
        }

        public class CrossEntry : IComparable<CrossEntry>
        {
            public ulong dst_key;
            public int solved_middles;
            public MoveTrack path;
            public bool handled;

            public CrossEntry(ulong dst_key, int solved_middles, MoveTrack path)
            {
                this.dst_key = dst_key;
                this.solved_middles = solved_middles;
                this.path = path;
                this.handled = false;
            }

            public int CompareTo(CrossEntry other)
            {
                int result = solved_middles - other.solved_middles;
                if (result == 0)
                {
                    result = other.path.Count - path.Count;
                    if (result == 0)
                    {
                        result = dst_key.CompareTo(other.dst_key);
                    }
                }
                return result;
            }
        }
    }
}
