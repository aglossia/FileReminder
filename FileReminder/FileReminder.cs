using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using FileReminder.utility;
using System.Diagnostics; //process
using System.Runtime.InteropServices;

namespace FileReminder
{
    public partial class FileReminder : Form
    {
        /****************** クラス ******************/

        // fremファイル作成用クラス
        public class filepaths
        {
            public List<string> filepathList = new List<string>() { };
        }

        // exe設定クラス
        public class settings
        {
            //public string dialog_frem_savepath { set; get; }
            //public string dialog_playlist_savepath { set; get; }
            public string[] dialog_savepath { set; get; }
            public string[] filename { set; get; }
            public string listbox_font_type { set; get; }
            public float listbox_font_size { set; get; }
            public bool highlightFlg { set; get; }
            public int form_width { set; get; }
            public int form_height { set; get; }
            public int form_start_x { set; get; }
            public int form_start_y { set; get; }

            public settings()
            {
                //dialog_frem_savepath = EXE_PATH;
                //dialog_playlist_savepath = EXE_PATH;
                dialog_savepath = new string[] { EXE_PATH , EXE_PATH };
                filename = new string[] { INIT_FILENAME, "" };
                listbox_font_size = 12;
                listbox_font_type = "メイリオ";
                highlightFlg = false;
                form_width = 452;
                form_height = 431;
                form_start_x = 200;
                form_start_y = 200;
            }
        }

        // ファイル種別
        public enum FileType
        {
            FREM,
            MPCPL,
        }

        /****************** クラス ******************/



        /****************** グローバル変数 ******************/
        /// <summary>フォームタイトルにつける開いているfremファイル名</summary>
        //string readFileName = "新規";
        const string EXE_NAME = "File Reminder";
        const string INIT_FILENAME = "新規";
        const string FREM_CONFIG = "frem.config";
        const string FREM_FILTER = "FREMファイル(*.frem)|*.frem|すべてのファイル(*.*)|*.*";
        const string MPCPL_FILTER = "MPCPLファイル(*.mpcpl)|*.mpcpl|すべてのファイル(*.*)|*.*";
        static string EXE_PATH = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string CONFIG_PATH = string.Format("{0}\\{1}", EXE_PATH, FREM_CONFIG);

        List<string> movie_ext = new List<string> { ".mp4", ".wmv", ".avi", ".mov", ".mpeg", ".mkv", ".flv" };

        // 保存済みフラグ
        bool alreadySaveFlg = false;
        // 新規フラグ
        bool initialFlg = true;
        // 更新フラグ
        bool updateFlg = false;
        // Ctrlキーフラグ
        bool ctrlFlg = false;
        // リストボックスマウスオーバー中のインデックス
        int mouseoverIndex = -1;

        filepaths appFilePaths = new filepaths();

        settings appSettings = new settings();
        
        static utilitys util = new utilitys();

        /****************** グローバル変数 ******************/



        /****************** 関数 ******************/

        /*
         * ファイルorフォルダを開く
         */
        private void fileExecute()
        {
            if (listBox1.SelectedIndex != -1)
            {
                string a = appFilePaths.filepathList[listBox1.SelectedIndex];

                if (File.Exists(a))
                {
                    Process p = Process.Start(a);
                    p.Close();
                }
                else if (Directory.Exists(a))
                {
                    Process.Start(a);
                }
            }
        }
        
        /*
         * フォームタイトル更新
         */
        private void formTitleUpdate()
        {
            if (alreadySaveFlg)
            {
                // 保存済みであればタイトルに更新をいれる
                this.Text += " (更新)";
                // 更新がかかったので保存済みフラグをおとす
                alreadySaveFlg = false;
            }
        }

