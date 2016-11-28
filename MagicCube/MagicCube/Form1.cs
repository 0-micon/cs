﻿using System;
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

        public const float cx = 24f;
        public const float cy = 24f;

        static readonly Point[] element_positions =
        {
            new Point(1, 0), // Up
            new Point(2, 1), // Right
            new Point(1, 2), // Down
            new Point(0, 1), // Left
        };

        uint selected_face;

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
            g.FillRectangle(br, x + cx, y + cy, cx, cy);
            if (selected)
            {
                g.DrawRectangle(Pens.Black, x + cx, y + cy, cx - 1, cy - 1);
            }

            foreach(uint i in Direction.Items())
            {
                br = new SolidBrush(Color.FromName(FaceInfo.items[cube.MiddleElementAt(face, Direction.Sum(direction, i))].color));
                g.FillRectangle(br, x + cx * element_positions[i].X, y + cy * element_positions[i].Y, cx, cy);
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
            cube.RotateRight(selected_face);
            cube.RotateRight(selected_face);
            cube.RotateRight(selected_face);
            RepaintCube();
        }

        private void button_RotateRight_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Right\r\n";
            cube.RotateRight(selected_face);
            RepaintCube();
        }

        private void button_MiddleKey_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += "Middle Key: " + cube.MiddleKey + "\r\n";
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

        private void button_Solve_Click(object sender, EventArgs e)
        {
            //Solution.PrecomputeMoves(7);
            var solution = Solution.RunTo(cube.MiddleKey);
            for (int i = 1; i < solution.Count; i++)
            {
                textBox_Log.Text += solution[i].ToString() + ": " + FaceInfo.MoveInfo(solution[i - 1], solution[i]) + "\r\n";
            }
        }
    }
}
