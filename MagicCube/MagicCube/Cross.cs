using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct MiddleCubelet
    {
        uint cubelet;

        public MiddleCubelet(uint cubelet)
        {
            this.cubelet = cubelet;
        }

        public MiddleCubelet(uint face_color, uint head_color)
        {
            cubelet = face_color | (head_color << 4);
        }

        public uint FaceColor
        {
            get
            {
                return cubelet & 0xF;
            }
        }

        public uint HeadColor
        {
            get
            {
                return cubelet >> 4;
            }
        }

        // Conversion from MiddleCubelet to unsigned int
        public static implicit operator uint(MiddleCubelet mc)
        {
            return mc.cubelet;
        }

        //  Conversion from int to MiddleCubelet
        public static implicit operator MiddleCubelet(uint i)
        {
            return new MiddleCubelet(i);
        }
    }

    public struct Cross
    {
        ulong key;

        // Conversion from MiddleCubelet to unsigned int
        public static implicit operator ulong(Cross c)
        {
            return c.key;
        }

        //  Conversion from int to MiddleCubelet
        public static implicit operator Cross(ulong i)
        {
            return new Cross(i);
        }

        public Cross(ulong key)
        {
            this.key = key;
        }

        public const uint
            F = 0,
            U = 1,
            R = 2,
            B = 3,
            D = 4,
            L = 5;
        public const uint FACE_NUM = 6;
        public const uint CUBELET_NUM = FACE_NUM * 2;
        public const int BIT_NUM = 5;
        public const uint BIT_MASK = 0x1F;

        public static readonly MiddleCubelet[] CUBELETS =
        {
            // normal cubelets
            new MiddleCubelet(F, U), new MiddleCubelet(F, D),
            new MiddleCubelet(U, R), new MiddleCubelet(U, L),
            new MiddleCubelet(R, B), new MiddleCubelet(R, F),
            new MiddleCubelet(B, D), new MiddleCubelet(B, U),
            new MiddleCubelet(D, L), new MiddleCubelet(D, R),
            new MiddleCubelet(L, F), new MiddleCubelet(L, B),
            // swapped cubelets
            new MiddleCubelet(U, F), new MiddleCubelet(D, F),
            new MiddleCubelet(R, U), new MiddleCubelet(L, U),
            new MiddleCubelet(B, R), new MiddleCubelet(F, R),
            new MiddleCubelet(D, B), new MiddleCubelet(U, B),
            new MiddleCubelet(L, D), new MiddleCubelet(R, D),
            new MiddleCubelet(F, L), new MiddleCubelet(B, L),
        };

        public static readonly ulong IDENTITY;

        static Cross()
        {
            ulong key = 0;
            for(uint i = 0; i < CUBELET_NUM; i++)
            {
                int shift = (int)(BIT_NUM * i);
                key |= ((ulong)i << shift);
            }
            IDENTITY = key;
        }

        public static string FaceAcronym
        {
            get
            {
                return "FURBDL";
            }
        }

        public static uint UpFace(uint face)
        {
            return (face + U) % FACE_NUM;
        }

        public static uint DownFace(uint face)
        {
            return (face + D) % FACE_NUM;
        }

        public static uint RightFace(uint face)
        {
            return (face + ((face & 1) == 1 ? L : R)) % FACE_NUM;
        }

        public static uint LeftFace(uint face)
        {
            return (face + ((face & 1) == 1 ? R : L)) % FACE_NUM;
        }

        public static uint IsLeftFaceDown(uint face)
        {
            return ((face & 1) == 1) ? 1U : 0U;
        }

        public static uint IsRightFaceDown(uint face)
        {
            return ((face & 1) == 1) ? 0U : 1U;
        }

        public static uint SwappedIndex(uint index)
        {
            return index < CUBELET_NUM ? index + CUBELET_NUM : index - CUBELET_NUM; 
        }

        public uint GetCubeletIndex(uint face, uint down)
        {
            return GetCubeletIndex(face * 2 + down);
        }

        public uint GetCubeletIndex(uint i)
        {
            int shift = (int)(BIT_NUM * i);
            return (uint)(key >> shift) & BIT_MASK;
        }

        public void SetCubeletIndex(uint face, uint down, uint value)
        {
            SetCubeletIndex(face * 2 + down, value);
        }

        public void SetCubeletIndex(uint i, uint value)
        {
            int shift = (int)(BIT_NUM * i);
            key &= ~((ulong)BIT_MASK << shift);
            key |= ((ulong)value << shift);
        }

        public MiddleCubelet CubeletAt(uint face, uint down)
        {
            uint pos = GetCubeletIndex(face, down);
            return CUBELETS[pos];
        }

        public uint MiddleElementAt(uint face, uint direction)
        {
            switch (direction)
            {
                case Direction.UP:    return CubeletAt(face, 0).FaceColor;
                case Direction.DOWN:  return CubeletAt(face, 1).FaceColor;
                case Direction.LEFT:  return CubeletAt(LeftFace(face),   face & 1     ).HeadColor;
                case Direction.RIGHT: return CubeletAt(RightFace(face), (face & 1) ^ 1).HeadColor;
            }
            throw new ArgumentOutOfRangeException("direction");
        }

        public void RotateFace(uint face)
        {
            uint face_type = face & 1;

            uint l_face = LeftFace(face);
            uint r_face = RightFace(face);

            uint u_index = GetCubeletIndex(face, 0);
            uint r_index = GetCubeletIndex(r_face, face_type ^ 1);
            uint d_index = GetCubeletIndex(face, 1);
            uint l_index = GetCubeletIndex(l_face, face_type    );

            // Up -> Right -> Down -> Left ->
            SetCubeletIndex(r_face, face_type ^ 1,
                SwappedIndex(u_index));
            SetCubeletIndex(face, 1,
                SwappedIndex(r_index));
            SetCubeletIndex(l_face, face_type,
                SwappedIndex(d_index));
            SetCubeletIndex(face, 0,
                SwappedIndex(l_index));

            //uint r_index_new = GetCubeletIndex(r_face, face_type ^ 1);
            //uint d_index_new = GetCubeletIndex(face, 1);
            //uint l_index_new = GetCubeletIndex(l_face, face_type);
            //uint u_index_new = GetCubeletIndex(face, 0);

            //Debug.Assert(u_index_new == SwappedIndex(l_index));
            //Debug.Assert(r_index_new == SwappedIndex(u_index));
            //Debug.Assert(d_index_new == SwappedIndex(r_index));
            //Debug.Assert(l_index_new == SwappedIndex(d_index));
        }

        public ulong Transform
        {
            set
            {
                Cross transform = new Cross(value);
                Cross old_cross = new Cross(key);

                for (uint i = 0; i < CUBELET_NUM; i++)
                {
                    uint pos = transform.GetCubeletIndex(i);
                    uint oci = old_cross.GetCubeletIndex(i);

                    if (pos < CUBELET_NUM)
                    {
                        SetCubeletIndex(pos, oci);
                    }
                    else
                    {
                        SetCubeletIndex(pos - CUBELET_NUM, SwappedIndex(oci));
                    }
                }
            }

            get
            {
                Cross result = 0;
                for (uint i = 0; i < CUBELET_NUM; i++)
                {
                    uint pos = GetCubeletIndex(i);
                    if(pos < CUBELET_NUM)
                    {
                        result.SetCubeletIndex(pos, i);
                    }
                    else
                    {
                        result.SetCubeletIndex(SwappedIndex(pos), SwappedIndex(i));
                    }
                }
                return result;
            }
        }

        public uint FindCubletPos(uint cublet_index)
        {
            for (uint i = 0; i < CUBELET_NUM; i++)
            {
                uint pos = GetCubeletIndex(i);

                if (pos == cublet_index)
                {
                    return i;
                }
                else if(pos == SwappedIndex(cublet_index))
                {
                    return SwappedIndex(i);
                }
            }
            throw new ArgumentOutOfRangeException("cublet_index");
        }

        public int CountSolvedMiddles
        {
            get
            {
                int count = 0;
                for (uint i = 0; i < CUBELET_NUM; i++)
                {
                    if(((key >> (int)(BIT_NUM * i)) & BIT_MASK) == i)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

    }
}
