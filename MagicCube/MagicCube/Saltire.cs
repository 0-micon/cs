﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct CornerCubelet
    {
        uint _cubelet;

        public CornerCubelet(uint cubelet)
        {
            _cubelet = cubelet;
        }

        public CornerCubelet(uint face_color, uint head_color, uint side_color)
        {
            _cubelet = (face_color | (head_color << 4) | (side_color << 8));
        }

        public uint FaceColor => _cubelet & 0xF;
        public uint HeadColor => (_cubelet >> 4) & 0xF;
        public uint SideColor => (_cubelet >> 8) & 0xF;

        // Conversion from CornerCubelet to unsigned int
        public static implicit operator uint(CornerCubelet cc) => cc._cubelet;

        //  Conversion from uint to CornerCubelet
        public static implicit operator CornerCubelet(uint i) => new CornerCubelet(i);

        public override string ToString()
        {
            return "" + Faces.Acronym[(int)HeadColor] + Faces.Acronym[(int)FaceColor] + Faces.Acronym[(int)SideColor];
        }
    }

    // X-shaped cross
    public struct Saltire : Faces.IRotatable
    {
        Uint5Array _key;

        // Conversion from Saltire to unsigned long
        public static implicit operator ulong(Saltire c) => c._key;

        //  Conversion from ulong to Saltire
        public static implicit operator Saltire(ulong i) => new Saltire(i);

        public Saltire(ulong key)
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
        public const uint CUBELET_NUM = 8;

        public static readonly uint[,] CUBELETS =
        {
            // normal cubelets
            {F, U, R}, {F, R, D}, {F, D, L}, {F, L, U},
            {B, D, R}, {B, R, U}, {B, U, L}, {B, L, D},
            // shifted cubelets
            {R, F, U}, {D, F, R}, {L, F, D}, {U, F, L},
            {R, B, D}, {U, B, R}, {L, B, U}, {D, B, L},
            // double-shifted cubelets
            {U, R, F}, {R, D, F}, {D, L, F}, {L, U, F},
            {D, R, B}, {R, U, B}, {U, L, B}, {L, D, B},
        };

        public static readonly ulong IDENTITY;

        static Saltire()
        {
            Uint5Array key = 0;
            for (uint i = 0; i < CUBELET_NUM; i++)
            {
                key[i] = i;
            }
            IDENTITY = key;
        }

        public CornerCubelet this[uint i]
        {
            get
            {
                uint idx = _key[i];
                return new CornerCubelet(CUBELETS[idx, 0], CUBELETS[idx, 1], CUBELETS[idx, 2]);
            }
        }

        public CornerCubelet this[int i] => this[(uint)i];

        public uint CornerElementAt(uint face, uint direction)
        {
            switch (face)
            {
                case F: return this[direction].FaceColor;
                case B: return this[direction + 4].FaceColor;
                case U:
                    switch (direction)
                    {
                        case Direction.UP:    return this[0].HeadColor;
                        case Direction.RIGHT: return this[3].SideColor;
                        case Direction.DOWN:  return this[6].HeadColor;
                        case Direction.LEFT:  return this[5].SideColor;
                    }
                    break;
                case D:
                    switch (direction)
                    {
                        case Direction.UP:    return this[2].HeadColor;
                        case Direction.RIGHT: return this[1].SideColor;
                        case Direction.DOWN:  return this[4].HeadColor;
                        case Direction.LEFT:  return this[7].SideColor;
                    }
                    break;
                case R:
                    switch (direction)
                    {
                        case Direction.UP:    return this[4].SideColor;
                        case Direction.RIGHT: return this[1].HeadColor;
                        case Direction.DOWN:  return this[0].SideColor;
                        case Direction.LEFT:  return this[5].HeadColor;
                    }
                    break;
                case L:
                    switch (direction)
                    {
                        case Direction.UP:    return this[2].SideColor;
                        case Direction.RIGHT: return this[7].HeadColor;
                        case Direction.DOWN:  return this[6].SideColor;
                        case Direction.LEFT:  return this[3].HeadColor;
                    }
                    break;
            }
            throw new ArgumentOutOfRangeException("direction");
        }

        public static uint Rotate_120(uint index) => (index + CUBELET_NUM              ) % (CUBELET_NUM * 3);
        public static uint Rotate_240(uint index) => (index + CUBELET_NUM + CUBELET_NUM) % (CUBELET_NUM * 3);

        public void RotateFace(uint face)
        {
            switch (face)
            {
                case F:
                    {
                        // 0,1,2,3 => 3,0,1,2
                        uint idx = _key[3];
                        _key[3]  = _key[2];
                        _key[2]  = _key[1];
                        _key[1]  = _key[0];
                        _key[0]  =  idx;
                    }
                    break;
                case B:
                    {
                        // 4,5,6,7 => 7,4,5,6
                        uint idx = _key[7];
                        _key[7]  = _key[6];
                        _key[6]  = _key[5];
                        _key[5]  = _key[4];
                        _key[4]  =  idx;
                    }
                    break;
                case U:
                    {
                        // 0,3,6,5 => 5'',0',3'',6'
                        uint idx = _key[0];
                        _key[0]  = Rotate_240(_key[5]);
                        _key[5]  = Rotate_120(_key[6]);
                        _key[6]  = Rotate_240(_key[3]);
                        _key[3]  = Rotate_120( idx   );
                    }
                    break;
                case D:
                    {
                        // 2,1,4,7 => 7,2,1,4
                        uint idx = _key[2];
                        _key[2]  = Rotate_240(_key[7]);
                        _key[7]  = Rotate_120(_key[4]);
                        _key[4]  = Rotate_240(_key[1]);
                        _key[1]  = Rotate_120( idx   );
                    }
                    break;
                case R:
                    {
                        // 4,1,0,5 => 5',4'',1',0''
                        uint idx = _key[4];
                        _key[4]  = Rotate_120(_key[5]);
                        _key[5]  = Rotate_240(_key[0]);
                        _key[0]  = Rotate_120(_key[1]);
                        _key[1]  = Rotate_240( idx   );
                    }
                    break;
                case L:
                    {
                        // 2,7,6,3 => 3',2'',7',6''
                        uint idx = _key[2];
                        _key[2]  = Rotate_120(_key[3]);
                        _key[3]  = Rotate_240(_key[6]);
                        _key[6]  = Rotate_120(_key[7]);
                        _key[7]  = Rotate_240( idx   );
                    }
                    break;
            }
        }

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
