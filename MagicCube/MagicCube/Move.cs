using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class Move
    {
        uint face;
        uint turn;

        public uint Face
        {
            get
            {
                return face;
            }
            set
            {
                face = value % Cube.FACE_NUM;
            }
        }

        public uint Turn
        {
            get
            {
                return turn;
            }
            set
            {
                turn = value % Direction.TURN_COUNT;
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
            Face = face;
            Turn = turn;
        }

        public override string ToString()
        {
            return ToName(Face, Turn);
        }

        public static string ToName(uint face, uint turn)
        {
            StringBuilder sb = new StringBuilder(2);
            sb.Append(Cube.FaceAcronym[(int)face]);

            turn %= Direction.TURN_COUNT;
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
    }
}
