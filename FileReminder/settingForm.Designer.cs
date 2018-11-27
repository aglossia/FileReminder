namespace FileReminder
{
    partial class SettingForm
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
            this.chboxStaWin = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCansel = new System.Windows.Forms.Button();
            this.chboxHighLight = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chboxStaWin
            // 
            this.chboxStaWin.AutoSize = true;
            this.chboxStaWin.Location = new System.Drawing.Point(23, 33);
            this.chboxStaWin.Name = "chboxStaWin";
            this.chboxStaWin.Size = new System.Drawing.Size(136, 28);
            this.chboxStaWin.TabIndex = 0;
            this.chboxStaWin.Text = "外部から起動するときは\r\n新しいウィンドウで開く";
            this.chboxStaWin.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(102, 227);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCansel
            // 
            this.btnCansel.Location = new System.Drawing.Point(186, 227);
            this.btnCansel.Name = "btnCansel";
            this.btnCansel.Size = new System.Drawing.Size(75, 23);
            this.btnCansel.TabIndex = 1;
            this.btnCansel.Text = "キャンセル";
            this.btnCansel.UseVisualStyleBackColor = true;
            this.btnCansel.Click += new System.EventHandler(this.btnCansel_Click);
            // 
            // chboxHighLight
            // 
            this.chboxHighLight.AutoSize = true;
            this.chboxHighLight.Location = new System.Drawing.Point(23, 95);
            this.chboxHighLight.Name = "chboxHighLight";
            this.chboxHighLight.Size = new System.Drawing.Size(204, 16);
            this.chboxHighLight.TabIndex = 0;
            this.chboxHighLight.Text = "カーソル位置の行をハイライト表示する";
            this.chboxHighLight.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(165, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "不完全";
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCansel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chboxHighLight);
            this.Controls.Add(this.chboxStaWin);
            this.Name = "SettingForm";
            this.ShowIcon = false;
            this.Text = "共通設定";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chboxStaWin;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCansel;
        private System.Windows.Forms.CheckBox chboxHighLight;
        private System.Windows.Forms.Label label1;
    }
}