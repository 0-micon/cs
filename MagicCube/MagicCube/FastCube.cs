﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public struct FastCube : Faces.IRotatable
    {
        Saltire _corners;
        Cross _middles;

        // Conversion from FastCube to CubeKey
        public static implicit operator CubeKey(FastCube c) => new CubeKey(c._corners, c._middles);

        //  Conversion from CubeKey to FastCube
        public static implicit operator FastCube(CubeKey k) => new FastCube(k.corners, k.middles);

        public static FastCube Identity => new FastCube(Saltire.IDENTITY, Cross.IDENTITY);

        public FastCube(Saltire corners, Cross middles)
        {
            _corners = corners;
            _middles = middles;
        }

        public Saltire Corners => _corners;
        public Cross   Middles => _middles;

        public static IEnumerable<CubeKey> NextKeys(CubeKey src) => Faces.NextKeys<CubeKey, FastCube>(src, x => x, x => x);

        public void RotateFace(uint face)
        {
            _corners.RotateFace(face);
            _middles.RotateFace(face);
        }
    }
}
