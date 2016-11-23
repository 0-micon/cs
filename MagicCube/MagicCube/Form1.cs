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

        uint selected_face;

        public Form_Main()
        {
            InitializeComponent();
        }

        private void DrawFace(Graphics g, uint face, uint direction, float x, float y, float cx, float cy, bool selected)
        {
            SolidBrush br = new SolidBrush(Color.FromName(FaceInfo.items[face].color));
            g.FillRectangle(br, x + cx, y + cy, cx, cy);
            if (selected)
            {
                g.DrawRectangle(Pens.Black, x + cx, y + cy, cx - 1, cy - 1);
            }

            br = new SolidBrush(Color.FromName(FaceInfo.items[cube.MiddleElementAt(face, Direction.Sum(direction, Direction.UP))].color));
            g.FillRectangle(br, x + cx, y, cx, cy);

            br = new SolidBrush(Color.FromName(FaceInfo.items[cube.MiddleElementAt(face, Direction.Sum(direction, Direction.DOWN))].color));
            g.FillRectangle(br, x + cx, y + 2 * cy, cx, cy);

            br = new SolidBrush(Color.FromName(FaceInfo.items[cube.MiddleElementAt(face, Direction.Sum(direction, Direction.LEFT))].color));
            g.FillRectangle(br, x, y + cy, cx, cy);

            br = new SolidBrush(Color.FromName(FaceInfo.items[cube.MiddleElementAt(face, Direction.Sum(direction, Direction.RIGHT))].color));
            g.FillRectangle(br, x + 2 * cx, y + cy, cx, cy);
        }


        private void panel_Cube_Paint(object sender, PaintEventArgs e)
        {

            Control ctrl = sender as Control;
            if(ctrl == null)
            {
                // no control => no paint
                return;
            }

            Graphics g = e.Graphics;
            //g.ResetTransform();
            //g.TranslateTransform(ctrl.Width / 3f, ctrl.Height / 2f);

            float cx = 24f;
            float cy = 24f;

            for(uint face = 0; face < Cube.FACE_NUM; face++)
            {
                float x = 3 * cx * FaceInfo.items[face].location.X;
                float y = 3 * cy * FaceInfo.items[face].location.Y;
                DrawFace(g, face, FaceInfo.items[face].direction,
                    x, y, cx, cy, selected_face == face);
            }
        }

        private void button_RotateLeft_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Left\r\n";
            cube.RotateRight(selected_face);
            cube.RotateRight(selected_face);
            cube.RotateRight(selected_face);
            panel_Cube.Invalidate();
        }

        private void panel_Cube_MouseUp(object sender, MouseEventArgs e)
        {
            float cx = 24f;
            float cy = 24f;

            for (uint face = 0; face < Cube.FACE_NUM; face++)
            {
                float x = 3 * cx * FaceInfo.items[face].location.X;
                float y = 3 * cy * FaceInfo.items[face].location.Y;
                RectangleF rc = new RectangleF(x, y, 3 * cx, 3 * cy);
                if (rc.Contains(e.Location))
                {
                    selected_face = face;
                    panel_Cube.Invalidate();
                }
            }
        }

        private void button_RotateRight_Click(object sender, EventArgs e)
        {
            textBox_Log.Text += FaceInfo.items[selected_face].name + " face (" + FaceInfo.items[selected_face].color + "): Rotate Right\r\n";
            cube.RotateRight(selected_face);
            panel_Cube.Invalidate();
        }
    }
}
