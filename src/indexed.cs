/**
 * Advanced T's Manipulater Module
 *
 * Command line options:
 *    -s  snapshot
 *    -d  date-indexed
 *    -b  backup/restore
 *    -r  restore
 *    -h  hashfile
 *    -o  checkout
 *    -i  checkin
 *    -t  tagging
 *    -c  add comment
 *    -f  folder open
 *    -p  program
 *    -z  archive
 *    -v  verbose
 *
 * 日付フォルダ管理
 *   1. フォルダを指定した場合
 *      フォルダの先頭に8桁の日付をつける。
 *      日付はフォルダ内にあるファイルの最新書き込み日付とする。
 *      フォルダ内にファイルがない場合は、日付は現在の日付を使用する。
 *      同じ日付のフォルダが、現在のパス、「_」で始まるパス、「@」で始まるパスに
 *      含まれる場合は、最新のリビジョン番号を追加する。
 *
 *   2. ファイルを指定した場合
 *      ファイルの先頭に8桁の日付をつける。
 *      日付はファイルの最新書き込み日付とする。
 *      同じ日付のファイルが、現在のパス、「_」で始まるパス、「@」で始まるパスに
 *      含まれる場合は、最新のリビジョン番号を追加する。
 *
 *   3. フォルダ、またはファイルを指定しない場合
 *      新規に日付フォルダを作成する。
 *      日付は現在の日付を使用する。
 *      ダイアログを開き、名前を入力する。
 *      同じ日付のフォルダが、現在のパス、「_」で始まるパス、「@」で始まるパスに
 *      含まれる場合は、最新のリビジョン番号を追加する。
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Threading;

namespace Tmm
{
    class Program
    {
        /////////////////////////////////////////////////////////////////////
        // dialog

        /// <summary>
        /// input dialog
        /// </summary>
        public class InputDialog : Form
        {
            Label textLabel = new Label();
            Button accept = new Button();
            Button cancel = new Button();
            ComboBox textBox = new ComboBox();
            Label srcLabel = new Label();
            Label dstLabel = new Label();
            Label modeLabel = new Label();
            ComboBox comboBox = new ComboBox();

            public InputDialog(string text, string caption)
            {
                int width = 400;
                int height = 190;
                this.Width = width;
                this.Height = height;
                //this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ShowIcon = false;
                this.Text = caption;
                this.MinimumSize = new Size(width, height);
                this.StartPosition = FormStartPosition.CenterScreen;
                int w = this.ClientRectangle.Width;
                int h = this.ClientRectangle.Height;

                textLabel.Left = 10;
                textLabel.Top = 10;
                textLabel.Text = text;
                textLabel.AutoSize = true;
                textLabel.MaximumSize = new Size(width - 20, 0);

                modeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                modeLabel.Text = "";
                modeLabel.Left = w - 10 - modeLabel.Width;
                modeLabel.Top = 10;
                modeLabel.AutoSize = true;
                modeLabel.Click += new EventHandler(on_mode);
                modeLabel.Visible = false;

                accept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                accept.Text = "Ok";
                accept.Width = 100;
                accept.Left = w - 2 * (10 + 100);
                accept.Top = h - 10 - 22;
                accept.AutoSize = true;
                accept.Top = h - 10 - accept.Height;
                accept.DialogResult = DialogResult.OK;
                accept.Click += new EventHandler(on_close);

                cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                cancel.Text = "Cancel";
                cancel.Width = 100;
                cancel.Left = w - 10 - 100;
                cancel.Top = h - 10 - cancel.Height;
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Click += new EventHandler(on_close);

                textBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                    | AnchorStyles.Right;
                textBox.Width = w - 10 * 2;
                textBox.Left = 10;
                textBox.AutoSize = true;
                textBox.Top = this.ClientSize.Height - 7 * 10
                    - accept.Height - textBox.Height;

                srcLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                srcLabel.Left = 10;
                srcLabel.Top = h - 7*10 + 5 - accept.Height;;
                srcLabel.Text = "src:";
                srcLabel.AutoSize = true;
                srcLabel.MaximumSize = new Size(width - 20, 0);

                dstLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                dstLabel.Left = 10;
                dstLabel.Top = h - 5*10 + 10 - accept.Height;
                dstLabel.Text = "dst:";
                dstLabel.AutoSize = true;
                dstLabel.MaximumSize = new Size(width - 20, 0);

                comboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                //comboBox.Width = w - 10 * 2;
                comboBox.Left = 10;
                comboBox.Top = h - 10 - accept.Height;
                comboBox.AutoSize = true;
                comboBox.Visible = false;

                this.Controls.Add(textBox);
                this.Controls.Add(srcLabel);
                this.Controls.Add(dstLabel);
                this.Controls.Add(comboBox);
                this.Controls.Add(accept);
                this.Controls.Add(cancel);
                this.Controls.Add(textLabel);
                this.Controls.Add(modeLabel);
                this.AcceptButton = accept;
                this.CancelButton = cancel;
                this.Focus();
            }

            void on_close(Object sender, EventArgs e)
            {
                this.Close();
            }

            /////////////////////////////////////////////////////////////////////

            public string Value
            {
                get
                {
                    return textBox.Text;
                }
                set
                {
                    textBox.Text = value;
                }
            }

            public string SrcName
            {
                set
                {
                    srcLabel.Text = value;
                }
            }
            public string DstName
            {
                set
                {
                    dstLabel.Text = value;
                }
            }



            /////////////////////////////////////////////////////////////////////

            int mode;
            public List<string> ModeList = new List<string>();

            void on_mode(Object sender, EventArgs e)
            {
                if (ModeList.Count > 0)
                {

                    ModeIndex = (mode + 1) % ModeList.Count;
                }
            }

            public int ModeIndex
            {
                set
                {
                    if (value < ModeList.Count)
                    {
                        mode = value;
                        int w = ClientRectangle.Width;
                        modeLabel.Text = ModeList[mode];
                        modeLabel.Left = w - 10 - modeLabel.Width;
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////

            public void AddText(string s)
            {
                textBox.Items.Add(s);
            }


            /////////////////////////////////////////////////////////////////////
            public void AddFormatType(string s)
            {
                if (comboBox.Items.Count == 1)
                {
                    comboBox.Visible = true;
                    comboBox.SelectedIndex = 0;
                }
                comboBox.Items.Add(s);
            }

            public string FormatType
            {
                get
                {
                    return comboBox.Text;
                }
                set
                {
                    comboBox.Text = value;
                }
            }

            public int FormatTypeIndex
            {
                set
                {
                    comboBox.SelectedIndex = value;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// tag input dialog
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string TaggingDialog(string tag, string src)
        {
            string title = "indexed";
            string text = "タグを入れてください。";
            InputDialog dlg = new InputDialog(text, title);
            dlg.SrcName = "変更前: " + src;
            dlg.DstName = " ";

            var skey = @"SOFTWARE\Classes\atmm\tag";
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(skey);
            if (rkey != null) {
                string ks = (string)rkey.GetValue("");
                foreach(var k in ks) {
                    string v = (string)rkey.GetValue("" + k);
                    dlg.AddText(v);
                }
            }
            dlg.Value = tag;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.Value;
            }
            return null;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// comment input dialog
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static string CommentDialog(string comment, string src)
        {
            string title = "indexed";
            string text = "コメントを入れてください。";
            InputDialog dlg = new InputDialog(text, title);
            dlg.SrcName = "変更前: " + src;
            dlg.DstName = " ";

            var skey = @"SOFTWARE\Classes\atmm\note";
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(skey);
            if (rkey != null) {
                string ks = (string)rkey.GetValue("");
                foreach(var k in ks) {
                    string v = (string)rkey.GetValue("" + k);
                    dlg.AddText(v);
                }
            }
            dlg.Value = comment;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.Value;
            }
            return null;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// file rename input dialog
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RenameDialog(string s)
        {
            while (File.Exists(s) || Directory.Exists(s))
            {
                var p = Path.GetDirectoryName(s);
                var n = Path.GetFileName(s);
                var e = Path.GetExtension(s);
                n = n.Substring(0, n.Length - e.Length);
                var title = "indexed";
                var msg = "フォルダ、またはファイルが存在します。";
                msg += "別名で保存してください。\r\n";
                msg += n + e + "  -> *" + e;
                var dlg = new InputDialog(msg, title);
                dlg.Value = n; //.Substring(0, n.Length - e.Length);
                DialogResult res = dlg.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return null;
                }
                s = Path.Combine(p, dlg.Value + e);
            }
            return s;
        }

        /// <summary>
        /// file save input dialog
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SaveDialog(string s)
        {
            while (File.Exists(s) || Directory.Exists(s))
            {
                string p = Path.GetDirectoryName(s);
                string n = Path.GetFileName(s);
                string e = Path.GetExtension(s);
                n = n.Substring(0, n.Length - e.Length);
                string title = "indexed";
                string text = "フォルダ 、またはファイルが存在します。";
                text += "別名で保存してください。";
                InputDialog dlg = new InputDialog(text, title);
                dlg.Value = n;
                DialogResult res = dlg.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return null;
                }
                s = Path.Combine(p, dlg.Value + e);
            }
            return s;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// new name dialog
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static InputDialog NewNameDialog(string name, string dt)
        {
            string text = string.Format("説明を入力してください。(日付：{0})", dt);
            InputDialog dlg = new InputDialog(text, "日付フォルダ");
            dlg.Value = name;
            dlg.AddFormatType("<ディレクトリ>");
            dlg.FormatTypeIndex = 0;
            return dlg;
        }

        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////

        public class ItemJob
        {
            public long StartTicks;
            public enum Mode
            {
                SnapshotMode,
                IndexedMode,
                BackupMode,
                RestoreMode,
                HashFileMode,
                CheckOutMode,
                CheckInMode,
                TaggingMode,
                CommentMode,
                FolderMode,
                OpenMode,
                ZipMode,
                VerboseMode
            };
            List<Mode> _mode = new List<Mode>();

            public string Comment;

            /////////////////////////////////////////////////////////////////////

            public ItemJob()
            {
                StartTicks = DateTime.Now.Ticks;
                ResetMode();
            }

            public ItemJob Clone()
            {
                ItemJob job = new ItemJob();
                job.ClearMode();
                job.StartTicks = StartTicks;
                job._mode.AddRange(_mode);
                job.Comment = Comment;
                return job;
            }

            /////////////////////////////////////////////////////////////////////
            // execute

            /// <summary>
            /// process of file
            /// </summary>
            /// <param name="src"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public FileInfo ExecuteFile(FileInfo src, string index)
            {
                Verbose("ExecuteFile:\n  src  ="+src+"\n  index="+index);
                ItemManager im = new ItemManager();
                //action snapshot/date indexed
                if (HasMode(Mode.IndexedMode))
                {
                    im.SetMode(1);
                    src = im.Indexed(src, index, 0, false);
                }
                else if (HasMode(Mode.SnapshotMode))
                {
                    src = im.Indexed(src, index, 0, false);
                }
                if (null == src) return null;
                //action backup/restore
                if (HasMode(Mode.BackupMode))
                {
                    src = im.BackupTo(src, RenameProc);
                }
                else if (HasMode(Mode.RestoreMode))
                {
                    src = im.RestoreFrom(src, RenameProc);
                }
                if (null == src) return null;
                //action hashfile
                if (HasMode(Mode.HashFileMode))
                {
                    src = im.TestHashFile(src);
                }
                if (null == src) return null;
                //action check-out/check-in
                if (HasMode(Mode.CheckOutMode))
                {
                    src = im.CheckOutFrom(src, _level, RenameProc);
                }
                else if (HasMode(Mode.CheckInMode))
                {
                    src = im.CheckInTo(src, RenameProc);
                }
                if (null == src) return null;
                //action commment
                if (HasMode(Mode.CommentMode))
                {
                    src = im.Comment(src, CommentProc);
                }
                if (null == src) return null;
                //action tagging
                if (HasMode(Mode.TaggingMode))
                {
                    src = im.Tagging(src, TaggingProc);
                }
                return src;
            }

            public static string RenameProc(ItemManager im, string name)
            {
                var msg = "ファイル、またはフォルダが存在します。上書きしますか？";
                var res = MessageBox.Show(msg, "indexed", MessageBoxButtons.YesNoCancel);
                if (res == DialogResult.Yes)
                {
                    return "*";
                }
                if (res == DialogResult.No)
                {
                    return RenameDialog(name);
                }
                return null;
            }

            public static string CommentProc(ItemManager im, string comment)
            {
                var src = im.FileName;
                return CommentDialog(comment, src);
            }

            public static string TaggingProc(ItemManager im, string tag)
            {
                var src = im.FileName;
                return TaggingDialog(tag, src);
            }

            /// <summary>
            /// process of diectory
            /// </summary>
            /// <param name="src"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public DirectoryInfo ExecuteDirectory(DirectoryInfo src, string index)
            {
                Verbose("ExecuteDirectory:\n  src  ="+src+"\n  index="+index);
                ItemManager im = new ItemManager();
                //action snapshot/date-indexed
                if (HasMode(Mode.IndexedMode))
                {
                    im.SetMode(1);
                    src = im.Indexed(src, index, 0);
                }
                else if (HasMode(Mode.SnapshotMode))
                {
                    src = im.Indexed(src, index, 0);
                }
                if (null == src) return null;
                //action backup/restore
                if (HasMode(Mode.BackupMode))
                {
                    src = im.BackupTo(src, RenameProc);
                }
                else if (HasMode(Mode.RestoreMode))
                {
                    src = im.RestoreFrom(src, RenameProc);
                }
                if (null == src) return null;
                //action commment
                if (HasMode(Mode.CommentMode))
                {
                    src = im.Comment(src, CommentProc);
                }
                if (null == src) return null;
                //action tagging
                if (HasMode(Mode.TaggingMode))
                {
                    src = im.Tagging(src, TaggingProc);
                }
                return src;
            }

            /// <summary>
            /// process of background
            /// </summary>
            /// <param name="dst"></param>
            /// <param name="index"></param>
            public void ExecuteBackground(DirectoryInfo dst, string index)
            {
                Verbose("ExecuteBackground:\n  src  ="+dst+"\n  index="+index);
                if (HasMode(ItemJob.Mode.IndexedMode))
                {
                    MessageBox.Show("ExecuteBackground");
                    InputDialog dlg = NewNameDialog("", index);
                    string path = System.Environment.GetFolderPath(
                        Environment.SpecialFolder.Personal);
                    path = Path.Combine(path, "Templates");
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo di = new DirectoryInfo(path);
                        foreach (FileInfo fi in di.GetFiles())
                        {
                            dlg.AddFormatType(fi.Name);
                        }
                    }
                    if (dlg.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    string name = ItemManager.RecommendName(dlg.Value);

                    //
                    ItemManager im = new ItemManager(1, index, 0);
                    dst = im.NewIndexed(dst, name);

                    string p = dst.FullName;
                    // if (HasMode(Mode.FolderMode))
                    // {
                    //     MessageBox.Show("NO: name="+name+" index="+index);
                    //     System.Diagnostics.Process.Start(p);
                    // }

                    //
                    string ft = dlg.FormatType;
                    if (ft[0] != '<')
                    {
                        path = Path.Combine(path, ft);
                        if (File.Exists(path))
                        {
                            FileInfo src = new FileInfo(path);
                            name = src.Name.Replace("xxx", name);
                            p = Path.Combine(dst.FullName, name);
                            src.CopyTo(p);
                        }
                        if (HasMode(Mode.OpenMode))
                        {
                            System.Diagnostics.Process.Start(p);
                        }
                    }
                }
                //action hashfile
                if (HasMode(Mode.HashFileMode))
                {
                    ItemManager im = new ItemManager(1, index, 0);
                    im.CreateHashFile(dst);
                }
            }

            /// <summary>
            /// post process
            /// </summary>
            /// <param name="src"></param>
            public void ExecutePost(FileInfo src)
            {
                if (HasMode(Mode.FolderMode))
                {
                    string p = src.Directory.FullName;
                    System.Diagnostics.Process.Start(p);
                }
                else if (HasMode(Mode.OpenMode))
                {
                    string p = src.FullName;
                    System.Diagnostics.Process.Start(p);
                }
            }

            /////////////////////////////////////////////////////////////////////
            // mode

            /// <summary>
            /// test mode
            /// </summary>
            /// <param name="mode"></param>
            /// <returns></returns>
            public bool HasMode(Mode mode)
            {
                return _mode.Contains(mode);
            }

            /// <summary>
            /// set mode
            /// </summary>
            /// <param name="mode"></param>
            /// <param name="flag"></param>
            public void SetMode(Mode mode, bool flag=true)
            {
                if (!flag && _mode.Contains(mode))
                {
                    _mode.Remove(mode);
                    return;
                }
                if (flag && !_mode.Contains(mode))
                {
                    _mode.Add(mode);
                }
            }

            int _level;
            public void SetLevel(int level)
            {
                _level = level;
            }

            public void ToggleMode(Mode mode)
            {
                SetMode(mode, !HasMode(mode));
            }

            /// <summary>
            /// clear mode
            /// </summary>
            public void ClearMode()
            {
                _mode.Clear();
                Comment = null;
            }

            /// <summary>
            /// reset mode
            /// </summary>
            public void ResetMode()
            {
                ClearMode();
                SetMode(Mode.IndexedMode);
            }

            /// <summary>
            /// verbose
            /// </summary>
            public void Verbose(string s)
            {
                if (HasMode(Mode.VerboseMode))
                {
                    MessageBox.Show(s);
                }
            }

            /////////////////////////////////////////////////////////////////////
            // options

            public bool IsOption(string s)
            {
                string opt = "-/+";
                return (opt.IndexOf(s[0]) >= 0);
            }

            public bool ParseOption(string s)
            {
                foreach (char c in s.ToLower())
                {
                    switch (c)
                    {
                        case '+':
                            break;
                        case '-':
                        case '/':
                            ClearMode();
                            break;
                        case 's':   //snapshot
                            ToggleMode(Mode.SnapshotMode);
                            break;
                        case 'd':   //indexed
                            ToggleMode(Mode.IndexedMode);
                            break;
                        case 'o':   //checkout
                            ToggleMode(Mode.CheckOutMode);
                            break;
                        case 'i':   //checkin
                            ToggleMode(Mode.CheckInMode);
                            break;
                        case 'b':   //backup
                            ToggleMode(Mode.BackupMode);
                            break;
                        case 'r':   //restore
                            ToggleMode(Mode.RestoreMode);
                            break;
                        case 'h':   //hashfile
                            ToggleMode(Mode.HashFileMode);
                            break;
                        case 't':   //tagging
                            ToggleMode(Mode.TaggingMode);
                            break;
                        case 'c':   //comment
                            ToggleMode(Mode.CommentMode);
                            break;
                        case 'f':   //folder open
                            ToggleMode(Mode.FolderMode);
                            break;
                        case 'p':   //program open
                            ToggleMode(Mode.OpenMode);
                            break;
                        case 'z':   //archive
                            ToggleMode(Mode.ZipMode);
                            break;
                        case 'v':   //verbose
                            ToggleMode(Mode.VerboseMode);
                            break;
                        case '1':   //level
                        case '2':   //level
                        case '3':   //level
                        case '4':   //level
                        case '5':   //level
                        case '6':   //level
                        case '7':   //level
                        case '8':   //level
                        case '9':   //level
                        case '0':   //level
                            SetLevel(Int32.Parse("" + c));
                            break;
                        default:
                            return false;
                    }
                }
                return true;
            }

        }

        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////


        private static string AppName = "indexedapp";
        private static Mutex mutexObject;

        /// <summary>
        ///main process
        /// </summary>
        /// <param name="args">file/folder path or options</param>
        [STAThread]
        public static void Main(string[] args)
        {
            OperatingSystem os = Environment.OSVersion;
            if ((os.Platform == PlatformID.Win32NT) && (os.Version.Major >= 5)) {
                AppName = @"Global\" + AppName;
            }
            using (mutexObject = new Mutex(false, AppName)) {
                if (!mutexObject.WaitOne(60000, true)) {
                    MessageBox.Show("すでに起動しています。2つ同時には起動できません。", AppName);
                    mutexObject.Close();
                    return;
                }
                Sub(args);
                mutexObject.ReleaseMutex();
            }
            mutexObject.Close();
        }

        static void Sub(string[] args)
        {
            try
            {
                ItemJob job = new ItemJob();
                FileInfo src = null;

                bool no_target = true;
                foreach (string arg in args)
                {
                    //option
                    if (job.IsOption(arg))
                    {
                        if (job.ParseOption(arg))
                        {
                            continue;
                        }
                        MessageBox.Show("option error(" + arg + ")");
                        return;
                    }

                    //parent or name
                    string name = System.IO.Path.GetFileName(arg);
                    string path = arg.Substring(0, arg.Length - name.Length);
                    path = Path.GetFullPath((path.Length == 0) ? @".\" : path);
                    DirectoryInfo parent = new DirectoryInfo(path);

                    //target directory
                    foreach (DirectoryInfo di in parent.GetDirectories(name))
                    {
                        long ticks = ItemManager.GetLastTimeInFolder(di, job.StartTicks);
                        string index = ItemManager.DateTimeFormat(ticks);
                        if (null == job.ExecuteDirectory(di, index))
                        {
                            job.Verbose("stop operation. target directory.\n" + di.Name);
                            return;
                        }
                        no_target = false;
                    }

                    //target file
                    foreach (FileInfo fi in parent.GetFiles(name))
                    {
                        long ticks = fi.LastWriteTime.Ticks;
                        string index = ItemManager.DateTimeFormat(ticks);
                        src = job.ExecuteFile(fi, index);
                        if (null == src)
                        {
                            job.Verbose("stop operation. target file.\n" + fi.Name);
                            return;
                        }
                        no_target = false;
                    }
                }

                //target directory background
                if (no_target)
                {
                    DirectoryInfo di = new DirectoryInfo(".");
                    string index = ItemManager.DateTimeFormat(job.StartTicks);
                    job.ExecuteBackground(di, index);
                }
                if (src != null)
                {
                    job.ExecutePost(src);
                }
            }
            catch
            {
                MessageBox.Show("operation error.");
            }
        }
    }
}
