using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FileReminder.utility
{
    class utilitys
    {
        /*
         * xml読み込み
         */
        public bool xmlRead<T>(ref T xmlClass, string xmlName, string errorMessage)
        {
            XmlSerializer serializer = new XmlSerializer( typeof( T ) );

            StreamReader sr = new StreamReader( xmlName, new UTF8Encoding( false ) );

            try
            {
                // エラーメッセージ表示中にファイルがロックされるため
                // 先にcloseさせるために二重try
                try
                {
                    //XMLファイルから読み込み、逆シリアル化する
                    xmlClass = ( T )serializer.Deserialize( sr );
                }
                finally
                {
                    // tryの如何に関わらずfilallyは実行されるのでここ以外でcloseの必要なし
                    if(sr != null) sr.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    errorMessage + "---\n" + e.Message,
                    "読み込みエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);    

                return false;
            }

            return true;
        }

        /*
            xml書み込み
        */
        public bool xmlWrite<T>(T xmlClass, string xmlName, string errorMessage)
        {
            XmlSerializer serializer1 = new XmlSerializer(typeof( T ));
            //ファイルを開く（UTF-8 BOM無し）
            StreamWriter sw = new StreamWriter(xmlName, false, new UTF8Encoding(false));
            
            try
            {
                // エラーメッセージ表示中にファイルがロックされるため
                // 先にcloseさせるために二重try
                try
                {
                    //シリアル化し、XMLファイルに保存する
                    serializer1.Serialize(sw, xmlClass);
                }
                finally
                {
                    // tryの如何に関わらずfilallyは実行されるのでここ以外でcloseの必要なし
                    if(sw != null) sw.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    errorMessage + "---\n" + e.Message,
                    "読み込みエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);    

                return false;
            }

            return true;            
        }
    }

    /// ボタンのテキストをカスタマイズできるメッセージボックスです。
    /// 
    public class ButtonTextCustomizableMessageBox
    {
        private IntPtr hHook = IntPtr.Zero;

        /// 

        /// ボタンに表示するテキストを指定します。
        /// 
        public CustomButtonText ButtonText { get; set; }

        /// 

        /// コンストラクタ。
        /// 
        public ButtonTextCustomizableMessageBox()
        {
            this.ButtonText = new CustomButtonText();
        }

        /// 

        /// ダイアログボックスを表示します。
        /// 
        public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icons)
        {
            try
            {
                BeginHook();
                return MessageBox.Show(text, caption, buttons, icons);
            }
            finally
            {
                EndHook();
            }
        }

        /// 

        /// フックを開始します。
        /// 
        void BeginHook()
        {
            EndHook();
            this.hHook = SetWindowsHookEx(WH_CBT, new HOOKPROC(this.HookProc), IntPtr.Zero, GetCurrentThreadId());
        }

        IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == HCBT_ACTIVATE)
            {
                if (this.ButtonText.Abort != null) SetDlgItemText(wParam, ID_BUT_ABORT, this.ButtonText.Abort);
                if (this.ButtonText.Cancel != null) SetDlgItemText(wParam, ID_BUT_CANCEL, this.ButtonText.Cancel);
                if (this.ButtonText.Ignore != null) SetDlgItemText(wParam, ID_BUT_IGNORE, this.ButtonText.Ignore);
                if (this.ButtonText.No != null) SetDlgItemText(wParam, ID_BUT_NO, this.ButtonText.No);
                if (this.ButtonText.OK != null) SetDlgItemText(wParam, ID_BUT_OK, this.ButtonText.OK);
                if (this.ButtonText.Retry != null) SetDlgItemText(wParam, ID_BUT_RETRY, this.ButtonText.Retry);
                if (this.ButtonText.Yes != null) SetDlgItemText(wParam, ID_BUT_YES, this.ButtonText.Yes);

                EndHook();
            }

            return CallNextHookEx(this.hHook, nCode, wParam, lParam);
        }

        /// 

        /// フックを終了します。何回呼んでもOKです。
        /// 
        void EndHook()
        {
            if (this.hHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(this.hHook);
                this.hHook = IntPtr.Zero;
            }
        }

        #region メッセージのテキストのクラス定義

        public class CustomButtonText
        {
            public string OK { get; set; }
            public string Cancel { get; set; }
            public string Abort { get; set; }
            public string Retry { get; set; }
            public string Ignore { get; set; }
            public string Yes { get; set; }
            public string No { get; set; }
        }

        #endregion

        #region Win32API

        const int WH_CBT = 5;
        const int HCBT_ACTIVATE = 5;

        const int ID_BUT_OK = 1;
        const int ID_BUT_CANCEL = 2;
        const int ID_BUT_ABORT = 3;
        const int ID_BUT_RETRY = 4;
        const int ID_BUT_IGNORE = 5;
        const int ID_BUT_YES = 6;
        const int ID_BUT_NO = 7;

        private delegate IntPtr HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hInstance, IntPtr threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThreadId();

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        private static extern bool SetDlgItemText(IntPtr hWnd, int nIDDlgItem, string lpString);

        #endregion
    }
}
