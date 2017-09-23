namespace EX1_Status
{
    partial class Status
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
            this.Label1 = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.enableButton = new System.Windows.Forms.Button();
            this.ListBox1 = new System.Windows.Forms.ListBox();
            this.statusTextBox = new System.Windows.Forms.TextBox();
            this.Label3 = new System.Windows.Forms.Label();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.posTextBox = new System.Windows.Forms.TextBox();
            this.readButton = new System.Windows.Forms.Button();
            this.GroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(22, 67);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(136, 16);
            this.Label1.TabIndex = 5;
            this.Label1.Text = "Actual Position (in counts)";
            // 
            // Timer1
            // 
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // enableButton
            // 
            this.enableButton.Location = new System.Drawing.Point(102, 3);
            this.enableButton.Name = "enableButton";
            this.enableButton.Size = new System.Drawing.Size(80, 24);
            this.enableButton.TabIndex = 4;
            this.enableButton.Text = "Amp Disable";
            this.enableButton.Click += new System.EventHandler(this.enableButton_Click);
            // 
            // ListBox1
            // 
            this.ListBox1.Location = new System.Drawing.Point(32, 136);
            this.ListBox1.Name = "ListBox1";
            this.ListBox1.Size = new System.Drawing.Size(232, 69);
            this.ListBox1.TabIndex = 4;
            // 
            // statusTextBox
            // 
            this.statusTextBox.Location = new System.Drawing.Point(192, 56);
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ReadOnly = true;
            this.statusTextBox.Size = new System.Drawing.Size(72, 20);
            this.statusTextBox.TabIndex = 4;
            this.statusTextBox.Text = "0";
            this.statusTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(24, 56);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(144, 16);
            this.Label3.TabIndex = 6;
            this.Label3.Text = "Amp event status";
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.Label3);
            this.GroupBox1.Controls.Add(this.posTextBox);
            this.GroupBox1.Controls.Add(this.ListBox1);
            this.GroupBox1.Controls.Add(this.statusTextBox);
            this.GroupBox1.Controls.Add(this.readButton);
            this.GroupBox1.Location = new System.Drawing.Point(-2, 43);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(288, 216);
            this.GroupBox1.TabIndex = 6;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Amp Status";
            // 
            // posTextBox
            // 
            this.posTextBox.Location = new System.Drawing.Point(192, 24);
            this.posTextBox.Name = "posTextBox";
            this.posTextBox.ReadOnly = true;
            this.posTextBox.Size = new System.Drawing.Size(72, 20);
            this.posTextBox.TabIndex = 0;
            this.posTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // readButton
            // 
            this.readButton.Location = new System.Drawing.Point(96, 96);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(104, 23);
            this.readButton.TabIndex = 4;
            this.readButton.Text = "ReadEventSticky";
            this.readButton.Click += new System.EventHandler(this.readButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.enableButton);
            this.Controls.Add(this.GroupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Timer Timer1;
        internal System.Windows.Forms.Button enableButton;
        internal System.Windows.Forms.ListBox ListBox1;
        internal System.Windows.Forms.TextBox statusTextBox;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.GroupBox GroupBox1;
        internal System.Windows.Forms.TextBox posTextBox;
        internal System.Windows.Forms.Button readButton;
    }
}

