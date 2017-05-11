using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    class Move
    {
        List<Position> _moves;
        BitSet32 _captures;
        int _piece;

        public List<Position> Path => _moves;
        public Position Start => _moves.First();
        public Position End => _moves.Last();
        public BitSet32 Captures => _captures;
        public int Piece => _piece;

        public Move(int piece)
        {
            _piece = piece;
            _captures = 0;
            _moves = new List<Position>();
        }

        public Move(int piece, Position start) : this(piece)
        {
            Add(start);
        }

        public Move(int piece, Position start, Position end) : this(piece)
        {
            Add(start);
            Add(end);
        }

        public Move(int piece, List<Position> moves, BitSet32 captures)
        {
            _piece = piece;
            _moves = new List<Position>(moves);
            _captures = captures;
        }

        public void Add(Position move)
        {
            _moves.Add(move);
            // If a man touches the kings row during a move, it continues as a king.
            if (Board.IsKingRow(move.Row, _piece))
            {
                _piece |= Pieces.King;
            }
        }

        public void Add(Position move, Position capture)
        {
            Add(move);
            _captures[capture] = 1;
        }

        // Move with capture is recorded as c5:e3, without as e3-d4.
        public override string ToString()
        {
            char delim = _captures.Any ? ':' : '-';
            var count = _moves.Count;
            var buf = new StringBuilder(count * 3);
            for (int i = 0; i < count; i++)
            {
                buf.Append(_moves[i].ToString());
                if (i + 1 < count)
                {
                    buf.Append(delim);
                }
            }
            return buf.ToString();
        }

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
        public IEnumerable<Move> GetCaptures(Board board, Func<Position, Position> move)
        {
            int opp_color = (_piece & Pieces.White) == Pieces.White ? Pieces.Black : Pieces.White;
            bool is_king = (_piece & Pieces.King) == Pieces.King;

            Position opp = move(End);

            // Skip over empty spaces (kings only).
            while (is_king && opp && board[opp] == Pieces.None)
            {
                opp = move(opp);
            }

            if (opp && (board[opp] & opp_color) == opp_color && !_captures.Test(opp))
            {
                for (Position vac = move(opp);
                    vac && (board[vac] == Pieces.None || _moves.Contains(vac));
                    vac = move(vac)
                    )
                {
                    Move cap = new Move(_piece, _moves, _captures);
                    cap.Add(vac, opp);
                    yield return cap;

                    // Only kings can choose where to land after the capture.
                    if (!is_king)
                    {
                        break;
                    }
                }
            }
        }
    }
}
