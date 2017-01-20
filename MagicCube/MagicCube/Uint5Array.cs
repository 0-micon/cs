using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct Uint5Array
    {
        ulong _key;

        public const uint BitNum  = 5;
        public const uint BitMask = 0x1F;
        public const uint Capacity = sizeof(ulong) * 8 / BitNum;

        // Conversion from Uint5Array to unsigned long
        public static implicit operator ulong(Uint5Array c) => c._key;

        //  Conversion from ulong to Uint5Array
        public static implicit operator Uint5Array(ulong i) => new Uint5Array(i);

        public Uint5Array(ulong key)
        {
            _key = key;
        }

        public uint this[uint i]
        {
            get
            {
                int shift = (int)(BitNum * i);
                return (uint)(_key >> shift) & BitMask;
            }
            set
            {
                int shift = (int)(BitNum * i);
                _key &= ~((ulong)BitMask << shift);
                _key |=  ((ulong)value   << shift);
            }
        }

        public uint this[int i]
        {
            get
            {
                return this[(uint)i];
            }
            set
            {
                this[(uint)i] = value;
            }
        }
    }
}
