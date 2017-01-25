using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

//using static MagicCube.Faces;

namespace MagicCube
{
    public class Cube : Faces.IRotatable
    {
        static readonly uint[,] LAYOUT;
        static readonly uint[,] NEIGHBOUR_POS;

        static Cube()
        {
            uint[,] lt = new uint[Faces.Count, Directions.Count];
            for (uint face = 0; face < Faces.Count; face++)
            {
                for (uint direction = 0; direction < Directions.Count; direction++)
                {
                    lt[face, direction] = Faces.Neighbour(face, direction);
                }
            }
            LAYOUT = lt;

            uint[,] np = new uint[Faces.Count, Faces.Count];
            for (uint face = 0; face < Faces.Count; face++)
            {
                foreach (uint direction in Directions.All())
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

        public Cube()
        {
        }

        public Cube(CubeKey key)
        {
            MiddleKey = key.middles;
            CornerKey = key.corners;
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
                    return move % Faces.Count;
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
                    return (move / Faces.Count) % Directions.Count;
                }
                set
                {
                    move = Face + Faces.Count * (value % Directions.Count);
                }
            }

            public override string ToString()
            {
                return $"{Faces.Acronym[(int)Face]} , {Turn} time(s)";
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
            for (uint i = 0; i < Faces.Count; i++)
            {
                uint start = (i + shift) % Faces.Count;
                for(uint j = 0; j < Directions.Count; j++)
                {
                    uint me = middle_elements[start * Directions.Count + j];
                    key *= Faces.Count;
                    key += (Faces.Count + me - shift) % Faces.Count;
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
                    key *= Faces.Count;
                    key += me;
                }
                return key;
            }
            set
            {
                int i = middle_elements.Length;
                while(i-- > 0)
                {
                    middle_elements[i] = (uint)(value % Faces.Count);
                    value /= Faces.Count;
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
                    key *= Faces.Count;
                    key += ce;
                }
                return key;
            }
            set
            {
                int i = corner_elements.Length;
                while (i-- > 0)
                {
                    corner_elements[i] = (uint)(value % Faces.Count);
                    value /= Faces.Count;
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
            foreach(uint direction in Directions.All())
            {
                key *= Faces.Count;
                key += order[CornerElementAt(face, direction)];
            }
            // neighbours
            foreach (uint direction in Directions.All())
            {
                uint nface = LAYOUT[face, direction];
                foreach(uint ndirection in Directions.All())
                {
                    key *= Faces.Count;
                    key += order[CornerElementAt(nface, ndirection)];
                }
            }
            // back
            face = (face + 3) % Faces.Count;
            foreach (uint direction in Directions.All())
            {
                key *= Faces.Count;
                key += order[CornerElementAt(face, direction)];
            }

            return key;
        }

        public IEnumerable<uint> Moves()
        {
            uint move = 0;
            for (uint face = 0; face < Faces.Count; face++)
            {
                RotateFace(face);
                yield return move++;

                RotateFace(face);
                yield return move++;

                RotateFace(face);
                yield return move++;

                RotateFace(face); // restore
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
            for (uint face = 0; face < Faces.Count; face++)
            {
                RotateFace(face);
                yield return CornerKey;

                RotateFace(face);
                yield return CornerKey;

                RotateFace(face);
                yield return CornerKey;

                RotateFace(face); // restore
            }
        }

        public static IEnumerable<ulong> NextCornerKeys(ulong corner_key)
        {
            Cube c = new Cube();
            c.CornerKey = corner_key;
            return c.NextCornerKeys();
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
            return CornerElementAt(neighbour, Directions.TurnLeft(neighbour_direction));
        }

        public uint SecondNeighbourCornerElementAt(uint face, uint direction, uint value)
        {
            uint neighbour = LAYOUT[face, direction];
            uint neighbour_direction = NEIGHBOUR_POS[neighbour, face];
            return CornerElementAt(neighbour, Directions.TurnLeft(neighbour_direction), value);
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
            return face * Directions.Count;
        }

        public void RotateFace(uint face)
        {
            // Rotate clockwise: 0,1,2,3 => 3,0,1,2
            const uint last = Directions.Count - 1;

            // self
            uint me = MiddleElementAt(face, last);
            uint ce = CornerElementAt(face, last);

            // neighbours
            uint nme = NeighbourMiddleElementAt(face, last);
            uint nce = NeighbourCornerElementAt(face, last);
            uint snc = SecondNeighbourCornerElementAt(face, last);

            foreach (uint i in Directions.All())
            {
                me = MiddleElementAt(face, i, me);
                ce = CornerElementAt(face, i, ce);

                nme = NeighbourMiddleElementAt(face, i, nme);
                nce = NeighbourCornerElementAt(face, i, nce);
                snc = SecondNeighbourCornerElementAt(face, i, snc);
            }
        }

        public int CountSolvedMiddles
        {
            get
            {
                int count = 0;
                for (uint face1 = 0; face1 < Faces.Count; face1++)
                {
                    uint direction = Directions.Up;
                    if (face1 == MiddleElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourMiddleElementAt(face1, direction))
                    {
                        count++;
                    }

                    direction = Directions.Down;
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
                for (uint face1 = 0; face1 < Faces.Count; face1++)
                {
                    uint direction = Directions.Up;

                    uint ce = CornerElementAt(face1, direction);
                    uint ne = NeighbourCornerElementAt(face1, direction);
                    uint sn = SecondNeighbourCornerElementAt(face1, direction);

                    if (face1 == CornerElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourCornerElementAt(face1, direction) &&
                        LAYOUT[face1, Directions.TurnLeft(direction)] == SecondNeighbourCornerElementAt(face1, direction))
                    {
                        count++;
                    }

                    direction = Directions.Down;
                    if (face1 == CornerElementAt(face1, direction) &&
                        LAYOUT[face1, direction] == NeighbourCornerElementAt(face1, direction) &&
                        LAYOUT[face1, Directions.TurnLeft(direction)] == SecondNeighbourCornerElementAt(face1, direction))
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

            dict[Faces.Left] = Faces.Front;
            dict[Faces.Front] = Faces.Up;
            dict[Faces.Up] = Faces.Left;
            dict[Faces.Right] = Faces.Back;
            dict[Faces.Back] = Faces.Down;
            dict[Faces.Down] = Faces.Right;

            Cube c = new Cube();

            foreach (uint d in Directions.All())
            {
                uint u = Directions.TurnAround(d);

                c.MiddleElementAt(Faces.Front, d, dict[MiddleElementAt(Faces.Left, d)]);
                c.MiddleElementAt(Faces.Up, u, dict[MiddleElementAt(Faces.Front, d)]);
                c.MiddleElementAt(Faces.Left, u, dict[MiddleElementAt(Faces.Up, d)]);

                c.MiddleElementAt(Faces.Back, d, dict[MiddleElementAt(Faces.Right, d)]);
                c.MiddleElementAt(Faces.Down, u, dict[MiddleElementAt(Faces.Back, d)]);
                c.MiddleElementAt(Faces.Right, u, dict[MiddleElementAt(Faces.Down, d)]);

                c.CornerElementAt(Faces.Front, d, dict[CornerElementAt(Faces.Left, d)]);
                c.CornerElementAt(Faces.Up, u, dict[CornerElementAt(Faces.Front, d)]);
                c.CornerElementAt(Faces.Left, u, dict[CornerElementAt(Faces.Up, d)]);

                c.CornerElementAt(Faces.Back, d, dict[CornerElementAt(Faces.Right, d)]);
                c.CornerElementAt(Faces.Down, u, dict[CornerElementAt(Faces.Back, d)]);
                c.CornerElementAt(Faces.Right, u, dict[CornerElementAt(Faces.Down, d)]);
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
                middle_elements[i] = (middle_elements[i] + 2) % Faces.Count;
            }
            for (int i = 0; i < corner_elements.Length; i++)
            {
                corner_elements[i] = (corner_elements[i] + 2) % Faces.Count;
            }

            const uint last = Faces.Count - 1;
            for (uint src_dir = 0; src_dir < Directions.Count; src_dir++)
            {
                uint me = MiddleElementAt(last, src_dir);
                uint ce = CornerElementAt(last, src_dir);
                for (uint face = 0; face < Faces.Count; face++)
                {
                    me = MiddleElementAt(face, src_dir, me);
                    ce = CornerElementAt(face, src_dir, ce);
                }
            }
            for (uint src_dir = 0; src_dir < Directions.Count; src_dir++)
            {
                uint me = MiddleElementAt(last, src_dir);
                uint ce = CornerElementAt(last, src_dir);
                for (uint face = 0; face < Faces.Count; face++)
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

            uint[] order = { Faces.Down, Faces.Front, Faces.Right, Faces.Up, Faces.Back, Faces.Left };

            foreach(uint direction in Directions.All())
            {
                uint dir_right = Directions.TurnRight(direction);
                uint dir_left = Directions.TurnLeft(direction);

                // Left -> clockwise
                tmp.MiddleElementAt(Faces.Left, dir_right, order[MiddleElementAt(Faces.Left, direction)]);
                tmp.CornerElementAt(Faces.Left, dir_right, order[CornerElementAt(Faces.Left, direction)]);

                // Right -> anticlockwise
                tmp.MiddleElementAt(Faces.Right, dir_left, order[MiddleElementAt(Faces.Right, direction)]);
                tmp.CornerElementAt(Faces.Right, dir_left, order[CornerElementAt(Faces.Right, direction)]);

                // Up -> Front
                tmp.MiddleElementAt(Faces.Front, dir_right, order[MiddleElementAt(Faces.Up, direction)]);
                tmp.CornerElementAt(Faces.Front, dir_right, order[CornerElementAt(Faces.Up, direction)]);

                // Front -> Down
                tmp.MiddleElementAt(Faces.Down, dir_left, order[MiddleElementAt(Faces.Front, direction)]);
                tmp.CornerElementAt(Faces.Down, dir_left, order[CornerElementAt(Faces.Front, direction)]);

                // Down -> Back
                tmp.MiddleElementAt(Faces.Back, dir_right, order[MiddleElementAt(Faces.Down, direction)]);
                tmp.CornerElementAt(Faces.Back, dir_right, order[CornerElementAt(Faces.Down, direction)]);

                // Back -> Up
                tmp.MiddleElementAt(Faces.Up, dir_left, order[MiddleElementAt(Faces.Back, direction)]);
                tmp.CornerElementAt(Faces.Up, dir_left, order[CornerElementAt(Faces.Back, direction)]);
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

            for (uint direction = 0; direction < Directions.Count; direction++)
            {
                uint mirror = Directions.TurnAround(direction);

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
                        key *= Faces.Count;
                        key += e;
                    }
                    return key;
                }
                set
                {
                    int i = elements.Length;
                    while (i-- > 0)
                    {
                        elements[i] = (uint)(value % Faces.Count);
                        value /= Faces.Count;
                    }
                }
            }
        }
    }
}
