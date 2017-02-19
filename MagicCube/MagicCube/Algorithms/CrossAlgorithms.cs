using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class CrossAlgorithms : GeneralAlgorithms<ulong>
    {
        public override bool CanAdd(MoveTrack track)
        {
            Saltire saltire = Saltire.IDENTITY;
            if (Saltire.IDENTITY != saltire.PlayForward(track))
            {
                Console.WriteLine($"Warning: invalid cross track: {track}");
                return false;
            }
            return true;
        }

        public override int CountChangedElements(MoveTrack track)
        {
            Cross cross = Cross.IDENTITY;
            return (int)(Cross.CUBELET_NUM - cross.PlayForward(track).CountSolvedCubelets);
        }

        public override void SaveKey(StreamWriter file, ulong key)
        {
            file.Write(key);
        }

        public override ulong ToKey(MoveTrack track)
        {
            Cross cross = Cross.IDENTITY;
            return cross.PlayForward(track).Transform;
        }

        public void AddShortAlgorithms(CubeGeneralSolution<FastCube> cube_solution)
        {
            for (int i = 1; i < cube_solution.Count; i++)
            {
                foreach(var cube in cube_solution[i])
                {
                    if (cube.Corners == Saltire.IDENTITY)
                    {
                        Add(cube_solution.PathTo(cube, i));
                    }
                }
            }

            var last = cube_solution.Last();
            foreach(var cube in last)
            {
                foreach(var next in Faces.NextCubes(cube))
                {
                    if(next.Corners == Saltire.IDENTITY && cube_solution.FindRow(next) < 0)
                    {
                        Add(cube_solution.PathTo(next, cube_solution.Count));
                    }
                }
            }
        }

        public void AddLongAlgorithms(CubeGeneralSolution<FastCube> cube_solution, int i)
        {
            var last = cube_solution.Last();
            foreach (var cube in last)
            {
                foreach (var next in cube_solution[i])
                {
                    Saltire src = cube.Corners;
                    src.Transform = next.Corners.Transform;

                    if (src == Saltire.IDENTITY)
                    {
                        MoveTrack path = cube_solution.PathTo(cube, cube_solution.Count) + cube_solution.PathTo(next, i);
                        Add(path);
                    }
                }
            }
        }

        public class SearchEntry : IComparable<SearchEntry>
        {
            public ulong _dst_key;
            public int _changed_element_count;
            public MoveTrack _path;
            public bool _handled;

            public SearchEntry(Cross dst, MoveTrack path)
            {
                _dst_key = dst;
                _changed_element_count = (int)Cross.CUBELET_NUM - dst.CountSolvedCubelets;
                _path = path;
                _handled = false;
            }

            public int CompareTo(SearchEntry other)
            {
                int result = _changed_element_count - other._changed_element_count;
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

        public void FirstIteration(Cross src, Dictionary<ulong, SearchEntry> done)
        {
            int count = src.CountSolvedCubelets;
            foreach (var pair in Tracks)
            {
                Cross dst = src;
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

        public void NextIteration(Cross src, MoveTrack path, Dictionary<ulong, SearchEntry> done, int threshold)
        {
            int count = src.CountSolvedCubelets;

            foreach (var pair in Tracks)
            {
                MoveTrack dst_path = path + pair.Value;
                if (dst_path.Count >= threshold)
                {
                    continue;
                }

                Cross dst = src;
                dst.Transform = pair.Key;

                if (dst.CountSolvedCubelets >= count)
                {
                    ulong dst_key = dst;

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

        public MoveTrack Solve(Cross src, int min_breadth)
        {
            ulong win_key = Cross.IDENTITY;

            MoveTrack path = null;
            var done = new Dictionary<ulong, SearchEntry>();

            FirstIteration(src, done);

            int threshold = 55;
            for (int try_count = 0; ; try_count++)
            {
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
                // make list
                // get top ten
                // test them

                var list = new List<SearchEntry>(
                    from entry in done.Values
                    where entry._path.Count < threshold && !entry._handled
                    select entry);

                list.Sort();
                //list.Reverse();
                if (list.Count > min_breadth)
                {
                    int i = min_breadth;
                    int unsolved = list[i]._changed_element_count;
                    while (++i < list.Count)
                    {
                        if (list[i]._changed_element_count != unsolved)
                        {
                            break;
                        }
                    }

                    if (i > min_breadth * 3 && path != null)
                    {
                        i = min_breadth * 3;
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
                    NextIteration(entry._dst_key, entry._path, done, threshold);
                }
            }

            return path;
        }
    }
}
