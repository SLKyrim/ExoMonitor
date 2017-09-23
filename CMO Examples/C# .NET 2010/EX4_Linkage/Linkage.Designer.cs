namespace EX4_Linkage
{
    partial class Linkage
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
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.HomeAxisButton = new System.Windows.Forms.Button();
            this.yAxisTextBox = new System.Windows.Forms.TextBox();
            this.xAxisTextBox = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.linkageMoveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(264, 157);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(39, 13);
            this.Label4.TabIndex = 15;
            this.Label4.Text = "counts";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(264, 125);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(39, 13);
            this.Label3.TabIndex = 14;
            this.Label3.Text = "counts";
            // 
            // HomeAxisButton
            // 
            this.HomeAxisButton.Location = new System.Drawing.Point(58, 26);
            this.HomeAxisButton.Name = "HomeAxisButton";
            this.HomeAxisButton.Size = new System.Drawing.Size(184, 32);
            this.HomeAxisButton.TabIndex = 13;
            this.HomeAxisButton.Text = "Home Axis";
            this.HomeAxisButton.Click += new System.EventHandler(this.HomeAxisButton_Click);
            // 
            // yAxisTextBox
            // 
            this.yAxisTextBox.Location = new System.Drawing.Point(170, 154);
            this.yAxisTextBox.Name = "yAxisTextBox";
            this.yAxisTextBox.Size = new System.Drawing.Size(88, 20);
            this.yAxisTextBox.TabIndex = 12;
            this.yAxisTextBox.Text = "10000";
            // 
            // xAxisTextBox
            // 
            this.xAxisTextBox.Location = new System.Drawing.Point(170, 122);
            this.xAxisTextBox.Name = "xAxisTextBox";
            this.xAxisTextBox.Size = new System.Drawing.Size(88, 20);
            this.xAxisTextBox.TabIndex = 9;
            this.xAxisTextBox.Text = "20000";
            // 
            // Label2
            // 
            this.Label2.Location = new System.Drawing.Point(26, 154);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(136, 24);
            this.Label2.TabIndex = 11;
            this.Label2.Text = "Y Axis Move to Position";
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(26, 122);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(136, 16);
            this.Label1.TabIndex = 10;
            this.Label1.Text = "X Axis Move to Position";
            // 
            // linkageMoveButton
            // 
            this.linkageMoveButton.Enabled = false;
            this.linkageMoveButton.Location = new System.Drawing.Point(58, 74);
            this.linkageMoveButton.Name = "linkageMoveButton";
            this.linkageMoveButton.Size = new System.Drawing.Size(184, 32);
            this.linkageMoveButton.TabIndex = 8;
            this.linkageMoveButton.Text = "Perform Linkage Move";
            this.linkageMoveButton.Click += new System.EventHandler(this.linkageMoveButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 198);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.HomeAxisButton);
            this.Controls.Add(this.yAxisTextBox);
            this.Controls.Add(this.xAxisTextBox);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.linkageMoveButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Button HomeAxisButton;
        internal System.Windows.Forms.TextBox yAxisTextBox;
        internal System.Windows.Forms.TextBox xAxisTextBox;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button linkageMoveButton;
    }
}

