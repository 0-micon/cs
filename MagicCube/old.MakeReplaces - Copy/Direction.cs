using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    //enum Turn { Up, Right, Down, Left };

    struct Turn
    {
        public const uint
            UP = 0,
            RIGHT = 1,
            DOWN = 2,
            LEFT = 3,
            COUNT = 4;

        uint count;

        public Turn(uint start = UP)
        {
            count = UP;
            Value = start;
        }

        public uint Value {
            get
            {
                return count;
            }
            set
            {
                count = value % COUNT;
            }
        }

        // 90 degree clockwise rotation (right-hand rule)
        public uint Next()
        {
            Value += RIGHT;
            return Value;
        }

        // 90 degree anticlockwise rotation
        public uint Prev()
        {
            Value += LEFT;
            return Value;
        }

        // 180 degree turn
        public uint Mirror()
        {
            Value += DOWN;
            return Value;
        }
    }

    class Direction
    {
        public const uint
            UP = 0,
            RIGHT = 1,
            DOWN = 2,
            LEFT = 3,
            TURN_COUNT = 4;

        // relative rotation
        public static uint Sum(uint a, uint b)
        {
            return (a + b) % TURN_COUNT;
        }

        // 90 degree clockwise rotation (right-hand rule)
        public static uint TurnRight(uint direction)
        {
            return Sum(direction, RIGHT);
        }

        // 90 degree anticlockwise rotation
        public static uint TurnLeft(uint direction)
        {
            return Sum(direction, LEFT);
        }

        // 180 degree turn
        public static uint TurnAround(uint direction)
        {
            return Sum(direction, DOWN);
        }

        public static IEnumerable<uint> Items(uint first = UP, uint last = LEFT)
        {
            for (; first <= last; first++)
            {
                yield return first;
            }
        }
    }
}
