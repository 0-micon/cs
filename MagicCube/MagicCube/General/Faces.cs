using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class Faces
    {
        public const uint
            Front   = 0,
            Up      = 1,
            Right   = 2,
            Back    = 3,
            Down    = 4,
            Left    = 5,
            Count   = 6;

        public static string Acronym => "FURBDL";

        public static uint[][] Orientations => _orientations;

        public static uint UpFace   (uint face) => (face + Up  ) % Count;
        public static uint DownFace (uint face) => (face + Down) % Count;
        public static uint RightFace(uint face) => (face + ((face & 1) == 1 ? Left  : Right)) % Count;
        public static uint LeftFace (uint face) => (face + ((face & 1) == 1 ? Right : Left )) % Count;

        public static uint Neighbour(uint face, uint direction)
        {
            switch (direction)
            {
                case Directions.Up: return UpFace(face);
                case Directions.Right: return RightFace(face);
                case Directions.Down: return DownFace(face);
                case Directions.Left: return LeftFace(face);
            }

            throw new ArgumentOutOfRangeException("direction");
        }

        static readonly uint[][] _orientations;

        static Faces()
        {
            HashSet<string> set = new HashSet<string>();
            var rot = new Rotator();
            set.Add(rot.ToString());
            foreach (uint dir_x in Directions.All())
            {
                rot.RotateClockwise(Down);
                set.Add(rot.ToString());
                foreach (uint dir_y in Directions.All())
                {
                    rot.RotateClockwise(Left);
                    set.Add(rot.ToString());
                    foreach (uint dir_z in Directions.All())
                    {
                        rot.RotateClockwise(Front);
                        set.Add(rot.ToString());
                    }
                }
            }

            uint[][] ori = new uint[set.Count][];
            uint row = 0;
            foreach (string s in set)
            {
                rot.FromString(s);
                ori[row] = (uint[])rot.Transform.Clone();
                row++;
            }
            _orientations = ori;
        }

        public interface IRotatable
        {
            void RotateFace(uint face);
        }

        public static IEnumerable<T> NextCubes<T>(T cube) where T : IRotatable
        {
            for (uint face = 0; face < Count; face++)
            {
                cube.RotateFace(face);
                yield return cube;

                cube.RotateFace(face);
                yield return cube;

                cube.RotateFace(face);
                yield return cube;

                cube.RotateFace(face); // restore
            }
        }

        public static IEnumerable<KeyValuePair<Move, T>> NextMoves<T>(T cube) where T : IRotatable
        {
            foreach(var pair in Utils.Index(NextCubes(cube)))
            {
                yield return new KeyValuePair<Move, T>(new Move((uint)pair.Key), pair.Value);
            }
        }

        public static IEnumerable<K> NextKeys<K, T>(K key, Func<K, T> KtoT, Func<T, K> TtoK) where T : IRotatable
        {
            T cube = KtoT(key);
            foreach (T dst in NextCubes(cube))
            {
                yield return TtoK(dst);
            }
        }

        public static K RandomKey<K, T>(K key, Func<K, T> KtoT, Func<T, K> TtoK) where T : IRotatable
        {
            T cube = KtoT(key);

            Random rnd = new Random();
            int count = rnd.Next(100, 1000);
            while(--count > 0)
            {
                uint face = (uint)rnd.Next((int)Count);
                cube.RotateFace(face);

            }
            return TtoK(cube);
        }
    }

    public class Rotator
    {
        uint[] faces = new uint[Faces.Count];

        public uint[] Transform => faces;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (uint face in faces)
            {
                sb.Append(Faces.Acronym[(int)face]);
            }
            return sb.ToString();
        }

        public void FromString(string src)
        {
            uint count = 0;
            foreach (char ch in src)
            {
                int face = Faces.Acronym.IndexOf(ch);
                if (face >= 0)
                {
                    if (count < Faces.Count)
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
            for (uint face = 0; face < Faces.Count; face++)
            {
                faces[face] = face;
            }
        }

        public void RotateClockwise(uint face)
        {
            // 0,1,2,3 => 3,0,1,2
            uint direction = Directions.Count - 1;
            uint dst_pos = Faces.Neighbour(face, direction);
            uint last = faces[dst_pos];
            while (direction-- > 0)
            {
                uint src_pos = Faces.Neighbour(face, direction);
                faces[dst_pos] = faces[src_pos];
                dst_pos = src_pos;
            }
            faces[dst_pos] = last;
        }
    }
}
