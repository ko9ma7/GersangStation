﻿namespace GersangStation {
    partial class Form_Patcher {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Patcher));
            materialCheckbox_delete = new MaterialSkin.Controls.MaterialCheckbox();
            materialCheckbox_apply = new MaterialSkin.Controls.MaterialCheckbox();
            materialCard1 = new MaterialSkin.Controls.MaterialCard();
            label2 = new Label();
            textBox_latestVersion = new TextBox();
            textBox_currentVersion = new TextBox();
            label1 = new Label();
            materialButton_startPatch = new CustomButton();
            label_status = new Label();
            toolTip1 = new ToolTip(components);
            label_total = new Label();
            materialCard1.SuspendLayout();
            SuspendLayout();
            // 
            // materialCheckbox_delete
            // 
            materialCheckbox_delete.AutoSize = true;
            materialCheckbox_delete.Checked = true;
            materialCheckbox_delete.CheckState = CheckState.Checked;
            materialCheckbox_delete.Depth = 0;
            materialCheckbox_delete.Location = new Point(212, 125);
            materialCheckbox_delete.Margin = new Padding(0);
            materialCheckbox_delete.MouseLocation = new Point(-1, -1);
            materialCheckbox_delete.MouseState = MaterialSkin.MouseState.HOVER;
            materialCheckbox_delete.Name = "materialCheckbox_delete";
            materialCheckbox_delete.ReadOnly = false;
            materialCheckbox_delete.Ripple = true;
            materialCheckbox_delete.Size = new Size(131, 37);
            materialCheckbox_delete.TabIndex = 41;
            materialCheckbox_delete.Text = "패치 후 파일 삭제";
            materialCheckbox_delete.UseVisualStyleBackColor = true;
            // 
            // materialCheckbox_apply
            // 
            materialCheckbox_apply.AutoSize = true;
            materialCheckbox_apply.Checked = true;
            materialCheckbox_apply.CheckState = CheckState.Checked;
            materialCheckbox_apply.Depth = 0;
            materialCheckbox_apply.Location = new Point(212, 83);
            materialCheckbox_apply.Margin = new Padding(0);
            materialCheckbox_apply.MouseLocation = new Point(-1, -1);
            materialCheckbox_apply.MouseState = MaterialSkin.MouseState.HOVER;
            materialCheckbox_apply.Name = "materialCheckbox_apply";
            materialCheckbox_apply.ReadOnly = false;
            materialCheckbox_apply.Ripple = true;
            materialCheckbox_apply.Size = new Size(127, 37);
            materialCheckbox_apply.TabIndex = 40;
            materialCheckbox_apply.Text = "다클라 패치 적용";
            materialCheckbox_apply.UseVisualStyleBackColor = true;
            // 
            // materialCard1
            // 
            materialCard1.BackColor = Color.FromArgb(255, 255, 255);
            materialCard1.BorderStyle = BorderStyle.FixedSingle;
            materialCard1.Controls.Add(label2);
            materialCard1.Controls.Add(textBox_latestVersion);
            materialCard1.Controls.Add(textBox_currentVersion);
            materialCard1.Controls.Add(label1);
            materialCard1.Depth = 0;
            materialCard1.ForeColor = Color.FromArgb(222, 0, 0, 0);
            materialCard1.Location = new Point(18, 81);
            materialCard1.Margin = new Padding(14);
            materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            materialCard1.Name = "materialCard1";
            materialCard1.Padding = new Padding(14);
            materialCard1.Size = new Size(181, 83);
            materialCard1.TabIndex = 43;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.FlatStyle = FlatStyle.System;
            label2.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(22, 48);
            label2.Name = "label2";
            label2.Size = new Size(60, 16);
            label2.TabIndex = 37;
            label2.Text = "최신 버전 :";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // textBox_latestVersion
            // 
            textBox_latestVersion.Location = new Point(88, 46);
            textBox_latestVersion.Name = "textBox_latestVersion";
            textBox_latestVersion.ReadOnly = true;
            textBox_latestVersion.Size = new Size(62, 23);
            textBox_latestVersion.TabIndex = 36;
            textBox_latestVersion.Text = "00000";
            textBox_latestVersion.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_currentVersion
            // 
            textBox_currentVersion.Location = new Point(88, 11);
            textBox_currentVersion.Name = "textBox_currentVersion";
            textBox_currentVersion.Size = new Size(62, 23);
            textBox_currentVersion.TabIndex = 34;
            textBox_currentVersion.Text = "00000";
            textBox_currentVersion.TextAlign = HorizontalAlignment.Center;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.FlatStyle = FlatStyle.System;
            label1.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(22, 13);
            label1.Name = "label1";
            label1.Size = new Size(60, 16);
            label1.TabIndex = 33;
            label1.Text = "현재 버전 :";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // materialButton_startPatch
            // 
            materialButton_startPatch.BackColor = Color.FromArgb(51, 71, 79);
            materialButton_startPatch.BackgroundColor = Color.FromArgb(51, 71, 79);
            materialButton_startPatch.BorderColor = Color.PaleVioletRed;
            materialButton_startPatch.BorderRadius = 5;
            materialButton_startPatch.BorderSize = 0;
            materialButton_startPatch.FlatAppearance.BorderSize = 0;
            materialButton_startPatch.FlatStyle = FlatStyle.Flat;
            materialButton_startPatch.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            materialButton_startPatch.ForeColor = Color.White;
            materialButton_startPatch.Image = (Image)resources.GetObject("materialButton_startPatch.Image");
            materialButton_startPatch.ImageAlign = ContentAlignment.MiddleRight;
            materialButton_startPatch.Location = new Point(130, 247);
            materialButton_startPatch.Name = "materialButton_startPatch";
            materialButton_startPatch.Padding = new Padding(5);
            materialButton_startPatch.Size = new Size(102, 36);
            materialButton_startPatch.TabIndex = 69;
            materialButton_startPatch.Text = "패치시작";
            materialButton_startPatch.TextAlign = ContentAlignment.MiddleLeft;
            materialButton_startPatch.TextColor = Color.White;
            materialButton_startPatch.UseVisualStyleBackColor = false;
            materialButton_startPatch.Click += materialButton_startPatch_Click;
            // 
            // label_status
            // 
            label_status.FlatStyle = FlatStyle.System;
            label_status.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            label_status.ForeColor = Color.Black;
            label_status.Location = new Point(6, 181);
            label_status.Name = "label_status";
            label_status.Size = new Size(353, 18);
            label_status.TabIndex = 72;
            label_status.Text = "패치 시작 버튼을 누르면 패치가 시작됩니다.";
            label_status.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label_total
            // 
            label_total.FlatStyle = FlatStyle.System;
            label_total.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point);
            label_total.ForeColor = Color.Black;
            label_total.Location = new Point(6, 210);
            label_total.Name = "label_total";
            label_total.Size = new Size(353, 18);
            label_total.TabIndex = 73;
            label_total.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form_Patcher
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(365, 295);
            Controls.Add(label_total);
            Controls.Add(label_status);
            Controls.Add(materialButton_startPatch);
            Controls.Add(materialCard1);
            Controls.Add(materialCheckbox_delete);
            Controls.Add(materialCheckbox_apply);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form_Patcher";
            ShowInTaskbar = false;
            Sizable = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "GersangPatcher";
            TopMost = true;
            Load += Form_Patcher_v2_Load;
            materialCard1.ResumeLayout(false);
            materialCard1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox_delete;
        private MaterialSkin.Controls.MaterialCheckbox materialCheckbox_apply;
        private MaterialSkin.Controls.MaterialCard materialCard1;
        private Label label2;
        private TextBox textBox_latestVersion;
        private TextBox textBox_currentVersion;
        private Label label1;
        private CustomButton materialButton_startPatch;
        private Label label_status;
        private ToolTip toolTip1;
        private Label label_total;
    }
}