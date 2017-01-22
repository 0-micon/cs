using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class MoveTrack : IComparable<MoveTrack>, IEnumerable<Move>
    {
        string _track;

        static SearchTree _substitute;
        static string _substitute_fname = "substitutes.txt";

        static MoveTrack()
        {
            _substitute = new SearchTree();
            _substitute.Load(_substitute_fname);
        }

        public Move this[int i] => Move.FromChar(_track[i]);
        public int Count        => _track.Length;
        public string Track     => _track;

        public void Clear()
        {
            _track = string.Empty;
        }

        public void Trim()
        {
            for(int i = 0; i < _track.Length - 1; i++)
            {
                int j = i;
                string dst = _substitute.GetReplacement(_track, ref j);
                if (dst != null)
                {
                    string src = _track.Substring(i, j - i + 1);
                    _track = _track.Replace(src, dst);
                    Trim();  // re-trim
                    return;
                }
            }
        }

        // Notation (known as "Singmaster notation"):
        // 1) the first letters from the names of the sides (F, U, R...) is used as a move name;
        // 2) 2 for a double face move (FF is equal to F2);
        // 3) 3 or ' for a triple face move (FFF is equal to F3 or F');
        // Example:
        // F R2 L' -> Turn Front Face clockwise, Right Face two times and Left Face anticlockwise;
        public void FromString(string src)
        {
            Clear();
            if (src != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (char ch in src.ToUpper())
                {
                    int i = Faces.Acronym.IndexOf(ch);
                    if (i >= 0)
                    {
                        sb.Append((new Move((uint)i, Direction.RIGHT)).ToChar());
                    }
                    else if (sb.Length > 0)
                    {
                        int last = sb.Length - 1;
                        if (ch == '\'' || ch == '3')
                        {
                            
                            sb[last] = (new Move(Move.FromChar(sb[last]).Face, Direction.LEFT)).ToChar();
                        }
                        else if (ch == '2')
                        {
                            sb[last] = (new Move(Move.FromChar(sb[last]).Face, Direction.DOWN)).ToChar();
                        }
                    }
                }
                _track = sb.ToString();
            }
        }

        public Move[] ToArray()
        {
            Move[] moves = new Move[Count];
            for(int i = 0; i < Count; i++)
            {
                moves[i] = this[i];
            }
            return moves;
        }

        public override string ToString()
        {
            return string.Join(" ", ToArray());
        }

        public MoveTrack(string src, bool singmaster_notation = true)
        {
            if (singmaster_notation)
            {
                FromString(src);
            }
            else
            {
                _track = src;
            }
        }

        public MoveTrack()
        {
            _track = string.Empty;
        }

        public MoveTrack Clone(uint[] transform = null)
        {
            MoveTrack dst = new MoveTrack();

            if (transform != null)
            {
                for(int i = 0; i < Count; i++)
                {
                    Move m = this[i];
                    dst.Add(new Move(transform[m.Face], m.Turn));
                }
            }
            else
            {
                dst._track = _track;
            }

            return dst;
        }

        public MoveTrack Reverse()
        {
            MoveTrack dst = new MoveTrack();
            for (int i = Count; i-- > 0;)
            {
                Move m = this[i];
                dst.Add(new Move(m.Face, m.TurnBack));
            }
            return dst;
        }

        public void Add(Move move)
        {
            _track += move.ToChar();
        }

        public MoveTrack SubTrack(int start, int length)
        {
            MoveTrack dst = new MoveTrack();
            dst._track = _track.Substring(start, length);
            return dst;
        }

        public static MoveTrack operator +(MoveTrack a, MoveTrack b)
        {
            MoveTrack dst = new MoveTrack();
            dst._track = a._track + b._track;
            dst.Trim();
            return dst;
        }

        public IEnumerable<MoveTrack> AllTransforms()
        {
            MoveTrack reverse = Reverse();
            foreach (uint[] transform in Faces.Orientations)
            {
                yield return Clone(transform);
                yield return reverse.Clone(transform);
            }
        }

        public static void MakeReplaces()
        {
            Dictionary<string, string> synonyms = new Dictionary<string, string>();

            Cube cube = new Cube();
            Dictionary<CubeKey, MoveTrack> done = new Dictionary<CubeKey, MoveTrack>();
            done[cube.Key] = new MoveTrack();

            // reset substitutes
            _substitute.Clear();

            for (int i = 0; i < 6; i++)
            {
                List<CubeKey> key_list = new List<CubeKey>(from entry in done where entry.Value.Count == i select entry.Key);
                foreach (CubeKey src_key in key_list)
                {
                    cube.Key = src_key;
                    MoveTrack src_path = done[src_key];
                    foreach (uint move_index in cube.Moves())
                    {
                        MoveTrack dst_path = src_path.Clone();
                        dst_path.Add(new Move(move_index));

                        CubeKey dst_key = cube.Key;

                        if (done.ContainsKey(dst_key))
                        {
                            MoveTrack rep_path = done[dst_key];
                            dst_path.Trim();

                            int delta = dst_path.Count - rep_path.Count;
                            if (delta >= 0)
                            {
                                int beg = 0;                // left trim count
                                int end = rep_path.Count;   // rep_path.Count - right trim count

                                while (beg < end && rep_path.Track[beg] == dst_path.Track[beg])
                                {
                                    beg++;
                                }


                                while (end > beg && rep_path.Track[end - 1] == dst_path.Track[end - 1 + delta])
                                {
                                    end--;
                                }

                                MoveTrack dst = dst_path.SubTrack(beg, end - beg + delta);
                                MoveTrack rep = rep_path.SubTrack(beg, end - beg);

                                if (dst.CompareTo(rep) != 0)
                                {
                                    Debug.Assert(dst.Count > 0);

                                    if (delta == 0)
                                    {
                                        if (!synonyms.ContainsKey(dst.Track))
                                        {
                                            synonyms.Add(dst.Track, rep.Track);
                                        }
                                        else
                                        {
                                            Debug.Assert(synonyms[dst.Track] == rep.Track);
                                        }
                                    }
                                    else
                                    {
                                        if (!_substitute.ContainsKey(dst.Track))
                                        {
                                            _substitute.Add(dst.Track, rep.Track);
                                        }
                                        else
                                        {
                                            Debug.Assert(_substitute[dst.Track] == rep.Track);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            done[dst_key] = dst_path;
                        }
                    }
                }
            }

            _substitute.Save(_substitute_fname);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter("synonyms.txt"))
            {
                foreach (KeyValuePair<string, string> pair in synonyms)
                {
                    file.WriteLine(pair.Key + ";" + pair.Value);
                }
            }
        }

        //
        // Summary:
        //     Compares the current object with another object of the same type.
        //
        // Parameters:
        //   other: An object to compare with this object.
        //
        // Returns:
        //     A value that indicates the relative order of the objects being compared. The
        //     return value has the following meanings: Value Meaning Less than zero This object
        //     is less than the other parameter.Zero This object is equal to other. Greater
        //     than zero This object is greater than other.
        public int CompareTo(MoveTrack other) => _track.CompareTo(other._track);

        public override int GetHashCode() => _track.GetHashCode();

        public bool Equals(MoveTrack other) => _track.Equals(other._track);

        public override bool Equals(object obj)
        {
            if (obj is MoveTrack)
            {
                return Equals((MoveTrack)obj);
            }
            return false;
        }

        // IEnumerable interface implementation:
        public IEnumerator<Move> GetEnumerator() => new SimpleEnumerator<Move>(ToArray());
        IEnumerator  IEnumerable.GetEnumerator() => new SimpleEnumerator<Move>(ToArray());
    }

    public static class MoveTrackExtension
    {
        public static T PlayForward<T>(this T cube, MoveTrack track) where T : Faces.IRotatable
        {
            for (int i = 0; i < track.Count; i++)
            {
                cube = cube.MoveForward(track[i]);
            }
            return cube;
        }

        public static T PlayBackward<T>(this T cube, MoveTrack track) where T : Faces.IRotatable
        {
            for (int i = track.Count; i-- > 0;)
            {
                cube = cube.MoveBackward(track[i]);
            }
            return cube;
        }
    }
}
