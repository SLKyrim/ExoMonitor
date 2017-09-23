namespace EX2_Homing
{
    partial class Homing
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
            this.Label2 = new System.Windows.Forms.Label();
            this.resetButton = new System.Windows.Forms.Button();
            this.referencedLabel = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.posTextBox = new System.Windows.Forms.TextBox();
            this.enableButton = new System.Windows.Forms.Button();
            this.HomeAxisButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(206, 142);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(39, 13);
            this.Label2.TabIndex = 15;
            this.Label2.Text = "counts";
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(47, 212);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(75, 23);
            this.resetButton.TabIndex = 14;
            this.resetButton.Text = "Reset Amp";
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // referencedLabel
            // 
            this.referencedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.referencedLabel.Location = new System.Drawing.Point(39, 180);
            this.referencedLabel.Name = "referencedLabel";
            this.referencedLabel.Size = new System.Drawing.Size(161, 19);
            this.referencedLabel.TabIndex = 13;
            this.referencedLabel.Text = "Not Referenced";
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(39, 142);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(83, 16);
            this.Label1.TabIndex = 12;
            this.Label1.Text = "Actual Position";
            // 
            // Timer1
            // 
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // posTextBox
            // 
            this.posTextBox.Location = new System.Drawing.Point(128, 139);
            this.posTextBox.Name = "posTextBox";
            this.posTextBox.ReadOnly = true;
            this.posTextBox.Size = new System.Drawing.Size(72, 20);
            this.posTextBox.TabIndex = 11;
            this.posTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // enableButton
            // 
            this.enableButton.Location = new System.Drawing.Point(39, 28);
            this.enableButton.Name = "enableButton";
            this.enableButton.Size = new System.Drawing.Size(168, 32);
            this.enableButton.TabIndex = 10;
            this.enableButton.Text = "Amp Disable";
            this.enableButton.Click += new System.EventHandler(this.enableButton_Click);
            // 
            // HomeAxisButton
            // 
            this.HomeAxisButton.Location = new System.Drawing.Point(39, 84);
            this.HomeAxisButton.Name = "HomeAxisButton";
            this.HomeAxisButton.Size = new System.Drawing.Size(168, 32);
            this.HomeAxisButton.TabIndex = 9;
            this.HomeAxisButton.Text = "Home Axis";
            this.HomeAxisButton.Click += new System.EventHandler(this.HomeAxisButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.referencedLabel);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.posTextBox);
            this.Controls.Add(this.enableButton);
            this.Controls.Add(this.HomeAxisButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Button resetButton;
        internal System.Windows.Forms.Label referencedLabel;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Timer Timer1;
        internal System.Windows.Forms.TextBox posTextBox;
        internal System.Windows.Forms.Button enableButton;
        internal System.Windows.Forms.Button HomeAxisButton;
    }
}

