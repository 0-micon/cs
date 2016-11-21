using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    class Cube
    {
        public const uint
            F = 0, // Front
            B = 1, // Back
            U = 2, // Up
            D = 3, // Down
            L = 4, // Left
            R = 5; // Right

        public const uint FACE_NUM = 6;

        static uint[,] LAYOUT = 
        {
            { U, R, D, L },
            { D, R, U, L },
            { R, F, L, B },
            { L, F, R, B },
            { F, D, B, U },
            { B, D, F, U }
        };

        Face[] faces = { new Face(F), new Face(B), new Face(U), new Face(D), new Face(L), new Face(R) };
    }
}
