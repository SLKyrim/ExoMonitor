namespace EX9_IndexerRegViaSDO
{
    partial class IndexerRegViaSDO
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
            this.Label1 = new System.Windows.Forms.Label();
            this.txtRegisterNumber = new System.Windows.Forms.TextBox();
            this.txtWriteData = new System.Windows.Forms.TextBox();
            this.cmdWrite = new System.Windows.Forms.Button();
            this.txtReadData = new System.Windows.Forms.TextBox();
            this.cmdRead = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(53, 17);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(96, 23);
            this.Label1.TabIndex = 11;
            this.Label1.Text = "Register Number";
            // 
            // txtRegisterNumber
            // 
            this.txtRegisterNumber.Location = new System.Drawing.Point(165, 17);
            this.txtRegisterNumber.Name = "txtRegisterNumber";
            this.txtRegisterNumber.Size = new System.Drawing.Size(32, 20);
            this.txtRegisterNumber.TabIndex = 10;
            this.txtRegisterNumber.Text = "0";
            this.txtRegisterNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtWriteData
            // 
            this.txtWriteData.Location = new System.Drawing.Point(165, 49);
            this.txtWriteData.Name = "txtWriteData";
            this.txtWriteData.Size = new System.Drawing.Size(80, 20);
            this.txtWriteData.TabIndex = 9;
            this.txtWriteData.Text = "0";
            this.txtWriteData.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cmdWrite
            // 
            this.cmdWrite.Location = new System.Drawing.Point(45, 49);
            this.cmdWrite.Name = "cmdWrite";
            this.cmdWrite.Size = new System.Drawing.Size(112, 23);
            this.cmdWrite.TabIndex = 8;
            this.cmdWrite.Text = "Write Register";
            this.cmdWrite.Click += new System.EventHandler(this.cmdWrite_Click);
            // 
            // txtReadData
            // 
            this.txtReadData.Location = new System.Drawing.Point(165, 81);
            this.txtReadData.Name = "txtReadData";
            this.txtReadData.Size = new System.Drawing.Size(80, 20);
            this.txtReadData.TabIndex = 7;
            this.txtReadData.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cmdRead
            // 
            this.cmdRead.Location = new System.Drawing.Point(45, 81);
            this.cmdRead.Name = "cmdRead";
            this.cmdRead.Size = new System.Drawing.Size(112, 23);
            this.cmdRead.TabIndex = 6;
            this.cmdRead.Text = "Read Register";
            this.cmdRead.Click += new System.EventHandler(this.cmdRead_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 132);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.txtRegisterNumber);
            this.Controls.Add(this.txtWriteData);
            this.Controls.Add(this.cmdWrite);
            this.Controls.Add(this.txtReadData);
            this.Controls.Add(this.cmdRead);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox txtRegisterNumber;
        internal System.Windows.Forms.TextBox txtWriteData;
        internal System.Windows.Forms.Button cmdWrite;
        internal System.Windows.Forms.TextBox txtReadData;
        internal System.Windows.Forms.Button cmdRead;
    }
}

