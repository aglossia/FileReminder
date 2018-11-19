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
    using NAME_PATH_DIC = Dictionary<string, string>;
    //using NAME_LIST_DIC = Dictionary<string, List<string>>;

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
            
            //public string[] dialog_savepath { set; get; }
            //public string[] filename { set; get; }

            
            

            [XmlAttribute]
            public string listbox_font_type { set; get; }
            [XmlAttribute]
            public float listbox_font_size { set; get; }
            [XmlAttribute]
            public bool highlightFlg { set; get; }
            [XmlAttribute]
            public int form_width { set; get; }
            [XmlAttribute]
            public int form_height { set; get; }
            [XmlAttribute]
            public int form_start_x { set; get; }
            [XmlAttribute]
            public int form_start_y { set; get; }

            [XmlIgnore]
            // key: file name, value: dialog path
            public NAME_PATH_DIC filename_path { get; set; }
            [XmlIgnore]
            public Dictionary<string, filepaths> filelist { get; set; }
            [XmlIgnore]
            private FileReminder parent;

            public settings()
            {

            }

            public settings(FileReminder parent)
            {
                //dialog_frem_savepath = EXE_PATH;
                //dialog_playlist_savepath = EXE_PATH;
                //dialog_savepath = new string[] { EXE_PATH , EXE_PATH };
                filename_path = new NAME_PATH_DIC{{INIT_SYMBOL, INIT_FILENAME}, {DEFAULT_SYMBOL, EXE_PATH}};
                filelist = new Dictionary<string, filepaths>();
                listbox_font_size = 12;
                listbox_font_type = "メイリオ";
                highlightFlg = false;
                form_width = 452;
                form_height = 431;
                form_start_x = 200;
                form_start_y = 200;

                this.parent = parent;
            }

            public string getFullPath(string fileName)
            {
                return string.Format("{0}\\{1}", this.filename_path[fileName], fileName);
            }

            public List<string> getCurrentList()
            {
                return this.filelist[parent.currentFileName[(int)FileType.FREM]].filepathList;
            }
        }

        // ファイル種別
        public enum FileType
        {
            FREM,
            MPCPL,
        }

        public struct filename_struct
        {
            public string path;
            public string filename;
        }

        /****************** クラス ******************/



        /****************** グローバル変数 ******************/
        /// <summary>フォームタイトルにつける開いているfremファイル名</summary>
        //string readFileName = "新規";
        const string EXE_NAME = "File Reminder";
        const string INIT_SYMBOL = "init";
        const string INIT_FILENAME = "新規";
        const string DEFAULT_SYMBOL = "default";
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
        // タブ数
        int tabCnt = 0;

        ListBox currentListBox = null;

        string[] currentFileName = new string[2].Select(i => i = DEFAULT_SYMBOL).ToArray();

        //filepaths appFilePaths = new filepaths();

        settings appSettings;

        //reminderTab reminderTabs = new reminderTab(FileReminder.Default);
        
        reminderTab reminderTabs;

        static utilitys util = new utilitys();

        /****************** グローバル変数 ******************/



        /****************** 関数 ******************/

        /*
         * fremファイル読み込み、リストボックス設定
         */
        private void fremRead(ref filepaths fP, string fremFile)
        {
            util.xmlRead(ref fP, fremFile, "1");

            string fileName = "";
            string filePath = "";

            fileName = Path.GetFileName(fremFile);
            filePath = Path.GetDirectoryName(fremFile);

            appSettings.filename_path.Add(fileName, filePath);
                
            // 現在のタブで開いているファイル名を変更
            currentFileName[(int)FileType.FREM] = fileName;

            // fremファイルごとにファイルリストを設定
            appSettings.filelist.Add(fileName, fP);

            // リストボックスにアイテムを追加
            foreach (string item in fP.filepathList)
            {
                currentListBox.Items.Add(Path.GetFileName(item));
            }
        }

        /*
         * タブ追加処理
         */
        private void addTab(string tabName)
        {
            reminderTabs.tabs.Add(new TabPage(tabName));
            reminderTabs.listboxs.Add(new ListBox());
            reminderTabs.tabInit(tabCnt);
            currentListBox = reminderTabs.listboxs[tabCnt];

            tabControl1.SelectedIndex = tabCnt;

            tabCnt++;
        }

        /*
         * ファイルorフォルダを開く
         */
        private void fileExecute()
        {
            if (currentListBox.SelectedIndex != -1)
            {
                string a = appSettings.getCurrentList()[currentListBox.SelectedIndex];

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
            string fileName = currentFileName[(int)filetype];                       // 保存ファイル名
            string savepath = appSettings.getFullPath(fileName);                    // 保存ファイルフルパス
            filepaths fP = appSettings.filelist[currentFileName[(int)filetype]];    // 保存用クラス

            // 保存ファイル切り替え
            switch (filetype)
            {
                case FileType.FREM:     // fremファイル

                    util.xmlWrite(fP, savepath, "書込みエラー");

                    this.Text = string.Format("{0} [{1}]", EXE_NAME, fileName);

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

                    foreach (string item in fP.filepathList)
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
            //sfd.InitialDirectory = appSettings.dialog_savepath[(int)filetype];
            sfd.InitialDirectory = appSettings.filename_path[currentFileName[(int)filetype]];

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
                //appSettings.dialog_savepath[(int)filetype] = Path.GetDirectoryName(sfd.FileName);
                // 保存ファイル名だけを取り出す
                //appSettings.filename[(int)filetype] = Path.GetFileName(sfd.FileName);

                // ダイアログパスとファイル名を設定する
                appSettings.filename_path.Add(Path.GetFileName(sfd.FileName), Path.GetDirectoryName(sfd.FileName));

                string fileName = "";
                string filePath = "";

                fileName = Path.GetFileName(sfd.FileName);
                filePath = Path.GetDirectoryName(sfd.FileName);

                appSettings.filename_path.Add(fileName, filePath);

                // "新規"をファイル名に変更
                appSettings.filename_path[INIT_SYMBOL] = fileName;
                
                // 最新保存ファイル名を変更（ファイル種別毎）
                currentFileName[(int)filetype] = fileName;

                fileSaveOut(filetype);
            }

            return dialog_ret;
        }

        /*
         * エクスプローラーでフォルダを開く
         */
        private void openFolder(int index)
        {
            Process.Start("EXPLORER.EXE", "/select," + appSettings.filelist[currentFileName[(int)FileType.FREM]].filepathList[index]);
        }

        /*
         * 設定イニシャル処理
         */
        private void settingInitialize()
        {
            currentListBox.Font = new Font(appSettings.listbox_font_type, appSettings.listbox_font_size);
            highligntToolStripMenuItem.Checked = appSettings.highlightFlg;
            this.Width = appSettings.form_width;
            this.Height = appSettings.form_height;
            this.Location = new Point(appSettings.form_start_x, appSettings.form_start_y);

            // ファイル読み込みがあればファイル名をタイトルに、なければ新規をタイトルにつける
            this.Text += string.Format(" [{0}]", appSettings.filename_path[INIT_SYMBOL]);
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

            reminderTabs = new reminderTab(this);

            appSettings = new settings(this);

            overwriteToolStripMenuItem.Enabled = false;

            label1.Text = "";

            string[] cmds = System.Environment.GetCommandLineArgs();

            // frem.config があれば読み込む
            if (File.Exists(CONFIG_PATH))
            {
                util.xmlRead(ref appSettings, CONFIG_PATH, "0");
            }

            if (cmds.Length == 1)
            {
                // 新規で起動
                addTab(INIT_FILENAME);
            }
            else if (cmds.Length == 2)
            {
                // fremファイルから起動

                // fremファイルパス
                string fremFile = cmds[1];
                // fremファイル名
                string fileName = Path.GetFileName(fremFile);

                filepaths fP = new filepaths();

                fremRead(ref fP, fremFile);

                // "新規"をファイル名に変更
                appSettings.filename_path[INIT_SYMBOL] = fileName;

                overwriteToolStripMenuItem.Enabled = true;
                // ファイル読み込みだからセーブ済みとし、新規ではない
                alreadySaveFlg = true;
                initialFlg = false;
            }

            // イニシャル処理
            settingInitialize();
        }

        private void listBox_DragEnter(object sender, DragEventArgs e)
        {
            // コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                // ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                // ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        private void listBox_DragDrop(object sender, DragEventArgs e)
        {
            // コントロール内にドロップされたとき実行される
            // ドロップされたすべてのファイル名を取得する
            string[] fileNames =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (Path.GetExtension(fileNames[0]) == ".frem")
            {
                filepaths fP = new filepaths();

                addTab(Path.GetFileName(fileNames[0]));
                fremRead(ref fP, fileNames[0]);
            }
            else
            {
                // ListBoxに追加する
                currentListBox.Items.AddRange(fileNames.Select(i => Path.GetFileName(i)).ToArray());
                // 参照パスリストに追加する
                //appFilePaths.filepathList.AddRange(fileNames);
                appSettings.getCurrentList().AddRange(fileNames);

                // フォームタイトル更新
                formTitleUpdate();
                updateFlg = true;
            }


        }

        /* 
         * リストボックスのダブルクリックでファイルorフォルダを開く
         */
        private void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            fileExecute();
        }

        /* 
         * リストボックスのキー操作
         */
        private void listBox_KeyUp(object sender, KeyEventArgs e)
        {
            Keys inputKey = e.KeyData;

            int sel = currentListBox.SelectedIndex;

            ListBox.SelectedIndexCollection Indices = currentListBox.SelectedIndices;

            List<string> itemList = new List<string>();

            if (sel == -1) return;

            switch (inputKey)
            {
                case Keys.Delete:

                    List<string> removeList = new List<string>();

                    // 削除リストを作成（パス参照リスト用）
                    foreach (int i in currentListBox.SelectedIndices)
                    {
                        removeList.Add(appSettings.getCurrentList()[i]);
                    }
                    // 削除リストに該当するものを削除（パス参照リスト用）
                    foreach (string s in removeList)
                    {
                        appSettings.getCurrentList().Remove(s);
                    }
                    // 削除リストを作成（リストボックス用）
                    for (int i = 0; i < currentListBox.SelectedItems.Count; i++)
                    {
                        itemList.Add(currentListBox.SelectedItems[i].ToString());
                    }
                    // 削除リストに該当するものを削除（リストボックス用）
                    foreach (var b in itemList)
                    {
                        currentListBox.Items.Remove(b.ToString());
                    }

                    formTitleUpdate();

                    updateFlg = true;
                    
                    break;

                case Keys.Enter:

                    fileExecute();

                    break;

                case Keys.ControlKey:

                    ctrlFlg = false;
                    ShowScrollBar(currentListBox.Handle, 1, true);

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
            currentListBox.Items.Clear();
            appSettings.getCurrentList().Clear();

            formTitleUpdate();

            updateFlg = true;
        }

        /*
         * リストボックスマウスムーブ
         */
        private void listBox_MouseMove(object sender, MouseEventArgs e)
        {
            // フォーム上の座標でマウスポインタの位置を取得する
            // 画面座標でマウスポインタの位置を取得する
            System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
            // 画面座標をクライアント座標に変換する
            System.Drawing.Point cp = this.PointToClient(sp);

            Point Location = e.Location;
            mouseoverIndex = currentListBox.IndexFromPoint(Location);

            if (mouseoverIndex != -1)
            {
                label1.Text = appSettings.getCurrentList()[mouseoverIndex];

                if (appSettings.highlightFlg)
                {
                    currentListBox.ClearSelected();
                    currentListBox.SetSelected(mouseoverIndex, true);
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
        private void listBox_MouseWheel(object sender, MouseEventArgs e)  
        {  
            if (ctrlFlg)
            {
                if (e.Delta > 0)
                {
                    if(currentListBox.Font.Size <= 20) currentListBox.Font = new Font("メイリオ", currentListBox.Font.Size + 2);
                    if(ctrlFlg) ShowScrollBar(currentListBox.Handle, 1, false);
                }
                else
                {
                    if(currentListBox.Font.Size >= 10) currentListBox.Font = new Font("メイリオ", currentListBox.Font.Size - 2);
                    if(ctrlFlg) ShowScrollBar(currentListBox.Handle, 1, false);
                }
                appSettings.listbox_font_size = currentListBox.Font.Size;
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
        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            Keys input = e.KeyCode;

            switch (input)
            {
                case Keys.ControlKey:

                    ctrlFlg = true;
                    
                    ShowScrollBar(currentListBox.Handle, 1, false);

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
        private void listBox_MouseLeave(object sender, EventArgs e)
        {
            label1.Text = "";
        }

        private void versionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Reflection.Assembly     assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName asmName  = assembly.GetName();
            System.Version                 version  = asmName.Version;
 
            string ver = string.Format("Ver {0}.{1}{2}", version.Major, version.Minor, version.Build);

            MessageBox.Show(ver);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
