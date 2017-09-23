namespace EX6_PVT
{
    partial class PVT
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
            this.HomeAxisButton = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.numberOfEventsTextBox = new System.Windows.Forms.TextBox();
            this.StopPVTButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // HomeAxisButton
            // 
            this.HomeAxisButton.Location = new System.Drawing.Point(38, 28);
            this.HomeAxisButton.Name = "HomeAxisButton";
            this.HomeAxisButton.Size = new System.Drawing.Size(208, 32);
            this.HomeAxisButton.TabIndex = 12;
            this.HomeAxisButton.Text = "Home Axis";
            this.HomeAxisButton.Click += new System.EventHandler(this.HomeAxisButton_Click);
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(62, 212);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(100, 23);
            this.Label1.TabIndex = 11;
            this.Label1.Text = "Number of Events";
            // 
            // numberOfEventsTextBox
            // 
            this.numberOfEventsTextBox.Location = new System.Drawing.Point(182, 212);
            this.numberOfEventsTextBox.Name = "numberOfEventsTextBox";
            this.numberOfEventsTextBox.Size = new System.Drawing.Size(56, 20);
            this.numberOfEventsTextBox.TabIndex = 10;
            this.numberOfEventsTextBox.Text = "0";
            this.numberOfEventsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // StopPVTButton
            // 
            this.StopPVTButton.Location = new System.Drawing.Point(38, 148);
            this.StopPVTButton.Name = "StopPVTButton";
            this.StopPVTButton.Size = new System.Drawing.Size(208, 32);
            this.StopPVTButton.TabIndex = 9;
            this.StopPVTButton.Text = "Stop PVT";
            this.StopPVTButton.Click += new System.EventHandler(this.StopPVTButton_Click);
            // 
            // startButton
            // 
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point(38, 84);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(208, 32);
            this.startButton.TabIndex = 8;
            this.startButton.Text = "Start PVT Motion";
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.HomeAxisButton);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.numberOfEventsTextBox);
            this.Controls.Add(this.StopPVTButton);
            this.Controls.Add(this.startButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button HomeAxisButton;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox numberOfEventsTextBox;
        internal System.Windows.Forms.Button StopPVTButton;
        internal System.Windows.Forms.Button startButton;
    }
}

