using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct CubeKey : IComparable<CubeKey>
    {
        public ulong corners;
        public ulong middles;

        public CubeKey(ulong corners, ulong middles)
        {
            this.corners = corners;
            this.middles = middles;
        }

        public CubeKey(Cube cube) : this(cube.CornerKey, cube.MiddleKey)
        {
        }

        public bool Equals(CubeKey other)
        {
            return (corners == other.corners) && (middles == other.middles);
        }

        // The == and != operators cannot operate on a struct unless the struct explicitly overloads them.
        public static bool operator ==(CubeKey a, CubeKey b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CubeKey a, CubeKey b)
        {
            return !(a.Equals(b));
        }

        public override bool Equals(object obj)
        {
            if (obj is CubeKey)
            {
                return Equals((CubeKey)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)((corners ^ middles) / 99991);
        }

        public int CompareTo(CubeKey other)
        {
            if (corners == other.corners)
            {
                if (middles == other.middles)
                {
                    return 0;
                }
                else
                {
                    return (middles < other.middles) ? -1 : 1;
                }
            }
            else
            {
                return (corners < other.corners) ? -1 : 1;
            }
        }
    }
}
