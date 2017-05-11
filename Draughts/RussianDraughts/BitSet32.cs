using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    public struct BitSet32 : IComparable<BitSet32>
    {
        uint _key;
        public const uint Capacity = sizeof(uint) * 8;

        // Conversion from BitSet32 to unsigned int
        public static implicit operator uint(BitSet32 c) => c._key;

        // Conversion from uint to BitSet32
        public static implicit operator BitSet32(uint i) => new BitSet32(i);

        // IComparable implementation
        public int CompareTo(BitSet32 other) => _key.CompareTo(other._key);

        public bool Any => _key != 0;

        public BitSet32(uint key)
        {
            _key = key;
        }

        public uint this[int i]
        {
            get
            {
                return (_key >> i) & 1;
            }
            set
            {
                if (value == 0)
                {
                    _key &= ~(1U << i);
                }
                else
                {
                    _key |= (1U << i);
                }
            }
        }

        public bool Test(int pos)
        {
            return this[pos] == 1;
        }
    }
}
