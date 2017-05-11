using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    class Pieces
    {
        // There are two kinds of pieces: "men" and "kings".
        // Kings are differentiated as consisting of two normal pieces of the
        // same color, stacked one on top of the other or by inverted pieces.
        public const int
            None  = 0,
            White = 1 << 0,
            Black = 1 << 1,
            King  = 1 << 2,
            WhiteKing = White | King,
            BlackKing = Black | King;

        // Returns opponent's color
        public static int Opponent(int piece)
        {
            if ((piece & White) == White)
            {
                return Black;
            }

            if ((piece & Black) == Black)
            {
                return White;
            }

            return None;
        }
    }
}
