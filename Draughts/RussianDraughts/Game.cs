using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    class Game
    {
        public const int PIECE_NUM = 12;

        Board _board = new Board();
        int _move_count = 0;
        List<Move> _moves = new List<Move>();
        List<Move> _next_moves = new List<Move>();

        public int Turn => (_move_count & 1) == 1 ? Pieces.Black : Pieces.White;
        public int MoveCount => _move_count;
        public List<Move> NextMoves => _next_moves;
        public Board GameBoard => _board;
        public List<Move> History => _moves;

        public void OnPositionChanged()
        {
            _next_moves.Clear();
            _next_moves.AddRange(Solution.NextMoves(_board, Turn));
        }

        public void Apply(Move move)
        {
            // remove redo moves
            int redo_count = _moves.Count - _move_count;
            if (redo_count > 0)
            {
                _moves.RemoveRange(_move_count, redo_count);
            }

            _board.Apply(move);
            _moves.Add(move);
            _move_count++;

            OnPositionChanged();
        }

        public bool UndoMove()
        {
            if (_move_count > 0)
            {
                _move_count--;
                _board.Reset();
                for (int i = 0; i < _move_count; i++)
                {
                    _board.Apply(_moves[i]);
                }
                OnPositionChanged();
                return true;
            }
            return false;
        }

        public bool RedoMove()
        {
            if (_move_count < _moves.Count)
            {
                _board.Apply(_moves[_move_count]);
                _move_count++;
                OnPositionChanged();
                return true;
            }
            return false;
        }

        // Each player starts with 12 pieces on the three rows closest to their own side.
        // The row closest to each player is called the "crownhead" or "kings row".
        // Usually, the colors of the pieces are black and white, but possible use other colors.
        // The player with white pieces (lighter color) moves first.
        public void NewGame()
        {
            _move_count = 0;
            _moves.Clear();
            _board.Reset();
            OnPositionChanged();

            //for (int i = 0; i < PIECE_NUM; i++)
            //{
            //    int col = i % Board.SPOT_IN_ROW;
            //    int row = i / Board.SPOT_IN_ROW;
            //    _board[col, row] = Pieces.White;

            //    col = Board.SPOT_IN_ROW - col - 1;
            //    row = Board.ROW_NUM - row - 1;
            //    _board[col, row] = Pieces.Black;
            //}
        }

        /*//
                public static Board StartingPosition(Board board)
                {
                    board.Reset();

                    for (int i = 0; i < PIECE_NUM; i++)
                    {
                        int col = i % Board.SPOT_IN_ROW;
                        int row = i / Board.SPOT_IN_ROW;
                        board[col, row] = Pieces.White;

                        col = Board.SPOT_IN_ROW - col - 1;
                        row = Board.ROW_NUM - row - 1;
                        board[col, row] = Pieces.Black;
                    }

                    return board;
                }
        //*/

        /*//
                // Capture.
                // If the adjacent square contains an opponent's piece, and the square
                // immediately beyond it is vacant, the opponent's piece may be
                // captured (and removed from the game) by jumping over it.
                // Jumping can be done forward and backward.
                // Multiple-jump moves are possible if, when the jumping piece lands,
                // there is another piece that can be jumped.
                // Jumping is mandatory and cannot be passed up to make a
                // non-jumping move. When there is more than one way for a player to
                // jump, one may choose which sequence to make, not necessarily the
                // sequence that will result in the most amount of captures. However,
                // one must make all the captures in that sequence.
                // A captured piece is left on the board until all captures in a
                // sequence have been made but cannot be jumped again (this rule also
                // applies for the kings).
                public void GetCaptures(Capture start, List<Capture> captures)
                {
                    var moves = new Func<Position, Position>[] {
                        p => p.LeftTop,
                        p => p.RightTop,
                        p => p.LeftBottom,
                        p => p.RightBottom
                    };

                    bool should_jump = false;

                    foreach (var move in moves)
                    {
                        foreach (Capture c in start.GetCaptures(_board, move))
                        {
                            should_jump = true;
                            GetCaptures(c, captures);
                        }
                    }

                    if (!should_jump && start.Captures.Any)
                    {
                        captures.Add(start);
                    }
                }

                public IEnumerable<Capture> GetCaptures()
                {
                    var captures = new List<Capture>();
                    int turn = Turn;

                    foreach (var cell in _board.Cells)
                    {
                        if ((cell.Value & turn) == turn)
                        {
                            GetCaptures(new Capture(cell.Key, cell.Value), captures);
                        }
                    }

                    return captures;
                }

                public IEnumerable<Move> GetMoves()
                {
                    int turn = Turn;

                    Func<Position, Position>[] moves;

                    foreach (var cell in _board.Cells)
                    {
                        if ((cell.Value & turn) == turn)
                        {
                            if ((cell.Value & Pieces.King) == Pieces.King)
                            {
                                moves = new Func<Position, Position>[] {
                                    p => p.LeftTop,    p => p.RightTop,
                                    p => p.LeftBottom, p => p.RightBottom
                                };
                            }
                            else if(turn == Pieces.White)
                            {
                                moves = new Func<Position, Position>[] { p => p.LeftTop, p => p.RightTop };
                            }
                            else
                            {
                                moves = new Func<Position, Position>[] { p => p.LeftBottom, p => p.RightBottom };
                            }

                            foreach (var move in moves)
                            {
                                for (Position pos = move(cell.Key); pos && _board[pos] == Pieces.None; pos = move(pos))
                                {
                                    yield return new Move(cell.Key, pos);
                                    if ((cell.Value & Pieces.King) != Pieces.King)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
        //*/
        //public bool GetManCapture(Capture capture, Func<Position, Position> move)
        //{
        //    int turn = Turn;
        //    int hunt = turn == Pieces.White ? Pieces.Black : Pieces.White;
        //
        //    Position pos = capture.Moves.Last();
        //    if (_board[pos] == turn)
        //    {
        //        Position opp = move(pos);
        //        if (opp && _board[opp] == hunt && !capture.Captures.Contains(opp))
        //        {
        //            Position vac = move(opp);
        //            if (vac && (_board[vac] == Pieces.None || capture.Moves.Contains(vac)))
        //            {
        //                capture.Moves.Add(vac);
        //                capture.Captures.Add(opp);
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        //public static bool GetManCapture(Board board, Capture capture, Func<Position, Position>move)
        //{
        //    Position pos = capture.Moves.Last();
        //    if (board[pos] == Pieces.White)
        //    {
        //        Position opp = move(pos);
        //        if (opp && board[opp] == Pieces.Black && !capture.Captures.Contains(opp))
        //        {
        //            Position vac = move(opp);
        //            if (vac && (board[vac] == Pieces.None || capture.Moves.Contains(vac)))
        //            {
        //                capture.Moves.Add(vac);
        //                capture.Captures[opp] = 1;
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        /*//
                public static void GetCaptures(Board board, Capture start, List<Capture> captures)
                {
                    var moves = new Func<Position, Position>[] {
                        p => p.LeftTop,
                        p => p.RightTop,
                        p => p.LeftBottom,
                        p => p.RightBottom
                    };

                    bool should_jump = false;

                    foreach (var move in moves)
                    {
                        foreach (Capture c in start.GetCaptures(board, move))
                        {
                            should_jump = true;
                            GetCaptures(board, c, captures);
                        }
                    }

                    if (!should_jump && start.Captures.Any)
                    {
                        captures.Add(start);
                    }
                }

                public static IEnumerable<Move> NextMoves(Board board)
                {
                    foreach (Position pos in board.OwnedPiecesAt())
                    {
                        Position move = pos.LeftTop;
                        if (move && board[move] == Pieces.None)
                        {
                            yield return new Move(pos, move);
                        }

                        move = pos.RightTop;
                        if (move && board[move] == Pieces.None)
                        {
                            yield return new Move(pos, move);
                        }

                    }

                }
//*/
    }
}
