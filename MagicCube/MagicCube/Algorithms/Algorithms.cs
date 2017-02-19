using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MagicCube
{
    public abstract class GeneralAlgorithms<TKey>
    {
        Dictionary<TKey, MoveTrack> _tracks = new Dictionary<TKey, MoveTrack>();

        public Dictionary<TKey, MoveTrack> Tracks => _tracks;

        public abstract TKey ToKey(MoveTrack track);
        public abstract bool CanAdd(MoveTrack track);
        public abstract int CountChangedElements(MoveTrack track);
        public abstract void SaveKey(StreamWriter file, TKey key);

        public IEnumerable<KeyValuePair<TKey, MoveTrack>> AllTransforms(MoveTrack src_track)
        {
            foreach (MoveTrack dst_track in src_track.AllTransforms())
            {
                yield return new KeyValuePair<TKey, MoveTrack>(ToKey(dst_track), dst_track);
            }
        }

        public int Add(MoveTrack track)
        {
            int result = 0;

            foreach (var pair in AllTransforms(track))
            {
                if (CanAdd(pair.Value))
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
                    }
                }
            }
            return result;
        }

        public void Save(string fname)
        {
            using (var file = new StreamWriter(fname))
            {
                var done = new HashSet<TKey>();

                foreach (var pair in _tracks)
                {
                    if (!done.Contains(pair.Key))
                    {
                        var save = pair;

                        foreach (var tran in AllTransforms(pair.Value))
                        {
                            done.Add(tran.Key);
                            if (tran.Value.Track.CompareTo(save.Value.Track) < 0)
                            {
                                save = tran;
                            }
                        }

                        int count = CountChangedElements(save.Value);

                        file.Write(save.Value.Count);
                        file.Write(';');
                        file.Write(save.Value.Track);
                        file.Write(';');
                        file.Write(count);
                        file.Write(';');
                        SaveKey(file, save.Key);
                        file.Write('\n');
                    }
                }
            }
        }

        public void Load(string fname)
        {
            using (var file = new StreamReader(fname))
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

        // Removes unneccesary algorithms
        public void Purge(IEnumerable<TKey> basis)
        {
            var expels = new HashSet<TKey>();
            var outset = new HashSet<TKey>();
            foreach (var a in basis)
            {
                foreach (var b in basis)
                {
                    var path = _tracks[a] + _tracks[b];
                    TKey key = ToKey(path);

                    if (_tracks.ContainsKey(key))
                    {
                        if (_tracks[key].Count >= path.Count)
                        {
                            expels.Add(key);
                            outset.Add(a);
                            outset.Add(b);
                        }
                        else
                        {
                            // TODO: It might be wise to add these tracks to substitutes.
                            ;
                        }
                    }
                }
            }

            foreach (var key in expels)
            {
                if (!outset.Contains(key))
                {
                    Tracks.Remove(key);
                }
            }
        }

    }
}
