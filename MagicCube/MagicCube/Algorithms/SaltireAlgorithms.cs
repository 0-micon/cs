using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    // Collection of move tracks which can change Saltire only and should leave Cross intact.
    class SaltireAlgorithms
    {
        Dictionary<ulong, MoveTrack> _tracks = new Dictionary<ulong, MoveTrack>();

        public Dictionary<ulong, MoveTrack> Tracks => _tracks;

        public static IEnumerable<KeyValuePair<ulong, MoveTrack>> AllTransforms(MoveTrack src_track)
        {
            foreach (MoveTrack dst_track in src_track.AllTransforms())
            {
                Saltire dst = Saltire.IDENTITY;
                dst = dst.PlayForward(dst_track);

                yield return new KeyValuePair<ulong, MoveTrack>(dst.Transform, dst_track);
            }
        }

        public int Add(MoveTrack track)
        {
            int result = 0;
            foreach (KeyValuePair<ulong, MoveTrack> pair in AllTransforms(track))
            {
                if (_tracks.ContainsKey(pair.Key))
                {
                    if (pair.Value.Count < _tracks[pair.Key].Count)
                    {
                        Console.WriteLine($"{pair.Key}: {_tracks[pair.Key]} => {pair.Value}");
                        _tracks[pair.Key] = pair.Value;
                        result++;
                    }
                }
                else
                {
                    _tracks.Add(pair.Key, pair.Value);
                    result++;

                    Cross cross = Cross.IDENTITY;
                    cross = cross.PlayForward(pair.Value);

                    if (Cross.IDENTITY != cross)
                    {
                        Console.WriteLine($"Warning: invalid saltire track: {pair.Value}");
                    }
                }
            }
            return result;
        }

        public IEnumerable<KeyValuePair<Saltire, MoveTrack>> Run(Saltire cube) =>
            from path in _tracks.Values select new KeyValuePair<Saltire, MoveTrack>(cube.PlayForward(path), path);

        public MoveTrack RunOnce(Saltire cube)
        {
            MoveTrack dst_track = null;
            int solved_count = cube.CountSolvedCubelets;

            foreach (var pair in _tracks)
            {
                Saltire dst = new Saltire(cube);
                dst.Transform = pair.Key; // pair.Value.PlayForward(cube);

                int i = dst.CountSolvedCubelets;
                if (i > solved_count)
                {
                    solved_count = i;
                    dst_track = pair.Value;
                }
                else if (i == solved_count && dst_track != null)
                {
                    if (pair.Value.Count < dst_track.Count)
                    {
                        dst_track = pair.Value;
                    }
                }
            }

            return dst_track;
        }

        public void Save(string fname)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fname))
            {
                HashSet<ulong> done = new HashSet<ulong>();
                foreach (KeyValuePair<ulong, MoveTrack> pair in _tracks)
                {
                    if (!done.Contains(pair.Key))
                    {
                        done.Add(pair.Key);

                        Saltire cross = Saltire.IDENTITY;
                        cross.Transform = pair.Key;

                        int count = (int)Saltire.CUBELET_NUM - cross.CountSolvedCubelets;

                        file.Write(pair.Value.Count);
                        file.Write(';');
                        file.Write(pair.Value.Track);
                        file.Write(';');
                        file.Write(count);
                        file.Write(';');
                        file.Write(pair.Key);
                        file.Write('\n');

                        foreach (var child in AllTransforms(pair.Value))
                        {
                            done.Add(child.Key);
                        }
                    }
                }
            }
        }

        public void Load(string fname)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(fname))
            {
                for (string buf; (buf = file.ReadLine()) != null;)
                {
                    string[] arr = buf.Split(';');
                    if (arr.Length > 1)
                    {
                        Add(new MoveTrack(arr[1], false));
                    }
                }
            }
        }

        public class SearchEntry : IComparable<SearchEntry>
        {
            public ulong _dst_key;
            public int  _solved_middles;
            public MoveTrack _path;
            public bool _handled;

            public SearchEntry(Saltire dst, MoveTrack path)
            {
                _dst_key = dst;
                _solved_middles = dst.CountSolvedCubelets;
                _path = path;
                _handled = false;
            }

            public int CompareTo(SearchEntry other)
            {
                int result = _solved_middles - other._solved_middles;
                if (result == 0)
                {
                    result = other._path.Count - _path.Count;
                    if (result == 0)
                    {
                        result = _dst_key.CompareTo(other._dst_key);
                    }
                }
                return result;
            }
        }

        public void FirstIteration(Saltire src, Dictionary<ulong, SearchEntry> done)
        {
            int count = src.CountSolvedCubelets;
            foreach (var pair in _tracks)
            {
                Saltire dst = new Saltire(src);
                dst.Transform = pair.Key;

                if (dst.CountSolvedCubelets > count)
                {
                    MoveTrack dst_path = pair.Value;
                    ulong dst_key = dst;

                    if (!done.ContainsKey(dst_key))
                    {
                        done[dst_key] = new SearchEntry(dst, dst_path);
                    }
                    else if (done[dst_key]._path.Count > dst_path.Count)
                    {
                        done[dst_key]._path = dst_path;
                    }
                }

            }
        }

        public void NextIteration(Saltire src, MoveTrack path, Dictionary<ulong, SearchEntry> done, int threshold)
        {
            int count = src.CountSolvedCubelets;

            foreach (var pair in _tracks)
            {
                if (pair.Value.Count + path.Count >= threshold)
                {
                    continue;
                }

                Saltire dst = src;
                dst.Transform = pair.Key;

                if (dst.CountSolvedCubelets > count)
                {
                    ulong dst_key = dst;
                    MoveTrack dst_path = path + pair.Value;

                    if (!done.ContainsKey(dst_key))
                    {
                        done[dst_key] = new SearchEntry(dst, dst_path);
                    }
                    else if (done[dst_key]._path.Count > dst_path.Count)
                    {
                        done[dst_key]._path = dst_path;
                        done[dst_key]._handled = false;
                    }
                }
            }
        }

        public MoveTrack Solve(Saltire src, int min_breadth)
        {
            ulong win_key = Saltire.IDENTITY;

            MoveTrack path = null;
            var done = new Dictionary<ulong, SearchEntry>();

            FirstIteration(src, done);

            int threshold = 10000;
            for (int try_count = 0; ; try_count++)
            {
                if (done.ContainsKey(win_key) && !done[win_key]._handled)
                {
                    path = done[win_key]._path;
                    done[win_key]._handled = true;
                    threshold = path.Count;
                }

                if (try_count > 12)
                {
                    break;
                }
                // make list
                // get top ten
                // test them

                var list = new List<SearchEntry>(
                    from entry in done.Values
                    where entry._path.Count < threshold - 6 && !entry._handled
                    select entry);

                list.Sort();
                list.Reverse();
                if (list.Count > min_breadth)
                {
                    int i = min_breadth;
                    int solved = list[i]._solved_middles;
                    while(++i < list.Count)
                    {
                        if(list[i]._solved_middles != solved)
                        {
                            break;
                        }
                    }

                    if(i > min_breadth * 3 && path != null)
                    {
                        i = min_breadth * 3;
                    }

                    if (list.Count > i)
                    {
                        list.RemoveRange(i, list.Count - i);
                    }
                }

                foreach (var entry in list)
                {
                    entry._handled = true;
                    NextIteration(entry._dst_key, entry._path, done, threshold);
                }
            }


            return path;
        }
    }
}
