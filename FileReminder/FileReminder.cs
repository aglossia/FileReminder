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
            public bool startup_windowFlg { set; get; }
            public int form_width { set; get; }
            public int form_height { set; get; }
            public int form_start_x { set; get; }
            public int form_start_y { set; get; }

            public save_config()
            {
                listbox_font_size = 12;
                listbox_font_type = "メイリオ";
                highlightFlg = false;
                startup_windowFlg = false;
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

            public int currentTabIndex
            {
                get
                {
                    return currentInfo[currentkey]._currentTabIndex;
                }
                set
                {
                    currentInfo[currentkey]._currentTabIndex = value;
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

            public string[] currentFileName
            {
                get
                {
                    return currentInfo[currentkey]._currentFileName;
                }
            }

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

        // 保存種別
        public enum SaveType
        {
            DEFAULT,
            SAVE_AS,
            OVERWRITE,
        }

        /****************** クラス ******************/



        /****************** グローバル変数 ******************/
        /// <summary>フォームタイトルにつける開いているfremファイル名</summary>
        //string readFileName = "新規";
        const string EXE_FORM_NAME = "File Reminder";
        const string EXE_FILE_NAME = "FileReminder.exe";
        const string INIT_SYMBOL = "init";
        const string INIT_FILENAME = "新規";
        const string UPDATE = " (更新)";
        const string DEFAULT_SYMBOL = "default";
        const string FREM_CONFIG = "frem.config";
        const string FREM_FILTER = "FREMファイル(*.frem)|*.frem|すべてのファイル(*.*)|*.*";
        const string MPCPL_FILTER = "MPCPLファイル(*.mpcpl)|*.mpcpl|すべてのファイル(*.*)|*.*";
        static string EXE_FOLDER = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string EXE_PATH = string.Format("{0}\\{1}", EXE_FOLDER, EXE_FILE_NAME);
        string CONFIG_PATH = string.Format("{0}\\{1}", EXE_FOLDER, FREM_CONFIG);

        List<string> movie_ext = new List<string> { ".mp4", ".wmv", ".avi", ".mov", ".mpeg", ".mkv", ".flv" };

        // Ctrlキーフラグ
        bool ctrlFlg = false;
        // リストボックスマウスオーバー中のインデックス
        int mouseoverIndex_listbox = -1;
        // タブのマウスオーバー中のインデックス
        int mouseoverIndex_tab = -1;
        // タブ数
        int tabCnt = 0;
        // 新規ファイル数
        int initCnt = 0;

        public save_config saveConfig = new save_config();

        settings appSettings = new settings();
        
        reminderTab reminderTabs;

        static utilitys util = new utilitys();

        /****************** グローバル変数 ******************/



        /****************** 関数 ******************/

        /*
         * タブ追加メイン処理
         */
        private void addTabMain(string key, fileInfomation fI)
        {
            // 新規タブが複数追加されたときナンバリングする
            if (appSettings.currentInfo.ContainsKey(key))
            {
                initCnt++;
                key = string.Format("{0}({1})", key, initCnt);
            }

            // 読み込みファイル一覧に追加
            appSettings.readfile_fullpath.Add(key);
            // タブ表示名はファイル名だが、フルパスのためファイル名にする    
            addTabControl(Path.GetFileName(key));
            // ファイル情報を生成
            addTabInfo(key, fI);
            // 選択タブを追加したタブにする
            this.tabControl1.SelectedIndex = tabCnt - 1;

            // リストボックスにアイテムを追加
            foreach (string item in fI.fPs.filepathList)
            {
                appSettings.currentListBox.Items.Add(Path.GetFileName(item));
            }
        }

        /*
         * タブ情報追加処理
         */
        private void addTabInfo(string fremFilePath, fileInfomation fI)
        {
            // カレントキーを読み込みファイルパスに
            appSettings.currentkey = fremFilePath;
            // カレント情報インスタンス生成
            currentInfomation curInfo = new currentInfomation();
            // fremファイル名設定
            curInfo._currentFileName[(int)FileType.FREM] = fremFilePath;
            // タブインデックス設定
            curInfo._currentTabIndex = tabCnt - 1;
            // addTabControlで生成したリストボックスを設定
            curInfo._currentListBox = reminderTabs.listboxs[tabCnt - 1];

            // ※tabCntは次のタブのインデックスを指しているためデクリメントする

            // ファイル情報設定
            curInfo._fileInfo = fI;

            // 全部の設定が完了、辞書に追加
            appSettings.currentInfo.Add(fremFilePath, curInfo);
        }

        /*
         * タブコントロール追加処理
         */
        private void addTabControl(string tabName)
        {
            reminderTabs.tabs.Add(new TabPage(tabName));
            reminderTabs.listboxs.Add(new ListBox());
            reminderTabs.tabInit(tabCnt);

            tabCnt++;
        }

        /*
         * タブ削除メイン処理
         */
        private void removeTabMain(int tabIndex, SaveType savetype = SaveType.DEFAULT)
        {
            // 名前を付けて保存の時は、終了保存確認をしない
            if (savetype != SaveType.SAVE_AS)
            {
                // 終了時保存確認
                if(closingFileSave(new List<string>{appSettings.readfile_fullpath[tabIndex]})) return;
            }

            int currentIndex = this.tabControl1.SelectedIndex;

            // 読み込みファイル情報辞書を削除
            appSettings.currentInfo.Remove(appSettings.readfile_fullpath[tabIndex]);
            // 読み込みファイル一覧を削除
            appSettings.readfile_fullpath.RemoveAt(tabIndex);
            // 実物のタブを削除
            this.tabControl1.TabPages.RemoveAt(tabIndex);
            // 対象のリストボックスを削除
            reminderTabs.listboxs.RemoveAt(tabIndex);
            // 対象のタブを削除
            reminderTabs.tabs.RemoveAt(tabIndex);
            // タブ数を現在の個数に
            tabCnt = this.tabControl1.TabPages.Count;

            if (tabCnt == 0)
            {
                // 削除後、タブが一つもないときは終了する
                Application.Exit();
            }
            else
            {
                // 削除するタブが自分より左側にあるときインデックスを一つさげる
                if (tabIndex < currentIndex) appSettings.currentTabIndex--;
            }
        }

        /*
         * 終了時ファイル保存処理
         * 
         * return: true キャンセルがあったexe継続、false キャンセルなし処理終了
         */
        private bool closingFileSave(List<string> checkList)
        {
            bool ret = false;

            foreach (string key in checkList)
            {
                // 保存されておらず更新があるときに保存確認ダイアログを出す
                if (!appSettings.currentInfo[key]._fileInfo.already_saveflg && appSettings.currentInfo[key]._fileInfo.updateFlg)
                {
                    var msg = new ButtonTextCustomizableMessageBox();
                    msg.ButtonText.Yes = "保存";
                    msg.ButtonText.No = "保存しない";
                    msg.ButtonText.Cancel = "キャンセル";
                    DialogResult result = 
                        msg.Show(key + "が保存されていません。", "File Reminder", 
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                    switch (result)
                    {
                        case DialogResult.Cancel:   // キャンセル
                            // キャンセルは終了を取り消す
                            ret = true;

                            break;

                        case DialogResult.Yes:  // 保存する
                            // 新規？
                            if (appSettings.currentInfo[key]._fileInfo.initialFlg)
                            {
                                // 新規なので名前をつけて保存する
                                if (saveAs(FileType.FREM, FREM_FILTER) == DialogResult.Cancel)
                                {
                                    // ダイアログでキャンセルで保存しない場合は終了しない
                                    ret = true;
                                }
                            }
                            else
                            {
                                // 新規ではないので上書き保存
                                fileSaveOut(FileType.FREM, SaveType.OVERWRITE);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            return ret;
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
         * fremファイル読み込み
         * 
         * return: true 読み込み成功、false 読み込みエラー
         */
        public bool fremRead(string fremFile)
        {
            // 既に開いているファイル
            if(appSettings.currentInfo.ContainsKey(fremFile)) return false;

            bool currentTab_initialFlg = appSettings.currentFileInfo.initialFlg;
            bool currentTab_alreadyFlg = appSettings.currentFileInfo.already_saveflg;

            filepaths fP = new filepaths();
            // fremファイル読み込み
            util.xmlRead(ref fP, fremFile, "1");
            // 読み込んだファイルをタブに追加
            addTabMain(fremFile, new fileInfomation{fPs = fP, already_saveflg = true, initialFlg = false});

            // 追加前のタブが一個で新規でかつ保存済みの場合、新規タブを削除する
            if (currentTab_initialFlg && currentTab_alreadyFlg && this.tabControl1.TabPages.Count == 2)
            {
                removeTabMain(0);
            }

            return true;
        }

        /*
         * フォームタイトル更新
         */
        private void formTitleUpdate()
        {
            if (appSettings.currentFileInfo.already_saveflg)
            {
                // 保存済みであればタイトルに更新をいれる
                this.Text = string.Format("{0}{1} - {2}", appSettings.currentFrem, UPDATE , EXE_FORM_NAME);
                // タブ名にも更新を追加
                reminderTabs.tabs[appSettings.currentTabIndex].Text += UPDATE;

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
                // 保存済みは更新をいれない
                this.Text = string.Format("{0} - {1}", appSettings.currentFrem, EXE_FORM_NAME);
            }
            else
            {
                // 未保存は更新をいれる
                this.Text = string.Format("{0}{1} - {2}", appSettings.currentFrem, UPDATE , EXE_FORM_NAME);
            }
        }

        /*
         * ファイル保存
         */
        private void fileSaveOut(FileType filetype, SaveType savetype = SaveType.DEFAULT)
        {
            // 保存ファイルフルパス
            string fileNameFullPath = appSettings.currentFileName[(int)filetype];

            filepaths fP = appSettings.currentFilePathClass;

            // 保存ファイル切り替え
            switch (filetype)
            {
                case FileType.FREM:     // fremファイル

                    util.xmlWrite(fP, fileNameFullPath, "書込みエラー");

                    switch (savetype)
                    {
                        case SaveType.SAVE_AS:  // 名前を付けて保存のときは新規にタブを追加する

                            // 保存した情報をタブに新規追加
                            addTabMain(fileNameFullPath, new fileInfomation{fPs = fP, already_saveflg = true, initialFlg = false});
                            // タブを更新したため現在のタブを削除
                            removeTabMain(appSettings.currentTabIndex - 1, savetype);

                            break;

                        case SaveType.OVERWRITE:    // 上書き保存のときは更新だけする

                            appSettings.currentFileInfo.already_saveflg = true;
                            appSettings.currentFileInfo.initialFlg = true;

                            reminderTabs.tabs[appSettings.currentTabIndex].Text = Path.GetFileName(appSettings.currentFrem);

                            break;
                    }

                    // フォームタイトル更新
                    formTitleRefresh();

                    overwriteToolStripMenuItem.Enabled = false;

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
                //appSettings.readfile_fullpath.Add(sfd.FileName);

                // 現在のファイル名を更新（ファイル種別毎）
                appSettings.currentFileName[(int)filetype] = sfd.FileName;

                fileSaveOut(filetype, SaveType.SAVE_AS);
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
            this.Width = saveConfig.form_width;
            this.Height = saveConfig.form_height;
            this.Location = new Point(saveConfig.form_start_x, saveConfig.form_start_y);

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
            //saveasToolStripMenuItem.Enabled = false;

            label1.Text = "";

            string[] cmds = System.Environment.GetCommandLineArgs();

#if DEBUG
            // デバッグ用
            if(false) cmds = new string[]{""};
#endif

            // frem.config があれば読み込む
            if (File.Exists(CONFIG_PATH))
            {
                util.xmlRead(ref saveConfig, CONFIG_PATH, "0");
            }

            // コマンドライン引数起動で新規タブは上書きされるためここで追加
            addTabMain(INIT_FILENAME, new fileInfomation());

            if (cmds.Length == 1)   // 単独起動
            {
                //addTabMain(INIT_FILENAME, new fileInfomation());                
            }
            else if (cmds.Length == 2)  // コマンドライン引数あり
            {
                if(!fremRead(cmds[1])) MessageBox.Show("読み込みエラー");
            }

            // イニシャル処理
            settingInitialize();
        }

        /* 
         * ドラッグエンター：リストボックス
         */
        private void listBox_DragEnter(object sender, DragEventArgs e)
        {
            // コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;            
            }
        }

        /* 
         * ドラッグドロップ：リストボックス
         */
        private void listBox_DragDrop(object sender, DragEventArgs e)
        {
            // コントロール内にドロップされたとき実行される
            // ドロップされたすべてのファイル名を取得する
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            // 拡張子だけを取り出す
            IEnumerable<string> exts = fileNames.Select(i => Path.GetExtension(i));

            // 取り出した拡張子をDistinctで種類わけする
            // その種類が２以上かつ
            // frem拡張子を含むときは混在あり
            if (exts.Distinct().Count() != 1 && exts.Contains(".frem"))
            {
                MessageBox.Show("fremファイルと他のファイルが混在しています。");
                return;
            }
            foreach (string fileName in fileNames)
            {
                string ext = Path.GetExtension(fileName);

                if (ext == ".frem")
                {
                    if (!fremRead(fileName))
                    {
                        //MessageBox.Show(
                        //    string.Format("\"{0}\"は既に開かれているファイルです。", fileName));
                        //backgroundWorker1.RunWorkerAsync(fileName);
                        continue;
                    }
                }
                else
                {
                    if (ext == "")
                    {
                        if(!Directory.Exists(fileName)) continue;
                    }
                    // ListBoxに追加する
                    appSettings.currentListBox.Items.Add(Path.GetFileName(fileName));
                    // 参照パスリストに追加する
                    appSettings.currentFileList.AddRange(fileNames);
                    // フォームタイトル更新
                    formTitleUpdate();
                    appSettings.currentFileInfo.updateFlg = true;
                }
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

                    // スクロールバー復帰
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
            // ファイル保存処理
            e.Cancel = closingFileSave(appSettings.readfile_fullpath);

            if (e.Cancel)   // ダイアログでキャンセルされたとき処理を継続する
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
            fileSaveOut(FileType.FREM, SaveType.OVERWRITE);
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
            // リストボックスに何もないときはクリアしない
            if (appSettings.currentFileList.Count != 0)
            {
                appSettings.currentListBox.Items.Clear();
                appSettings.currentFileList.Clear();

                formTitleUpdate();

                appSettings.currentFileInfo.updateFlg = true;
            }
        }

        /*
         * リストボックスマウスムーブ
         */
        private void listBox_MouseMove(object sender, MouseEventArgs e)
        {
            mouseoverIndex_listbox = appSettings.currentListBox.IndexFromPoint(e.Location);

            if (mouseoverIndex_listbox != -1)
            {
                label1.Text = appSettings.currentFileList[mouseoverIndex_listbox];

                if (saveConfig.highlightFlg)
                {
                    appSettings.currentListBox.ClearSelected();
                    appSettings.currentListBox.SetSelected(mouseoverIndex_listbox, true);
                }
            }
        }

        /*
         * リストボックス右クリック：フォルダを開く
         */
        private void folderOpneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(mouseoverIndex_listbox != -1) openFolder(mouseoverIndex_listbox);
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
                    
                    // スクロールバーを消す
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
        //private void highligntToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    ToolStripMenuItem item = (ToolStripMenuItem)sender;

        //    item.Checked = !item.Checked;
        //    saveConfig.highlightFlg = item.Checked;
        //}

        /*
         * マウスリーブ
         */
        private void listBox_MouseLeave(object sender, EventArgs e)
        {
            label1.Text = "";
        }

        /*
         * ヘルプ：バージョン表示
         */
        private void versionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Reflection.Assembly     assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName asmName  = assembly.GetName();
            System.Version                 version  = asmName.Version;
 
            string ver = string.Format("Ver {0}.{1}{2}", version.Major, version.Minor, version.Build);

            MessageBox.Show(ver);
        }

        /*
         * タブ変更イベント
         */
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex != -1)
            {
                if (ctrlFlg)
                {
                    ShowScrollBar(appSettings.currentListBox.Handle, 1, true);
                    ctrlFlg = false;
                }

                appSettings.currentkey = appSettings.readfile_fullpath[this.tabControl1.SelectedIndex];

                overwriteToolStripMenuItem.Enabled = !appSettings.currentFileInfo.already_saveflg;

                formTitleRefresh();
            }
        }
        /*
         * ヘルプ：debug
         */
        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string temp = "";

            foreach (var i in appSettings.readfile_fullpath)
            {
                temp += i + Environment.NewLine;
            }

            MessageBox.Show(string.Format("読み込みファイル：{0}選択ファイル：{1}currentTabIndex：{2}", 
                Environment.NewLine + temp + Environment.NewLine,
                Environment.NewLine + appSettings.currentkey + Environment.NewLine + Environment.NewLine,
                appSettings.currentTabIndex));
        }

        /*
         * タブ右クリック：削除
         */
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeTabMain(mouseoverIndex_tab);
        }

        /*
         * マウスムーブ：タブ
         */
        private void tabControl1_MouseMove(object sender, MouseEventArgs e)
        {
            TabControl tab = (TabControl)sender; 

            Rectangle NowRect	=	System.Drawing.Rectangle.Empty;

            for(int i = 0; i < tab.TabCount; i++) 
            { 
                Rectangle rect = tab.GetTabRect(i); 
                
                // X座標のタブの境界が微妙なので-1して調整
                if( (rect.Left - 1 <= e.X) && (e.X <= rect.Right - 1) && 
                (rect.Top <= e.Y) && (e.Y <= rect.Bottom)	) 
                { 
                    mouseoverIndex_tab=i; 
                    NowRect=rect; 
                    break; 
                } 
            } 

            //label1.Text = mouseoverIndex_tab.ToString();
        }

        /*
         * メニュー：新規作成
         */
        private void newfileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            addTabMain(INIT_FILENAME, new fileInfomation());
        }

        /*
         * メニュー：開く
         */
        private void openfileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = appSettings.getFileDirectory(FileType.FREM);

            ofd.Filter = FREM_FILTER;

            ofd.FilterIndex = 1;

            ofd.Title = "開くファイルを選択してください";

            ofd.RestoreDirectory = true;

            // ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if(!fremRead(ofd.FileName)) MessageBox.Show("既に開かれているファイルです。");
            }                
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //MessageBox.Show(string.Format("\"{0}\"は既に開かれているファイルです。", e.Argument));
        }

        int keybit = 0;
        bool oneShot = true;

        private void commonSettingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SettingForm.showSettingForm( ref saveConfig );
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            //switch (e.KeyData)
            //{
            //    case Keys.Control | Keys.Tab:

            //        this.tabControl1.SelectedIndex = appSettings.currentTabIndex;

            //        break;

            //    case Keys.Control | Keys.Shift | Keys.Tab:

            //        this.tabControl1.SelectedIndex--;

            //        break;

            //    default:
            //        break;
            //}
        }

        private void tabControl1_KeyUp(object sender, KeyEventArgs e)
        {
            //switch (e.KeyData)
            //{
            //    case Keys.Control | Keys.Tab:

            //        label1.Text = "";

            //        break;

            //    default:
            //        break;
            //}
        }
    }
}
