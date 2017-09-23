namespace EX3_BasicMoves
{
    partial class BasicMoves
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
            this.HomeAxisButton = new System.Windows.Forms.Button();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.posTextBox = new System.Windows.Forms.TextBox();
            this.DecelTextBox = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.AccelTextBox = new System.Windows.Forms.TextBox();
            this.AccelerationLabel = new System.Windows.Forms.Label();
            this.enableButton = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.DistanceTextBox = new System.Windows.Forms.TextBox();
            this.VelocityTextBox = new System.Windows.Forms.TextBox();
            this.haltMoveButton = new System.Windows.Forms.Button();
            this.velocityLabel = new System.Windows.Forms.Label();
            this.doMoveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // HomeAxisButton
            // 
            this.HomeAxisButton.Location = new System.Drawing.Point(59, 12);
            this.HomeAxisButton.Name = "HomeAxisButton";
            this.HomeAxisButton.Size = new System.Drawing.Size(208, 32);
            this.HomeAxisButton.TabIndex = 42;
            this.HomeAxisButton.Text = "Home Axis";
            this.HomeAxisButton.Click += new System.EventHandler(this.HomeAxisButton_Click);
            // 
            // Label8
            // 
            this.Label8.Location = new System.Drawing.Point(243, 132);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(82, 16);
            this.Label8.TabIndex = 41;
            this.Label8.Text = "counts/s^2";
            // 
            // Label6
            // 
            this.Label6.Location = new System.Drawing.Point(243, 60);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(71, 16);
            this.Label6.TabIndex = 39;
            this.Label6.Text = "counts/s";
            // 
            // Label7
            // 
            this.Label7.Location = new System.Drawing.Point(243, 92);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(82, 16);
            this.Label7.TabIndex = 40;
            this.Label7.Text = "counts/s^2";
            // 
            // Timer1
            // 
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // Label5
            // 
            this.Label5.Location = new System.Drawing.Point(243, 212);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(48, 16);
            this.Label5.TabIndex = 38;
            this.Label5.Text = "counts";
            // 
            // Label4
            // 
            this.Label4.Location = new System.Drawing.Point(243, 172);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(48, 16);
            this.Label4.TabIndex = 37;
            this.Label4.Text = "counts";
            // 
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(27, 212);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(136, 16);
            this.Label3.TabIndex = 36;
            this.Label3.Text = "Actual Position";
            // 
            // posTextBox
            // 
            this.posTextBox.Location = new System.Drawing.Point(163, 212);
            this.posTextBox.Name = "posTextBox";
            this.posTextBox.ReadOnly = true;
            this.posTextBox.Size = new System.Drawing.Size(72, 20);
            this.posTextBox.TabIndex = 35;
            this.posTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DecelTextBox
            // 
            this.DecelTextBox.Location = new System.Drawing.Point(163, 132);
            this.DecelTextBox.Name = "DecelTextBox";
            this.DecelTextBox.Size = new System.Drawing.Size(72, 20);
            this.DecelTextBox.TabIndex = 34;
            this.DecelTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(27, 132);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(100, 23);
            this.Label1.TabIndex = 33;
            this.Label1.Text = "Deceleration";
            // 
            // AccelTextBox
            // 
            this.AccelTextBox.Location = new System.Drawing.Point(163, 92);
            this.AccelTextBox.Name = "AccelTextBox";
            this.AccelTextBox.Size = new System.Drawing.Size(72, 20);
            this.AccelTextBox.TabIndex = 32;
            this.AccelTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // AccelerationLabel
            // 
            this.AccelerationLabel.Location = new System.Drawing.Point(27, 92);
            this.AccelerationLabel.Name = "AccelerationLabel";
            this.AccelerationLabel.Size = new System.Drawing.Size(100, 23);
            this.AccelerationLabel.TabIndex = 31;
            this.AccelerationLabel.Text = "Acceleration";
            // 
            // enableButton
            // 
            this.enableButton.Location = new System.Drawing.Point(11, 252);
            this.enableButton.Name = "enableButton";
            this.enableButton.Size = new System.Drawing.Size(88, 24);
            this.enableButton.TabIndex = 30;
            this.enableButton.Text = "Amp Disable";
            this.enableButton.Click += new System.EventHandler(this.enableButton_Click);
            // 
            // Label2
            // 
            this.Label2.Location = new System.Drawing.Point(27, 172);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(88, 16);
            this.Label2.TabIndex = 29;
            this.Label2.Text = "Move Distance ";
            // 
            // DistanceTextBox
            // 
            this.DistanceTextBox.Location = new System.Drawing.Point(163, 172);
            this.DistanceTextBox.Name = "DistanceTextBox";
            this.DistanceTextBox.Size = new System.Drawing.Size(72, 20);
            this.DistanceTextBox.TabIndex = 28;
            this.DistanceTextBox.Text = "1000";
            this.DistanceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // VelocityTextBox
            // 
            this.VelocityTextBox.Location = new System.Drawing.Point(163, 60);
            this.VelocityTextBox.Name = "VelocityTextBox";
            this.VelocityTextBox.Size = new System.Drawing.Size(72, 20);
            this.VelocityTextBox.TabIndex = 24;
            this.VelocityTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // haltMoveButton
            // 
            this.haltMoveButton.Location = new System.Drawing.Point(219, 252);
            this.haltMoveButton.Name = "haltMoveButton";
            this.haltMoveButton.Size = new System.Drawing.Size(88, 24);
            this.haltMoveButton.TabIndex = 27;
            this.haltMoveButton.Text = "Halt Move";
            this.haltMoveButton.Click += new System.EventHandler(this.haltMoveButton_Click);
            // 
            // velocityLabel
            // 
            this.velocityLabel.Location = new System.Drawing.Point(27, 60);
            this.velocityLabel.Name = "velocityLabel";
            this.velocityLabel.Size = new System.Drawing.Size(64, 16);
            this.velocityLabel.TabIndex = 26;
            this.velocityLabel.Text = "Velocity";
            // 
            // doMoveButton
            // 
            this.doMoveButton.Enabled = false;
            this.doMoveButton.Location = new System.Drawing.Point(123, 252);
            this.doMoveButton.Name = "doMoveButton";
            this.doMoveButton.Size = new System.Drawing.Size(80, 24);
            this.doMoveButton.TabIndex = 25;
            this.doMoveButton.Text = "Do Move";
            this.doMoveButton.Click += new System.EventHandler(this.doMoveButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(333, 301);
            this.Controls.Add(this.HomeAxisButton);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.posTextBox);
            this.Controls.Add(this.DecelTextBox);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.AccelTextBox);
            this.Controls.Add(this.AccelerationLabel);
            this.Controls.Add(this.enableButton);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.DistanceTextBox);
            this.Controls.Add(this.VelocityTextBox);
            this.Controls.Add(this.haltMoveButton);
            this.Controls.Add(this.velocityLabel);
            this.Controls.Add(this.doMoveButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button HomeAxisButton;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Timer Timer1;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.TextBox posTextBox;
        internal System.Windows.Forms.TextBox DecelTextBox;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox AccelTextBox;
        internal System.Windows.Forms.Label AccelerationLabel;
        internal System.Windows.Forms.Button enableButton;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox DistanceTextBox;
        internal System.Windows.Forms.TextBox VelocityTextBox;
        internal System.Windows.Forms.Button haltMoveButton;
        internal System.Windows.Forms.Label velocityLabel;
        internal System.Windows.Forms.Button doMoveButton;

    }
}

