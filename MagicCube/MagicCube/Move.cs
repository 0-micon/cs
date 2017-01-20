﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct Move
    {
        const uint TURN_NUM = Direction.TURN_COUNT - 1;

        uint move_index;

        public uint Face
        {
            get
            {
                return move_index / TURN_NUM;
            }
            set
            {
                move_index = value * TURN_NUM + Turn - 1;
            }
        }

        public uint Turn
        {
            get
            {
                return 1 + (move_index % TURN_NUM);
            }
            set
            {
                move_index = Face * TURN_NUM + value - 1;
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

        public Move(uint face, uint turn) : this(0)
        {
            Face = face;
            Turn = turn;
        }

        public Move(uint index)
        {
            move_index = index;
        }

        public Move Reverse()
        {
            return new Move(Face, TurnBack);
        }

        public override string ToString()
        {
            return ToName(Face, Turn);
        }

        public char ToChar()
        {
            return (char)('a' + move_index);
        }

        public static Move FromChar(char ch)
        {
            return new Move((uint)ch - 'a');
        }

        public static string ToName(uint face, uint turn)
        {
            StringBuilder sb = new StringBuilder(2);
            sb.Append(Faces.Acronym[(int)face]);

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

        public T Apply<T>(T cube, bool backwards = false) where T : Faces.IRotatable
        {
            uint face = Face;
            uint turn = backwards ? TurnBack : Turn;

            switch (turn)
            {
                case 3: cube.RotateFace(face); goto case 2;    // fall through
                case 2: cube.RotateFace(face); goto case 1;    // fall through
                case 1: cube.RotateFace(face); break;
            }
            return cube;
        }
    }
}
