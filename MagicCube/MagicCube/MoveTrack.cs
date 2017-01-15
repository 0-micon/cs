using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class MoveTrack
    {
        string _track;

        static SearchTree _substitute;

        static MoveTrack()
        {
            _substitute = new SearchTree();
            _substitute.Load("substitites.txt");
        }

        public Move this[int i] => Move.FromChar(_track[i]);
        public int Count        => _track.Length;
        public string Track     => _track;

        public void Clear()
        {
            _track = string.Empty;
        }

        void Trim()
        {
            for(int i = 0; i < _track.Length - 1; i++)
            {
                int j = i;
                string dst = _substitute.GetReplacement(_track, ref j);
                if (dst != null)
                {
                    string src = _track.Substring(i, j - i + 1);
                    _track = _track.Replace(src, dst);
                    Trim();
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
                    int i = Cube.FaceAcronym.IndexOf(ch);
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

        public void PlayForward(Cube cube)
        {
            for (int i = 0; i < Count; i++)
            {
                PlayForward(cube, i);
            }
        }

        public void PlayForward(Cube cube, int idx)
        {
            Move m = this[idx];

            uint face = m.Face;
            uint turn = m.Turn;

            switch (turn)
            {
                case 3: cube.RotateRight(face); goto case 2;    // fall through
                case 2: cube.RotateRight(face); goto case 1;    // fall through
                case 1: cube.RotateRight(face); break;
            }
        }

        public Cross PlayForward(Cross cross)
        {
            for (int i = 0; i < Count; i++)
            {
                Move m = this[i];
                for (uint t = 0; t < m.Turn; t++)
                {
                    cross.RotateFace(m.Face);
                }
            }
            return cross;
        }

        public void PlayBackward(Cube cube)
        {
            for (int i = Count; i-- > 0;)
            {
                Move m = this[i];
                for (uint t = 0; t < m.TurnBack; t++)
                {
                    cube.RotateRight(m.Face);
                }
            }
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
            foreach (uint[] transform in Cube.ORIENTATION)
            {
                yield return Clone(transform);
                yield return reverse.Clone(transform);
            }
        }

    }
}
