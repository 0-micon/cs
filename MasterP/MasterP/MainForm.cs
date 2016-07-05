using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace MasterP
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonClipboard_Click(object sender, EventArgs e)
        {
            // todo: put content to the clipboard
            var s = textBox1.Text + '-' + textBox2.Text + '\n';
            byte[] b = Encoding.ASCII.GetBytes(s);

            using (var md5 = MD5.Create())
            {
                byte[] h = md5.ComputeHash(b);
                byte[] hex = new byte[h.Length * 2];

                for (int i = 0; i < h.Length; i++)
                {
                    var hb = (h[i] >> 4);
                    var lb = (h[i] & 0xF);

                    hex[i * 2    ] = (byte)(hb < 10 ? '0' + hb : 'a' + hb - 10);
                    hex[i * 2 + 1] = (byte)(lb < 10 ? '0' + lb : 'a' + lb - 10);
                }
                s = Convert.ToBase64String(hex);
            }

            int start = (int)numericUpDown1.Value - 1;
            int count = (int)numericUpDown2.Value - start;
            s = s.Substring(start, count);

            textBoxResult.Text = s;
            Clipboard.SetText(s);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;

            numericUpDown1.Value = 1;
            numericUpDown2.Value = 1;

            textBoxResult.Text = string.Empty;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.UseSystemPasswordChar = !checkBox1.Checked;
            checkBox1.ImageIndex = checkBox1.Checked ? 1 : 0;
            toolTip1.SetToolTip(checkBox1, checkBox1.Checked ? "Lock" : "View");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !checkBox2.Checked;
            checkBox2.ImageIndex = checkBox2.Checked ? 1 : 0;
            toolTip1.SetToolTip(checkBox2, checkBox2.Checked ? "Lock" : "View");
        }

        private void checkBoxResult_CheckedChanged(object sender, EventArgs e)
        {
            textBoxResult.UseSystemPasswordChar = !checkBoxResult.Checked;
            checkBoxResult.ImageIndex = checkBoxResult.Checked ? 1 : 0;
            toolTip1.SetToolTip(checkBoxResult, checkBoxResult.Checked ? "Lock" : "View");
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }
    }
}
