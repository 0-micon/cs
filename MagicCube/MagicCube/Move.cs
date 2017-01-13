using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct Move
    {
        public const uint FACE_NUM = Cube.FACE_NUM;
        public const uint TURN_NUM = Direction.TURN_COUNT;

        uint move_index;

        public uint Face
        {
            get
            {
                return move_index % FACE_NUM;
            }
            set
            {
                move_index = (value % FACE_NUM) + FACE_NUM * Turn;
            }
        }

        public uint Turn
        {
            get
            {
                return (move_index / FACE_NUM) % TURN_NUM;
            }
            set
            {
                move_index = Face + FACE_NUM * (value % TURN_NUM);
            }
        }

        public uint TurnBack
        {
            get
            {
                uint t = Turn;
                if (t == Direction.LEFT)
                {
                    t = Direction.RIGHT;
                }
                else if (t == Direction.RIGHT)
                {
                    t = Direction.LEFT;
                }
                return t;
            }
        }

        public Move(uint face, uint turn)
        {
            move_index = 0;

            Face = face;
            Turn = turn;
        }

        public Move(uint index) : this(index / 3, 1 + (index % 3))
        {
        }

        public Move Reverse()
        {
            return new Move(Face, TurnBack);
        }

        public override string ToString()
        {
            return ToName(Face, Turn);
        }

        public static string ToName(uint face, uint turn)
        {
            StringBuilder sb = new StringBuilder(2);
            sb.Append(Cube.FaceAcronym[(int)face]);

            turn %= TURN_NUM;
            if(turn == 2)
            {
                sb.Append('2');
            }
            else if(turn == 3)
            {
                sb.Append('\'');
            }

            return sb.ToString();
        }

        public static bool operator true(Move m)
        {
            return m.Turn != 0;
        }

        public static bool operator false(Move m)
        {
            return m.Turn == 0;
        }

        public static implicit operator uint(Move m)
        {
            return m.move_index;
        }
    }
}
