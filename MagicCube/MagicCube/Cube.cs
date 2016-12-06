using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace MagicCube
{
    class Cube
    {
        public const uint
            Front = 0,
            Up    = 1,
            Right = 2,
            Back  = 3,
            Down  = 4,
            Left  = 5;

        public static uint UpFace(uint face)
        {
            return (face + Up) % FACE_NUM;
        }

        public static uint DownFace(uint face)
        {
            return (face + Down) % FACE_NUM;
        }

        public static uint RightFace(uint face)
        {
            return (face + ((face & 1) == 1 ? Left : Right)) % FACE_NUM;
        }

        public static uint LeftFace(uint face)
        {
            return (face + ((face & 1) == 1 ? Right : Left)) % FACE_NUM;
        }

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

            uint i = FACE_NUM * Direction.TURN_COUNT;
            denominators = new ulong[i];
            while (i-- > 0)
            {
                if(i + 1 < denominators.Length)
                {
                    denominators[i] = FACE_NUM * denominators[i + 1];
                }
                else
                {
                    denominators[i] = 1;
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

        uint[] corner_elements =
        {
            F, F, F, F, // Front
            B, B, B, B, // Back
            U, U, U, U, // Up
            D, D, D, D, // Down
            L, L, L, L, // Left
            R, R, R, R, // Right
        };

        static ulong[] denominators;

        public struct Move
        {
            uint move;

            public Move(uint face, uint turn)
            {
                move = face;
                Turn = turn;
            }

            public uint Face
            {
                get
                {
                    return move % FACE_NUM;
                }
                set
                {
                    move = value;
                }
            }

            public uint Turn
            {
                get
                {
                    return (move / FACE_NUM) % Direction.TURN_COUNT;
                }
                set
                {
                    move = Face + FACE_NUM * (value % Direction.TURN_COUNT);
                }
            }

            public override string ToString()
            {
                return FaceInfo.items[Face].color + ", " + Turn + " time(s)";
            }

            public static Move KeysToMove(Solution.Key src_key, Solution.Key dst_key)
            {
                Cube c = new Cube();
                c.MiddleKey = src_key.middles;
                c.CornerKey = src_key.corners;

                foreach (uint move in c.Moves())
                {
                    if (c.CornerKey == dst_key.corners && c.MiddleKey == dst_key.middles)
                    {
                        uint face = move / 3;
                        uint turn = 1 + (move % 3);
                        return new Move(face, turn);
                    }
                }
                return new Move(0, 0);
            }

            public static Move MiddleKeysToMove(ulong src_key, ulong dst_key)
            {
                int count = 0;
                foreach (ulong key in Cube.NextMiddleKeys(src_key))
                {
                    if (key == dst_key)
                    {
                        uint face = (uint) (count / 3);
                        uint turn = (uint) (1 + (count % 3));
                        return new Move(face, turn);
                    }
                    count++;
                }
                return new Move(0, 0);
            }

            public static Move CornerKeysToMove(ulong src_key, ulong dst_key)
            {
                int count = 0;
                foreach (ulong key in Cube.NextCornerKeys(src_key))
                {
                    if (key == dst_key)
                    {
                        uint face = (uint)(count / 3);
                        uint turn = (uint)(1 + (count % 3));
                        return new Move(face, turn);
                    }
                    count++;
                }
                return new Move(0, 0);
            }
        }


        public ulong GetMiddleKey(uint shift)
        {
            ulong key = 0;
            for (uint i = 0; i < FACE_NUM; i++)
            {
                uint start = (i + shift) % FACE_NUM;
                for(uint j = 0; j < Direction.TURN_COUNT; j++)
                {
                    uint me = middle_elements[start * Direction.TURN_COUNT + j];
                    key *= FACE_NUM;
                    key += (FACE_NUM + me - shift) % FACE_NUM;
                }
            }
            return key;
        }


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

        public ulong CornerKey
        {
            get
            {
                ulong key = 0;
                foreach (uint ce in corner_elements)
                {
                    key *= FACE_NUM;
                    key += ce;
                }
                return key;
            }
            set
            {
                int i = corner_elements.Length;
                while (i-- > 0)
                {
                    corner_elements[i] = (uint)(value % FACE_NUM);
                    value /= FACE_NUM;
                }
            }
        }

        public IEnumerable<uint> Moves()
        {
            uint move = 0;
            for (uint face = 0; face < FACE_NUM; face++)
            {
                RotateRight(face);
                yield return move++;

                RotateRight(face);
                yield return move++;

                RotateRight(face);
                yield return move++;

                RotateRight(face); // restore
            }
        }

        public static IEnumerable<uint> Moves(ulong middle_key, ulong corner_key)
        {
            Cube c = new Cube();
            c.MiddleKey = middle_key;
            c.CornerKey = corner_key;
            return c.Moves();
        }

        public IEnumerable<ulong> NextCornerKeys()
        {
            for (uint face = 0; face < FACE_NUM; face++)
            {
                RotateRight(face);
                yield return CornerKey;

                RotateRight(face);
                yield return CornerKey;

                RotateRight(face);
                yield return CornerKey;

                RotateRight(face); // restore
            }
        }

        public static IEnumerable<ulong> NextCornerKeys(ulong corner_key)
        {
            Cube c = new Cube();
            c.CornerKey = corner_key;
            return c.NextCornerKeys();
        }

        public IEnumerable<ulong> NextMiddleKeys()
        {
            for (uint face = 0; face < FACE_NUM; face++)
            {
                RotateRight(face);
                yield return MiddleKey;

                RotateRight(face);
                yield return MiddleKey;

                RotateRight(face);
                yield return MiddleKey;

                RotateRight(face); // restore
            }
        }

        public static IEnumerable<ulong> NextMiddleKeys(ulong middle_key)
        {
            Cube c = new Cube();
            c.MiddleKey = middle_key;
            return c.NextMiddleKeys();
        }

        public uint CornerElementAt(uint face, uint direction)
        {
            uint pos = FaceIndex(face) + direction;
            uint cel = corner_elements[pos];
            return cel;
        }

        public uint CornerElementAt(uint face, uint direction, uint value)
        {
            uint pos = FaceIndex(face) + direction;
            uint cel = corner_elements[pos];
            corner_elements[pos] = value;
            return cel;
        }

        public uint NeighbourCornerElementAt(uint face, uint direction)
        {
            uint neighbour = LAYOUT[face, direction];
            return CornerElementAt(neighbour, NEIGHBOUR_POS[neighbour, face]);
        }

        public uint NeighbourCornerElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            return CornerElementAt(neighbour, NEIGHBOUR_POS[neighbour, face], value);
        }

        public uint SecondNeighbourCornerElementAt(uint face, uint direction)
        {
            uint neighbour = LAYOUT[face, direction];
            return CornerElementAt(neighbour, Direction.TurnLeft(NEIGHBOUR_POS[neighbour, face]));
        }

        public uint SecondNeighbourCornerElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            return CornerElementAt(neighbour, Direction.TurnLeft(NEIGHBOUR_POS[neighbour, face]), value);
        }


        public uint MiddleElementAt(uint face, uint direction)
        {
            uint pos = FaceIndex(face) + direction;
            uint mel = middle_elements[pos];

            ulong key = MiddleKey;
            key /= denominators[pos];
            key %= FACE_NUM;
            Debug.Assert(mel == (uint)(key));
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
            uint c = CornerElementAt(face, Direction.TURN_COUNT - 1);
            foreach (uint i in Direction.Items())
            {
                m = MiddleElementAt(face, i, m);
                c = CornerElementAt(face, i, c);
            }

            // neighbours rotation
            m = NeighbourMiddleElementAt(face, Direction.TURN_COUNT - 1);
            c = NeighbourCornerElementAt(face, Direction.TURN_COUNT - 1);
            uint s = SecondNeighbourCornerElementAt(face, Direction.TURN_COUNT - 1);
            foreach (uint i in Direction.Items())
            {
                m = NeighbourMiddleElementAt(face, i, m);
                c = NeighbourCornerElementAt(face, i, c);
                s = SecondNeighbourCornerElementAt(face, i, s);
            }
        }

        //Face[] faces = { new Face(F), new Face(B), new Face(U), new Face(D), new Face(L), new Face(R) };
    }
}
