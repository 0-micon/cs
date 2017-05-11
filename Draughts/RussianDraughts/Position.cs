using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    struct Position : IComparable<Position>
    {
        public const int COL_BIT_NUM = 2; // Number of bits for column position
        public const int COL_MASK = 3;           // Column mask
        public const int ROW_MASK = ~COL_MASK;    // Row mask
        public const int POS_NUM = Board.ROW_NUM * Board.SPOT_IN_ROW;

        int _position;  // Position on the board (0-31).

        public static readonly Position None = new Position(-1);

        public Position(int i)
        {
             _position = i;
        }

        public Position(int col, int row)
        {
            _position = 0;
            Column = col;
            Row = row;
        }

        public static implicit operator int(Position p) => p._position;
        public static implicit operator bool(Position p) => p._position >= 0 && p._position < POS_NUM;
        public static bool operator ==(Position a, Position b) => a._position == b._position;
        public static bool operator !=(Position a, Position b) => a._position != b._position;

        public int Column
        {
            get
            {
                return (_position & COL_MASK) * 2 + (Row & 1);
            }
            set
            {
                _position = ((value / 2 & COL_MASK) | (_position & ROW_MASK));
            }
        }

        public int ColumnShort
        {
            get
            {
                return _position & COL_MASK;
            }
            set
            {
                _position = ((value & COL_MASK) | (_position & ROW_MASK));
            }
        }

        public int Row
        {
            get
            {
                return _position >> COL_BIT_NUM;
            }

            set
            {
                _position = ((_position & COL_MASK) | (value << COL_BIT_NUM));
            }
        }

        public Position LeftBottom
        {
            get
            {
                int r = Row;
                int c = Column;
                if (r > 0 && c > 0)
                {
                    return new Position(c - 1, r - 1);
                }
                return None;
            }
        }

        public Position LeftTop
        {
            get
            {
                int r = Row;
                int c = Column;
                if (r + 1 < Board.ROW_NUM && c > 0)
                {
                    return new Position(c - 1, r + 1);
                }
                return None;
            }
        }

        public Position RightTop
        {
            get
            {
                int r = Row;
                int c = Column;
                if (r + 1 < Board.ROW_NUM && c + 1 < Board.COL_NUM)
                {
                    return new Position(c + 1, r + 1);
                }
                return None;
            }
        }

        public Position RightBottom
        {
            get
            {
                int r = Row;
                int c = Column;
                if (r > 0 && c + 1 < Board.COL_NUM)
                {
                    return new Position(c + 1, r - 1);
                }
                return None;
            }
        }

        public override int GetHashCode()
        {
            return _position;
        }

        // Positions are recorded using a special notation – algebraic notation.
        // The vertical columns of squares are labeled from a to h.
        // The horizontal rows of squares are numbered 1 to 8.
        public override string ToString()
        {
            char col = (char)('a' + Column);
            char row = (char)('1' + Row   );

            return $"{col}{row}";
        }

        // IComparable<Position> implementation.
        // Compares the current object with another object of the same type.
        public int CompareTo(Position other)
        {
            return _position - other._position;
        }
    }
}
