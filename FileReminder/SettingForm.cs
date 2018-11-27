using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileReminder
{
    public partial class SettingForm : Form
    {
        static SettingForm fm;
        static FileReminder.save_config retSaveConfigs = new FileReminder.save_config();

        public SettingForm(FileReminder.save_config saveConfigs)
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            chboxStaWin.Checked = saveConfigs.startup_windowFlg;
            chboxHighLight.Checked = saveConfigs.highlightFlg;
        }

        static public void showSettingForm( ref FileReminder.save_config saveConfigs )
        {
            retSaveConfigs = saveConfigs;
            fm = new SettingForm( retSaveConfigs );
            fm.StartPosition = FormStartPosition.CenterParent;
            fm.ShowDialog();
            fm.Dispose();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            retSaveConfigs.startup_windowFlg = chboxStaWin.Checked;
            retSaveConfigs.highlightFlg = chboxHighLight.Checked;

            this.Close();
        }

        private void btnCansel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
