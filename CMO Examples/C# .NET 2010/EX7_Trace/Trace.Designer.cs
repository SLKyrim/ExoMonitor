namespace EX7_Trace
{
    partial class Trace
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
            this.SendTrigSettingsButton = new System.Windows.Forms.Button();
            this.TrigDelayTextBox = new System.Windows.Forms.TextBox();
            this.TotalTraceTimeUnits = new System.Windows.Forms.Label();
            this.TrigDelayLabel = new System.Windows.Forms.Label();
            this.TrigLevelTextBox = new System.Windows.Forms.TextBox();
            this.TrigLevelLabel = new System.Windows.Forms.Label();
            this.StartTraceButton = new System.Windows.Forms.Button();
            this.StopTraceButton = new System.Windows.Forms.Button();
            this.Timer1 = new System.Windows.Forms.Timer(this.components);
            this.SendPeriodButton = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.TotalTraceTimeLabel = new System.Windows.Forms.Label();
            this.TotalTraceTimeValue = new System.Windows.Forms.Label();
            this.TracePeriodTextBox = new System.Windows.Forms.TextBox();
            this.MaxSamplesLabel = new System.Windows.Forms.Label();
            this.TraceTimeGroupBox = new System.Windows.Forms.GroupBox();
            this.TraceRefPeriodLabel = new System.Windows.Forms.Label();
            this.TraceRefPeriodValue = new System.Windows.Forms.Label();
            this.TracePeriodLabel = new System.Windows.Forms.Label();
            this.MaxSamplesValue = new System.Windows.Forms.Label();
            this.EnableDisableButton = new System.Windows.Forms.Button();
            this.TraceStatusLabel = new System.Windows.Forms.Label();
            this.TraceStatusValue = new System.Windows.Forms.Label();
            this.TrigChannelTextBox = new System.Windows.Forms.TextBox();
            this.TriggerGroupBox = new System.Windows.Forms.GroupBox();
            this.TrigChannelLabel = new System.Windows.Forms.Label();
            this.TrigTypeLabel = new System.Windows.Forms.Label();
            this.TrigTypeTextBox = new System.Windows.Forms.TextBox();
            this.TraceTimeGroupBox.SuspendLayout();
            this.TriggerGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendTrigSettingsButton
            // 
            this.SendTrigSettingsButton.Location = new System.Drawing.Point(48, 192);
            this.SendTrigSettingsButton.Name = "SendTrigSettingsButton";
            this.SendTrigSettingsButton.Size = new System.Drawing.Size(48, 23);
            this.SendTrigSettingsButton.TabIndex = 17;
            this.SendTrigSettingsButton.Text = "Send";
            this.SendTrigSettingsButton.Click += new System.EventHandler(this.SendTrigSettingsButton_Click);
            // 
            // TrigDelayTextBox
            // 
            this.TrigDelayTextBox.Location = new System.Drawing.Point(64, 152);
            this.TrigDelayTextBox.Name = "TrigDelayTextBox";
            this.TrigDelayTextBox.Size = new System.Drawing.Size(56, 20);
            this.TrigDelayTextBox.TabIndex = 16;
            // 
            // TotalTraceTimeUnits
            // 
            this.TotalTraceTimeUnits.Location = new System.Drawing.Point(184, 144);
            this.TotalTraceTimeUnits.Name = "TotalTraceTimeUnits";
            this.TotalTraceTimeUnits.Size = new System.Drawing.Size(24, 16);
            this.TotalTraceTimeUnits.TabIndex = 20;
            this.TotalTraceTimeUnits.Text = "ms";
            // 
            // TrigDelayLabel
            // 
            this.TrigDelayLabel.Location = new System.Drawing.Point(8, 152);
            this.TrigDelayLabel.Name = "TrigDelayLabel";
            this.TrigDelayLabel.Size = new System.Drawing.Size(40, 16);
            this.TrigDelayLabel.TabIndex = 15;
            this.TrigDelayLabel.Text = "Delay:";
            // 
            // TrigLevelTextBox
            // 
            this.TrigLevelTextBox.Location = new System.Drawing.Point(64, 112);
            this.TrigLevelTextBox.Name = "TrigLevelTextBox";
            this.TrigLevelTextBox.Size = new System.Drawing.Size(56, 20);
            this.TrigLevelTextBox.TabIndex = 13;
            // 
            // TrigLevelLabel
            // 
            this.TrigLevelLabel.Location = new System.Drawing.Point(8, 112);
            this.TrigLevelLabel.Name = "TrigLevelLabel";
            this.TrigLevelLabel.Size = new System.Drawing.Size(40, 16);
            this.TrigLevelLabel.TabIndex = 12;
            this.TrigLevelLabel.Text = "Level:";
            // 
            // StartTraceButton
            // 
            this.StartTraceButton.Location = new System.Drawing.Point(39, 231);
            this.StartTraceButton.Name = "StartTraceButton";
            this.StartTraceButton.Size = new System.Drawing.Size(75, 23);
            this.StartTraceButton.TabIndex = 25;
            this.StartTraceButton.Text = "Start Trace";
            this.StartTraceButton.Click += new System.EventHandler(this.StartTraceButton_Click);
            // 
            // StopTraceButton
            // 
            this.StopTraceButton.Location = new System.Drawing.Point(143, 231);
            this.StopTraceButton.Name = "StopTraceButton";
            this.StopTraceButton.Size = new System.Drawing.Size(75, 23);
            this.StopTraceButton.TabIndex = 26;
            this.StopTraceButton.Text = "Stop Trace";
            this.StopTraceButton.Click += new System.EventHandler(this.StopTraceButton_Click);
            // 
            // Timer1
            // 
            this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // SendPeriodButton
            // 
            this.SendPeriodButton.Location = new System.Drawing.Point(168, 64);
            this.SendPeriodButton.Name = "SendPeriodButton";
            this.SendPeriodButton.Size = new System.Drawing.Size(48, 23);
            this.SendPeriodButton.TabIndex = 21;
            this.SendPeriodButton.Text = "Send";
            this.SendPeriodButton.Click += new System.EventHandler(this.SendPeriodButton_Click);
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(160, 24);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(16, 16);
            this.Label1.TabIndex = 17;
            this.Label1.Text = "ns";
            // 
            // TotalTraceTimeLabel
            // 
            this.TotalTraceTimeLabel.Location = new System.Drawing.Point(16, 144);
            this.TotalTraceTimeLabel.Name = "TotalTraceTimeLabel";
            this.TotalTraceTimeLabel.Size = new System.Drawing.Size(96, 16);
            this.TotalTraceTimeLabel.TabIndex = 18;
            this.TotalTraceTimeLabel.Text = "Total Trace Time:";
            // 
            // TotalTraceTimeValue
            // 
            this.TotalTraceTimeValue.BackColor = System.Drawing.Color.Black;
            this.TotalTraceTimeValue.ForeColor = System.Drawing.Color.Lime;
            this.TotalTraceTimeValue.Location = new System.Drawing.Point(112, 144);
            this.TotalTraceTimeValue.Name = "TotalTraceTimeValue";
            this.TotalTraceTimeValue.Size = new System.Drawing.Size(64, 16);
            this.TotalTraceTimeValue.TabIndex = 19;
            this.TotalTraceTimeValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TracePeriodTextBox
            // 
            this.TracePeriodTextBox.Location = new System.Drawing.Point(112, 64);
            this.TracePeriodTextBox.Name = "TracePeriodTextBox";
            this.TracePeriodTextBox.Size = new System.Drawing.Size(40, 20);
            this.TracePeriodTextBox.TabIndex = 16;
            // 
            // MaxSamplesLabel
            // 
            this.MaxSamplesLabel.Location = new System.Drawing.Point(16, 104);
            this.MaxSamplesLabel.Name = "MaxSamplesLabel";
            this.MaxSamplesLabel.Size = new System.Drawing.Size(80, 16);
            this.MaxSamplesLabel.TabIndex = 16;
            this.MaxSamplesLabel.Text = "Max Samples:";
            // 
            // TraceTimeGroupBox
            // 
            this.TraceTimeGroupBox.Controls.Add(this.SendPeriodButton);
            this.TraceTimeGroupBox.Controls.Add(this.TotalTraceTimeUnits);
            this.TraceTimeGroupBox.Controls.Add(this.Label1);
            this.TraceTimeGroupBox.Controls.Add(this.TraceRefPeriodLabel);
            this.TraceTimeGroupBox.Controls.Add(this.TraceRefPeriodValue);
            this.TraceTimeGroupBox.Controls.Add(this.TotalTraceTimeLabel);
            this.TraceTimeGroupBox.Controls.Add(this.TotalTraceTimeValue);
            this.TraceTimeGroupBox.Controls.Add(this.TracePeriodLabel);
            this.TraceTimeGroupBox.Controls.Add(this.TracePeriodTextBox);
            this.TraceTimeGroupBox.Controls.Add(this.MaxSamplesValue);
            this.TraceTimeGroupBox.Controls.Add(this.MaxSamplesLabel);
            this.TraceTimeGroupBox.Location = new System.Drawing.Point(15, 31);
            this.TraceTimeGroupBox.Name = "TraceTimeGroupBox";
            this.TraceTimeGroupBox.Size = new System.Drawing.Size(224, 176);
            this.TraceTimeGroupBox.TabIndex = 27;
            this.TraceTimeGroupBox.TabStop = false;
            this.TraceTimeGroupBox.Text = "Trace Time";
            // 
            // TraceRefPeriodLabel
            // 
            this.TraceRefPeriodLabel.Location = new System.Drawing.Point(16, 24);
            this.TraceRefPeriodLabel.Name = "TraceRefPeriodLabel";
            this.TraceRefPeriodLabel.Size = new System.Drawing.Size(96, 16);
            this.TraceRefPeriodLabel.TabIndex = 3;
            this.TraceRefPeriodLabel.Text = "Trace Ref Period:";
            // 
            // TraceRefPeriodValue
            // 
            this.TraceRefPeriodValue.BackColor = System.Drawing.Color.Black;
            this.TraceRefPeriodValue.ForeColor = System.Drawing.Color.Lime;
            this.TraceRefPeriodValue.Location = new System.Drawing.Point(112, 24);
            this.TraceRefPeriodValue.Name = "TraceRefPeriodValue";
            this.TraceRefPeriodValue.Size = new System.Drawing.Size(40, 16);
            this.TraceRefPeriodValue.TabIndex = 4;
            this.TraceRefPeriodValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TracePeriodLabel
            // 
            this.TracePeriodLabel.Location = new System.Drawing.Point(16, 64);
            this.TracePeriodLabel.Name = "TracePeriodLabel";
            this.TracePeriodLabel.Size = new System.Drawing.Size(72, 16);
            this.TracePeriodLabel.TabIndex = 5;
            this.TracePeriodLabel.Text = "Trace Period:";
            // 
            // MaxSamplesValue
            // 
            this.MaxSamplesValue.BackColor = System.Drawing.Color.Black;
            this.MaxSamplesValue.ForeColor = System.Drawing.Color.Lime;
            this.MaxSamplesValue.Location = new System.Drawing.Point(112, 104);
            this.MaxSamplesValue.Name = "MaxSamplesValue";
            this.MaxSamplesValue.Size = new System.Drawing.Size(40, 16);
            this.MaxSamplesValue.TabIndex = 17;
            // 
            // EnableDisableButton
            // 
            this.EnableDisableButton.Location = new System.Drawing.Point(103, 327);
            this.EnableDisableButton.Name = "EnableDisableButton";
            this.EnableDisableButton.Size = new System.Drawing.Size(80, 23);
            this.EnableDisableButton.TabIndex = 28;
            this.EnableDisableButton.Click += new System.EventHandler(this.EnableDisableButton_Click);
            // 
            // TraceStatusLabel
            // 
            this.TraceStatusLabel.Location = new System.Drawing.Point(15, 279);
            this.TraceStatusLabel.Name = "TraceStatusLabel";
            this.TraceStatusLabel.Size = new System.Drawing.Size(72, 16);
            this.TraceStatusLabel.TabIndex = 22;
            this.TraceStatusLabel.Text = "Trace Status:";
            // 
            // TraceStatusValue
            // 
            this.TraceStatusValue.BackColor = System.Drawing.Color.Black;
            this.TraceStatusValue.ForeColor = System.Drawing.Color.Lime;
            this.TraceStatusValue.Location = new System.Drawing.Point(95, 279);
            this.TraceStatusValue.Name = "TraceStatusValue";
            this.TraceStatusValue.Size = new System.Drawing.Size(176, 16);
            this.TraceStatusValue.TabIndex = 23;
            // 
            // TrigChannelTextBox
            // 
            this.TrigChannelTextBox.Location = new System.Drawing.Point(64, 72);
            this.TrigChannelTextBox.Name = "TrigChannelTextBox";
            this.TrigChannelTextBox.Size = new System.Drawing.Size(56, 20);
            this.TrigChannelTextBox.TabIndex = 10;
            // 
            // TriggerGroupBox
            // 
            this.TriggerGroupBox.Controls.Add(this.SendTrigSettingsButton);
            this.TriggerGroupBox.Controls.Add(this.TrigDelayTextBox);
            this.TriggerGroupBox.Controls.Add(this.TrigDelayLabel);
            this.TriggerGroupBox.Controls.Add(this.TrigLevelTextBox);
            this.TriggerGroupBox.Controls.Add(this.TrigLevelLabel);
            this.TriggerGroupBox.Controls.Add(this.TrigChannelTextBox);
            this.TriggerGroupBox.Controls.Add(this.TrigChannelLabel);
            this.TriggerGroupBox.Controls.Add(this.TrigTypeLabel);
            this.TriggerGroupBox.Controls.Add(this.TrigTypeTextBox);
            this.TriggerGroupBox.Location = new System.Drawing.Point(255, 31);
            this.TriggerGroupBox.Name = "TriggerGroupBox";
            this.TriggerGroupBox.Size = new System.Drawing.Size(136, 224);
            this.TriggerGroupBox.TabIndex = 24;
            this.TriggerGroupBox.TabStop = false;
            this.TriggerGroupBox.Text = "Trigger";
            // 
            // TrigChannelLabel
            // 
            this.TrigChannelLabel.Location = new System.Drawing.Point(8, 72);
            this.TrigChannelLabel.Name = "TrigChannelLabel";
            this.TrigChannelLabel.Size = new System.Drawing.Size(56, 16);
            this.TrigChannelLabel.TabIndex = 9;
            this.TrigChannelLabel.Text = "Channel:";
            // 
            // TrigTypeLabel
            // 
            this.TrigTypeLabel.Location = new System.Drawing.Point(8, 32);
            this.TrigTypeLabel.Name = "TrigTypeLabel";
            this.TrigTypeLabel.Size = new System.Drawing.Size(40, 16);
            this.TrigTypeLabel.TabIndex = 7;
            this.TrigTypeLabel.Text = "Type:";
            // 
            // TrigTypeTextBox
            // 
            this.TrigTypeTextBox.Location = new System.Drawing.Point(64, 32);
            this.TrigTypeTextBox.Name = "TrigTypeTextBox";
            this.TrigTypeTextBox.Size = new System.Drawing.Size(56, 20);
            this.TrigTypeTextBox.TabIndex = 0;
            // 
            // Trace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 380);
            this.Controls.Add(this.StartTraceButton);
            this.Controls.Add(this.StopTraceButton);
            this.Controls.Add(this.TraceTimeGroupBox);
            this.Controls.Add(this.EnableDisableButton);
            this.Controls.Add(this.TraceStatusLabel);
            this.Controls.Add(this.TraceStatusValue);
            this.Controls.Add(this.TriggerGroupBox);
            this.Name = "Trace";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Trace_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Trace_FormClosing);
            this.TraceTimeGroupBox.ResumeLayout(false);
            this.TraceTimeGroupBox.PerformLayout();
            this.TriggerGroupBox.ResumeLayout(false);
            this.TriggerGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Button SendTrigSettingsButton;
        internal System.Windows.Forms.TextBox TrigDelayTextBox;
        internal System.Windows.Forms.Label TotalTraceTimeUnits;
        internal System.Windows.Forms.Label TrigDelayLabel;
        internal System.Windows.Forms.TextBox TrigLevelTextBox;
        internal System.Windows.Forms.Label TrigLevelLabel;
        internal System.Windows.Forms.Button StartTraceButton;
        internal System.Windows.Forms.Button StopTraceButton;
        internal System.Windows.Forms.Timer Timer1;
        internal System.Windows.Forms.Button SendPeriodButton;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Label TotalTraceTimeLabel;
        internal System.Windows.Forms.Label TotalTraceTimeValue;
        internal System.Windows.Forms.TextBox TracePeriodTextBox;
        internal System.Windows.Forms.Label MaxSamplesLabel;
        internal System.Windows.Forms.GroupBox TraceTimeGroupBox;
        internal System.Windows.Forms.Label TraceRefPeriodLabel;
        internal System.Windows.Forms.Label TraceRefPeriodValue;
        internal System.Windows.Forms.Label TracePeriodLabel;
        internal System.Windows.Forms.Label MaxSamplesValue;
        internal System.Windows.Forms.Button EnableDisableButton;
        internal System.Windows.Forms.Label TraceStatusLabel;
        internal System.Windows.Forms.Label TraceStatusValue;
        internal System.Windows.Forms.TextBox TrigChannelTextBox;
        internal System.Windows.Forms.GroupBox TriggerGroupBox;
        internal System.Windows.Forms.Label TrigChannelLabel;
        internal System.Windows.Forms.Label TrigTypeLabel;
        internal System.Windows.Forms.TextBox TrigTypeTextBox;
    }
}

