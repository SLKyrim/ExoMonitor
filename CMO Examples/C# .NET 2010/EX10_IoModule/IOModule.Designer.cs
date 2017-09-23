namespace EX10_IoModule
{
    partial class IOModule
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
            this.txtInput1State = new System.Windows.Forms.TextBox();
            this.TimerBCD = new System.Windows.Forms.Timer(this.components);
            this.txtInput2State = new System.Windows.Forms.TextBox();
            this.TimerStat = new System.Windows.Forms.Timer(this.components);
            this.Label3 = new System.Windows.Forms.Label();
            this.TimerGO = new System.Windows.Forms.Timer(this.components);
            this.Label2 = new System.Windows.Forms.Label();
            this.cbOut2 = new System.Windows.Forms.CheckBox();
            this.cbOut1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtInput1State
            // 
            this.txtInput1State.Location = new System.Drawing.Point(99, 27);
            this.txtInput1State.Name = "txtInput1State";
            this.txtInput1State.Size = new System.Drawing.Size(32, 20);
            this.txtInput1State.TabIndex = 17;
            this.txtInput1State.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TimerBCD
            // 
            this.TimerBCD.Interval = 10;
            // 
            // txtInput2State
            // 
            this.txtInput2State.Location = new System.Drawing.Point(99, 51);
            this.txtInput2State.Name = "txtInput2State";
            this.txtInput2State.Size = new System.Drawing.Size(32, 20);
            this.txtInput2State.TabIndex = 19;
            this.txtInput2State.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TimerStat
            // 
            this.TimerStat.Interval = 250;
            this.TimerStat.Tick += new System.EventHandler(this.TimerStat_Tick);
            // 
            // Label3
            // 
            this.Label3.Location = new System.Drawing.Point(139, 51);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(48, 16);
            this.Label3.TabIndex = 20;
            this.Label3.Text = "Input 2";
            // 
            // TimerGO
            // 
            this.TimerGO.Interval = 10;
            // 
            // Label2
            // 
            this.Label2.Location = new System.Drawing.Point(139, 27);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(48, 16);
            this.Label2.TabIndex = 18;
            this.Label2.Text = "Input 1";
            // 
            // cbOut2
            // 
            this.cbOut2.Location = new System.Drawing.Point(19, 43);
            this.cbOut2.Name = "cbOut2";
            this.cbOut2.Size = new System.Drawing.Size(80, 24);
            this.cbOut2.TabIndex = 16;
            this.cbOut2.Text = "Output 2";
            this.cbOut2.CheckedChanged += new System.EventHandler(this.cbOut2_CheckedChanged);
            // 
            // cbOut1
            // 
            this.cbOut1.Location = new System.Drawing.Point(19, 19);
            this.cbOut1.Name = "cbOut1";
            this.cbOut1.Size = new System.Drawing.Size(80, 24);
            this.cbOut1.TabIndex = 15;
            this.cbOut1.Text = "Output 1";
            this.cbOut1.CheckedChanged += new System.EventHandler(this.cbOut1_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(220, 128);
            this.Controls.Add(this.txtInput1State);
            this.Controls.Add(this.txtInput2State);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.cbOut2);
            this.Controls.Add(this.cbOut1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox txtInput1State;
        internal System.Windows.Forms.Timer TimerBCD;
        internal System.Windows.Forms.TextBox txtInput2State;
        internal System.Windows.Forms.Timer TimerStat;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Timer TimerGO;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.CheckBox cbOut2;
        internal System.Windows.Forms.CheckBox cbOut1;
    }
}

