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
            this.button_GetKeys = new System.Windows.Forms.Button();
            this.button_SetMiddleKey = new System.Windows.Forms.Button();
            this.textBox_MiddleKey = new System.Windows.Forms.TextBox();
            this.button_Solve = new System.Windows.Forms.Button();
            this.comboBox_MoveUndo = new System.Windows.Forms.ComboBox();
            this.comboBox_MoveRedo = new System.Windows.Forms.ComboBox();
            this.button_MoveUndo = new System.Windows.Forms.Button();
            this.button_MoveRedo = new System.Windows.Forms.Button();
            this.button_RandomMove = new System.Windows.Forms.Button();
            this.button_SolveCorners = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panel_Cube
            // 
            this.panel_Cube.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_Cube.Location = new System.Drawing.Point(12, 32);
            this.panel_Cube.Name = "panel_Cube";
            this.panel_Cube.Size = new System.Drawing.Size(400, 300);
            this.panel_Cube.TabIndex = 0;
            this.panel_Cube.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Cube_Paint);
            this.panel_Cube.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_Cube_MouseUp);
            // 
            // textBox_Log
            // 
            this.textBox_Log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Log.Location = new System.Drawing.Point(418, 32);
            this.textBox_Log.Multiline = true;
            this.textBox_Log.Name = "textBox_Log";
            this.textBox_Log.ReadOnly = true;
            this.textBox_Log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_Log.Size = new System.Drawing.Size(234, 398);
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
            // button_GetKeys
            // 
            this.button_GetKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_GetKeys.Location = new System.Drawing.Point(204, 338);
            this.button_GetKeys.Name = "button_GetKeys";
            this.button_GetKeys.Size = new System.Drawing.Size(75, 23);
            this.button_GetKeys.TabIndex = 4;
            this.button_GetKeys.Text = "&Get Keys";
            this.button_GetKeys.UseVisualStyleBackColor = true;
            this.button_GetKeys.Click += new System.EventHandler(this.button_GetKeys_Click);
            // 
            // button_SetMiddleKey
            // 
            this.button_SetMiddleKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_SetMiddleKey.Location = new System.Drawing.Point(12, 396);
            this.button_SetMiddleKey.Name = "button_SetMiddleKey";
            this.button_SetMiddleKey.Size = new System.Drawing.Size(87, 23);
            this.button_SetMiddleKey.TabIndex = 5;
            this.button_SetMiddleKey.Text = "Set Middle &Key";
            this.button_SetMiddleKey.UseVisualStyleBackColor = true;
            this.button_SetMiddleKey.Click += new System.EventHandler(this.button_SetMiddleKey_Click);
            // 
            // textBox_MiddleKey
            // 
            this.textBox_MiddleKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_MiddleKey.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textBox_MiddleKey.Location = new System.Drawing.Point(105, 398);
            this.textBox_MiddleKey.Name = "textBox_MiddleKey";
            this.textBox_MiddleKey.Size = new System.Drawing.Size(138, 20);
            this.textBox_MiddleKey.TabIndex = 6;
            this.textBox_MiddleKey.TextChanged += new System.EventHandler(this.textBox_MiddleKey_TextChanged);
            this.textBox_MiddleKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_MiddleKey_KeyPress);
            // 
            // button_Solve
            // 
            this.button_Solve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Solve.Location = new System.Drawing.Point(12, 367);
            this.button_Solve.Name = "button_Solve";
            this.button_Solve.Size = new System.Drawing.Size(90, 23);
            this.button_Solve.TabIndex = 7;
            this.button_Solve.Text = "&Solve Middle";
            this.button_Solve.UseVisualStyleBackColor = true;
            this.button_Solve.Click += new System.EventHandler(this.button_Solve_Click);
            // 
            // comboBox_MoveUndo
            // 
            this.comboBox_MoveUndo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MoveUndo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBox_MoveUndo.FormattingEnabled = true;
            this.comboBox_MoveUndo.IntegralHeight = false;
            this.comboBox_MoveUndo.Location = new System.Drawing.Point(12, 5);
            this.comboBox_MoveUndo.Name = "comboBox_MoveUndo";
            this.comboBox_MoveUndo.Size = new System.Drawing.Size(137, 21);
            this.comboBox_MoveUndo.TabIndex = 8;
            // 
            // comboBox_MoveRedo
            // 
            this.comboBox_MoveRedo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_MoveRedo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MoveRedo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBox_MoveRedo.FormattingEnabled = true;
            this.comboBox_MoveRedo.Location = new System.Drawing.Point(315, 5);
            this.comboBox_MoveRedo.Name = "comboBox_MoveRedo";
            this.comboBox_MoveRedo.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.comboBox_MoveRedo.Size = new System.Drawing.Size(137, 21);
            this.comboBox_MoveRedo.TabIndex = 9;
            // 
            // button_MoveUndo
            // 
            this.button_MoveUndo.Location = new System.Drawing.Point(155, 5);
            this.button_MoveUndo.Name = "button_MoveUndo";
            this.button_MoveUndo.Size = new System.Drawing.Size(75, 23);
            this.button_MoveUndo.TabIndex = 10;
            this.button_MoveUndo.Text = "&Undo";
            this.button_MoveUndo.UseVisualStyleBackColor = true;
            this.button_MoveUndo.Click += new System.EventHandler(this.button_MoveUndo_Click);
            // 
            // button_MoveRedo
            // 
            this.button_MoveRedo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_MoveRedo.Location = new System.Drawing.Point(234, 5);
            this.button_MoveRedo.Name = "button_MoveRedo";
            this.button_MoveRedo.Size = new System.Drawing.Size(75, 23);
            this.button_MoveRedo.TabIndex = 11;
            this.button_MoveRedo.Text = "R&edo";
            this.button_MoveRedo.UseVisualStyleBackColor = true;
            this.button_MoveRedo.Click += new System.EventHandler(this.button_MoveRedo_Click);
            // 
            // button_RandomMove
            // 
            this.button_RandomMove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_RandomMove.Location = new System.Drawing.Point(577, 3);
            this.button_RandomMove.Name = "button_RandomMove";
            this.button_RandomMove.Size = new System.Drawing.Size(75, 23);
            this.button_RandomMove.TabIndex = 12;
            this.button_RandomMove.Text = "R&andom";
            this.button_RandomMove.UseVisualStyleBackColor = true;
            this.button_RandomMove.Click += new System.EventHandler(this.button_RandomMove_Click);
            // 
            // button_SolveCorners
            // 
            this.button_SolveCorners.Location = new System.Drawing.Point(108, 367);
            this.button_SolveCorners.Name = "button_SolveCorners";
            this.button_SolveCorners.Size = new System.Drawing.Size(90, 23);
            this.button_SolveCorners.TabIndex = 13;
            this.button_SolveCorners.Text = "Solve &Corners";
            this.button_SolveCorners.UseVisualStyleBackColor = true;
            this.button_SolveCorners.Click += new System.EventHandler(this.button_SolveCorners_Click);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 442);
            this.Controls.Add(this.button_SolveCorners);
            this.Controls.Add(this.button_RandomMove);
            this.Controls.Add(this.button_MoveRedo);
            this.Controls.Add(this.button_MoveUndo);
            this.Controls.Add(this.comboBox_MoveRedo);
            this.Controls.Add(this.comboBox_MoveUndo);
            this.Controls.Add(this.button_Solve);
            this.Controls.Add(this.textBox_MiddleKey);
            this.Controls.Add(this.button_SetMiddleKey);
            this.Controls.Add(this.button_GetKeys);
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
        private System.Windows.Forms.Button button_GetKeys;
        private System.Windows.Forms.Button button_SetMiddleKey;
        private System.Windows.Forms.TextBox textBox_MiddleKey;
        private System.Windows.Forms.Button button_Solve;
        private System.Windows.Forms.ComboBox comboBox_MoveUndo;
        private System.Windows.Forms.ComboBox comboBox_MoveRedo;
        private System.Windows.Forms.Button button_MoveUndo;
        private System.Windows.Forms.Button button_MoveRedo;
        private System.Windows.Forms.Button button_RandomMove;
        private System.Windows.Forms.Button button_SolveCorners;
    }
}

