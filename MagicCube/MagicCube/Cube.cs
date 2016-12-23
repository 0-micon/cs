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
        public const uint FACE_NUM = 6;
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

        public static uint Neighbour(uint face, uint direction)
        {
            switch (direction)
            {
                case Direction.UP:    return UpFace(face);
                case Direction.RIGHT: return RightFace(face);
                case Direction.DOWN:  return DownFace(face);
                case Direction.LEFT:  return LeftFace(face);
            }

            throw new ArgumentOutOfRangeException("direction");
        }

        static readonly uint[,] LAYOUT;
        static readonly uint[,] NEIGHBOUR_POS;

        static Cube()
        {
            uint[,] layout = new uint[FACE_NUM, Direction.TURN_COUNT];
            for(uint face = 0; face < FACE_NUM; face++)
            {
                for(uint direction = 0; direction < Direction.TURN_COUNT; direction++)
                {
                    layout[face, direction] = Neighbour(face, direction);
                }
            }
            LAYOUT = layout;

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
            0, 0, 0, 0,
            1, 1, 1, 1,
            2, 2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4, 4,
            5, 5, 5, 5
        };

        uint[] corner_elements =
        {
            0, 0, 0, 0,
            1, 1, 1, 1,
            2, 2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4, 4,
            5, 5, 5, 5
        };

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
                return FaceInfo.items[Face].name + ", " + Turn + " time(s)";
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


        uint[,] face_order =
        {
            { Front, Up, Right, Back, Down, Left },

        };


        public ulong GetCornerKey(uint face, uint[] order)
        {
            ulong key = 0;
            // front
            foreach(uint direction in Direction.Items())
            {
                key *= FACE_NUM;
                key += order[CornerElementAt(face, direction)];
            }
            // neighbours
            foreach (uint direction in Direction.Items())
            {
                uint nface = LAYOUT[face, direction];
                foreach(uint ndirection in Direction.Items())
                {
                    key *= FACE_NUM;
                    key += order[CornerElementAt(nface, ndirection)];
                }
            }
            // back
            face = (face + 3) % FACE_NUM;
            foreach (uint direction in Direction.Items())
            {
                key *= FACE_NUM;
                key += order[CornerElementAt(face, direction)];
            }

            return key;
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
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return CornerElementAt(neighbour, neighbour_direction);
        }

        public uint NeighbourCornerElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return CornerElementAt(neighbour, neighbour_direction, value);
        }

        public uint SecondNeighbourCornerElementAt(uint face, uint direction)
        {
            uint neighbour = LAYOUT[face, direction];
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return CornerElementAt(neighbour, Direction.TurnLeft(neighbour_direction));
        }

        public uint SecondNeighbourCornerElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return CornerElementAt(neighbour, Direction.TurnLeft(neighbour_direction), value);
        }

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
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return MiddleElementAt(neighbour, neighbour_direction);
        }

        public uint NeighbourMiddleElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return MiddleElementAt(neighbour, neighbour_direction, value);
        }

        static uint FaceIndex(uint face)
        {
            return face * Direction.TURN_COUNT;
        }

        public void RotateRight(uint face)
        {
            // Rotate clockwise: 0,1,2,3 => 3,0,1,2
            const uint last = Direction.TURN_COUNT - 1;

            // self
            uint me = MiddleElementAt(face, last);
            uint ce = CornerElementAt(face, last);

            // neighbours
            uint nme = NeighbourMiddleElementAt(face, last);
            uint nce = NeighbourCornerElementAt(face, last);
            uint snc = SecondNeighbourCornerElementAt(face, last);

            foreach (uint i in Direction.Items())
            {
                me = MiddleElementAt(face, i, me);
                ce = CornerElementAt(face, i, ce);

                nme = NeighbourMiddleElementAt(face, i, nme);
                nce = NeighbourCornerElementAt(face, i, nce);
                snc = SecondNeighbourCornerElementAt(face, i, snc);
            }
        }

        public void Shift1()
        {
            // Left - Front + Up + Left
            // Right - Back + Down + Right

            Dictionary<uint, uint> dict = new Dictionary<uint, uint>();

            dict[Left] = Front;
            dict[Front] = Up;
            dict[Up] = Left;
            dict[Right] = Back;
            dict[Back] = Down;
            dict[Down] = Right;

            Cube c = new Cube();

            foreach (uint d in Direction.Items())
            {
                uint u = Direction.TurnAround(d);

                c.MiddleElementAt(Front, d, dict[MiddleElementAt(Left, d)]);
                c.MiddleElementAt(Up, u, dict[MiddleElementAt(Front, d)]);
                c.MiddleElementAt(Left, u, dict[MiddleElementAt(Up, d)]);

                c.MiddleElementAt(Back, d, dict[MiddleElementAt(Right, d)]);
                c.MiddleElementAt(Down, u, dict[MiddleElementAt(Back, d)]);
                c.MiddleElementAt(Right, u, dict[MiddleElementAt(Down, d)]);

                c.CornerElementAt(Front, d, dict[CornerElementAt(Left, d)]);
                c.CornerElementAt(Up, u, dict[CornerElementAt(Front, d)]);
                c.CornerElementAt(Left, u, dict[CornerElementAt(Up, d)]);

                c.CornerElementAt(Back, d, dict[CornerElementAt(Right, d)]);
                c.CornerElementAt(Down, u, dict[CornerElementAt(Back, d)]);
                c.CornerElementAt(Right, u, dict[CornerElementAt(Down, d)]);
            }

            MiddleKey = c.MiddleKey;
            CornerKey = c.CornerKey;
        }

        public void Shift2()
        { 
            // Front-Right-Down
            // Left-Up-Back
            for (int i = 0; i < middle_elements.Length; i++)
            {
                middle_elements[i] = (middle_elements[i] + 2) % FACE_NUM;
            }
            for (int i = 0; i < corner_elements.Length; i++)
            {
                corner_elements[i] = (corner_elements[i] + 2) % FACE_NUM;
            }

            const uint last = FACE_NUM - 1;
            for (uint src_dir = 0; src_dir < Direction.TURN_COUNT; src_dir++)
            {
                uint me = MiddleElementAt(last, src_dir);
                uint ce = CornerElementAt(last, src_dir);
                for (uint face = 0; face < FACE_NUM; face++)
                {
                    me = MiddleElementAt(face, src_dir, me);
                    ce = CornerElementAt(face, src_dir, ce);
                }
            }
            for (uint src_dir = 0; src_dir < Direction.TURN_COUNT; src_dir++)
            {
                uint me = MiddleElementAt(last, src_dir);
                uint ce = CornerElementAt(last, src_dir);
                for (uint face = 0; face < FACE_NUM; face++)
                {
                    me = MiddleElementAt(face, src_dir, me);
                    ce = CornerElementAt(face, src_dir, ce);
                }
            }
        }

        // Left->Right axis
        public void RotateX()
        {
            Cube tmp = new Cube();

            uint[] order = { Down, Front, Right, Up, Back, Left };

            foreach(uint direction in Direction.Items())
            {
                uint dir_right = Direction.TurnRight(direction);
                uint dir_left = Direction.TurnLeft(direction);

                // Left -> clockwise
                tmp.MiddleElementAt(Left, dir_right, order[MiddleElementAt(Left, direction)]);
                tmp.CornerElementAt(Left, dir_right, order[CornerElementAt(Left, direction)]);

                // Right -> anticlockwise
                tmp.MiddleElementAt(Right, dir_left, order[MiddleElementAt(Right, direction)]);
                tmp.CornerElementAt(Right, dir_left, order[CornerElementAt(Right, direction)]);

                // Up -> Front
                tmp.MiddleElementAt(Front, dir_right, order[MiddleElementAt(Up, direction)]);
                tmp.CornerElementAt(Front, dir_right, order[CornerElementAt(Up, direction)]);

                // Front -> Down
                tmp.MiddleElementAt(Down, dir_left, order[MiddleElementAt(Front, direction)]);
                tmp.CornerElementAt(Down, dir_left, order[CornerElementAt(Front, direction)]);

                // Down -> Back
                tmp.MiddleElementAt(Back, dir_right, order[MiddleElementAt(Down, direction)]);
                tmp.CornerElementAt(Back, dir_right, order[CornerElementAt(Down, direction)]);

                // Back -> Up
                tmp.MiddleElementAt(Up, dir_left, order[MiddleElementAt(Back, direction)]);
                tmp.CornerElementAt(Up, dir_left, order[CornerElementAt(Back, direction)]);
            }

            MiddleKey = tmp.MiddleKey;
            CornerKey = tmp.CornerKey;
        }

        public void Swap(uint face_a, uint face_b)
        {
            for (int i = 0; i < middle_elements.Length; i++)
            {
                if(middle_elements[i] == face_a)
                {
                    middle_elements[i] = face_b;
                }
                else if(middle_elements[i] == face_b)
                {
                    middle_elements[i] = face_a;
                }
            }
            for (int i = 0; i < corner_elements.Length; i++)
            {
                if (corner_elements[i] == face_a)
                {
                    corner_elements[i] = face_b;
                }
                else if (corner_elements[i] == face_b)
                {
                    corner_elements[i] = face_a;
                }
            }

            for (uint direction = 0; direction < Direction.TURN_COUNT; direction++)
            {
                uint mirror = Direction.TurnAround(direction);

                MiddleElementAt(face_b, direction,
                    MiddleElementAt(face_a, mirror,
                        MiddleElementAt(face_b, direction)));
                CornerElementAt(face_b, direction,
                    CornerElementAt(face_a, mirror,
                        CornerElementAt(face_b, direction)));
            }
        }

        class Elements
        {
            uint[] elements =
            {
                0, 0, 0, 0,
                1, 1, 1, 1,
                2, 2, 2, 2,
                3, 3, 3, 3,
                4, 4, 4, 4,
                5, 5, 5, 5
            };

            public int Length
            {
                get
                {
                    return elements.Length;
                }
            }

            public uint this[uint index]
            {
                get
                {
                    return elements[index];
                }
                set
                {
                    elements[index] = value;
                }
            }

            public ulong Key
            {
                get
                {
                    ulong key = 0;
                    foreach (uint e in elements)
                    {
                        key *= FACE_NUM;
                        key += e;
                    }
                    return key;
                }
                set
                {
                    int i = elements.Length;
                    while (i-- > 0)
                    {
                        elements[i] = (uint)(value % FACE_NUM);
                        value /= FACE_NUM;
                    }
                }
            }
        }
    }
}
