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

        public MoveTrack RunOnce(Saltire cube)
        {
            MoveTrack dst_track = null;
            int solved_count = cube.CountSolvedCubelets;

            foreach (var pair in _tracks)
            {
                Saltire dst = new Saltire(cube);
                dst.Transform = pair.Key; // pair.Value.PlayForward(cross);

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
    }
}
