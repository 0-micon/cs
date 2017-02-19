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

        public override string ToString()
        {
            return "" + Faces.Acronym[(int)HeadColor] + Faces.Acronym[(int)FaceColor];
        }
    }

    public struct Cross : Faces.IRotatable, IComparable<Cross>
    {
        Uint5Array _key;

        // Conversion from Cross to unsigned long
        public static implicit operator ulong(Cross c) => c._key;

        //  Conversion from ulong to Cross
        public static implicit operator Cross(ulong i) => new Cross(i);

        // IComparable implementation
        public int CompareTo(Cross other) => _key.CompareTo(other._key);

        public static IEnumerable<ulong> NextKeys(ulong src) => Faces.NextKeys<ulong, Cross>(src, x => x, x => x);

        public Cross(ulong key)
        {
            _key = key;
        }

        public const uint
            F = Faces.Front,
            U = Faces.Up,
            R = Faces.Right,
            B = Faces.Back,
            D = Faces.Down,
            L = Faces.Left;
        public const uint CUBELET_NUM = Faces.Count * 2;

        public const uint
            F0 = 2 * F,
            F1 = 2 * F + 1,
            U0 = 2 * U,
            U1 = 2 * U + 1,
            R0 = 2 * R,
            R1 = 2 * R + 1,
            B0 = 2 * B,
            B1 = 2 * B + 1,
            D0 = 2 * D,
            D1 = 2 * D + 1,
            L0 = 2 * L,
            L1 = 2 * L + 1;

        public static readonly uint[,] CUBELETS =
        {
            // normal cubelets
            {F, U}, {F, D},
            {U, R}, {U, L},
            {R, B}, {R, F},
            {B, D}, {B, U},
            {D, L}, {D, R},
            {L, F}, {L, B},
            // swapped cubelets
            {U, F}, {D, F},
            {R, U}, {L, U},
            {B, R}, {F, R},
            {D, B}, {U, B},
            {L, D}, {R, D},
            {F, L}, {B, L},
        };

        public static readonly uint[,] INDICES;

        public static readonly ulong IDENTITY;
        public static readonly ulong ROTATE_DU; // Down => Up axis rotation
        public static readonly ulong ROTATE_FB; // Front => Back axis rotation
        public static readonly ulong ROTATE_LR; // Left => Right axis rotation

        static Cross()
        {
            Uint5Array key = 0;
            for(uint i = 0; i < CUBELET_NUM; i++)
            {
                key[i] = i;
            }
            IDENTITY = key;

            INDICES = new uint[Faces.Count, Faces.Count];
            for(uint i = 0; i < CUBELETS.GetLength(0); i++)
            {
                INDICES[CUBELETS[i, 0], CUBELETS[i, 1]] = i;
            }

            Uint5Array tmp = 0;

            tmp[F0] = Rotate_180(U0);
            tmp[F1] = Rotate_180(D1);
            tmp[U0] = Rotate_180(B1);
            tmp[U1] = Rotate_180(F0);
            tmp[R0] = Rotate_180(L1);
            tmp[R1] = Rotate_180(R0);
            tmp[B0] = Rotate_180(D0);
            tmp[B1] = Rotate_180(U1);
            tmp[D0] = Rotate_180(F1);
            tmp[D1] = Rotate_180(B0);
            tmp[L0] = Rotate_180(R1);
            tmp[L1] = Rotate_180(L0);

            ROTATE_DU = tmp;

            tmp[F0] = Rotate_180(R1);
            tmp[F1] = Rotate_180(L0);
            tmp[U0] = Rotate_180(D1);
            tmp[U1] = Rotate_180(U0);
            tmp[R0] = Rotate_180(B0);
            tmp[R1] = Rotate_180(F1);
            tmp[B0] = Rotate_180(L1);
            tmp[B1] = Rotate_180(R0);
            tmp[D0] = Rotate_180(U1);
            tmp[D1] = Rotate_180(D0);
            tmp[L0] = Rotate_180(F0);
            tmp[L1] = Rotate_180(B1);

            ROTATE_FB = tmp;

            tmp[F0] = Rotate_180(F1);
            tmp[F1] = Rotate_180(B0);
            tmp[U0] = Rotate_180(R1);
            tmp[U1] = Rotate_180(L0);
            tmp[R0] = Rotate_180(U0);
            tmp[R1] = Rotate_180(D1);
            tmp[B0] = Rotate_180(B1);
            tmp[B1] = Rotate_180(F0);
            tmp[D0] = Rotate_180(L1);
            tmp[D1] = Rotate_180(R0);
            tmp[L0] = Rotate_180(D0);
            tmp[L1] = Rotate_180(U1);

            ROTATE_LR = tmp;
        }

        public static uint IsLeftFaceDown (uint face) => ((face & 1) == 1) ? 1U : 0U;
        public static uint IsRightFaceDown(uint face) => ((face & 1) == 1) ? 0U : 1U;

        public static uint Rotate_180(uint index) => index < CUBELET_NUM ? index + CUBELET_NUM : index - CUBELET_NUM;

        public MiddleCubelet CubeletAt(uint face, uint down)
        {
            uint pos = _key[2 * face + down];
            return new MiddleCubelet(CUBELETS[pos,0], CUBELETS[pos,1]);
        }

        public uint MiddleElementAt(uint face, uint direction)
        {
            switch (direction)
            {
                case Directions.Up:    return CubeletAt(face, 0).FaceColor;
                case Directions.Down:  return CubeletAt(face, 1).FaceColor;
                case Directions.Left:  return CubeletAt(Faces.LeftFace(face),   face & 1     ).HeadColor;
                case Directions.Right: return CubeletAt(Faces.RightFace(face), (face & 1) ^ 1).HeadColor;
            }
            throw new ArgumentOutOfRangeException("direction");
        }

        public void RotateFace(uint face)
        {
            uint face_type = face & 1;

            uint l_face = Faces.LeftFace(face);
            uint r_face = Faces.RightFace(face);

            uint u = 2 * face + 0;
            uint r = 2 * r_face + (face_type ^ 1);
            uint d = 2 * face + 1;
            uint l = 2 * l_face + (face_type);

            uint u_index = _key[u];
            uint r_index = _key[r];
            uint d_index = _key[d];
            uint l_index = _key[l];

            // Up -> Right -> Down -> Left ->
            _key[r] = Rotate_180(u_index);
            _key[d] = Rotate_180(r_index);
            _key[l] = Rotate_180(d_index);
            _key[u] = Rotate_180(l_index);
        }

        public void Rotate(ulong transform, uint[] face_orientation)
        {
            Transform = transform;

            for (uint i = 0; i < CUBELET_NUM; i++)
            {
                uint src_idx = _key[i];
                uint dst_idx = INDICES[
                    face_orientation[CUBELETS[src_idx, 0]],
                    face_orientation[CUBELETS[src_idx, 1]]];

                _key[i] = dst_idx;
            }
        }

        public void RotateLR()
        {
            // FURBDL => DFRUBL
            uint[] orientation = { D, F, R, U, B, L };
            Rotate(ROTATE_LR, orientation);
        }

        public void RotateFB()
        {
            // FURBDL => FRDBLU
            uint[] orientation = { F, R, D, B, L, U };
            Rotate(ROTATE_FB, orientation);
        }

        public void RotateDU()
        {
            // FURBDL => RUBLDF
            uint[] orientation = { R, U, B, L, D, F };
            Rotate(ROTATE_DU, orientation);
        }

        public ulong Transform
        {
            set
            {
                Uint5Array transform = value;
                Uint5Array old_cross = _key;

                for (uint i = 0; i < CUBELET_NUM; i++)
                {
                    uint pos = transform[i];
                    uint oci = old_cross[i];

                    if (pos < CUBELET_NUM)
                    {
                        _key[pos] = oci;
                    }
                    else
                    {
                        _key[pos - CUBELET_NUM] = Rotate_180(oci);
                    }
                }
            }

            get
            {
                Uint5Array transform = 0;
                for (uint i = 0; i < CUBELET_NUM; i++)
                {
                    uint pos = _key[i];
                    if (pos < CUBELET_NUM)
                    {
                        transform[pos] = i;
                    }
                    else
                    {
                        transform[pos - CUBELET_NUM] = Rotate_180(i);
                    }
                }
                return transform;
            }
        }

        /*
        public ulong MinKey
        {
            get
            {
                ulong min_key = key;

                Cross tmp = key;

                for(int x = 0; x < 4; x++)
                {
                    tmp.RotateLR();
                    if(tmp < min_key)
                    {
                        min_key = tmp;
                    }
                    for (int y = 0; y < 4; y++)
                    {
                        tmp.RotateDU();
                        if (tmp < min_key)
                        {
                            min_key = tmp;
                        }
                        for (int z = 0; z < 4; z++)
                        {
                            tmp.RotateFB();
                            if (tmp < min_key)
                            {
                                min_key = tmp;
                            }
                        }
                    }
                }

                return min_key;
            }
        }
        */

/*//
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
//*/

        public int CountSolvedCubelets
        {
            get
            {
                int count = 0;
                for (uint i = 0; i < CUBELET_NUM; i++)
                {
                    if (_key[i] == i)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }
}
