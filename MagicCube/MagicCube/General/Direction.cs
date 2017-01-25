using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public static class Directions
    {
        public const uint
            Up    = 0,
            Right = 1,
            Down  = 2,
            Left  = 3,
            Count = 4;

        static uint[] _all = { Up, Right, Down, Left };

        // relative rotation
        public static uint Sum(uint a, uint b) => (a + b) % Count;

        // 90 degree clockwise rotation (right-hand rule)
        public static uint TurnRight(uint direction) => Sum(direction, Right);

        // 90 degree anticlockwise rotation
        public static uint TurnLeft(uint direction) => Sum(direction, Left);

        // 180 degree turn
        public static uint TurnAround(uint direction) => Sum(direction, Down);

        public static IEnumerable<uint> All() => _all;
    }
}
