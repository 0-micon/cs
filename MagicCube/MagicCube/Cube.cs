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

        static readonly uint[,] LAYOUT = 
        {
            { U, R, D, L },
            { D, R, U, L },
            { R, F, L, B },
            { L, F, R, B },
            { F, D, B, U },
            { B, D, F, U }
        };

        static readonly uint[,] NEIGHBOUR_POS;
/*//
        =
        {
        //    F  B  U  D  L  R
            { 8, 8, 0, 2, 3, 1 },   // F
            { 8, 8, 2, 0, 3, 1 },   // B
            { 1, 3, 8, 8, 2, 0 },   // U
            { 1, 3, 8, 8, 0, 2 },   // D
            { 0, 2, 3, 1, 8, 8 },   // L
            { 2, 0, 3, 1, 8, 8 },   // R
        };
//*/
        static Cube()
        {
            uint[,] np = new uint[FACE_NUM, FACE_NUM];
            for(uint face = 0; face < FACE_NUM; face++)
            {
                foreach(uint direction in Direction.Items())
                {
                    uint neighbour = LAYOUT[face, direction];
                    np[face, neighbour] = direction;
                }
            }
            NEIGHBOUR_POS = np;
        }

        uint[] middle_elements =
        {
            0, 0, 0, 0, // Front
            1, 1, 1, 1, // Back
            2, 2, 2, 2, // Up
            3, 3, 3, 3, // Down
            4, 4, 4, 4, // Left
            5, 5, 5, 5, // Right
        };

        public uint MiddleElementAt(uint face, uint direction)
        {
            uint pos = FaceIndex(face) + direction;
            uint mel = middle_elements[pos];
            return mel;
        }

        public uint MiddleElementAt(uint face, uint direction, uint value)
        {
            uint pos = FaceIndex(face) + direction;
            uint mel = middle_elements[pos];
            middle_elements[pos] = value;
            return mel;
        }

        public uint NeighbourMiddleElementAt(uint face, uint direction)
        {
            uint neighbour = LAYOUT[face, direction];
            return MiddleElementAt(neighbour, NEIGHBOUR_POS[neighbour, face]);
        }

        public uint NeighbourMiddleElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            return MiddleElementAt(neighbour, NEIGHBOUR_POS[neighbour, face], value);
        }

        static uint FaceIndex(uint face)
        {
            return face * Direction.TURN_COUNT;
        }

        public void RotateRight(uint face)
        {
            // Rotate clockwise: 0,1,2,3 => 3,0,1,2
            uint m = MiddleElementAt(face, Direction.TURN_COUNT - 1);
            foreach (uint i in Direction.Items())
            {
                m = MiddleElementAt(face, i, m);
            }

            // neighbours rotation
            m = NeighbourMiddleElementAt(face, Direction.TURN_COUNT - 1);
            foreach (uint i in Direction.Items())
            {
                m = NeighbourMiddleElementAt(face, i, m);
            }
        }

        static uint[,] MIDDLE_ELEMENTS =
        {
            { 0,  0,  1,  2,  3,  4 },
            { 0,  0,  5,  6,  7,  8 },
            { 1,  5,  0,  0,  9, 10 },
            { 2,  6,  0,  0, 11, 12 },
            { 3,  7,  9, 11,  0,  0 },
            { 4,  8, 10, 12,  0,  0 },
        };

        Face[] faces = { new Face(F), new Face(B), new Face(U), new Face(D), new Face(L), new Face(R) };
    }
}
