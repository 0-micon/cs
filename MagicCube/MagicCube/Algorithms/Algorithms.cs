using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class Algorithms<TKey, TCube>
        where TKey : IComparable<TKey>
        where TCube : Faces.IRotatable, Faces.IConvertible<TKey>, new()
    {
        Dictionary<TKey, MoveTrack> _tracks = new Dictionary<TKey, MoveTrack>();

        public Dictionary<TKey, MoveTrack> Tracks => _tracks;

        public static IEnumerable<KeyValuePair<TKey, MoveTrack>> AllTransforms(MoveTrack src)
        {
            foreach (MoveTrack dst in src.AllTransforms())
            {
                yield return new KeyValuePair<TKey, MoveTrack>((new TCube()).PlayForward(dst).Key, dst);
            }
        }

        public int Add(MoveTrack track)
        {
            int result = 0;
            foreach (var pair in AllTransforms(track))
            {
                if (_tracks.ContainsKey(pair.Key))
                {
                    if (pair.Value.Count < _tracks[pair.Key].Count)
                    {
                        _tracks[pair.Key] = pair.Value;
                        result++;
                    }
                }
                else
                {
                    _tracks.Add(pair.Key, pair.Value);
                    result++;
                }
            }
            return result;
        }

        public void Save(string fname)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fname))
            {
                var done = new HashSet<TKey>();

                foreach (var pair in _tracks)
                {
                    if (!done.Contains(pair.Key))
                    {
                        done.Add(pair.Key);

                        file.Write(pair.Value.Count);
                        file.Write(';');
                        file.Write(pair.Value.Track);
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
            public TKey _dst_key;
            public int _solved_count;
            public MoveTrack _path;
            public bool _handled;

            public SearchEntry(TKey key, MoveTrack path, int solved_count)
            {
                _dst_key = key;
                _solved_count = solved_count;
                _path = path;
                _handled = false;
            }

            public int CompareTo(SearchEntry other)
            {
                int result = _solved_count - other._solved_count;
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

        public void FirstIteration(TKey src_key, Dictionary<TKey, SearchEntry> done, Func<TCube, int> solved_counter)
        {
            TCube dst = new TCube();

            foreach (var pair in _tracks)
            {
                dst.Key = src_key;
                dst = dst.PlayForward(pair.Value);

                MoveTrack dst_path = pair.Value;
                TKey dst_key = dst.Key;

                if (!done.ContainsKey(dst_key))
                {
                    done[dst_key] = new SearchEntry(dst_key, dst_path, solved_counter(dst));
                }
                else if (done[dst_key]._path.Count > dst_path.Count)
                {
                    done[dst_key]._path = dst_path;
                }
            }
        }

        public void NextIteration(TKey src_key, MoveTrack path, int threshold, Dictionary<TKey, SearchEntry> done, Func<TCube, int> solved_counter)
        {
            TCube dst = new TCube();

            foreach (var pair in _tracks)
            {
                if (pair.Value.Count + path.Count >= threshold)
                {
                    continue;
                }
                                        
                dst.Key = src_key;
                dst = dst.PlayForward(pair.Value);

                TKey dst_key = dst.Key;
                MoveTrack dst_path = path + pair.Value;

                if (!done.ContainsKey(dst_key))
                {
                    done[dst_key] = new SearchEntry(dst_key, dst_path, solved_counter(dst));
                }
                else if (done[dst_key]._path.Count > dst_path.Count)
                {
                    done[dst_key]._path = dst_path;
                    done[dst_key]._handled = false;
                }
            }
        }

        public MoveTrack Solve(TKey src_key, TKey win_key, int breadth, Func<TCube, int> solved_counter)
        {
            MoveTrack path = null;
            var done = new Dictionary<TKey, SearchEntry>();

            FirstIteration(src_key, done, solved_counter);

            int threshold = 10000;
            for (int try_count = 0; ; try_count++)
            {
                if (done.ContainsKey(win_key) && !done[win_key]._handled)
                {
                    path = done[win_key]._path;
                    done[win_key]._handled = true;
                    threshold = path.Count;
                }

                if (try_count > 10)
                {
                    break;
                }
                // make list
                // get top ten
                // test them

                var list = new List<SearchEntry>(
                    from entry in done.Values
                    where entry._path.Count < threshold && !entry._handled
                    select entry);

                list.Sort();
                list.Reverse();
                if (list.Count > breadth)
                {
                    list.RemoveRange(breadth, list.Count - breadth);
                }

                foreach (var entry in list)
                {
                    entry._handled = true;
                    NextIteration(entry._dst_key, entry._path, threshold, done, solved_counter);
                }
            }

            return path;
        }
    }
}