        /*
         * ファイル保存
         */
        private void fileSaveOut(FileType filetype)
        {
            string savepath = string.Format("{0}\\{1}", appSettings.dialog_savepath[(int)filetype], appSettings.filename[(int)filetype]);

            // 保存ファイル切り替え
            switch (filetype)
            {
                case FileType.FREM:     // fremファイル

                    util.xmlWrite(appFilePaths, savepath, "書込みエラー");

                    this.Text = string.Format("{0} [{1}]", EXE_NAME, appSettings.filename[(int)FileType.FREM]);

                    overwriteToolStripMenuItem.Enabled = true;
                    // 保存済みとし、新規をはずす
                    alreadySaveFlg = true;
                    initialFlg = false;

                    break;

                case FileType.MPCPL:    // mpcplファイル

                    int lineCnt = 1;

                    // ファイルを上書きし、書き込む
                    StreamWriter sw = new StreamWriter(savepath, false, Encoding.GetEncoding("utf-8"));

                    sw.WriteLine("MPCPLAYLIST");

                    foreach (string item in appFilePaths.filepathList)
                    {
                        if (File.Exists(item))
                        {
                            if (movie_ext.Contains(Path.GetExtension(item)))
                            {
                                sw.WriteLine(string.Format("{0},type,0", lineCnt));
                                sw.WriteLine(string.Format("{0},filename,{1}", lineCnt, item));
                                lineCnt++;
                            }
                        }
                    }
                    // 閉じる
                    sw.Close();

                    break;

                default:
                    break;
            }
        }

        /*
         * 名前をつけて保存
         */
        private DialogResult saveAs(FileType filetype, string filter)
        {
            // SaveFileDialogクラスのインスタンスを作成
            SaveFileDialog sfd = new SaveFileDialog();

            // はじめのファイル名を指定する
            sfd.FileName = "";

            // はじめに表示されるフォルダを指定する
            sfd.InitialDirectory = appSettings.dialog_savepath[(int)filetype];

            // [ファイルの種類]に表示される選択肢を指定する
            // 指定しない（空の文字列）の時は、現在のディレクトリが表示される
            //sfd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";

            sfd.Filter = filter;

            // [ファイルの種類]ではじめに選択されるものを指定する
            sfd.FilterIndex = 1;

            // ダイアログタイトル
            sfd.Title = "保存先のファイルを選択してください";

            // ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;

            DialogResult dialog_ret = sfd.ShowDialog();

            // ダイアログを表示する
            if (dialog_ret == DialogResult.OK)
            {
                // ダイアログ初期パスを記憶する
                appSettings.dialog_savepath[(int)filetype] = Path.GetDirectoryName(sfd.FileName);
                // 保存ファイル名だけを取り出す
                appSettings.filename[(int)filetype] = Path.GetFileName(sfd.FileName);

                fileSaveOut(filetype);
            }

            return dialog_ret;
        }

        /*
         * エクスプローラーでフォルダを開く
         */
        private void openFolder(int index)
        {
            Process.Start("EXPLORER.EXE", "/select," + appFilePaths.filepathList[index]);
        }

        /*
         * 設定イニシャル処理
         */
        private void settingInitialize()
        {
            listBox1.Font = new Font(appSettings.listbox_font_type, appSettings.listbox_font_size);
            highligntToolStripMenuItem.Checked = appSettings.highlightFlg;
            this.Width = appSettings.form_width;
            this.Height = appSettings.form_height;
            this.Location = new Point(appSettings.form_start_x, appSettings.form_start_y);

            // ファイル読み込みがあればファイル名をタイトルに、なければ新規をタイトルにつける
            this.Text += string.Format(" [{0}]", appSettings.filename[(int)FileType.FREM]);
        }

        /*
         * 設定ファイナル処理
         */
        private void settingFinalize()
        {
            appSettings.form_width = this.Width;
            appSettings.form_height = this.Height;
            appSettings.form_start_x = this.Location.X;
            appSettings.form_start_y = this.Location.Y;
        }

        /****************** 関数 ******************/



