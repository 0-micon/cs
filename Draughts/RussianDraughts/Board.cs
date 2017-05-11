using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    struct Board : IComparable<Board>
    {
        public const int ROW_NUM = 8;   // number of rows
        public const int COL_NUM = 8;   // number of column
        public const int SPOT_IN_ROW = 4;  // number of spots in a row
        public const int SPOT_NUM = ROW_NUM * SPOT_IN_ROW; // number of spots

        uint _pieces;   // all pieces
        uint _whites;   // owned pieces
        uint _kings;    // all kings

        public static void Write(System.IO.BinaryWriter bw, Board board)
        {
            bw.Write(board._pieces);
            bw.Write(board._whites);
            bw.Write(board._kings);
        }

        public static Board Read(System.IO.BinaryReader br)
        {
            Board board;
            board._pieces = br.ReadUInt32();
            board._whites = br.ReadUInt32();
            board._kings = br.ReadUInt32();

            return board;
        }

        public static bool IsKingRow(int row, int piece)
        {
            if ((piece & Pieces.White) == Pieces.White)
            {
                return row + 1 == ROW_NUM;
            }
            if ((piece & Pieces.Black) == Pieces.Black)
            {
                return row == 0;
            }
            return false;
        }

        public int this[int col, int row]
        {
            get
            {
                int shift = col + row * SPOT_IN_ROW;
                uint mask = (uint)(1 << shift);

                if ((mask & _pieces) == mask)
                {
                    if ((mask & _kings) == mask)
                    {
                        return (mask & _whites) == mask ? Pieces.WhiteKing : Pieces.BlackKing;
                    }
                    else
                    {
                        return (mask & _whites) == mask ? Pieces.White : Pieces.Black;
                    }
                }
                else
                {
                    return Pieces.None;
                }
            }

            set
            {
                int shift = col + row * SPOT_IN_ROW;
                uint mask = (uint)(1 << shift);

                // clean up first, than set
                _pieces &= ~mask;
                _whites &= ~mask;
                _kings  &= ~mask;

                if ((value & Pieces.White) == Pieces.White)
                {
                    _pieces |= mask;
                    _whites |= mask;
                }
                else if ((value & Pieces.Black) == Pieces.Black)
                {
                    _pieces |= mask;
                }

                if ((value & Pieces.King) == Pieces.King)
                {
                    _kings |= mask;
                }
            }
        }

        public int this[Position pos]
        {
            get
            {
                return this[pos.ColumnShort, pos.Row];
            }

            set
            {
                this[pos.ColumnShort, pos.Row] = value;
            }
        }

        public void Reset()
        {
            _pieces = 0xFFF00FFFU;
            _whites = 0xFFFU;
            _kings  = 0;
        }

        public void SwapPieces(Position a, Position b)
        {
            var tmp = this[a];
            this[a] = this[b];
            this[b] = tmp;
        }

        public void SwapSides()
        {
            uint pieces = _pieces;
            uint whites = _whites;
            uint kings = _kings;

            for (int i = 0; i < SPOT_NUM; i++)
            {
                uint p = pieces & 1;
                uint w = whites & 1;
                uint k = kings & 1;

                _pieces <<= 1;
                _whites <<= 1;
                _kings <<= 1;

                _kings  |= k;
                _pieces |= p;
                _whites |= (p ^ w);

                pieces >>= 1;
                whites >>= 1;
                kings >>= 1;
            }
        }

        //public static IEnumerable<Position> Adjacent(Position pos)
        //{
        //    var r = pos.Row;
        //    var c = pos.Column;
        //    if (r + 1 < ROW_NUM)
        //    {
        //        if (c > 0)
        //    }
        //}

        public void Apply(Move move)
        {
            this[move.Start] = Pieces.None;
            this[move.End] = move.Piece;

            if (move.Captures.Any)
            {
                uint mask = ~move.Captures;
                _kings &= mask;
                _pieces &= mask;
                _whites &= mask;
            }
        }

        public static Board operator+ (Board board, Move move)
        {
            board.Apply(move);
            return board;
        }

        public IEnumerable<KeyValuePair<Position, int>> Cells
        {
            get
            {
                for (int i = 0; i < SPOT_NUM; i++)
                {
                    var pos = new Position(i);
                    yield return new KeyValuePair<Position, int>(pos, this[pos]);
                }
            }
        }

        public int Estimation
        {
            get
            {
                int result = 0;
                foreach (var cell in Cells)
                {
                    if ((cell.Value & Pieces.White) == Pieces.White)
                    {
                        result += 1;
                        if ((cell.Value & Pieces.King) == Pieces.King)
                        {
                            result += 10;
                        }
                    }
                    if ((cell.Value & Pieces.Black) == Pieces.Black)
                    {
                        result -= 1;
                        if ((cell.Value & Pieces.King) == Pieces.King)
                        {
                            result -= 10;
                        }
                    }
                }
                return result;
            }
        }

        public IEnumerable<Position> OwnedPiecesAt()
        {
            for (int i = 0; i < SPOT_NUM; i++)
            {
                uint mask = (uint)(1 << i);
                if ((_whites & mask) == mask)
                {
                    yield return new Position(i);
                }
            }
        } 

        public override string ToString()
        {
            var buf = new StringBuilder(ROW_NUM * COL_NUM + ROW_NUM);

            for (int row = ROW_NUM; row-- > 0;)
            {
                for (int col = 0; col < COL_NUM; col++)
                {
                    if ((row & 1) != (col & 1))
                    {
                        buf.Append(' ');
                    }
                    else
                    {
                        var piece = this[col / 2, row];

                        switch (piece)
                        {
                            case Pieces.White:     buf.Append('w'); break;
                            case Pieces.WhiteKing: buf.Append('W'); break;
                            case Pieces.Black:     buf.Append('b'); break;
                            case Pieces.BlackKing: buf.Append('B'); break;
                            case Pieces.None:
                            default:
                                buf.Append('X');
                                break;
                        }
                    }
                }

                buf.Append('\n');
            }

            return buf.ToString();
        }

        // IComparable<Board> implementation.
        //
        // Summary:
        //     Compares the current object with another object of the same type.
        //
        // Parameters:
        //   other:
        //     An object to compare with this object.
        //
        // Returns:
        //     A value that indicates the relative order of the objects being compared. The
        //     return value has the following meanings:
        //       1) Less than zero. This object is less than the other parameter.
        //       2) Zero. This object is equal to other.
        //       3) Greater than zero. This object is greater than other.
        public int CompareTo(Board other)
        {
            if (_pieces == other._pieces)
            {
                if (_whites == other._whites)
                {
                    if (_kings == other._kings)
                    {
                        return 0;
                    }
                    else if (_kings < other._kings)
                    {
                        return -1;
                    }
                }
                else if (_whites < other._whites)
                {
                    return -1;
                }
            }
            else if (_pieces < other._pieces)
            {
                return -1;
            }
            return 1;

            //if (_pieces < other._pieces)
            //{
            //    return -1;
            //}
            //if (_pieces > other._pieces)
            //{
            //    return 1;
            //}

            //if (_whites < other._whites)
            //{
            //    return -1;
            //}
            //if (_whites > other._whites)
            //{
            //    return 1;
            //}

            //if (_kings < other._kings)
            //{
            //    return -1;
            //}
            //if (_kings > other._kings)
            //{
            //    return 1;
            //}
            //return 0;
        }
    }
}
