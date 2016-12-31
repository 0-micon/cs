using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicCube
{
    public class MoveTrack
    {
        public List<Move> moves = new List<Move>();

        public int Count
        {
            get
            {
                return moves.Count;
            }
        }

        // Notation (known as "Singmaster notation"):
        // 1) the first letters from the names of the sides (F, U, R...) is used as a move name;
        // 2) 2 for a double face move (FF is equal to F2);
        // 3) 3 or ' for a triple face move (FFF is equal to F3 or F');
        // Example:
        // F R2 L' -> Turn Front Face clockwise, Right Face two times and Left Face anticlockwise;
        public void FromString(string src)
        {
            moves.Clear();
            if (src != null)
            {
                foreach (char ch in src.ToUpper())
                {
                    int i = Cube.FaceAcronym.IndexOf(ch);
                    if (i >= 0)
                    {
                        moves.Add(new Move((uint)i, Direction.RIGHT));
                    }
                    else if (moves.Count > 0)
                    {
                        if (ch == '\'' || ch == '3')
                        {
                            moves.Last().Turn = Direction.LEFT;
                        }
                        else if (ch == '2')
                        {
                            moves.Last().Turn = Direction.DOWN;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Join(" ", moves);
        }

        public MoveTrack(string src = null)
        {
            FromString(src);
        }

        public void PlayForward(Cube cube)
        {
            for(int i = 0; i < moves.Count; i++)
            {
                Move m = moves[i];
                for(uint t = 0; t < m.Turn; t++)
                {
                    cube.RotateRight(m.Face);
                }
            }
        }

        public void PlayBackward(Cube cube)
        {
            for(int i = moves.Count; i-- > 0;)
            {
                Move m = moves[i];
                for (uint t = 0; t < m.TurnBack; t++)
                {
                    cube.RotateRight(m.Face);
                }
            }
        }

        public MoveTrack Clone(uint[] transform = null)
        {
            MoveTrack dst = new MoveTrack();
            
            if(transform != null)
            {
                foreach(Move m in moves)
                {
                    dst.moves.Add(new Move(transform[m.Face], m.Turn));
                }
            }
            else
            {
                foreach (Move m in moves)
                {
                    dst.moves.Add(new Move(m.Face, m.Turn));
                }
            }

            return dst;
        }

        public MoveTrack Reverse()
        {
            MoveTrack dst = new MoveTrack();
            for(int i = moves.Count; i-- > 0;)
            {
                Move m = moves[i];
                dst.moves.Add(new Move(m.Face, m.TurnBack));
            }
            return dst;
        }

        public void Add(Move move)
        {
            int last = moves.Count - 1;
            if (last >= 0 && moves[last].Face == move.Face)
            {
                moves[last].Turn += move.Turn;
                if(moves[last].Turn == 0)
                {
                    moves.RemoveAt(last);
                }
            }
            else
            {
                moves.Add(move);
            }
        }

        public static MoveTrack operator+(MoveTrack a, MoveTrack b)
        {
            MoveTrack dst = a.Clone();
            foreach (Move m in b.moves)
            {
                dst.Add(new Move(m.Face, m.Turn));
            }
            return dst;
        }
    }

    public class Algorithm
    {
        public Dictionary<Solution.Key, MoveTrack> tracks = new Dictionary<Solution.Key, MoveTrack>();

        public void Add(MoveTrack track)
        {
            Cube cube = new Cube();
            Solution.Key key = new Solution.Key(cube);
            foreach(uint[] transform in Cube.ORIENTATION)
            {
                MoveTrack dst_track = track.Clone(transform);

                cube.MiddleKey = key.middles;
                cube.CornerKey = key.corners;

                dst_track.PlayBackward(cube);

                Solution.Key dst_key = new Solution.Key(cube);

                if (tracks.ContainsKey(dst_key))
                {
                    if(dst_track.Count < tracks[dst_key].Count)
                    {
                        tracks[dst_key] = dst_track;
                    }
                }
                else
                {
                    tracks.Add(dst_key, dst_track);
                }
            }
        }

        public void Save(string fname)
        {
            using(System.IO.StreamWriter file = new System.IO.StreamWriter(fname))
            {
                Cube cube = new Cube();
                foreach (KeyValuePair<Solution.Key, MoveTrack> pair in tracks)
                {
                    cube.MiddleKey = pair.Key.middles;
                    cube.CornerKey = pair.Key.corners;
                    int m_count = 12 - cube.CountSolvedMiddles;
                    //int c_count = 12 - cube.CountSolvedCorners;

                    file.Write(m_count);
                    //file.Write('.');
                    //file.Write(c_count);
                    file.Write(';');
                    file.Write(pair.Value.ToString());
                    file.Write(';');
                    file.Write(pair.Value.Count);
                    file.Write('\n');
                }
            }
        }

        public void Load(string fname)
        {
            using(System.IO.StreamReader file = new System.IO.StreamReader(fname))
            {
                for(string buf; (buf = file.ReadLine()) != null;)
                {
                    string[] arr = buf.Split(';');
                    if(arr.Length > 1)
                    {
                        Add(new MoveTrack(arr[1]));
                    }
                }
            }
        }

        public void Solve(Solution.Key key, MoveTrack current, ref MoveTrack result)
        {
            Cube cube = new Cube();
            cube.MiddleKey = key.middles;
            cube.CornerKey = key.corners;
            int src_count = cube.CountSolvedMiddles;

            foreach (MoveTrack track in tracks.Values)
            {
                if(current.moves.Count + track.moves.Count < 40)
                {
                    if (result == null || result.moves.Count > current.moves.Count + track.moves.Count)
                    {
                        cube.MiddleKey = key.middles;
                        cube.CornerKey = key.corners;
                        track.PlayForward(cube);

                        int dst_count = cube.CountSolvedMiddles;
                        if (dst_count == 12)
                        {
                            result = current + track;
                        }
                        else if (dst_count > src_count)
                        {
                            Solve(new Solution.Key(cube), current + track, ref result);
                        }
                    }
                }
            }
        }

        public MoveTrack RunOnce(Cube cube)
        {
            MoveTrack dst_track = null;
            int solved_middles = cube.CountSolvedMiddles;

            Solution.Key key = new Solution.Key(cube);
            Cube dst_cube = new Cube();

            foreach (MoveTrack track in tracks.Values)
            {
                dst_cube.CornerKey = key.corners;
                dst_cube.MiddleKey = key.middles;

                track.PlayForward(dst_cube);
                int i = dst_cube.CountSolvedMiddles;
                if(i > solved_middles)
                {
                    solved_middles = i;
                    dst_track = track;
                }
                else if(i == solved_middles && dst_track != null)
                {
                    if(track.Count < dst_track.Count)
                    {
                        dst_track = track;
                    }
                }
            }

            return dst_track;
        }

        public MoveTrack Run(Cube cube)
        {
            MoveTrack track = null;
            int threshold = 1000;

            List<Entry> result = new List<Entry>();
            result.Add(new Entry(new Solution.Key(cube), cube.CountSolvedMiddles, new MoveTrack()));

            for (int try_count = 0; result.Count > 0 && try_count < 100; try_count++)
            {
                List<Entry> next_result = new List<Entry>();

                foreach(Entry entry in result)
                {
                    Test(entry, next_result, threshold);
                }

                next_result.Sort();
                int i = next_result.Count;
                while (i-- > 0 && next_result[i].solved_middles == 12)
                {
                    Entry entry = next_result[i];
                    if(track == null || track.moves.Count > next_result[i].path.moves.Count)
                    {
                        track = next_result[i].path;
                        threshold = track.moves.Count;
                    }
                }
                next_result.RemoveRange(i + 1, next_result.Count - i - 1);

                next_result.Reverse();
                if(next_result.Count > 100)
                {
                    int pos = 50;
                    int solved_middles = Math.Max(next_result[pos].solved_middles, 1);
                    while (++pos < next_result.Count && solved_middles == next_result[pos].solved_middles) ;

                    next_result.RemoveRange(pos, next_result.Count - pos);
                }

                result = next_result;
            }

            return track;
        }

        public void Test(Entry entry, List<Entry> result, int threshold)
        {
            Cube cube = new Cube();
            threshold -= entry.path.moves.Count;

            foreach (MoveTrack track in tracks.Values)
            {
                if(track.moves.Count < threshold)
                {
                    cube.MiddleKey = entry.dst_key.middles;
                    cube.CornerKey = entry.dst_key.corners;

                    track.PlayForward(cube);
                    result.Add(new Entry(new Solution.Key(cube), cube.CountSolvedMiddles, entry.path + track));
                }
            }
        }

        public class Entry : IComparable<Entry>
        {
            public Solution.Key dst_key;
            public int solved_middles;
            public MoveTrack path;

            public Entry(Solution.Key dst_key, int solved_middles, MoveTrack path)
            {
                this.dst_key = dst_key;
                this.solved_middles = solved_middles;
                this.path = path;
            }

            public int CompareTo(Entry other)
            {
                int result = solved_middles - other.solved_middles;
                if(result == 0)
                {
                    result = other.path.moves.Count - path.moves.Count;
                    if(result == 0)
                    {
                        result = dst_key.CompareTo(other.dst_key);
                    }
                }
                return result;
            }
        }
    }
}
