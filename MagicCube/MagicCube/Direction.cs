using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    enum Turn { Up, Right, Down, Left };

    class Direction
    {
        public const int
            UP = 0,
            RIGHT = 1,
            DOWN = 2,
            LEFT = 3,
            TURN_COUNT = 4;

        // relative rotation
        public static int Sum(int a, int b)
        {
            return (a + b) % TURN_COUNT;
        }

        // 90 degree clockwise rotation (right-hand rule)
        public static int TurnRight(int direction)
        {
            return Sum(direction, RIGHT);
        }

        // 90 degree anticlockwise rotation
        public static int TurnLeft(int direction)
        {
            return Sum(direction, LEFT);
        }

        // 180 degree turn
        public static int TurnAround(int direction)
        {
            return Sum(direction, DOWN);
        }
    }
}
