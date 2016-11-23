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
        static uint[,,] middle_element_pairs;
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

            middle_element_pairs = new uint[FACE_NUM, Direction.TURN_COUNT, 2];
            for(uint face = 0; face < FACE_NUM; face++)
            {
                foreach(uint direction in Direction.Items())
                {
                    middle_element_pairs[face, direction, 0] = FaceIndex(face) + direction;

                    uint neighbour_face = LAYOUT[face, direction];
                    uint neighbour_pos = NEIGHBOUR_POS[neighbour_face, face];

                    middle_element_pairs[face, direction, 1] = FaceIndex(neighbour_face) + neighbour_pos;
                }
            }
        }

        uint[] middle_elements =
        {
            F, F, F, F, // Front
            B, B, B, B, // Back
            U, U, U, U, // Up
            D, D, D, D, // Down
            L, L, L, L, // Left
            R, R, R, R, // Right
        };

        
        public ulong MiddleKey
        {
            get
            {
                ulong key = 0;
                foreach (uint me in middle_elements)
                {
                    key *= FACE_NUM;
                    key += me;
                }
                return key;
            }
            set
            {
                int i = middle_elements.Length;
                while(i-- > 0)
                {
                    middle_elements[i] = (uint)(value % FACE_NUM);
                    value /= FACE_NUM;
                }
            }
        }


        public uint MiddleElementAt(uint face, uint direction)
        {
            uint pos = FaceIndex(face) + direction;
            uint mel = middle_elements[pos];
            return mel;

            //mel = middle_elements[middle_element_pairs[face, direction, 0]];
        }

        public uint MiddleElementAt(uint face, uint direction, uint value)
        {
            uint pos = FaceIndex(face) + direction;
            uint mel = middle_elements[pos];
            middle_elements[pos] = value;
            return mel;

            //mel = middle_elements[middle_element_pairs[face, direction, 0]];
            //middle_elements[middle_element_pairs[face, direction, 0]] = value;
        }

        public uint NeighbourMiddleElementAt(uint face, uint direction)
        {
            uint neighbour = LAYOUT[face, direction];
            return MiddleElementAt(neighbour, NEIGHBOUR_POS[neighbour, face]);
            //uint mel = middle_elements[middle_element_pairs[face, direction, 1]];
            //return mel;
        }

        public uint NeighbourMiddleElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            return MiddleElementAt(neighbour, NEIGHBOUR_POS[neighbour, face], value);
            //uint mel = NeighbourMiddleElementAt(face, direction);
            //middle_elements[middle_element_pairs[face, direction, 1]] = value;
            //return mel;
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

        //Face[] faces = { new Face(F), new Face(B), new Face(U), new Face(D), new Face(L), new Face(R) };
    }
}
