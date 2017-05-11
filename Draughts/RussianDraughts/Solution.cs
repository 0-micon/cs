using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    class Solution
    {
        List<List<Board>> _rings = new List<List<Board>>();

        public List<List<Board>> Rings => _rings;

        static Func<Position, Position>[] _white_moves = new Func<Position, Position>[]
        {
            p => p.LeftTop,
            p => p.RightTop
        };

        static Func<Position, Position>[] _black_moves = new Func<Position, Position>[]
        {
            p => p.LeftBottom,
            p => p.RightBottom
        };

        static Func<Position, Position>[] _king_moves = new Func<Position, Position>[]
        {
            p => p.LeftTop,
            p => p.RightTop,
            p => p.LeftBottom,
            p => p.RightBottom
        };

        public void Save(string fname)
        {
            for (int i = 0; i < _rings.Count; i++)
            {
                _rings[i].Save(fname + $"_{i}.txt", Board.Write);
            }
        }

        public void Load(string fname, int count)
        {
            _rings.Clear();
            _rings.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                var ring = new List<Board>();
                ring.Load(fname + $"_{i}.txt", sizeof(uint) * 3, Board.Read);
                _rings.Add(ring);
            }
        }

        public int GetRing(Board board, int color)
        {
            int i = _rings.Count;

            if ((color == Pieces.White && (i & 1) != 1) ||
                (color == Pieces.Black && (i & 1) == 1))
            {
                i--;
            }

            i--;
            while (i >= 0)
            {
                if (_rings[i].BinarySearch(board) >= 0)
                {
                    break;
                }
                i -= 2;
            }

            return i;
        }

        public static int Estimate(Board board, int color)
        {
            int next_color = (color & Pieces.White) == Pieces.White ? Pieces.Black : Pieces.White;

            // get worst estimation
            int ret = int.MaxValue;
            foreach (var fb in NextBoards(board, color, true))
            {
                // get best opponent's estimation
                int est = int.MinValue;
                foreach (var sb in NextBoards(fb, next_color))
                {
                    int e = sb.Estimation;
                    if (e > est)
                    {
                        est = e;
                    }
                }

                if (ret > est)
                {
                    ret = est;
                }
            }

            return ret == int.MaxValue ? board.Estimation : ret;
        }

        public void FirstIteration()
        {
            _rings.Clear();

            var first_ring = new List<Board>();
            var board = new Board();
            board.Reset();
            first_ring.Add(board);
            _rings.Add(first_ring);
        }

        public static IEnumerable<Move> NextMoves(Board board, int color, bool captures_only = false)
        {
            foreach (var c in GetCaptures(board, color))
            {
                captures_only = true;
                yield return c;
            }

            // Jumping is mandatory and cannot be passed up to make a non-jumping move.
            if (!captures_only)
            {
                foreach (var m in GetMoves(board, color))
                {
                    yield return m;
                }
            }
        }

        public static IEnumerable<Board> NextBoards(Board board, int color, bool captures_only = false)
        {
            foreach (var m in NextMoves(board, color, captures_only))
            {
                yield return board + m;
            }
        }

        public void NextIteration()
        {
            var last_ring = _rings.Last();
            int color = (_rings.Count & 1) == 1 ? Pieces.White : Pieces.Black;
            int next_color = (_rings.Count & 1) == 1 ? Pieces.Black : Pieces.White;
            var next_ring = new List<Board>(
                //from board in last_ring
                //from next_board in NextBoards(board, color)
                //where GetRing(next_board, next_color) < 0
                //select next_board
                );

            foreach (var board in last_ring)
            {
                foreach (var next_board in NextBoards(board, color))
                {
                    if (GetRing(next_board, next_color) < 0)
                    {
                        next_ring.Add(next_board);
                    }
                }
            }

            next_ring.DistinctValues();
            _rings.Add(next_ring);
        }

        public static IEnumerable<Move> GetMoves(Board board, int color)
        {
            Func<Position, Position>[] moves;

            foreach (var cell in board.Cells)
            {
                if ((cell.Value & color) == color)
                {
                    if ((cell.Value & Pieces.King) == Pieces.King)
                    {
                        moves = _king_moves;
                    }
                    else if (color == Pieces.White)
                    {
                        moves = _white_moves;
                    }
                    else
                    {
                        moves = _black_moves;
                    }

                    foreach (var move in moves)
                    {
                        for (Position pos = move(cell.Key); pos && board[pos] == Pieces.None; pos = move(pos))
                        {
                            yield return new Move(cell.Value, cell.Key, pos);
                            if ((cell.Value & Pieces.King) != Pieces.King)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static IEnumerable<Move> GetCaptures(Board board, int color)
        {
            var captures = new List<Move>();

            foreach (var cell in board.Cells)
            {
                if ((cell.Value & color) == color)
                {
                    AddCaptures(board, new Move(cell.Value, cell.Key), captures);
                }
            }

            return captures;
        }

        public static void AddCaptures(Board board, Move start, List<Move> captures)
        {
            bool should_jump = false;

            foreach (var move in _king_moves)
            {
                foreach (var c in start.GetCaptures(board, move))
                {
                    should_jump = true;
                    AddCaptures(board, c, captures);
                }
            }

            if (!should_jump && start.Captures.Any)
            {
                captures.Add(start);
            }
        }
    }
}
