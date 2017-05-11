namespace RussianDraughtsGUI
{
    partial class form_Main
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
            this.panel_Board = new System.Windows.Forms.Panel();
            this.button_ShowMoves = new System.Windows.Forms.Button();
            this.textBox_GameLog = new System.Windows.Forms.TextBox();
            this.button_Apply = new System.Windows.Forms.Button();
            this.button_NextBoard = new System.Windows.Forms.Button();
            this.button_UndoMove = new System.Windows.Forms.Button();
            this.button_RedoMove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panel_Board
            // 
            this.panel_Board.Location = new System.Drawing.Point(0, 0);
            this.panel_Board.Name = "panel_Board";
            this.panel_Board.Size = new System.Drawing.Size(320, 320);
            this.panel_Board.TabIndex = 0;
            this.panel_Board.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Board_Paint);
            this.panel_Board.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_Board_MouseDown);
            // 
            // button_ShowMoves
            // 
            this.button_ShowMoves.Location = new System.Drawing.Point(13, 407);
            this.button_ShowMoves.Name = "button_ShowMoves";
            this.button_ShowMoves.Size = new System.Drawing.Size(82, 23);
            this.button_ShowMoves.TabIndex = 1;
            this.button_ShowMoves.Text = "Show &Moves";
            this.button_ShowMoves.UseVisualStyleBackColor = true;
            this.button_ShowMoves.Click += new System.EventHandler(this.button_ShowMoves_Click);
            // 
            // textBox_GameLog
            // 
            this.textBox_GameLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_GameLog.Location = new System.Drawing.Point(327, 0);
            this.textBox_GameLog.Multiline = true;
            this.textBox_GameLog.Name = "textBox_GameLog";
            this.textBox_GameLog.ReadOnly = true;
            this.textBox_GameLog.Size = new System.Drawing.Size(132, 320);
            this.textBox_GameLog.TabIndex = 2;
            // 
            // button_Apply
            // 
            this.button_Apply.Location = new System.Drawing.Point(101, 407);
            this.button_Apply.Name = "button_Apply";
            this.button_Apply.Size = new System.Drawing.Size(75, 23);
            this.button_Apply.TabIndex = 3;
            this.button_Apply.Text = "&Apply";
            this.button_Apply.UseVisualStyleBackColor = true;
            this.button_Apply.Click += new System.EventHandler(this.button_Apply_Click);
            // 
            // button_NextBoard
            // 
            this.button_NextBoard.Location = new System.Drawing.Point(183, 407);
            this.button_NextBoard.Name = "button_NextBoard";
            this.button_NextBoard.Size = new System.Drawing.Size(75, 23);
            this.button_NextBoard.TabIndex = 4;
            this.button_NextBoard.Text = "Next Board";
            this.button_NextBoard.UseVisualStyleBackColor = true;
            this.button_NextBoard.Click += new System.EventHandler(this.button_NextBoard_Click);
            // 
            // button_UndoMove
            // 
            this.button_UndoMove.Location = new System.Drawing.Point(265, 407);
            this.button_UndoMove.Name = "button_UndoMove";
            this.button_UndoMove.Size = new System.Drawing.Size(75, 23);
            this.button_UndoMove.TabIndex = 5;
            this.button_UndoMove.Text = "&Undo";
            this.button_UndoMove.UseVisualStyleBackColor = true;
            this.button_UndoMove.Click += new System.EventHandler(this.button_UndoMove_Click);
            // 
            // button_RedoMove
            // 
            this.button_RedoMove.Location = new System.Drawing.Point(347, 407);
            this.button_RedoMove.Name = "button_RedoMove";
            this.button_RedoMove.Size = new System.Drawing.Size(75, 23);
            this.button_RedoMove.TabIndex = 6;
            this.button_RedoMove.Text = "&Redo";
            this.button_RedoMove.UseVisualStyleBackColor = true;
            this.button_RedoMove.Click += new System.EventHandler(this.button_RedoMove_Click);
            // 
            // form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 442);
            this.Controls.Add(this.button_RedoMove);
            this.Controls.Add(this.button_UndoMove);
            this.Controls.Add(this.button_NextBoard);
            this.Controls.Add(this.button_Apply);
            this.Controls.Add(this.textBox_GameLog);
            this.Controls.Add(this.button_ShowMoves);
            this.Controls.Add(this.panel_Board);
            this.Name = "form_Main";
            this.Text = "Russian Draughts";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel_Board;
        private System.Windows.Forms.Button button_ShowMoves;
        private System.Windows.Forms.TextBox textBox_GameLog;
        private System.Windows.Forms.Button button_Apply;
        private System.Windows.Forms.Button button_NextBoard;
        private System.Windows.Forms.Button button_UndoMove;
        private System.Windows.Forms.Button button_RedoMove;
    }
}

