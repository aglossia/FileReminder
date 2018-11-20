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

        public class fileInfomation
        {
            public filepaths fPs { get; set; }
            public bool already_saveflg { get; set; }
            public bool initialFlg { get; set; }
            public bool updateFlg { get; set; }

            public fileInfomation()
            {
                fPs = new filepaths();
                already_saveflg =true;
                initialFlg = true;
                updateFlg = false;
            }
        }

        // config設定クラス
        public class save_config
        {
            public string listbox_font_type { set; get; }
            public float listbox_font_size { set; get; }
            public bool highlightFlg { set; get; }
            public int form_width { set; get; }
            public int form_height { set; get; }
            public int form_start_x { set; get; }
            public int form_start_y { set; get; }

            public save_config()
            {
                listbox_font_size = 12;
                listbox_font_type = "メイリオ";
                highlightFlg = false;
                form_width = 452;
                form_height = 431;
                form_start_x = 200;
                form_start_y = 200;
            }
        }

        // カレントクラス
        public class currentInfomation
        {
            public string[] _currentFileName { get; set; }

            public int _currentTabIndex { get; set; }

            public ListBox _currentListBox { get; set; }

            public fileInfomation _fileInfo { get; set; }

            public currentInfomation()
            {
                _currentFileName = new string[2].Select(i => i = INIT_FILENAME).ToArray();
                _currentTabIndex = 0;
                _currentListBox = new ListBox();
                _fileInfo = new fileInfomation();
            }
        }

        // 設定クラス
        public class settings
        {
            public List<string> readfile_fullpath { get; set; }

            public Dictionary<string, currentInfomation> currentInfo { get; set; }

            public string currentkey { get; set; }

            //↓主にゲッタ

            public string currentFrem
            {
                get
                {
                    return currentInfo[currentkey]._currentFileName[(int)FileType.FREM];
                }
                set
                {
                    currentInfo[currentkey]._currentFileName[(int)FileType.FREM] = value;
                }
            }

            public ListBox currentListBox
            {
                get
                {
                    return currentInfo[currentkey]._currentListBox;
                }
                set
                {
                    currentInfo[currentkey]._currentListBox = value;
                }
            }

            public fileInfomation currentFileInfo
            {
                get
                {
                    return currentInfo[currentkey]._fileInfo;
                }
            }

            //-------

            public filepaths currentFilePathClass
            {
                get
                {
                    return currentInfo[currentkey]._fileInfo.fPs;
                }
            }

            public List<string> currentFileList
            {
                get
                {
                    return currentInfo[currentkey]._fileInfo.fPs.filepathList;
                }
            }

            //public currentInfomation currentInfoMain
            //{
            //    get
            //    {
            //        return currentInfo[currentkey];
            //    }                               
            //}

            public settings()
            {
                readfile_fullpath = new List<string>();
                currentInfo = new Dictionary<string,currentInfomation>();
            }

            public string getFileDirectory(FileType filetype)
            {
                return (this.currentFrem == INIT_FILENAME) ? EXE_FOLDER : Path.GetDirectoryName(this.currentFrem);
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

        public struct tagElement_struct
        {
            public string titlename;
        }

        /****************** クラス ******************/



        /****************** グローバル変数 ******************/
        /// <summary>フォームタイトルにつける開いているfremファイル名</summary>
        //string readFileName = "新規";
        const string EXE_FORM_NAME = "File Reminder";
        const string EXE_FILE_NAME = "FileReminder.exe";
        const string INIT_SYMBOL = "init";
        const string INIT_FILENAME = "新規";
        const string UPDATE = "(更新)";
        const string DEFAULT_SYMBOL = "default";
        const string FREM_CONFIG = "frem.config";
        const string FREM_FILTER = "FREMファイル(*.frem)|*.frem|すべてのファイル(*.*)|*.*";
        const string MPCPL_FILTER = "MPCPLファイル(*.mpcpl)|*.mpcpl|すべてのファイル(*.*)|*.*";
        static string EXE_FOLDER = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string EXE_PATH = string.Format("{0}\\{1}", EXE_FOLDER, EXE_FILE_NAME);
        string CONFIG_PATH = string.Format("{0}\\{1}", EXE_FOLDER, FREM_CONFIG);

        List<string> movie_ext = new List<string> { ".mp4", ".wmv", ".avi", ".mov", ".mpeg", ".mkv", ".flv" };

        // 保存済みフラグ
        //bool alreadySaveFlg = false;
        // 新規フラグ
        //bool initialFlg = true;
        // 更新フラグ
        //bool updateFlg = false;
        // Ctrlキーフラグ
        bool ctrlFlg = false;
        // リストボックスマウスオーバー中のインデックス
        int mouseoverIndex = -1;
        // タブ数
        int tabCnt = 0;

        //ListBox currentListBox = null;

        //string[] currentFileName = new string[2].Select(i => i = DEFAULT_SYMBOL).ToArray();

        save_config saveConfig = new save_config();

        settings appSettings = new settings();
        
        reminderTab reminderTabs;

        static utilitys util = new utilitys();

        /****************** グローバル変数 ******************/



        /****************** 関数 ******************/

        /*
         * タブ情報追加処理
         */
        private void addTabInfo(string fremFilePath, fileInfomation fI)
        {
            appSettings.currentkey = fremFilePath;

            currentInfomation curInfo = new currentInfomation();

            curInfo._currentFileName[(int)FileType.FREM] = fremFilePath;

            curInfo._currentTabIndex = this.tabControl1.SelectedIndex;
            // タブ作成済みで次のタブインデックスをさす準備をしているためデクリメントする
            curInfo._currentListBox = reminderTabs.listboxs[tabCnt - 1];

            curInfo._fileInfo = fI;

            appSettings.currentInfo.Add(fremFilePath, curInfo);
        }

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

            //appSettings.filename_path.Add(fremFile, filePath);
                
            // 現在のタブで開いているファイルフルパスを変更
            //appSettings.currentInfoMain.currentFileName[(int)FileType.FREM] = fremFile;

            //appSettings.currentFrem = fremFile;

            // 読み込みファイルに追加
            appSettings.readfile_fullpath.Add(fremFile);

            // fremファイルごとにファイルリストを設定
            //appSettings.filelist.Add(fremFile, new fileInfo{fPs = fP, already_saveflg = true, initialFlg = false});


            /// タブ情報追加

            addTabInfo(fremFile, new fileInfomation{fPs = fP, already_saveflg = true, initialFlg = false});

            ///

            //appSettings.currentTabIndex = this.tabControl1.SelectedIndex;

            //appSettings.currentFileList = fP.filepathList;

            //appSettings.filelist_key = fremFile;

            formTitleRefresh();

            // リストボックスにアイテムを追加
            foreach (string item in fP.filepathList)
            {
                appSettings.currentListBox.Items.Add(Path.GetFileName(item));
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
            //appSettings.currentListBox = reminderTabs.listboxs[tabCnt];

            //tabControl1.SelectedIndex = tabCnt;

            tabCnt++;
        }

        /*
         * ファイルorフォルダを開く
         */
        private void fileExecute()
        {
            if (appSettings.currentListBox.SelectedIndex != -1)
            {
                string a = appSettings.currentFileList[appSettings.currentListBox.SelectedIndex];

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
            if (appSettings.currentFileInfo.already_saveflg)
            {
                // 保存済みであればタイトルに更新をいれる
                //this.Text += " (更新)";
                this.Text = string.Format("{0} {1} - {2}", appSettings.currentFrem,UPDATE , EXE_FORM_NAME);
                // 更新がかかったので保存済みフラグをおとす
                appSettings.currentFileInfo.already_saveflg = false;
                overwriteToolStripMenuItem.Enabled = true;
            }
        }

        /*
         * フォームタイトル読み込みファイル名変更
         */
        private void formTitleRefresh()
        {
            if (appSettings.currentFileInfo.already_saveflg)
            {
                this.Text = string.Format("{0} - {1}", appSettings.currentFrem, EXE_FORM_NAME);
            }
            else
            {
                this.Text = string.Format("{0} {1} - {2}", appSettings.currentFrem,UPDATE , EXE_FORM_NAME);
            }
        }

        /*
         * カレント変更処理
         */
        //private void currentChange(int index)
        //{
        //    appSettings.currentTabIndex = index;
            
        //    appSettings.currentFrem = appSettings.readfile_fullpath[index];

        //    appSettings.currentListBox = reminderTabs.listboxs[index];

        //    appSettings.filelist_key = appSettings.readfile_fullpath[index];
        //}

        /*
         * ファイル保存
         */
        private void fileSaveOut(FileType filetype)
        {
            //string fileName = appSettings.currentFileName[(int)filetype];                       // 保存ファイル名
            string fileNameFullPath = appSettings.currentFrem;                    // 保存ファイルフルパス
            //filepaths fP = appSettings.filelist[appSettings.filelist_key].fPs;    // 保存用クラス
            filepaths fP = appSettings.currentFilePathClass;

            // 保存ファイル切り替え
            switch (filetype)
            {
                case FileType.FREM:     // fremファイル

                    util.xmlWrite(fP, fileNameFullPath, "書込みエラー");

                    overwriteToolStripMenuItem.Enabled = true;
                    // 保存済みとし、新規をはずす
                    appSettings.currentFileInfo.already_saveflg = true;
                    appSettings.currentFileInfo.initialFlg = false;

                    formTitleRefresh();

                    break;

                case FileType.MPCPL:    // mpcplファイル

                    int lineCnt = 1;

                    // ファイルを上書きし、書き込む
                    StreamWriter sw = new StreamWriter(fileNameFullPath, false, Encoding.GetEncoding("utf-8"));

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
            //sfd.InitialDirectory = appSettings.filename_path[currentFileName[(int)filetype]];
            sfd.InitialDirectory = appSettings.getFileDirectory(filetype);

            // [ファイルの種類]に表示される選択肢を指定する
            // 指定しない（空の文字列）の時は、現在のディレクトリが表示される

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
                string fileName = "";
                string filePath = "";

                fileName = Path.GetFileName(sfd.FileName);
                filePath = Path.GetDirectoryName(sfd.FileName);

                appSettings.readfile_fullpath.Add(sfd.FileName);

                // 現在のファイル名を更新（ファイル種別毎）
                //appSettings.currentFileName[(int)FileType.FREM] = sfd.FileName;
                appSettings.currentFrem = sfd.FileName;
                
                // 最新保存ファイル名を変更（ファイル種別毎）
                //currentFileName[(int)filetype] = sfd.FileName;

                fileSaveOut(filetype);
            }

            return dialog_ret;
        }

        /*
         * エクスプローラーでフォルダを開く
         */
        private void openFolder(int index)
        {
            Process.Start("EXPLORER.EXE", "/select," + appSettings.currentFileList[index]);
        }

        /*
         * 設定イニシャル処理
         */
        private void settingInitialize()
        {
            appSettings.currentListBox.Font = new Font(saveConfig.listbox_font_type, saveConfig.listbox_font_size);
            highligntToolStripMenuItem.Checked = saveConfig.highlightFlg;
            this.Width = saveConfig.form_width;
            this.Height = saveConfig.form_height;
            this.Location = new Point(saveConfig.form_start_x, saveConfig.form_start_y);

            // ファイル読み込みがあればファイル名をタイトルに、なければ新規をタイトルにつける
            //this.Text += string.Format(" [{0}]", appSettings.currentFileName[(int)FileType.FREM]);
            formTitleRefresh();
        }

        /*
         * 設定ファイナル処理
         */
        private void settingFinalize()
        {
            saveConfig.form_width = this.Width;
            saveConfig.form_height = this.Height;
            saveConfig.form_start_x = this.Location.X;
            saveConfig.form_start_y = this.Location.Y;
        }

        /****************** 関数 ******************/



        public FileReminder()
        {
            InitializeComponent();

            reminderTabs = new reminderTab(this);

            overwriteToolStripMenuItem.Enabled = false;

            label1.Text = "";

            string[] cmds = System.Environment.GetCommandLineArgs();

            // frem.config があれば読み込む
            if (File.Exists(CONFIG_PATH))
            {
                util.xmlRead(ref saveConfig, CONFIG_PATH, "0");

                //appSettings.readfile_fullpath = new List<string>();
                //appSettings.filelist = new Dictionary<string, filepaths>();
                //appSettings.currentFileName = new string[2].Select(i => i = INIT_FILENAME).ToArray();
                //appSettings.currentFileList = new List<string>();
                //appSettings.currentListBox = new ListBox();
            }

            if (cmds.Length == 1)
            {
                // 新規で起動


                appSettings.currentkey = INIT_FILENAME;
                appSettings.readfile_fullpath.Add(INIT_FILENAME);
                
                addTab(INIT_FILENAME);
                addTabInfo(INIT_FILENAME, new fileInfomation());
                
                //addTab(INIT_FILENAME);
                // 新規タブが追加されるのでインデックス管理するために実行ファイルパスを追加しておく
                //appSettings.readfile_fullpath.Add(INIT_FILENAME);
                //appSettings.currentkey = INIT_FILENAME;
                //addTabInfo(INIT_FILENAME, new fileInfomation());
            }
            else if (cmds.Length == 2)
            {
                // fremファイルから起動

                // fremファイルパス
                string fremFile = cmds[1];
                // fremファイル名
                string fileName = Path.GetFileName(fremFile);

                addTab(fileName);

                filepaths fP = new filepaths();

                fremRead(ref fP, fremFile);

                // "新規"をファイル名に変更
                //appSettings.filename_path[INIT_SYMBOL] = fileName;

                //appSettings.currentFileName[(int)FileType.FREM] = fremFile;

                // ファイル読み込みだからセーブ済みとし、新規ではない
                overwriteToolStripMenuItem.Enabled = true;
                appSettings.currentFileInfo.already_saveflg = true;
                appSettings.currentFileInfo.initialFlg = false;
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
                // 既に読み込んでいたらスキップ
                if (appSettings.readfile_fullpath.Contains(fileNames[0])) return;

                filepaths fP = new filepaths();

                //fremRead(ref fP, fileNames[0]);
                addTab(Path.GetFileName(fileNames[0]));
                fremRead(ref fP, fileNames[0]);

                tabControl1.SelectedIndex = tabCnt - 1;
            }
            else
            {
                // ListBoxに追加する
                appSettings.currentListBox.Items.AddRange(fileNames.Select(i => Path.GetFileName(i)).ToArray());
                // 参照パスリストに追加する
                //appFilePaths.filepathList.AddRange(fileNames);
                appSettings.currentFileList.AddRange(fileNames);
                // フォームタイトル更新
                formTitleUpdate();
                appSettings.currentFileInfo.updateFlg = true;
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

            int sel = appSettings.currentListBox.SelectedIndex;

            ListBox.SelectedIndexCollection Indices = appSettings.currentListBox.SelectedIndices;

            List<string> itemList = new List<string>();

            if (sel == -1) return;

            switch (inputKey)
            {
                case Keys.Delete:

                    List<string> removeList = new List<string>();

                    // 削除リストを作成（パス参照リスト用）
                    foreach (int i in appSettings.currentListBox.SelectedIndices)
                    {
                        removeList.Add(appSettings.currentFileList[i]);
                    }
                    // 削除リストに該当するものを削除（パス参照リスト用）
                    foreach (string s in removeList)
                    {
                        appSettings.currentFileList.Remove(s);
                    }
                    // 削除リストを作成（リストボックス用）
                    for (int i = 0; i < appSettings.currentListBox.SelectedItems.Count; i++)
                    {
                        itemList.Add(appSettings.currentListBox.SelectedItems[i].ToString());
                    }
                    // 削除リストに該当するものを削除（リストボックス用）
                    foreach (var b in itemList)
                    {
                        appSettings.currentListBox.Items.Remove(b.ToString());
                    }

                    formTitleUpdate();

                    appSettings.currentFileInfo.updateFlg = true;
                    
                    break;

                case Keys.Enter:

                    fileExecute();

                    break;

                case Keys.ControlKey:

                    ctrlFlg = false;
                    ShowScrollBar(appSettings.currentListBox.Handle, 1, true);

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
            foreach (string key in appSettings.readfile_fullpath)
            {
                appSettings.currentkey = key;
                // 保存されておらず更新があるときに保存確認ダイアログを出す
                if (!appSettings.currentFileInfo.already_saveflg && appSettings.currentFileInfo.updateFlg)
                {
                    var msg = new ButtonTextCustomizableMessageBox();
                    msg.ButtonText.Yes = "保存";
                    msg.ButtonText.No = "保存しない";
                    msg.ButtonText.Cancel = "キャンセル";
                    DialogResult result = 
                        msg.Show(appSettings.currentFrem + "が保存されていません。", "File Reminder", 
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                    switch (result)
                    {
                        case DialogResult.Cancel:   // キャンセル
                            // キャンセルは終了を取り消す
                            e.Cancel = true;

                            break;

                        case DialogResult.Yes:  // 保存する
                            // 新規？
                            if (appSettings.currentFileInfo.initialFlg)
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
            }

            if (e.Cancel)
            {
                appSettings.currentkey = appSettings.readfile_fullpath[this.tabControl1.SelectedIndex];
            }
            else
            {
                // ファイナル処理
                settingFinalize();
                // config設定を保存する
                util.xmlWrite(saveConfig, CONFIG_PATH, "書込みエラー");
            }
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
            appSettings.currentListBox.Items.Clear();
            appSettings.currentFileList.Clear();

            formTitleUpdate();

            appSettings.currentFileInfo.updateFlg = true;
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
            mouseoverIndex = appSettings.currentListBox.IndexFromPoint(Location);

            if (mouseoverIndex != -1)
            {
                label1.Text = appSettings.currentFileList[mouseoverIndex];

                if (saveConfig.highlightFlg)
                {
                    appSettings.currentListBox.ClearSelected();
                    appSettings.currentListBox.SetSelected(mouseoverIndex, true);
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
                    if(appSettings.currentListBox.Font.Size <= 20) appSettings.currentListBox.Font = new Font("メイリオ", appSettings.currentListBox.Font.Size + 2);
                    if(ctrlFlg) ShowScrollBar(appSettings.currentListBox.Handle, 1, false);
                }
                else
                {
                    if(appSettings.currentListBox.Font.Size >= 10) appSettings.currentListBox.Font = new Font("メイリオ", appSettings.currentListBox.Font.Size - 2);
                    if(ctrlFlg) ShowScrollBar(appSettings.currentListBox.Handle, 1, false);
                }
                saveConfig.listbox_font_size = appSettings.currentListBox.Font.Size;
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
                    
                    ShowScrollBar(appSettings.currentListBox.Handle, 1, false);

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
            saveConfig.highlightFlg = item.Checked;
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
            appSettings.currentkey = appSettings.readfile_fullpath[this.tabControl1.SelectedIndex];

            overwriteToolStripMenuItem.Enabled = !appSettings.currentFileInfo.already_saveflg;

            formTitleRefresh();
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string temp = "";

            foreach (var i in appSettings.readfile_fullpath)
            {
                temp += i + Environment.NewLine;
            }

            MessageBox.Show(temp);
            MessageBox.Show(appSettings.currentkey);
        }
    }
}
