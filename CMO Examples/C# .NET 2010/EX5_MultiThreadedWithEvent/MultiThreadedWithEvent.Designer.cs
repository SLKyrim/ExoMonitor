namespace EX5_MultiThreadedWithEvent
{
    partial class MultiThreadedWithEvent
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
            this.yAxisMoveDistanceTextBox = new System.Windows.Forms.TextBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.HomeAxisButton = new System.Windows.Forms.Button();
            this.yAxisStopButton = new System.Windows.Forms.Button();
            this.yAxisStartButton = new System.Windows.Forms.Button();
            this.xAxisStartButton = new System.Windows.Forms.Button();
            this.YaxisGroupBox = new System.Windows.Forms.GroupBox();
            this.xMoveDistanceTextBox = new System.Windows.Forms.TextBox();
            this.MoveLabel = new System.Windows.Forms.Label();
            this.xAxisStopButton = new System.Windows.Forms.Button();
            this.xAxisGroupBox = new System.Windows.Forms.GroupBox();
            this.YaxisGroupBox.SuspendLayout();
            this.xAxisGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // yAxisMoveDistanceTextBox
            // 
            this.yAxisMoveDistanceTextBox.Location = new System.Drawing.Point(110, 19);
            this.yAxisMoveDistanceTextBox.Name = "yAxisMoveDistanceTextBox";
            this.yAxisMoveDistanceTextBox.Size = new System.Drawing.Size(64, 20);
            this.yAxisMoveDistanceTextBox.TabIndex = 6;
            this.yAxisMoveDistanceTextBox.Text = "1000";
            this.yAxisMoveDistanceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(19, 22);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(80, 16);
            this.Label1.TabIndex = 5;
            this.Label1.Text = "Move Distance";
            // 
            // HomeAxisButton
            // 
            this.HomeAxisButton.Location = new System.Drawing.Point(155, 12);
            this.HomeAxisButton.Name = "HomeAxisButton";
            this.HomeAxisButton.Size = new System.Drawing.Size(184, 32);
            this.HomeAxisButton.TabIndex = 9;
            this.HomeAxisButton.Text = "Home Axis";
            this.HomeAxisButton.Click += new System.EventHandler(this.HomeAxisButton_Click);
            // 
            // yAxisStopButton
            // 
            this.yAxisStopButton.Location = new System.Drawing.Point(120, 64);
            this.yAxisStopButton.Name = "yAxisStopButton";
            this.yAxisStopButton.Size = new System.Drawing.Size(75, 23);
            this.yAxisStopButton.TabIndex = 1;
            this.yAxisStopButton.Text = "Stop";
            this.yAxisStopButton.Click += new System.EventHandler(this.yAxisStopButton_Click);
            // 
            // yAxisStartButton
            // 
            this.yAxisStartButton.Enabled = false;
            this.yAxisStartButton.Location = new System.Drawing.Point(24, 64);
            this.yAxisStartButton.Name = "yAxisStartButton";
            this.yAxisStartButton.Size = new System.Drawing.Size(75, 23);
            this.yAxisStartButton.TabIndex = 0;
            this.yAxisStartButton.Text = "Start";
            this.yAxisStartButton.Click += new System.EventHandler(this.yAxisStartButton_Click);
            // 
            // xAxisStartButton
            // 
            this.xAxisStartButton.Enabled = false;
            this.xAxisStartButton.Location = new System.Drawing.Point(16, 64);
            this.xAxisStartButton.Name = "xAxisStartButton";
            this.xAxisStartButton.Size = new System.Drawing.Size(75, 23);
            this.xAxisStartButton.TabIndex = 0;
            this.xAxisStartButton.Text = "Start";
            this.xAxisStartButton.Click += new System.EventHandler(this.xAxisStartButton_Click);
            // 
            // YaxisGroupBox
            // 
            this.YaxisGroupBox.Controls.Add(this.yAxisMoveDistanceTextBox);
            this.YaxisGroupBox.Controls.Add(this.Label1);
            this.YaxisGroupBox.Controls.Add(this.yAxisStopButton);
            this.YaxisGroupBox.Controls.Add(this.yAxisStartButton);
            this.YaxisGroupBox.Location = new System.Drawing.Point(259, 52);
            this.YaxisGroupBox.Name = "YaxisGroupBox";
            this.YaxisGroupBox.Size = new System.Drawing.Size(224, 100);
            this.YaxisGroupBox.TabIndex = 8;
            this.YaxisGroupBox.TabStop = false;
            this.YaxisGroupBox.Text = "Y Axis";
            // 
            // xMoveDistanceTextBox
            // 
            this.xMoveDistanceTextBox.Location = new System.Drawing.Point(110, 19);
            this.xMoveDistanceTextBox.Name = "xMoveDistanceTextBox";
            this.xMoveDistanceTextBox.Size = new System.Drawing.Size(64, 20);
            this.xMoveDistanceTextBox.TabIndex = 5;
            this.xMoveDistanceTextBox.Text = "1000";
            this.xMoveDistanceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MoveLabel
            // 
            this.MoveLabel.Location = new System.Drawing.Point(16, 22);
            this.MoveLabel.Name = "MoveLabel";
            this.MoveLabel.Size = new System.Drawing.Size(88, 16);
            this.MoveLabel.TabIndex = 4;
            this.MoveLabel.Text = "Move Distance";
            // 
            // xAxisStopButton
            // 
            this.xAxisStopButton.Location = new System.Drawing.Point(112, 64);
            this.xAxisStopButton.Name = "xAxisStopButton";
            this.xAxisStopButton.Size = new System.Drawing.Size(75, 23);
            this.xAxisStopButton.TabIndex = 1;
            this.xAxisStopButton.Text = "Stop";
            this.xAxisStopButton.Click += new System.EventHandler(this.xAxisStopButton_Click);
            // 
            // xAxisGroupBox
            // 
            this.xAxisGroupBox.Controls.Add(this.xMoveDistanceTextBox);
            this.xAxisGroupBox.Controls.Add(this.MoveLabel);
            this.xAxisGroupBox.Controls.Add(this.xAxisStopButton);
            this.xAxisGroupBox.Controls.Add(this.xAxisStartButton);
            this.xAxisGroupBox.Location = new System.Drawing.Point(11, 52);
            this.xAxisGroupBox.Name = "xAxisGroupBox";
            this.xAxisGroupBox.Size = new System.Drawing.Size(230, 100);
            this.xAxisGroupBox.TabIndex = 7;
            this.xAxisGroupBox.TabStop = false;
            this.xAxisGroupBox.Text = "X Axis";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 177);
            this.Controls.Add(this.HomeAxisButton);
            this.Controls.Add(this.YaxisGroupBox);
            this.Controls.Add(this.xAxisGroupBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.YaxisGroupBox.ResumeLayout(false);
            this.YaxisGroupBox.PerformLayout();
            this.xAxisGroupBox.ResumeLayout(false);
            this.xAxisGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TextBox yAxisMoveDistanceTextBox;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button HomeAxisButton;
        internal System.Windows.Forms.Button yAxisStopButton;
        internal System.Windows.Forms.Button yAxisStartButton;
        internal System.Windows.Forms.Button xAxisStartButton;
        internal System.Windows.Forms.GroupBox YaxisGroupBox;
        internal System.Windows.Forms.TextBox xMoveDistanceTextBox;
        internal System.Windows.Forms.Label MoveLabel;
        internal System.Windows.Forms.Button xAxisStopButton;
        internal System.Windows.Forms.GroupBox xAxisGroupBox;
    }
}