        public FileReminder()
        {
            InitializeComponent();

            listBox1.ContextMenuStrip = this.contextMenuStrip1;

            listBox1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.listBox1_MouseWheel);  

            overwriteToolStripMenuItem.Enabled = false;

            label1.Text = "";

            string[] cmds = System.Environment.GetCommandLineArgs();

            // frem.config があれば読み込む
            if (File.Exists(CONFIG_PATH))
            {
                util.xmlRead(ref appSettings, CONFIG_PATH, "0");
            }

            if (cmds.Length == 2)
            {
                // fremファイル
                string fremFile = cmds[1];

                util.xmlRead(ref appFilePaths, fremFile, "1");

                foreach (string item in appFilePaths.filepathList)
                {
                    listBox1.Items.Add(Path.GetFileName(item));
                }

                appSettings.dialog_savepath[(int)FileType.FREM] = Path.GetDirectoryName(fremFile);
                appSettings.filename[(int)FileType.FREM] = Path.GetFileName(fremFile);

                overwriteToolStripMenuItem.Enabled = true;
                // ファイル読み込みだからセーブ済みとし、新規ではない
                alreadySaveFlg = true;
                initialFlg = false;
            }

            // イニシャル処理
            settingInitialize();
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            // コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                // ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                // ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            // コントロール内にドロップされたとき実行される
            // ドロップされたすべてのファイル名を取得する
            string[] fileNames =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);
            // ListBoxに追加する
            listBox1.Items.AddRange(fileNames.Select(i => Path.GetFileName(i)).ToArray());
            // 参照パスリストに追加する
            appFilePaths.filepathList.AddRange(fileNames);

