namespace RedPhoneDXConfig
{
    partial class MainForm
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
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.portNameCbx = new System.Windows.Forms.ComboBox();
            this.connectionBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.deviceInfoGroup = new System.Windows.Forms.GroupBox();
            this.deviceInfoTxb = new System.Windows.Forms.RichTextBox();
            this.refreshPortsBtn = new System.Windows.Forms.Button();
            this.querySettingsBtn = new System.Windows.Forms.Button();
            this.applySettingsBtn = new System.Windows.Forms.Button();
            this.settingsGroup = new System.Windows.Forms.GroupBox();
            this.rwltChannelIDEdit = new System.Windows.Forms.NumericUpDown();
            this.rwltChannelIDLbl = new System.Windows.Forms.Label();
            this.rwltDiverIDEdit = new System.Windows.Forms.NumericUpDown();
            this.rwltDiverIDLbl = new System.Windows.Forms.Label();
            this.rwltEnabledChb = new System.Windows.Forms.CheckBox();
            this.channelDescriptionLbl = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.channelIDEdit = new System.Windows.Forms.NumericUpDown();
            this.statusStrip.SuspendLayout();
            this.deviceInfoGroup.SuspendLayout();
            this.settingsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rwltChannelIDEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rwltDiverIDEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelIDEdit)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLbl});
            this.statusStrip.Location = new System.Drawing.Point(0, 574);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(582, 25);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLbl
            // 
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(26, 20);
            this.statusLbl.Text = ". . .";
            // 
            // portNameCbx
            // 
            this.portNameCbx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.portNameCbx.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.portNameCbx.FormattingEnabled = true;
            this.portNameCbx.Location = new System.Drawing.Point(62, 43);
            this.portNameCbx.Name = "portNameCbx";
            this.portNameCbx.Size = new System.Drawing.Size(221, 31);
            this.portNameCbx.TabIndex = 1;
            // 
            // connectionBtn
            // 
            this.connectionBtn.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.connectionBtn.Location = new System.Drawing.Point(289, 43);
            this.connectionBtn.Name = "connectionBtn";
            this.connectionBtn.Size = new System.Drawing.Size(172, 32);
            this.connectionBtn.TabIndex = 2;
            this.connectionBtn.Text = "CONNECTION";
            this.connectionBtn.UseVisualStyleBackColor = true;
            this.connectionBtn.Click += new System.EventHandler(this.connectionBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(62, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "RedPhone DX Dongle PORT";
            // 
            // deviceInfoGroup
            // 
            this.deviceInfoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceInfoGroup.Controls.Add(this.deviceInfoTxb);
            this.deviceInfoGroup.Enabled = false;
            this.deviceInfoGroup.Location = new System.Drawing.Point(12, 99);
            this.deviceInfoGroup.Name = "deviceInfoGroup";
            this.deviceInfoGroup.Size = new System.Drawing.Size(558, 140);
            this.deviceInfoGroup.TabIndex = 4;
            this.deviceInfoGroup.TabStop = false;
            this.deviceInfoGroup.Text = "RedPhone DX Device Information";
            // 
            // deviceInfoTxb
            // 
            this.deviceInfoTxb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceInfoTxb.Location = new System.Drawing.Point(6, 29);
            this.deviceInfoTxb.Name = "deviceInfoTxb";
            this.deviceInfoTxb.ReadOnly = true;
            this.deviceInfoTxb.Size = new System.Drawing.Size(546, 94);
            this.deviceInfoTxb.TabIndex = 0;
            this.deviceInfoTxb.Text = "";
            // 
            // refreshPortsBtn
            // 
            this.refreshPortsBtn.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.refreshPortsBtn.Location = new System.Drawing.Point(12, 43);
            this.refreshPortsBtn.Name = "refreshPortsBtn";
            this.refreshPortsBtn.Size = new System.Drawing.Size(44, 32);
            this.refreshPortsBtn.TabIndex = 5;
            this.refreshPortsBtn.Text = "🔄";
            this.refreshPortsBtn.UseVisualStyleBackColor = true;
            this.refreshPortsBtn.Click += new System.EventHandler(this.refreshPortsBtn_Click);
            // 
            // querySettingsBtn
            // 
            this.querySettingsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.querySettingsBtn.Enabled = false;
            this.querySettingsBtn.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.querySettingsBtn.Location = new System.Drawing.Point(12, 525);
            this.querySettingsBtn.Name = "querySettingsBtn";
            this.querySettingsBtn.Size = new System.Drawing.Size(211, 32);
            this.querySettingsBtn.TabIndex = 6;
            this.querySettingsBtn.Text = "QUERY DEVICE INFO";
            this.querySettingsBtn.UseVisualStyleBackColor = true;
            this.querySettingsBtn.Click += new System.EventHandler(this.querySettingsBtn_Click);
            // 
            // applySettingsBtn
            // 
            this.applySettingsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applySettingsBtn.Enabled = false;
            this.applySettingsBtn.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.applySettingsBtn.Location = new System.Drawing.Point(359, 525);
            this.applySettingsBtn.Name = "applySettingsBtn";
            this.applySettingsBtn.Size = new System.Drawing.Size(211, 32);
            this.applySettingsBtn.TabIndex = 7;
            this.applySettingsBtn.Text = "APPLY SETTINGS";
            this.applySettingsBtn.UseVisualStyleBackColor = true;
            this.applySettingsBtn.Click += new System.EventHandler(this.applySettingsBtn_Click);
            // 
            // settingsGroup
            // 
            this.settingsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsGroup.Controls.Add(this.channelIDEdit);
            this.settingsGroup.Controls.Add(this.rwltChannelIDEdit);
            this.settingsGroup.Controls.Add(this.rwltChannelIDLbl);
            this.settingsGroup.Controls.Add(this.rwltDiverIDEdit);
            this.settingsGroup.Controls.Add(this.rwltDiverIDLbl);
            this.settingsGroup.Controls.Add(this.rwltEnabledChb);
            this.settingsGroup.Controls.Add(this.channelDescriptionLbl);
            this.settingsGroup.Controls.Add(this.label2);
            this.settingsGroup.Enabled = false;
            this.settingsGroup.Location = new System.Drawing.Point(12, 245);
            this.settingsGroup.Name = "settingsGroup";
            this.settingsGroup.Size = new System.Drawing.Size(558, 241);
            this.settingsGroup.TabIndex = 5;
            this.settingsGroup.TabStop = false;
            this.settingsGroup.Text = "RedPhone DX Device settings";
            // 
            // rwltChannelIDEdit
            // 
            this.rwltChannelIDEdit.Enabled = false;
            this.rwltChannelIDEdit.Location = new System.Drawing.Point(165, 153);
            this.rwltChannelIDEdit.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.rwltChannelIDEdit.Name = "rwltChannelIDEdit";
            this.rwltChannelIDEdit.Size = new System.Drawing.Size(106, 30);
            this.rwltChannelIDEdit.TabIndex = 7;
            // 
            // rwltChannelIDLbl
            // 
            this.rwltChannelIDLbl.AutoSize = true;
            this.rwltChannelIDLbl.Enabled = false;
            this.rwltChannelIDLbl.Location = new System.Drawing.Point(30, 155);
            this.rwltChannelIDLbl.Name = "rwltChannelIDLbl";
            this.rwltChannelIDLbl.Size = new System.Drawing.Size(120, 23);
            this.rwltChannelIDLbl.TabIndex = 6;
            this.rwltChannelIDLbl.Text = "RWLT Channel";
            // 
            // rwltDiverIDEdit
            // 
            this.rwltDiverIDEdit.Enabled = false;
            this.rwltDiverIDEdit.Location = new System.Drawing.Point(165, 117);
            this.rwltDiverIDEdit.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.rwltDiverIDEdit.Name = "rwltDiverIDEdit";
            this.rwltDiverIDEdit.Size = new System.Drawing.Size(106, 30);
            this.rwltDiverIDEdit.TabIndex = 5;
            // 
            // rwltDiverIDLbl
            // 
            this.rwltDiverIDLbl.AutoSize = true;
            this.rwltDiverIDLbl.Enabled = false;
            this.rwltDiverIDLbl.Location = new System.Drawing.Point(30, 119);
            this.rwltDiverIDLbl.Name = "rwltDiverIDLbl";
            this.rwltDiverIDLbl.Size = new System.Drawing.Size(129, 23);
            this.rwltDiverIDLbl.TabIndex = 4;
            this.rwltDiverIDLbl.Text = "RWLT Diver\'s ID";
            // 
            // rwltEnabledChb
            // 
            this.rwltEnabledChb.AutoSize = true;
            this.rwltEnabledChb.Location = new System.Drawing.Point(10, 84);
            this.rwltEnabledChb.Name = "rwltEnabledChb";
            this.rwltEnabledChb.Size = new System.Drawing.Size(194, 27);
            this.rwltEnabledChb.TabIndex = 3;
            this.rwltEnabledChb.Text = "RWLT Pinger enabled";
            this.rwltEnabledChb.UseVisualStyleBackColor = true;
            this.rwltEnabledChb.CheckedChanged += new System.EventHandler(this.rwltEnabledChb_CheckedChanged);
            // 
            // channelDescriptionLbl
            // 
            this.channelDescriptionLbl.AutoSize = true;
            this.channelDescriptionLbl.Location = new System.Drawing.Point(197, 39);
            this.channelDescriptionLbl.Name = "channelDescriptionLbl";
            this.channelDescriptionLbl.Size = new System.Drawing.Size(216, 23);
            this.channelDescriptionLbl.TabIndex = 2;
            this.channelDescriptionLbl.Text = "32768 Hz, LOW SIDE BAND";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "Channel";
            // 
            // channelIDEdit
            // 
            this.channelIDEdit.Location = new System.Drawing.Point(85, 37);
            this.channelIDEdit.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.channelIDEdit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.channelIDEdit.Name = "channelIDEdit";
            this.channelIDEdit.Size = new System.Drawing.Size(106, 30);
            this.channelIDEdit.TabIndex = 8;
            this.channelIDEdit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.channelIDEdit.ValueChanged += new System.EventHandler(this.channelIDEdit_ValueChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 599);
            this.Controls.Add(this.settingsGroup);
            this.Controls.Add(this.applySettingsBtn);
            this.Controls.Add(this.querySettingsBtn);
            this.Controls.Add(this.refreshPortsBtn);
            this.Controls.Add(this.deviceInfoGroup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.connectionBtn);
            this.Controls.Add(this.portNameCbx);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.deviceInfoGroup.ResumeLayout(false);
            this.settingsGroup.ResumeLayout(false);
            this.settingsGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rwltChannelIDEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rwltDiverIDEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.channelIDEdit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ComboBox portNameCbx;
        private System.Windows.Forms.Button connectionBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox deviceInfoGroup;
        private System.Windows.Forms.Button refreshPortsBtn;
        private System.Windows.Forms.ToolStripStatusLabel statusLbl;
        private System.Windows.Forms.Button querySettingsBtn;
        private System.Windows.Forms.Button applySettingsBtn;
        private System.Windows.Forms.RichTextBox deviceInfoTxb;
        private System.Windows.Forms.GroupBox settingsGroup;
        private System.Windows.Forms.CheckBox rwltEnabledChb;
        private System.Windows.Forms.Label channelDescriptionLbl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown rwltChannelIDEdit;
        private System.Windows.Forms.Label rwltChannelIDLbl;
        private System.Windows.Forms.NumericUpDown rwltDiverIDEdit;
        private System.Windows.Forms.Label rwltDiverIDLbl;
        private System.Windows.Forms.NumericUpDown channelIDEdit;
    }
}

