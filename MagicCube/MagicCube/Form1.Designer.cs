namespace MagicCube
{
    partial class Form_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_Cube = new System.Windows.Forms.Panel();
            this.textBox_Log = new System.Windows.Forms.TextBox();
            this.button_RotateLeft = new System.Windows.Forms.Button();
            this.button_RotateRight = new System.Windows.Forms.Button();
            this.button_MiddleKey = new System.Windows.Forms.Button();
            this.button_SetMiddleKey = new System.Windows.Forms.Button();
            this.textBox_MiddleKey = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // panel_Cube
            // 
            this.panel_Cube.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_Cube.Location = new System.Drawing.Point(12, 12);
            this.panel_Cube.Name = "panel_Cube";
            this.panel_Cube.Size = new System.Drawing.Size(360, 320);
            this.panel_Cube.TabIndex = 0;
            this.panel_Cube.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Cube_Paint);
            this.panel_Cube.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_Cube_MouseUp);
            // 
            // textBox_Log
            // 
            this.textBox_Log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Log.Location = new System.Drawing.Point(378, 12);
            this.textBox_Log.Multiline = true;
            this.textBox_Log.Name = "textBox_Log";
            this.textBox_Log.ReadOnly = true;
            this.textBox_Log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_Log.Size = new System.Drawing.Size(194, 320);
            this.textBox_Log.TabIndex = 2;
            // 
            // button_RotateLeft
            // 
            this.button_RotateLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_RotateLeft.Location = new System.Drawing.Point(12, 338);
            this.button_RotateLeft.Name = "button_RotateLeft";
            this.button_RotateLeft.Size = new System.Drawing.Size(90, 23);
            this.button_RotateLeft.TabIndex = 3;
            this.button_RotateLeft.Text = "<= Rotate &Left";
            this.button_RotateLeft.UseVisualStyleBackColor = true;
            this.button_RotateLeft.Click += new System.EventHandler(this.button_RotateLeft_Click);
            // 
            // button_RotateRight
            // 
            this.button_RotateRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_RotateRight.Location = new System.Drawing.Point(108, 338);
            this.button_RotateRight.Name = "button_RotateRight";
            this.button_RotateRight.Size = new System.Drawing.Size(90, 23);
            this.button_RotateRight.TabIndex = 1;
            this.button_RotateRight.Text = "Rotate &Right =>";
            this.button_RotateRight.UseVisualStyleBackColor = true;
            this.button_RotateRight.Click += new System.EventHandler(this.button_RotateRight_Click);
            // 
            // button_MiddleKey
            // 
            this.button_MiddleKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_MiddleKey.Location = new System.Drawing.Point(204, 338);
            this.button_MiddleKey.Name = "button_MiddleKey";
            this.button_MiddleKey.Size = new System.Drawing.Size(75, 23);
            this.button_MiddleKey.TabIndex = 4;
            this.button_MiddleKey.Text = "&Middle Key";
            this.button_MiddleKey.UseVisualStyleBackColor = true;
            this.button_MiddleKey.Click += new System.EventHandler(this.button_MiddleKey_Click);
            // 
            // button_SetMiddleKey
            // 
            this.button_SetMiddleKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_SetMiddleKey.Location = new System.Drawing.Point(285, 338);
            this.button_SetMiddleKey.Name = "button_SetMiddleKey";
            this.button_SetMiddleKey.Size = new System.Drawing.Size(87, 23);
            this.button_SetMiddleKey.TabIndex = 5;
            this.button_SetMiddleKey.Text = "&Set Middle Key";
            this.button_SetMiddleKey.UseVisualStyleBackColor = true;
            this.button_SetMiddleKey.Click += new System.EventHandler(this.button_SetMiddleKey_Click);
            // 
            // textBox_MiddleKey
            // 
            this.textBox_MiddleKey.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBox_MiddleKey.Location = new System.Drawing.Point(378, 338);
            this.textBox_MiddleKey.Name = "textBox_MiddleKey";
            this.textBox_MiddleKey.Size = new System.Drawing.Size(194, 20);
            this.textBox_MiddleKey.TabIndex = 6;
            this.textBox_MiddleKey.TextChanged += new System.EventHandler(this.textBox_MiddleKey_TextChanged);
            this.textBox_MiddleKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_MiddleKey_KeyPress);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 362);
            this.Controls.Add(this.textBox_MiddleKey);
            this.Controls.Add(this.button_SetMiddleKey);
            this.Controls.Add(this.button_MiddleKey);
            this.Controls.Add(this.button_RotateLeft);
            this.Controls.Add(this.textBox_Log);
            this.Controls.Add(this.button_RotateRight);
            this.Controls.Add(this.panel_Cube);
            this.Name = "Form_Main";
            this.Text = "Magic Cube";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel_Cube;
        private System.Windows.Forms.TextBox textBox_Log;
        private System.Windows.Forms.Button button_RotateLeft;
        private System.Windows.Forms.Button button_RotateRight;
        private System.Windows.Forms.Button button_MiddleKey;
        private System.Windows.Forms.Button button_SetMiddleKey;
        private System.Windows.Forms.TextBox textBox_MiddleKey;
    }
}

