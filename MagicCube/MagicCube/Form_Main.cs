using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagicCube
{
    public partial class Form_Main : Form
    {
        Cube cube = new Cube();
        Solution solution = new Solution();

        public const float cx = 24f;
        public const float cy = 24f;

        static readonly Point[] element_positions =
        {
            new Point(1, 0), // Up
            new Point(2, 1), // Right
            new Point(1, 2), // Down
            new Point(0, 1), // Left
        };

        static readonly Point[] corner_positions =
        {
            new Point(2, 0), // Up
            new Point(2, 2), // Right
            new Point(0, 2), // Down
            new Point(0, 0), // Left
        };

        uint selected_face = 0;
        int selected_cube = 0;

        public Form_Main()
        {
            InitializeComponent();
        }

        public void RepaintCube()
        {
            panel_Cube.Invalidate();
        }

        private void DrawFace(Graphics g, uint face, uint direction, float x, float y, float cx, float cy, bool selected)
        {
            SolidBrush br = new SolidBrush(Color.FromName(FaceInfo.items[face].color));
            g.FillRectangle(br, x + cx, y + cy, cx - 1, cy - 1);
            if (selected)
            {
                g.DrawRectangle(Pens.Black, x + cx - 1, y + cy - 1, cx, cy);
            }

            foreach(uint i in Direction.Items())
            {
                br = new SolidBrush(Color.FromName(FaceInfo.items[cube.MiddleElementAt(face, Direction.Sum(direction, i))].color));
                g.FillRectangle(br, x + cx * element_positions[i].X, y + cy * element_positions[i].Y, cx - 1, cy - 1);

                br = new SolidBrush(Color.FromName(FaceInfo.items[cube.CornerElementAt(face, Direction.Sum(direction, i))].color));
                g.FillRectangle(br, x + cx * corner_positions[i].X, y + cy * corner_positions[i].Y, cx - 1, cy - 1);
            }
        }


        private void panel_Cube_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //g.ResetTransform();
            //g.TranslateTransform(ctrl.Width / 3f, ctrl.Height / 2f);

            for(uint face = 0; face < Cube.FACE_NUM; face++)
            {
                float x = 3 * cx * FaceInfo.items[face].location.X;
                float y = 3 * cy * FaceInfo.items[face].location.Y;
                DrawFace(g, face, FaceInfo.items[face].direction,
                    x, y, cx, cy, selected_face == face);
            }

            g.DrawString(selected_cube.ToString(), Font, Brushes.Black, 0f, 0f);
        }

        private void panel_Cube_MouseUp(object sender, MouseEventArgs e)
        {
            for (uint face = 0; face < Cube.FACE_NUM; face++)
            {
                float x = 3 * cx * FaceInfo.items[face].location.X;
                float y = 3 * cy * FaceInfo.items[face].location.Y;
                RectangleF rc = new RectangleF(x, y, 3 * cx, 3 * cy);
                if (rc.Contains(e.Location))
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        foreach (uint i in Direction.Items())
                        {
                            RectangleF rc_elem = new RectangleF(x + cx * element_positions[i].X, y + cy * element_positions[i].Y, cx, cy);
                            if (rc_elem.Contains(e.Location))
                            {
                                using (var frm = new FaceColorForm())
                                {
                                    frm.selection = cube.MiddleElementAt(face, Direction.Sum(FaceInfo.items[face].direction, i));
                                    frm.ShowDialog(this);
                                    cube.MiddleElementAt(face, Direction.Sum(FaceInfo.items[face].direction, i), frm.selection);
                                    RepaintCube();
                                }
                            }
                        }
                    }
                    else
                    {
                        selected_face = face;
                        RepaintCube();
                    }
                }
            }
        }

        private void button_RotateLeft_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Left\r\n";

            UpdateUndoMove(selected_face, Direction.RIGHT);

            cube.RotateRight(selected_face);
            cube.RotateRight(selected_face);
            cube.RotateRight(selected_face);
            RepaintCube();
        }

        private void UpdateUndoMove(uint face, uint turn)
        {
            if (comboBox_MoveUndo.Items.Count > 0 && ((Cube.Move)comboBox_MoveUndo.Items[0]).Face == face)
            {
                turn = Direction.Sum(turn, ((Cube.Move)comboBox_MoveUndo.Items[0]).Turn);
                comboBox_MoveUndo.Items.RemoveAt(0);
            }
            if (turn > 0)
            {
                comboBox_MoveUndo.Items.Insert(0, new Cube.Move(selected_face, turn));
            }
            if (comboBox_MoveUndo.Items.Count > 0)
            {
                comboBox_MoveUndo.SelectedIndex = 0;
            }

            comboBox_MoveRedo.Items.Clear();
        }

        private void button_RotateRight_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Right\r\n";

            UpdateUndoMove(selected_face, Direction.LEFT);

            cube.RotateRight(selected_face);
            RepaintCube();
        }

        private void button_GetKeys_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += "Middle Key: " + cube.MiddleKey + "\r\n";
            textBox_Log.Text += "Corner Key: " + cube.CornerKey + "\r\n";
        }

        private void textBox_MiddleKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
            {
                // Only digits please. Consume this invalid key.
                e.Handled = true;
            }
        }

        private void textBox_MiddleKey_TextChanged(object sender, EventArgs e)
        {
            string text = textBox_MiddleKey.Text;
            StringBuilder sb = new StringBuilder(text.Length);
            foreach(char ch in text)
            {
                if (Char.IsDigit(ch))
                {
                    sb.Append(ch);
                }
            }
            string str = sb.ToString();
            if(text != str)
            {
                textBox_MiddleKey.Text = str;
            }
        }

        private void button_SetMiddleKey_Click(object sender, EventArgs e)
        {
            ulong key;
            if(UInt64.TryParse(textBox_MiddleKey.Text, out key))
            {
                cube.MiddleKey = key;
                RepaintCube();
            }
        }

        private void button_SolveMiddle_Click(object sender, EventArgs e)
        {
            button_Solve_Middle.Enabled = false;
            //Solution.PrecomputeCornerMoves(7);
            var path = solution.SolveMiddle(cube.MiddleKey);

            comboBox_MoveUndo.Items.Clear();
            comboBox_MoveRedo.Items.Clear();
            for (int i = 1; i < path.Count; i++)
            {
                Cube.Move move = Cube.Move.MiddleKeysToMove(path[i - 1], path[i]);
                textBox_Log.Text += path[i].ToString() + ": " + move.ToString() + "\r\n";

                comboBox_MoveUndo.Items.Add(move);
            }
            comboBox_MoveUndo.SelectedIndex = 0;

            button_Solve_Middle.Enabled = true;
        }

        private void button_RandomMove_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            selected_face = (uint) rnd.Next(0, (int)Cube.FACE_NUM);
            for (int i = rnd.Next(1, (int)Direction.TURN_COUNT); i-- > 0;)
            {
                button_RotateRight_Click(sender, e);
            }
        }

        private void OnMoveUndo(ComboBox undoBox, ComboBox redoBox)
        {
            if (undoBox.Items.Count > 0)
            {
                Cube.Move undo_move = (Cube.Move)undoBox.Items[0];
                uint turn = undo_move.Turn;
                if (turn == Direction.RIGHT || turn == Direction.LEFT)
                {
                    turn = Direction.TurnAround(turn);
                }

                undoBox.Items.RemoveAt(0);
                if (undoBox.Items.Count > 0)
                {
                    undoBox.SelectedIndex = 0;
                }

                redoBox.Items.Insert(0, new Cube.Move(undo_move.Face, turn));
                redoBox.SelectedIndex = 0;

                for (uint i = 0; i < undo_move.Turn; i++)
                {
                    cube.RotateRight(undo_move.Face);
                }
                RepaintCube();
            }
        }

        private void button_MoveUndo_Click(object sender, EventArgs e)
        {
            OnMoveUndo(comboBox_MoveUndo, comboBox_MoveRedo);
        }

        private void button_MoveRedo_Click(object sender, EventArgs e)
        {
            OnMoveUndo(comboBox_MoveRedo, comboBox_MoveUndo);
        }

        private void button_SolveCorners_Click(object sender, EventArgs e)
        {
            button_SolveCorners.Enabled = false;

            //Solution.PrecomputeMoves(6);
            var path = solution.SolveCorners(cube.CornerKey);

            comboBox_MoveUndo.Items.Clear();
            comboBox_MoveRedo.Items.Clear();
            for (int i = 1; i < path.Count; i++)
            {
                Cube.Move move = Cube.Move.CornerKeysToMove(path[i - 1], path[i]);
                textBox_Log.Text += path[i].ToString() + ": " + move.ToString() + "\r\n";

                comboBox_MoveUndo.Items.Add(move);
            }
            comboBox_MoveUndo.SelectedIndex = 0;

            button_SolveCorners.Enabled = true;
        }

        private void button_Solve_Click(object sender, EventArgs e)
        {
            button_Solve.Enabled = false;

            var path = solution.Solve(new Solution.Key(cube));

            comboBox_MoveUndo.Items.Clear();
            comboBox_MoveRedo.Items.Clear();
            for (int i = 1; i < path.Count; i++)
            {
                Cube.Move move = Cube.Move.KeysToMove(path[i - 1], path[i]);
                textBox_Log.Text += path[i].ToString() + ": " + move.ToString() + "\r\n";

                comboBox_MoveUndo.Items.Add(move);
            }
            comboBox_MoveUndo.SelectedIndex = 0;

            button_Solve.Enabled = true;
        }

        private void button_ShowNext_Click(object sender, EventArgs e)
        {
            var last = solution.cube_rings.Last();
            if (++selected_cube >= last.Count)
            {
                selected_cube = 0;
            }

            Random rnd = new Random();
            selected_cube = rnd.Next(0, last.Count);

            cube.MiddleKey = last[selected_cube].middles;
            cube.CornerKey = last[selected_cube].corners;
            RepaintCube();
        }
    }
}
