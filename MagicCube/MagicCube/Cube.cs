using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace MagicCube
{
    public class MiddleElement
    {
        uint face1, face2;
        uint color1, color2;

        public override string ToString()
        {
            return string.Format(
                "{0}({1}:{2},{3}:{4})",
                GetType().Name,
                FaceInfo.items[face1].name,
                FaceInfo.items[color1].color,
                FaceInfo.items[face2].name,
                FaceInfo.items[color2].color
            );
        }

        public MiddleElement(uint face1, uint color1, uint face2, uint color2)
        {
            if(face1 <= face2)
            {
                this.face1 = face1;
                this.face2 = face2;
                this.color1 = color1;
                this.color2 = color2;
            }
            else
            {
                this.face1 = face2;
                this.face2 = face1;
                this.color1 = color2;
                this.color2 = color1;
            }
        }

        public uint Face1
        {
            get
            {
                return face1;
            }
        }

        public uint Face2
        {
            get
            {
                return face2;
            }
        }

        public uint Color1
        {
            get
            {
                return color1;
            }
        }

        public uint Color2
        {
            get
            {
                return color2;
            }
        }
    }

    public class Cube
    {
        public const uint FACE_NUM = 6;
        public const uint
            Front = 0,
            Up = 1,
            Right = 2,
            Back = 3,
            Down = 4,
            Left = 5;
        public static string FaceAcronym
        {
            get
            {
                return "FURBDL";
            }
        }

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
                case Direction.UP: return UpFace(face);
                case Direction.RIGHT: return RightFace(face);
                case Direction.DOWN: return DownFace(face);
                case Direction.LEFT: return LeftFace(face);
            }

            throw new ArgumentOutOfRangeException("direction");
        }

        static readonly uint[,] LAYOUT;
        static readonly uint[,] NEIGHBOUR_POS;
        public static readonly uint[][] ORIENTATION;

        static Cube()
        {
            uint[,] lt = new uint[FACE_NUM, Direction.TURN_COUNT];
            for (uint face = 0; face < FACE_NUM; face++)
            {
                for (uint direction = 0; direction < Direction.TURN_COUNT; direction++)
                {
                    lt[face, direction] = Neighbour(face, direction);
                }
            }
            LAYOUT = lt;

            uint[,] np = new uint[FACE_NUM, FACE_NUM];
            for (uint face = 0; face < FACE_NUM; face++)
            {
                foreach (uint direction in Direction.Items())
                {
                    uint neighbour = LAYOUT[face, direction];
                    np[face, neighbour] = direction;
                }
            }
            NEIGHBOUR_POS = np;

            HashSet<string> set = new HashSet<string>();
            var rot = new Cube.Rotator();
            set.Add(rot.ToString());
            foreach (uint dir_x in Direction.Items())
            {
                rot.RotateClockwise(Cube.Down);
                set.Add(rot.ToString());
                foreach (uint dir_y in Direction.Items())
                {
                    rot.RotateClockwise(Cube.Left);
                    set.Add(rot.ToString());
                    foreach (uint dir_z in Direction.Items())
                    {
                        rot.RotateClockwise(Cube.Front);
                        set.Add(rot.ToString());
                    }
                }
            }

            uint[][] ori = new uint[set.Count][];
            uint row = 0;
            foreach(string s in set)
            {
                rot.FromString(s);
                ori[row] = (uint[])rot.Faces.Clone();
                row++;
            }
            ORIENTATION = ori;
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

        public Cube()
        {
        }

        public Cube(CubeKey key)
        {
            MiddleKey = key.middles;
            CornerKey = key.corners;
        }

        public class Rotator
        {
            uint[] faces = new uint[Cube.FACE_NUM];

            public uint[] Faces
            {
                get
                {
                    return faces;
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach(uint face in faces)
                {
                    sb.Append(Cube.FaceAcronym[(int)face]);
                }
                return sb.ToString();
            }

            public void FromString(string src)
            {
                uint count = 0;
                foreach(char ch in src)
                {
                    int face = Cube.FaceAcronym.IndexOf(ch);
                    if(face >= 0)
                    {
                        if(count < Cube.FACE_NUM)
                        {
                            faces[count] = (uint)face;
                            count++;
                        }
                    }
                }
            }

            public Rotator()
            {
                Identity();
            }

            public void Identity()
            {
                for (uint face = 0; face < Cube.FACE_NUM; face++)
                {
                    faces[face] = face;
                }
            }

            public void RotateClockwise(uint face)
            {
                // 0,1,2,3 => 3,0,1,2
                uint direction = Direction.TURN_COUNT - 1;
                uint dst_pos = Cube.Neighbour(face, direction);
                uint last = faces[dst_pos];
                while(direction-- > 0)
                {
                    uint src_pos = Cube.Neighbour(face, direction);
                    faces[dst_pos] = faces[src_pos];
                    dst_pos = src_pos;
                }
                faces[dst_pos] = last;
            }
        }

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

            public static Move KeysToMove(CubeKey src_key, CubeKey dst_key)
            {
                Cube c = new Cube(src_key);

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

        public CubeKey Key
        {
            get
            {
                return new CubeKey(this);
            }
            set
            {
                MiddleKey = value.middles;
                CornerKey = value.corners;
            }
        }

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

        public IEnumerable<MiddleElement> MiddleElements()
        {
            for(uint face1 = 0; face1 < FACE_NUM; face1++)
            {
                uint direction = Direction.UP;
                uint color1 = MiddleElementAt(face1, direction);
                uint face2 = LAYOUT[face1, direction];
                uint color2 = NeighbourMiddleElementAt(face1, direction);

                yield return new MiddleElement(face1, color1, face2, color2);

                direction = Direction.DOWN;
                color1 = MiddleElementAt(face1, direction);
                face2 = LAYOUT[face1, direction];
                color2 = NeighbourMiddleElementAt(face1, direction);

                yield return new MiddleElement(face1, color1, face2, color2);
            }
        }

        public IEnumerable<MiddleElement> MiddleDiff(Cube other)
        {
            foreach(MiddleElement me in MiddleElements())
            {
                uint direction = NEIGHBOUR_POS[me.Face1, me.Face2];
                if (me.Color1 != other.MiddleElementAt(me.Face1, direction) ||
                    me.Color2 != other.NeighbourMiddleElementAt(me.Face1, direction))
                {
                    yield return me;
                }
            }
        }

        public int CountSolvedMiddles
        {
            get
            {
                int count = 0;
                for (uint face1 = 0; face1 < FACE_NUM; face1++)
                {
                    uint direction = Direction.UP;
                    if (face1 == MiddleElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourMiddleElementAt(face1, direction))
                    {
                        count++;
                    }

                    direction = Direction.DOWN;
                    if (face1 == MiddleElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourMiddleElementAt(face1, direction))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public int CountSolvedCorners
        {
            get
            {
        //        public uint SecondNeighbourCornerElementAt(uint face, uint direction)
        //{
        //    uint neighbour = LAYOUT[face, direction];
        //    uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
        //    return CornerElementAt(neighbour, Direction.TurnLeft(neighbour_direction));
        //}

                int count = 0;
                for (uint face1 = 0; face1 < FACE_NUM; face1++)
                {
                    uint direction = Direction.UP;

                    uint ce = CornerElementAt(face1, direction);
                    uint ne = NeighbourCornerElementAt(face1, direction);
                    uint sn = SecondNeighbourCornerElementAt(face1, direction);

                    if (face1 == CornerElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourCornerElementAt(face1, direction) &&
                        LAYOUT[face1, Direction.TurnLeft(direction)] == SecondNeighbourCornerElementAt(face1, direction))
                    {
                        count++;
                    }

                    direction = Direction.DOWN;
                    if (face1 == CornerElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourCornerElementAt(face1, direction) &&
                        LAYOUT[face1, Direction.TurnLeft(direction)] == SecondNeighbourCornerElementAt(face1, direction))
                    {
                        count++;
                    }
                }
                return count;
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
