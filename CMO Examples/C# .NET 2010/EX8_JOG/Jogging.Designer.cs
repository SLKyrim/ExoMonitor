namespace EX8_JOG
{
    partial class Jogging
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
            this.components = new System.ComponentModel.Container();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.JogNegButton = new System.Windows.Forms.Button();
            this.JogPosButton = new System.Windows.Forms.Button();
            this.ActPosVar = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.ActPosLabel = new System.Windows.Forms.Label();
            this.VelocityTextBox = new System.Windows.Forms.TextBox();
            this.DecelTextBox = new System.Windows.Forms.TextBox();
            this.AccelTextBox = new System.Windows.Forms.TextBox();
            this.DecelLabel = new System.Windows.Forms.Label();
            this.AccelLabel = new System.Windows.Forms.Label();
            this.VelocityLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(165, 168);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(39, 13);
            this.Label4.TabIndex = 27;
            this.Label4.Text = "counts";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(200, 122);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(61, 13);
            this.Label3.TabIndex = 26;
            this.Label3.Text = "counts/s^2";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(200, 80);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(61, 13);
            this.Label2.TabIndex = 25;
            this.Label2.Text = "counts/s^2";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(200, 36);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(49, 13);
            this.Label1.TabIndex = 24;
            this.Label1.Text = "counts/s";
            // 
            // JogNegButton
            // 
            this.JogNegButton.Location = new System.Drawing.Point(44, 207);
            this.JogNegButton.Name = "JogNegButton";
            this.JogNegButton.Size = new System.Drawing.Size(75, 23);
            this.JogNegButton.TabIndex = 23;
            this.JogNegButton.Text = "Jog Neg";
            this.JogNegButton.UseVisualStyleBackColor = true;
            // 
            // JogPosButton
            // 
            this.JogPosButton.Location = new System.Drawing.Point(145, 207);
            this.JogPosButton.Name = "JogPosButton";
            this.JogPosButton.Size = new System.Drawing.Size(75, 23);
            this.JogPosButton.TabIndex = 22;
            this.JogPosButton.Text = "Jog Pos";
            this.JogPosButton.UseVisualStyleBackColor = true;
            // 
            // ActPosVar
            // 
            this.ActPosVar.AutoSize = true;
            this.ActPosVar.Location = new System.Drawing.Point(107, 168);
            this.ActPosVar.Name = "ActPosVar";
            this.ActPosVar.Size = new System.Drawing.Size(52, 13);
            this.ActPosVar.TabIndex = 21;
            this.ActPosVar.Text = "               ";
            // 
            // Timer1
            // 
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // ActPosLabel
            // 
            this.ActPosLabel.AutoSize = true;
            this.ActPosLabel.Location = new System.Drawing.Point(24, 168);
            this.ActPosLabel.Name = "ActPosLabel";
            this.ActPosLabel.Size = new System.Drawing.Size(77, 13);
            this.ActPosLabel.TabIndex = 20;
            this.ActPosLabel.Text = "Actual Position";
            // 
            // VelocityTextBox
            // 
            this.VelocityTextBox.Location = new System.Drawing.Point(110, 33);
            this.VelocityTextBox.Name = "VelocityTextBox";
            this.VelocityTextBox.Size = new System.Drawing.Size(84, 20);
            this.VelocityTextBox.TabIndex = 19;
            // 
            // DecelTextBox
            // 
            this.DecelTextBox.Location = new System.Drawing.Point(110, 119);
            this.DecelTextBox.Name = "DecelTextBox";
            this.DecelTextBox.Size = new System.Drawing.Size(84, 20);
            this.DecelTextBox.TabIndex = 18;
            // 
            // AccelTextBox
            // 
            this.AccelTextBox.Location = new System.Drawing.Point(110, 77);
            this.AccelTextBox.Name = "AccelTextBox";
            this.AccelTextBox.Size = new System.Drawing.Size(84, 20);
            this.AccelTextBox.TabIndex = 17;
            // 
            // DecelLabel
            // 
            this.DecelLabel.AutoSize = true;
            this.DecelLabel.Location = new System.Drawing.Point(24, 122);
            this.DecelLabel.Name = "DecelLabel";
            this.DecelLabel.Size = new System.Drawing.Size(70, 13);
            this.DecelLabel.TabIndex = 16;
            this.DecelLabel.Text = "Deceleration:";
            // 
            // AccelLabel
            // 
            this.AccelLabel.AutoSize = true;
            this.AccelLabel.Location = new System.Drawing.Point(24, 80);
            this.AccelLabel.Name = "AccelLabel";
            this.AccelLabel.Size = new System.Drawing.Size(69, 13);
            this.AccelLabel.TabIndex = 15;
            this.AccelLabel.Text = "Acceleration:";
            // 
            // VelocityLabel
            // 
            this.VelocityLabel.AutoSize = true;
            this.VelocityLabel.Location = new System.Drawing.Point(24, 33);
            this.VelocityLabel.Name = "VelocityLabel";
            this.VelocityLabel.Size = new System.Drawing.Size(47, 13);
            this.VelocityLabel.TabIndex = 14;
            this.VelocityLabel.Text = "Velocity:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.JogNegButton);
            this.Controls.Add(this.JogPosButton);
            this.Controls.Add(this.ActPosVar);
            this.Controls.Add(this.ActPosLabel);
            this.Controls.Add(this.VelocityTextBox);
            this.Controls.Add(this.DecelTextBox);
            this.Controls.Add(this.AccelTextBox);
            this.Controls.Add(this.DecelLabel);
            this.Controls.Add(this.AccelLabel);
            this.Controls.Add(this.VelocityLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button JogNegButton;
        internal System.Windows.Forms.Button JogPosButton;
        internal System.Windows.Forms.Label ActPosVar;
        internal System.Windows.Forms.Timer Timer1;
        internal System.Windows.Forms.Label ActPosLabel;
        internal System.Windows.Forms.TextBox VelocityTextBox;
        internal System.Windows.Forms.TextBox DecelTextBox;
        internal System.Windows.Forms.TextBox AccelTextBox;
        internal System.Windows.Forms.Label DecelLabel;
        internal System.Windows.Forms.Label AccelLabel;
        internal System.Windows.Forms.Label VelocityLabel;
    }
}

