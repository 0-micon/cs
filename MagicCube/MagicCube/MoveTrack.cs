using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class MoveTrack : List<Move>
    {
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
                foreach (char ch in src.ToUpper())
                {
                    int i = Cube.FaceAcronym.IndexOf(ch);
                    if (i >= 0)
                    {
                        base.Add(new Move((uint)i, Direction.RIGHT));
                    }
                    else if (Count > 0)
                    {
                        int last = Count - 1;
                        if (ch == '\'' || ch == '3')
                        {
                            this[last] = new Move(this[last].Face, Direction.LEFT);
                        }
                        else if (ch == '2')
                        {
                            this[last] = new Move(this[last].Face, Direction.DOWN);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Join(" ", this);
        }

        public MoveTrack(string src = null)
        {
            FromString(src);
        }

        public void PlayForward(Cube cube)
        {
            for (int i = 0; i < Count; i++)
            {
                Move m = this[i];
                for (uint t = 0; t < m.Turn; t++)
                {
                    cube.RotateRight(m.Face);
                }
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
                foreach (Move m in this)
                {
                    dst.Add(new Move(transform[m.Face], m.Turn));
                }
            }
            else
            {
                foreach (Move m in this)
                {
                    dst.Add(new Move(m.Face, m.Turn));
                }
            }

            return dst;
        }

        public new MoveTrack Reverse()
        {
            MoveTrack dst = new MoveTrack();
            for (int i = Count; i-- > 0;)
            {
                Move m = this[i];
                dst.Add(new Move(m.Face, m.TurnBack));
            }
            return dst;
        }

        public new void Add(Move move)
        {
            int last = Count - 1;
            if (last >= 0 && this[last].Face == move.Face)
            {
                Move dst_move = new Move(move.Face, this[last].Turn + move.Turn);
                if (dst_move)
                {
                    this[last] = dst_move;
                }
                else
                {
                    RemoveAt(last);
                }
            }
            else
            {
                base.Add(move);
            }
        }

        public MoveTrack SubTrack(int start, int length)
        {
            MoveTrack dst = new MoveTrack();
            for(int i = start; i < start + length; i++)
            {
                dst.Add(this[i]);
            }
            return dst;
        }

        public static MoveTrack operator +(MoveTrack a, MoveTrack b)
        {
            MoveTrack dst = a.Clone();
            foreach (Move m in b)
            {
                dst.Add(new Move(m.Face, m.Turn));
            }
            return dst;
        }

        public int LastIndex
        {
            get
            {
                return Count - 1;
            }
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
