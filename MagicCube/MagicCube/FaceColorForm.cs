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
    public partial class FaceColorForm : Form
    {
        public const uint COL_NUM = 3;
        public const uint ROW_NUM = 2;

        public uint selection = 0;

        public FaceColorForm()
        {
            InitializeComponent();
        }

        SizeF ElementSize
        {
            get
            {
                Rectangle rc = ClientRectangle;
                SizeF size = new SizeF(rc.Width / (float)COL_NUM, rc.Height / (float)ROW_NUM);
                return size;
            }
        }

        private void FaceColorForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            SizeF size = ElementSize;

            float y = 0f;
            for(uint row = 0; row < ROW_NUM; row++, y += size.Height)
            {
                float x = 0f;
                for(uint col = 0; col < COL_NUM; col++, x += size.Width)
                {
                    uint i = row + ROW_NUM * col;
                    g.FillRectangle(new SolidBrush(Color.FromName(FaceInfo.items[i].color)),
                        x, y, size.Width, size.Height);
                    if(i == selection)
                    {
                        g.DrawRectangle(Pens.Black, x, y, size.Width - 1f, size.Height - 1f);
                    }
                }
            }
        }

        private void FaceColorForm_MouseUp(object sender, MouseEventArgs e)
        {
            SizeF size = ElementSize;

            float y = 0f;
            for (uint row = 0; row < ROW_NUM; row++, y += size.Height)
            {
                float x = 0f;
                for (uint col = 0; col < COL_NUM; col++, x += size.Width)
                {
                    uint i = row + ROW_NUM * col;
                    RectangleF rc = new RectangleF(x, y, size.Width, size.Height);
                    if(rc.Contains(e.X, e.Y))
                    {
                        selection = i;
                        Close();
                        return;
                    }
                }
            }
        }
    }
}
