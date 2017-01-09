using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MagicCube
{
    public partial class Form_Main : Form
    {
        Cube cube = new Cube();
        Solution solution = new Solution();
        Algorithm algorithm = new Algorithm();
        Cross cross = new Cross(Cross.IDENTITY);

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

            ///*
            for (int i = 6; i <= 11; i++)
            {
                algorithm.Load(string.Format("sequences_{0:D2}.txt", i));
            }
            //*/
            //algorithm.Load("sequences.txt");
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

                using (SolidBrush sb = new SolidBrush(Color.FromName(FaceInfo.items[cross.MiddleElementAt(face, Direction.Sum(direction, i))].color)))
                {
                    g.FillEllipse(sb, x + cx * element_positions[i].X, y + cy * element_positions[i].Y, cx - 1, cy - 1);
                }

                using(Pen pen = new Pen(Color.Black))
                {
                    g.DrawEllipse(pen, x + cx * element_positions[i].X, y + cy * element_positions[i].Y, cx - 1, cy - 1);
                }
                    
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
                            rc_elem = new RectangleF(x + cx * corner_positions[i].X, y + cy * corner_positions[i].Y, cx, cy);
                            if (rc_elem.Contains(e.Location))
                            {
                                using (var frm = new FaceColorForm())
                                {
                                    frm.selection = cube.CornerElementAt(face, Direction.Sum(FaceInfo.items[face].direction, i));
                                    frm.ShowDialog(this);
                                    cube.CornerElementAt(face, Direction.Sum(FaceInfo.items[face].direction, i), frm.selection);
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
            textBox_Log.AppendText(FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Left\r\n");

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
            textBox_Log.AppendText(FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Right\r\n");

            UpdateUndoMove(selected_face, Direction.LEFT);

            cube.RotateRight(selected_face);
            cross.RotateFace(selected_face);
            RepaintCube();
        }

        private void button_GetKeys_Click(object sender, EventArgs e)
        {
            textBox_Log.AppendText(
                "Middle Key: " + cube.MiddleKey + "\r\n" +
                "Corner Key: " + cube.CornerKey + "\r\n"
                );
            //string pattern = "a2 b' c d' a2 c' d b' a2";
            List<char> faces = new List<char>("furbdl");
            Random rnd = new Random();

            int i = rnd.Next(faces.Count);
            char a = faces[i];
            faces.RemoveAt(i);
            i = rnd.Next(faces.Count);
            char b = faces[i];
            faces.RemoveAt(i);
            i = rnd.Next(faces.Count);
            char c = faces[i];
            faces.RemoveAt(i);
            i = rnd.Next(faces.Count);
            char d = faces[i];
            faces.RemoveAt(i);

            string str = string.Format("{0}2 {1}' {2} {3}' {0}2 {2}' {3} {1}' {0}2", a, b, c, d);
            textBox_Log.AppendText(str + "\r\n");
        }

        private void digitTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && !Char.IsControl(e.KeyChar))
            {
                // Only digits please. Consume this invalid key.
                e.Handled = true;
            }
        }

        private void digitTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if(textBox != null)
            {
                string text = textBox.Text;
                StringBuilder sb = new StringBuilder(text.Length);
                foreach (char ch in text)
                {
                    if (Char.IsDigit(ch))
                    {
                        sb.Append(ch);
                    }
                }
                string str = sb.ToString();
                if (text != str)
                {
                    textBox.Text = str;
                }
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

        private void button_SetCornerKey_Click(object sender, EventArgs e)
        {
            ulong key;
            if (UInt64.TryParse(textBox_CornerKey.Text, out key))
            {
                cube.CornerKey = key;
                RepaintCube();
            }
        }


        private void button_SolveMiddle_Click(object sender, EventArgs e)
        {
            button_Solve_Middle.Enabled = false;
         //   Solution.PrecomputeMiddleMoves(7);
            var path = solution.SolveMiddle(cube.MiddleKey);

            comboBox_MoveUndo.Items.Clear();
            comboBox_MoveRedo.Items.Clear();
            for (int i = 1; i < path.Count; i++)
            {
                Cube.Move move = Cube.Move.MiddleKeysToMove(path[i - 1], path[i]);
                textBox_Log.AppendText(path[i].ToString() + ": " + move.ToString() + "\r\n");

                comboBox_MoveUndo.Items.Add(move);
            }
            comboBox_MoveUndo.SelectedIndex = 0;

            button_Solve_Middle.Enabled = true;

/*//
            Cube c = new Cube();
            var ring = solution.middle_rings[1];
            List<ulong> test_ring = new List<ulong>();
            foreach (ulong key in ring)
            {
                c.MiddleKey = key;
                bool done = false;
                for(uint shift = 1; shift < Cube.FACE_NUM; shift++)
                {
                    ulong key_shift = c.GetMiddleKey(shift);
                    if(test_ring.BinarySearch(key_shift) >= 0)
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                {
                    test_ring.Add(key);
                }
            }
//*/
        }

        private void button_RandomMove_Click(object sender, EventArgs e)
        {
            /*//
            algorithm.PlayRandom(cube, 6);
            RepaintCube();
            //*/
            
            Random rnd = new Random();
            selected_face = (uint) rnd.Next(0, (int)Cube.FACE_NUM);
            for (int i = rnd.Next(1, (int)Direction.TURN_COUNT); i-- > 0;)
            {
                button_RotateRight_Click(sender, e);
            }
            RepaintCube();

            /*//
            Cube c = new Cube();
            c.RotateRight(Cube.Back);
            c.RotateRight(Cube.Back);
            Solution.Key key_0 = new Solution.Key(c);
            c.Shift1();
            Solution.Key key_1 = new Solution.Key(c);
            c.Shift1();
            Solution.Key key_2 = new Solution.Key(c);
            c.Shift1();
            Solution.Key key_3 = new Solution.Key(c);
            Debug.Assert(key_0.CompareTo(key_3) == 0);
            c.Shift2();
            Solution.Key key_4 = new Solution.Key(c);
            c.Shift2();
            Solution.Key key_5 = new Solution.Key(c);
            c.Shift2();
            Solution.Key key_6 = new Solution.Key(c);
            Debug.Assert(key_0.CompareTo(key_6) == 0);
            c.Shift1();
            c.Shift2();
            Solution.Key key_7 = new Solution.Key(c);
            //*/
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
                textBox_Log.AppendText(path[i].ToString() + ": " + move.ToString() + "\r\n");

                comboBox_MoveUndo.Items.Add(move);
            }
            comboBox_MoveUndo.SelectedIndex = 0;

            button_SolveCorners.Enabled = true;
        }

        private void button_Solve_Click(object sender, EventArgs e)
        {
            button_Solve.Enabled = false;

            var path = solution.Solve(new CubeKey(cube));
            if(path != null)
            {
                comboBox_MoveUndo.Items.Clear();
                comboBox_MoveRedo.Items.Clear();
                for (int i = 1; i < path.Count; i++)
                {
                    Cube.Move move = Cube.Move.KeysToMove(path[i - 1], path[i]);
                    textBox_Log.AppendText(path[i].ToString() + ": " + move.ToString() + "\r\n");

                    comboBox_MoveUndo.Items.Add(move);
                }
                comboBox_MoveUndo.SelectedIndex = 0;
            }

            button_Solve.Enabled = true;
        }

        private void button_ShowNext_Click(object sender, EventArgs e)
        {
            uint face = Cube.Front;
            Debug.Assert(Cube.UpFace(face) == Cube.Up);
            Debug.Assert(Cube.DownFace(face) == Cube.Down);
            Debug.Assert(Cube.LeftFace(face) == Cube.Left);
            Debug.Assert(Cube.RightFace(face) == Cube.Right);

            face = Cube.Up;
            Debug.Assert(Cube.UpFace(face) == Cube.Right);
            Debug.Assert(Cube.DownFace(face) == Cube.Left);
            Debug.Assert(Cube.LeftFace(face) == Cube.Back);
            Debug.Assert(Cube.RightFace(face) == Cube.Front);

            face = Cube.Right;
            Debug.Assert(Cube.UpFace(face) == Cube.Back);
            Debug.Assert(Cube.DownFace(face) == Cube.Front);
            Debug.Assert(Cube.LeftFace(face) == Cube.Up);
            Debug.Assert(Cube.RightFace(face) == Cube.Down);

            face = Cube.Back;
            Debug.Assert(Cube.UpFace(face) == Cube.Down);
            Debug.Assert(Cube.DownFace(face) == Cube.Up);
            Debug.Assert(Cube.LeftFace(face) == Cube.Left);
            Debug.Assert(Cube.RightFace(face) == Cube.Right);

            face = Cube.Down;
            Debug.Assert(Cube.UpFace(face) == Cube.Left);
            Debug.Assert(Cube.DownFace(face) == Cube.Right);
            Debug.Assert(Cube.LeftFace(face) == Cube.Back);
            Debug.Assert(Cube.RightFace(face) == Cube.Front);

            face = Cube.Left;
            Debug.Assert(Cube.UpFace(face) == Cube.Front);
            Debug.Assert(Cube.DownFace(face) == Cube.Back);
            Debug.Assert(Cube.LeftFace(face) == Cube.Up);
            Debug.Assert(Cube.RightFace(face) == Cube.Down);

            var last = solution.cube_rings[1];//.Last();
            if (++selected_cube >= last.Count)
            {
                selected_cube = 0;
            }

            Random rnd = new Random();
            selected_cube = rnd.Next(0, last.Count);

            //cube.RotateX();
            //RepaintCube();
            //if(true)
            //    return;

//            cube.MiddleKey = last[selected_cube].middles;
//            cube.CornerKey = last[selected_cube].corners;

            List<CubeKey> sk = new List<CubeKey>(last.Count);

            foreach(var key in last)
            {
                Cube c = new Cube(key);

                CubeKey x0 = new CubeKey(c);
                c.Shift1();
                CubeKey x1 = new CubeKey(c);
                c.Shift1();
                CubeKey x2 = new CubeKey(c);
                c.Shift1();

                Debug.Assert(c.MiddleKey == x0.middles);
                Debug.Assert(c.CornerKey == x0.corners);

                c.Shift2();
                CubeKey x3 = new CubeKey(c);
                c.Shift2();
                CubeKey x4 = new CubeKey(c);
                c.Shift2();

                Debug.Assert(c.MiddleKey == x0.middles);
                Debug.Assert(c.CornerKey == x0.corners);

                c.Shift1();
                c.Shift2();
                CubeKey x5 = new CubeKey(c);

                Debug.Assert(last.BinarySearch(x0) >= 0);
                Debug.Assert(last.BinarySearch(x1) >= 0);
                Debug.Assert(last.BinarySearch(x2) >= 0);
                Debug.Assert(last.BinarySearch(x3) >= 0);
                Debug.Assert(last.BinarySearch(x4) >= 0);
                Debug.Assert(last.BinarySearch(x5) >= 0);

                //CubeKey m = Utils.Min(Utils.Min(Utils.Min(Utils.Min(Utils.Min(x0, x1), x2), x3), x4), x5);

                //CubeKey m = Utils.Min(Utils.Min(x3, x4), x0);
                //CubeKey m = Utils.Min(Utils.Min(Utils.Min(Utils.Min(Utils.Min(x5, x4), x3), x2), x1), x0);
                CubeKey m = Utils.Min(Utils.Min(x3, x4), x0);
                sk.Add(m);
            }
            sk.DistinctValues();

            /*//
            List<CubeKey> test = new List<CubeKey>(last.Count);
            foreach (var key in sk)
            {
                Cube c = new Cube();
                c.MiddleKey = key.middles;
                c.CornerKey = key.corners;

                CubeKey x = new CubeKey(c);

                c.Shift();
                c.Shift();
                CubeKey y = new CubeKey(c);

                c.Shift();
                c.Shift();
                CubeKey z = new CubeKey(c);

                Debug.Assert(last.BinarySearch(x) >= 0);
                Debug.Assert(last.BinarySearch(y) >= 0);
                Debug.Assert(last.BinarySearch(z) >= 0);

                test.Add(x);
                test.Add(y);
                test.Add(z);
            }
            test.DistinctValues();

            Debug.Assert(test.Count == last.Count);
            for(int i = 0; i < test.Count; i++)
            {
                Debug.Assert(test[i] == last[i]);
            }
            //*/

            /*//
            MoveTrack track = algorithm.Run(cube);
            if(track != null)
            {
                int a = cube.CountSolvedMiddles;
                track.PlayForward(cube);
                int b = cube.CountSolvedMiddles;
                textBox_Log.AppendText(a.ToString() + " -> " + b.ToString() + "\r\n");
                textBox_Log.AppendText(track.Count.ToString() + ": " + track.ToString() + "\r\n");
            }
            else
            //*/
            {
                int total = 0;
                for (MoveTrack path; (path = algorithm.RunOnce(cross)) != null;)
                {
                    int a = cube.CountSolvedMiddles;
                    path.PlayForward(cube);
                    total += path.Count;
                    int b = cube.CountSolvedMiddles;
                    textBox_Log.AppendText(a.ToString() + " -> " + b.ToString() + "\r\n");
                    textBox_Log.AppendText(path.Count.ToString() + ": " + path.ToString() + "\r\n");

                    cross.Transform = path.PlayForward(Cross.IDENTITY).Transform;

                    Console.WriteLine(cross);
                }
                textBox_Log.AppendText("Total moves: " + total.ToString() + "\r\n");
            }

            RepaintCube();
        }

        private void button_ApplyCommand_Click(object sender, EventArgs e)
        {
            string command = textBox_Command.Text.ToUpper();

            MoveTrack track = new MoveTrack(command);
            textBox_Log.AppendText(track.ToString() + "\r\n");

            track.PlayForward(cube);

            //foreach(MiddleElement me in cube.MiddleDiff(new Cube()))
            //{
            //    textBox_Log.AppendText(me.ToString() + "\r\n");
            //}
/*//
            Algorithm alg = new Algorithm(track);

            CubeKey key = new CubeKey(cube);

            int i = cube.CountSolvedMiddles;
            MoveTrack best = null;
            foreach(MoveTrack moves in alg.tracks)
            {
                moves.PlayForward(cube);

                int j = cube.CountSolvedMiddles;
                if(j > i)
                {
                    i = j;
                    best = moves;
                }

                moves.PlayBackward(cube);
                Debug.Assert(cube.MiddleKey == key.middles);
                Debug.Assert(cube.CornerKey == key.corners);
            }

            if(best != null)
            {
                best.PlayForward(cube);
                textBox_Log.AppendText(best.ToString() + "\r\n");
            }
*///
            RepaintCube();

            /*//
            HashSet<string> map = new HashSet<string>();

            var test = new Cube.Rotator();
            map.Add(test.ToString());
            foreach(uint dir_x in Direction.Items())
            {
                test.RotateClockwise(Cube.Down);
                map.Add(test.ToString());
                foreach (uint dir_y in Direction.Items())
                {
                    test.RotateClockwise(Cube.Left);
                    map.Add(test.ToString());
                    foreach (uint dir_z in Direction.Items())
                    {
                        test.RotateClockwise(Cube.Front);
                        map.Add(test.ToString());
                    }
                }
            }
            foreach(string s in map)
            {
                textBox_Log.AppendText(s + "\r\n");
            }
            //*/
        }

        private void buttonRewindCommand_Click(object sender, EventArgs e)
        {
            string command = textBox_Command.Text.ToUpper();

            MoveTrack alg = new MoveTrack(command).Reverse();

            textBox_Log.AppendText(alg.ToString() + "\r\n");

            alg.PlayForward(cube);
            RepaintCube();
        }

        private void button_AddCommand_Click(object sender, EventArgs e)
        {
            string command = textBox_Command.Text.ToUpper();
            MoveTrack track = new MoveTrack(command);
            algorithm.Add(track);
            algorithm.Add(track.Reverse());
        }

        private void button_SaveCommand_Click(object sender, EventArgs e)
        {
            algorithm.Save("sequences.txt");
        }
    }
}
