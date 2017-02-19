using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    // Collection of move tracks which can change Saltire only and should leave Cross intact.
    public class SaltireAlgorithms : GeneralAlgorithms<ulong>
    {
        // Base class abstract function implementation:
        public override ulong ToKey(MoveTrack track)
        {
            Saltire salti = Saltire.IDENTITY;
            return salti.PlayForward(track).Transform;
        }

        public override bool CanAdd(MoveTrack track)
        {
            Cross cross = Cross.IDENTITY;
            if (Cross.IDENTITY != cross.PlayForward(track))
            {
                Console.WriteLine($"Warning: invalid cross track: {track}");
                return false;
            }
            return true;
        }

        public override int CountChangedElements(MoveTrack track)
        {
            Saltire salti = Saltire.IDENTITY;
            return (int)(Saltire.CUBELET_NUM - salti.PlayForward(track).CountSolvedCubelets);
        }

        public override void SaveKey(StreamWriter file, ulong key)
        {
            file.Write(key);
        }

        public IEnumerable<KeyValuePair<Saltire, MoveTrack>> Run(Saltire cube) =>
            from path in Tracks.Values select new KeyValuePair<Saltire, MoveTrack>(cube.PlayForward(path), path);

        public MoveTrack RunOnce(Saltire cube)
        {
            MoveTrack dst_track = null;
            int solved_count = cube.CountSolvedCubelets;

            var tracks = Tracks;
            foreach (var pair in tracks)
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

        public void FirstIteration(Saltire src, Dictionary<ulong, SearchEntry> done, SearchEntry dst_entry)
        {
            int count = src.CountSolvedCubelets;
            var tracks = Tracks;
            foreach (var pair in tracks)
            {
                Saltire dst = new Saltire(src);
                dst.Transform = pair.Key;

                //if (dst.CountSolvedCubelets > count)
                {
                    MoveTrack dst_path = pair.Value;
                    ulong dst_key = dst;

                    if (dst_entry._dst_key == dst_key)
                    {
                        if (dst_entry._path == null || dst_entry._path.Count > dst_path.Count)
                        {
                            dst_entry._path = dst_path;
                            Console.WriteLine($"\n found {dst_path.Count}:{dst_path}");
                        }
                    }
                    else if (!done.ContainsKey(dst_key))
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

        public void NextIteration(Saltire src, MoveTrack path, Dictionary<ulong, SearchEntry> done, SearchEntry dst_entry)
        {
            int threshold = dst_entry._path == null ? 35 : dst_entry._path.Count;

            int count = src.CountSolvedCubelets;
            var tracks = Tracks;
            foreach (var pair in tracks)
            {
                MoveTrack dst_path = path + pair.Value;
                if (dst_path.Count >= threshold)
                {
                    continue;
                }

                Saltire dst = src;
                dst.Transform = pair.Key;

                if (dst.CountSolvedCubelets > count)
                {
                    ulong dst_key = dst;

                    if (dst_entry._dst_key == dst_key)
                    {
                        if (dst_entry._path == null || dst_entry._path.Count > dst_path.Count)
                        {
                            dst_entry._path = dst_path;
                            threshold = dst_path.Count;
                            Console.WriteLine($"\n found {dst_path.Count}:{dst_path}");
                        }
                    }
                    else if (!done.ContainsKey(dst_key))
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
            var dst_entry = new SearchEntry(Saltire.IDENTITY, null);
            //ulong win_key = Saltire.IDENTITY;

            //MoveTrack path = null;
            var done = new Dictionary<ulong, SearchEntry>();

            FirstIteration(src, done, dst_entry);

            //int threshold = 35;
            for (int try_count = 0; try_count < 12; try_count++)
            {
                /*//
                if (done.ContainsKey(win_key) && !done[win_key]._handled)
                {
                    path = done[win_key]._path;
                    done[win_key]._handled = true;
                    threshold = path.Count;

                    Console.WriteLine($"\n found {path.Count}:{path}");
                }
                

                if (try_count > 12)
                {
                    break;
                }
                //*/
                // make list
                // get top ten
                // test them

                int threshold = dst_entry._path == null ? 35 : dst_entry._path.Count - 1;
                var list = new List<SearchEntry>(
                    from entry in done.Values
                    where entry._path.Count < threshold && !entry._handled
                    select entry);

                /*//
                var numbers = new Dictionary<int, int>();
                int max_solved = 0;
                foreach(var entry in list)
                {
                    int key = entry._solved_middles;

                    if (numbers.ContainsKey(key))
                    {
                        numbers[key] += 1;
                    }
                    else
                    {
                        numbers[key] = 1;
                    }
                    
                    if(max_solved < key)
                    {
                        max_solved = key;
                    }
                }

                Console.WriteLine($"\nRound {try_count}:");
                for(int i = max_solved; i >= 0; i--)
                {
                    if (numbers.ContainsKey(i))
                    {
                        Console.WriteLine($"{i}:{numbers[i]}");
                    }
                }
                //*/

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

                    if (i > min_breadth * 2 && dst_entry._path != null)
                    {
                        i = min_breadth * 2;
                    }

                    if (i > min_breadth * 5)
                    {
                        i = min_breadth * 5;
                    }

                    if (list.Count > i)
                    {
                        list.RemoveRange(i, list.Count - i);
                    }
                }

                //Console.WriteLine($"\tlist size: {list.Count}");
                foreach (var entry in list)
                {
                    entry._handled = true;
                    NextIteration(entry._dst_key, entry._path, done, dst_entry);
                }

                if(dst_entry._path != null && try_count < 9)
                {
                    try_count = 9;
                }
            }


            return dst_entry._path;
        }
    }
}
