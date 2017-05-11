using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RussianDraughtsGUI
{
    public partial class form_Main : Form
    {
        RussianDraughts.Game game = new RussianDraughts.Game();
        const int ROW_NUM = RussianDraughts.Board.ROW_NUM + 2;   // + 2 notation rows: a, b, c, ...
        const int COL_NUM = RussianDraughts.Board.COL_NUM + 2;   // + 2 notation rows: 1, 2, 3, ...

        RussianDraughts.Position curSel = RussianDraughts.Position.None;

        RussianDraughts.Solution solution = new RussianDraughts.Solution();

        SortedDictionary<int, List<RussianDraughts.Board>> estimations = new SortedDictionary<int, List<RussianDraughts.Board>>();

        public form_Main()
        {
            InitializeComponent();

            game.NewGame();

            solution.Load(@"..\..\..\boards.whites", 13);

            //foreach(var board in solution.Rings.Last())
            //{
            //    int key = board.Estimation;
            //    if (!estimations.ContainsKey(key))
            //    {
            //        estimations[key] = new List<RussianDraughts.Board>();
            //    }
            //    estimations[key].Add(board);
            //}

            //var worst = estimations.First();
            //var last_ring = solution.Rings.Last();

            //RussianDraughts.Utils.RemoveItems(last_ring, worst.Value);
        }

        void DrawLastMove(Graphics g, float span)
        {
            if (game.MoveCount > 0)
            {
                Pen pen = new Pen(Color.FromArgb(180, 255, 0, 0), 6);
                pen.StartCap = LineCap.RoundAnchor;
                pen.EndCap = LineCap.ArrowAnchor;
                pen.LineJoin = LineJoin.Bevel;

                GraphicsPath path = new GraphicsPath();
                path.StartFigure();

                var m = game.History[game.MoveCount - 1];

                for (int i = 1; i < m.Path.Count; i++)
                {
                    float x1 = span * (m.Path[i - 1].Column + 1.5f);
                    float y1 = span * (ROW_NUM - 1.5f - m.Path[i - 1].Row);   // start from the bottom

                    float x2 = span * (m.Path[i].Column + 1.5f);
                    float y2 = span * (ROW_NUM - 1.5f - m.Path[i].Row);

                    path.AddLine(x1, y1, x2, y2);
                }
                g.DrawPath(pen, path);
            }
        }

        void DrawAllowedMoves(Graphics g, float span)
        {
            Pen pen = game.Turn == RussianDraughts.Pieces.White ?
                new Pen(Color.FromArgb(140, 200, 200, 200), 6) :
                new Pen(Color.FromArgb(140,  55,  55,  55), 6) ;
            pen.StartCap = LineCap.RoundAnchor;
            pen.EndCap = LineCap.ArrowAnchor;
            pen.LineJoin = LineJoin.Bevel;

            foreach (var m in game.NextMoves)
            {
                if (curSel && m.Start != curSel)
                {
                    continue;
                }

                GraphicsPath path = new GraphicsPath();
                path.StartFigure();

                for (int i = 1; i < m.Path.Count; i++)
                {
                    float x1 = span * (m.Path[i - 1].Column + 1.5f);
                    float y1 = span * (ROW_NUM - 1.5f - m.Path[i - 1].Row);   // start from the bottom

                    float x2 = span * (m.Path[i].Column + 1.5f);
                    float y2 = span * (ROW_NUM - 1.5f - m.Path[i].Row);

                    path.AddLine(x1, y1, x2, y2);

                    //if (i == 1)
                    //{
                    //    pen.StartCap = System.Drawing.Drawing2D.LineCap.RoundAnchor;
                    //}
                    //else
                    //{
                    //    pen.StartCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                    //}

                    //if (i + 1 == m.Path.Count)
                    //{
                    //    pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                    //}
                    //else
                    //{
                    //    pen.EndCap = System.Drawing.Drawing2D.LineCap.NoAnchor;
                    //}


                    //g.DrawLine(pen, x0, y0, x1, y1);
                }
                g.DrawPath(pen, path);
            }
        }

        RectangleF GetCellRectangle(int col, int row)
        {
            float span = Math.Min(
                (float)panel_Board.Size.Width / COL_NUM,
                (float)panel_Board.Size.Height / ROW_NUM
                );
            return new RectangleF(span * (col + 1), span * (ROW_NUM - 1 - row), span, span);
        }

        private void panel_Board_Paint(object sender, PaintEventArgs e)
        {
            float span = Math.Min(
                (float)panel_Board.Size.Width  / COL_NUM,
                (float)panel_Board.Size.Height / ROW_NUM
                );

            Brush light_brush = Brushes.Moccasin;
            Brush dark_brush = Brushes.Peru;
            Brush field_brush = Brushes.OldLace;

            var g = e.Graphics;

            g.FillRectangle(field_brush, 0f, 0f, span * COL_NUM, span * ROW_NUM);
            g.FillRectangle(light_brush, span, span, span * (COL_NUM - 2), span * (COL_NUM - 2));

            // Draw the board from left-bottom to right-top
            for (int r = 1; r < ROW_NUM - 1; r++)
            {
                for (int c = 1; c < COL_NUM - 1; c++)
                {
                    if ((r & 1) == (c & 1))
                    {
                        float x = span * c;
                        float y = span * (ROW_NUM - 1 - r);   // start from the bottom

                        g.FillRectangle(dark_brush, x, y, span, span);

                        var pos = new RussianDraughts.Position(c - 1, r - 1);
                        switch (game.GameBoard[pos])
                        {
                            case RussianDraughts.Pieces.White:
                                g.FillEllipse(Brushes.White, x + span / 4, y + span / 4, span / 2, span / 2);
                                break;
                            case RussianDraughts.Pieces.Black:
                                g.FillEllipse(Brushes.Black, x + span / 4, y + span / 4, span / 2, span / 2);
                                break;
                            case RussianDraughts.Pieces.WhiteKing:
                                g.FillEllipse(Brushes.White, x + span / 4, y + span / 4, span / 2, span / 2);
                                g.FillEllipse(Brushes.Gray,  x + span / 3, y + span / 3, span / 3, span / 3);
                                break;
                            case RussianDraughts.Pieces.BlackKing:
                                g.FillEllipse(Brushes.Black, x + span / 4, y + span / 4, span / 2, span / 2);
                                g.FillEllipse(Brushes.Gray,  x + span / 3, y + span / 3, span / 3, span / 3);
                                break;
                        }

                        if (curSel && curSel.Column == c - 1 && curSel.Row == r - 1)
                        {
                            g.DrawRectangle(Pens.Black, x, y, span, span);
                        }
                    }
                }
            }

            // Draw the notation
            var sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            for (int r = 1; r < ROW_NUM - 1; r++)
            {
                string s = r.ToString();
                var rc = new RectangleF(0f, span * (ROW_NUM - 1 - r), span, span);
                g.DrawString(s, Font, dark_brush, rc, sf);
                rc.X += span * (COL_NUM - 1);
                g.DrawString(s, Font, dark_brush, rc, sf);
            }

            for (int c = 1; c < COL_NUM - 1; c++)
            {
                string s = ((char)('a' + (c - 1))).ToString();
                var rc = new RectangleF(span * c, 0, span, span);
                g.DrawString(s, Font, dark_brush, rc, sf);
                rc.Y += span * (ROW_NUM - 1);
                g.DrawString(s, Font, dark_brush, rc, sf);
            }

            DrawLastMove(g, span);
            DrawAllowedMoves(g, span);
        }

        private void button_ShowMoves_Click(object sender, EventArgs e)
        {
            //var moves = new List<RussianDraughts.Move>(RussianDraughts.Game.NextMoves(board));
            //foreach (var move in moves)
            //{
            //    textBox_GameLog.AppendText($"{move}\r\n");
            //}
            /*//
            var captures = game.GetCaptures();
            textBox_GameLog.AppendText("-=-=-=-=-=-=-=-=-=-=-\r\n");
            foreach (var c in captures)
            {
                textBox_GameLog.AppendText($"{c}\r\n");
            }
            //*/
        }

        private void panel_Board_MouseDown(object sender, MouseEventArgs e)
        {
            float span = Math.Min(
                (float)panel_Board.Size.Width / COL_NUM,
                (float)panel_Board.Size.Height / ROW_NUM
                );
            float x = e.X;
            float y = e.Y;

            int c = (int) Math.Floor(x / span) - 1;
            int r = RussianDraughts.Board.ROW_NUM - (int) Math.Floor(y / span);

            if (c >= 0 && c < RussianDraughts.Board.COL_NUM &&
                r >= 0 && r < RussianDraughts.Board.ROW_NUM &&
                (c & 1) == (r & 1))
            {
                var newSel = new RussianDraughts.Position(c, r);

                if (newSel == curSel)
                {
                    curSel = RussianDraughts.Position.None;
                }
                else if (curSel)
                {
                    foreach (var m in game.NextMoves)
                    {
                        if (curSel == m.Start && newSel == m.End)
                        {
                            curSel = RussianDraughts.Position.None;
                            game.Apply(m);
                            break;
                        }
                        else if (newSel == m.Start)
                        {
                            curSel = newSel;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var m in game.NextMoves)
                    {
                        if (newSel == m.Start)
                        {
                            curSel = newSel;
                            break;
                        }
                    }
                }

                panel_Board.Invalidate();
            }
        }

        private void button_Apply_Click(object sender, EventArgs e)
        {
            /*//
            if (curSel)
            {
                var game = new RussianDraughts.Game();
                game._board = board;
                var captures = new List<RussianDraughts.Capture>();
                game.GetCaptures(new RussianDraughts.Capture(curSel, board[curSel]), captures);
                if (captures.Count > 0)
                {
                    textBox_GameLog.AppendText("---------------------------\r\n");
                    var cap = captures.First();
                    textBox_GameLog.AppendText($"{cap}\r\n");
                    board.Apply(cap);

                    curSel = RussianDraughts.Position.None;
                    panel_Board.Invalidate();
                }
            }
            //*/
            textBox_GameLog.AppendText("---------------------------\r\n");

            /*//
            var rnd = new Random();
            List<RussianDraughts.Capture> captures = new List<RussianDraughts.Capture>(game.GetCaptures());
            if (captures.Count > 0)
            {
                var capture = captures[rnd.Next(captures.Count)];
                textBox_GameLog.AppendText($"{capture}\r\n");
                game.Apply(capture);
            }
            else
            {
                List<RussianDraughts.Move> moves = new List<RussianDraughts.Move>(game.GetMoves());
                if (moves.Count > 0)
                {
                    var move = moves[rnd.Next(moves.Count)];
                    textBox_GameLog.AppendText($"{move}\r\n");
                    game.Apply(move);
                }
            }
            //*/
            curSel = RussianDraughts.Position.None;
            panel_Board.Invalidate();
        }

        private void button_NextBoard_Click(object sender, EventArgs e)
        {
            var boards = solution.Rings.Last();
            var rnd = new Random();

            var new_board = boards[rnd.Next(boards.Count)];

            //var pair = estimations.First();
            //var new_board = pair.Value.Last();

            //pair.Value.RemoveAt(pair.Value.Count - 1);

            //if (pair.Value.Count == 0)
            //{
            //    estimations.Remove(pair.Key);
            //}

            //game._board = new_board;

            textBox_GameLog.AppendText($"{new_board.Estimation}\r\n");
            panel_Board.Invalidate();
        }

        private void button_UndoMove_Click(object sender, EventArgs e)
        {
            if (game.UndoMove())
            {
                panel_Board.Invalidate();
            }
        }

        private void button_RedoMove_Click(object sender, EventArgs e)
        {
            if (game.RedoMove())
            {
                panel_Board.Invalidate();
            }
        }
    }
}