            // フォームタイトル更新
            formTitleUpdate();
            updateFlg = true;
        }

        /* 
         * リストボックスのダブルクリックでファイルorフォルダを開く
         */
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            fileExecute();
        }

        /* 
         * リストボックスのキー操作
         */
        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            Keys inputKey = e.KeyData;

            int sel = listBox1.SelectedIndex;

            ListBox.SelectedIndexCollection Indices = listBox1.SelectedIndices;

            List<string> itemList = new List<string>();

            if (sel == -1) return;

            switch (inputKey)
            {
                case Keys.Delete:

                    List<string> removeList = new List<string>();

                    // 削除リストを作成（パス参照リスト用）
                    foreach (int i in listBox1.SelectedIndices)
                    {
                        removeList.Add(appFilePaths.filepathList[i]);
                    }
                    // 削除リストに該当するものを削除（パス参照リスト用）
                    foreach (string s in removeList)
                    {
                        appFilePaths.filepathList.Remove(s);
                    }
                    // 削除リストを作成（リストボックス用）
                    for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                    {
                        itemList.Add(listBox1.SelectedItems[i].ToString());
                    }
                    // 削除リストに該当するものを削除（リストボックス用）
                    foreach (var b in itemList)
                    {
                        listBox1.Items.Remove(b.ToString());
                    }

                    formTitleUpdate();

                    updateFlg = true;
                    
                    break;

                case Keys.Enter:

                    fileExecute();

                    break;

                case Keys.ControlKey:

                    ctrlFlg = false;
                    ShowScrollBar(listBox1.Handle, 1, true);

                    break;

                default:
                    break;
            }
        }

        /*
         * フォームを閉じるときに保存するか確認する
         */
        private void FileReminder_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 保存されておらず更新があるときに保存確認ダイアログを出す
            if (!alreadySaveFlg && updateFlg)
            {
                var msg = new ButtonTextCustomizableMessageBox();
                msg.ButtonText.Yes = "保存";
                msg.ButtonText.No = "保存しない";
                msg.ButtonText.Cancel = "キャンセル";
                DialogResult result = 
                    msg.Show("ファイルが保存されていません。", "File Reminder", 
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                switch (result)
                {
                    case DialogResult.Cancel:   // キャンセル
                        // キャンセルは終了を取り消す
                        e.Cancel = true;

                        break;

                    case DialogResult.Yes:  // 保存する
                        // 新規？
                        if (initialFlg)
                        {
                            // 新規なので名前をつけて保存する
                            if (saveAs(FileType.FREM, FREM_FILTER) == DialogResult.Cancel)
                            {
                                // ダイアログでキャンセルで保存しない場合は終了しない
                                e.Cancel = true;
                            }
                        }
                        else
                        {
                            // 新規ではないので上書き保存
                            fileSaveOut(FileType.FREM);
                        }
                        break;

                    default:
                        break;
                }
            }
            // ファイナル処理
            settingFinalize();
            // exe設定を保存する
            util.xmlWrite(appSettings, CONFIG_PATH, "書込みエラー");
        }

        /*
         * メニュー：上書き
         */
        private void overwriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileSaveOut(FileType.FREM);
        }

        /*
         * メニュー：名前をつけて保存
         */
        private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs(FileType.FREM, FREM_FILTER);
        }

        /*
         * メニュー：プレイリスト作成
         */
        private void playlistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs(FileType.MPCPL, MPCPL_FILTER);
        }

        /* 
         * 編集：リストクリア
         */
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            appFilePaths.filepathList.Clear();

            formTitleUpdate();

            updateFlg = true;
        }

        /*
         * リストボックスマウスムーブ
         */
        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // フォーム上の座標でマウスポインタの位置を取得する
            // 画面座標でマウスポインタの位置を取得する
            System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
            // 画面座標をクライアント座標に変換する
            System.Drawing.Point cp = this.PointToClient(sp);

            Point Location = e.Location;
            mouseoverIndex = listBox1.IndexFromPoint(Location);

            if (mouseoverIndex != -1)
            {
                label1.Text = appFilePaths.filepathList[mouseoverIndex];

                if (appSettings.highlightFlg)
                {
                    listBox1.ClearSelected();
                    listBox1.SetSelected(mouseoverIndex, true);
                }
            }
        }

        /*
         * リストボックス右クリック：フォルダを開く
         */
        private void folderOpneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(mouseoverIndex != -1) openFolder(mouseoverIndex);
        }

        /*
         * リストボックスマウスホイール：拡大縮小
         */          
        private void listBox1_MouseWheel(object sender, MouseEventArgs e)  
        {  
            if (ctrlFlg)
            {
                if (e.Delta > 0)
                {
                    if(listBox1.Font.Size <= 20) listBox1.Font = new Font("メイリオ", listBox1.Font.Size + 2);
                    if(ctrlFlg) ShowScrollBar(listBox1.Handle, 1, false);
                }
                else
                {
                    if(listBox1.Font.Size >= 10) listBox1.Font = new Font("メイリオ", listBox1.Font.Size - 2);
                    if(ctrlFlg) ShowScrollBar(listBox1.Handle, 1, false);
                }
                appSettings.listbox_font_size = listBox1.Font.Size;
            }
        }

        // スクロールバー操作関数
        [DllImport("user32.dll")]
        static extern bool ShowScrollBar(
            IntPtr hWnd,  // ウィンドウのハンドル
            int wBar,   // スクロールバー
            bool bShow  // スクロールバーを表示するかどうか
        );

        /*
         * リストボックスキーダウン
         */
        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            Keys input = e.KeyCode;

            switch (input)
            {
                case Keys.ControlKey:

                    ctrlFlg = true;
                    
                    ShowScrollBar(listBox1.Handle, 1, false);

                    break;

                default:
                    break;
            }
        }

        /*
         * メニュー：終了
         */
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /*
         * 設定：行ハイライト
         */
        private void highligntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            item.Checked = !item.Checked;
            appSettings.highlightFlg = item.Checked;
        }

        /*
         * マウスリーブ
         */
        private void listBox1_MouseLeave(object sender, EventArgs e)
        {
            label1.Text = "";
        }
    }
}
