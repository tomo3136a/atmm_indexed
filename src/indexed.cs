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
using System.Reflection;

namespace Tmm
{
    partial class Program
    {

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
                UpdateMode,
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
                    src = im.Indexed(src, index, 0, _level);
                }
                else if (HasMode(Mode.SnapshotMode))
                {
                    src = im.Indexed(src, index, 0, _level);
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
            public int GetLevel()
            {
                return _level;
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
                        case 'u':   //update
                            ToggleMode(Mode.UpdateMode);
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


        private static string JobName = "indexedapp";
        private static string AppName = "indexed";
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
                JobName = @"Global\" + JobName;
            }
            using (mutexObject = new Mutex(false, JobName)) {
                if (!mutexObject.WaitOne(60000, true)) {
                    MessageBox.Show("すでに起動しています。2つ同時には起動できません。\n" + JobName, AppName);
                    mutexObject.Close();
                    return;
                }
                AppMain(args);
                mutexObject.ReleaseMutex();
            }
            mutexObject.Close();
        }

        static void AppMain(string[] args)
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
                        if ((di.Attributes & FileAttributes.System) != 0)
                        {
                            MessageBox.Show("not support system folder.\n"+di.Name, "indexted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
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
                        if ((fi.Attributes & FileAttributes.System) != 0)
                        {
                            MessageBox.Show("not support system file.\n" + fi.Name, "indexted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
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

                if (job.HasMode(ItemJob.Mode.UpdateMode))
                {
                    if (1 == job.GetLevel())
                    {
                        MessageBox.Show("uninstall.");
                        UninstallReg();
                    }
                    else
                    {
                        MessageBox.Show("install.");
                        InstallReg();
                    }
                    return;
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

        static void InstallReg()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //var path = @"c:\opt\";

            Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_2_snapshot");
            regkey.SetValue("MUIVerb", "スナップショット(&H)");
            regkey.SetValue("Description", "日付を付けたファイルを作成します。");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_2_snapshot\command");
            regkey.SetValue("", "\"" + path + "\" -s \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_3_restore");
            regkey.SetValue("MUIVerb", "バックアップから戻す(&B)");
            regkey.SetValue("Description", "OLDフォルダから戻す。");
            regkey.SetValue("AppliesTo", "folder:~>\\_old");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_3_restore\command");
            regkey.SetValue("", "\"" + path + "\" -r \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_4_backup");
            regkey.SetValue("MUIVerb", "バックアップへ移動(&B)");
            regkey.SetValue("Description", "OLDフォルダに移動します。");
            regkey.SetValue("AppliesTo", "NOT folder:~=\\_old");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_4_backup\command");
            regkey.SetValue("", "\"" + path + "\" -b \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_8_tagging");
            regkey.SetValue("MUIVerb", "タグ編集(&T)");
            regkey.SetValue("Description", "ファイル名タグを編集する。");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_8_tagging\command");
            regkey.SetValue("", "\"" + path + "\" -t \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_9_comment");
            regkey.SetValue("MUIVerb", "コメント(&A)");
            regkey.SetValue("Description", "ファイル名にコメントを追加・編集する。");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\*\shell\at_9_comment\command");
            regkey.SetValue("", "\"" + path + "\" -c \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder");
            regkey.SetValue("MUIVerb", "日付フォルダに変更(&H)");
            regkey.SetValue("Description", "日付付きフォルダに変更する。");
            regkey.SetValue("AppliesTo", "NOT system.filename:~<\"20\"");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder\command");
            regkey.SetValue("", "\"" + path + "\" -d \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder2");
            regkey.SetValue("MUIVerb", "日付フォルダを戻す(&H)");
            regkey.SetValue("Description", "日付付きフォルダから日付を削除する。");
            regkey.SetValue("AppliesTo", "system.filename:~<\"20\"");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder2\command");
            regkey.SetValue("", "\"" + path + "\" -d1 \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_2_snapshot");
            regkey.SetValue("MUIVerb", "スナップショットに変更(&H)");
            regkey.SetValue("Description", "スナップショットフォルダに変更する。");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_2_snapshot\command");
            regkey.SetValue("", "\"" + path + "\" -s \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_4_backup");
            regkey.SetValue("MUIVerb", "バックアップへ移動(&B)");
            regkey.SetValue("Description", "OLDフォルダに移動します。");
            regkey.SetValue("AppliesTo", "NOT folder:~=\\_old");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_4_backup\command");
            regkey.SetValue("", "\"" + path + "\" -b \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_8_tagging");
            regkey.SetValue("MUIVerb", "タグ追加(&T)");
            regkey.SetValue("Description", "フォルダ名にタグを追加する。");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_8_tagging\command");
            regkey.SetValue("", "\"" + path + "\" -t \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_9_comment");
            regkey.SetValue("MUIVerb", "コメント追加(&A)");
            regkey.SetValue("Description", "フォルダ名にコメントを追加する。");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\shell\at_9_comment\command");
            regkey.SetValue("", "\"" + path + "\" -c \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_1_datefolder");
            regkey.SetValue("MUIVerb", "日付フォルダ作成(&H)");
            regkey.SetValue("Description", "日付付きフォルダを作成する。");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_1_datefolder\command");
            regkey.SetValue("", "\"" + path + "\" -d");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_2_hashfile");
            regkey.SetValue("MUIVerb", "ハッシュファイル作成(&H)");
            regkey.SetValue("Description", "ハッシュファイルを作成する。");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_2_hashfile\command");
            regkey.SetValue("", "\"" + path + "\" -h");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.sum");
            regkey.SetValue("", "hashfile");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.md5");
            regkey.SetValue("", "hashfile");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.sha1");
            regkey.SetValue("", "hashfile");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.sha256");
            regkey.SetValue("", "hashfile");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.sha384");
            regkey.SetValue("", "hashfile");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.sha512");
            regkey.SetValue("", "hashfile");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\hashfile\shell\test");
            regkey.SetValue("MUIVerb", "ハッシュ値テスト(&H)");
            regkey.SetValue("Description", "ハッシュ値をテストする。");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\hashfile\shell\test\command");
            regkey.SetValue("", "\"" + path + "\" -h \"%V\"");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.tmm");
            regkey.SetValue("", "atmm");
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\.tmm\ShellNew");
            regkey.SetValue("MenuText", "日付フォルダ");
            regkey.SetValue("ItemName", @"@%SystemRoot%\system32\notepad.exe,-470");
            regkey.SetValue("", "\"" + path + "\" -d");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\atmm");
            regkey.SetValue("", "Advainced T's Manipulator Modules");
            regkey.Close();

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\atmm\tag");
            regkey.SetValue("", "abc");
            regkey.SetValue("a", "参考");
            regkey.SetValue("b", "編集中");
            regkey.SetValue("c", "破棄");
            regkey.Close();
            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\atmm\note");
            regkey.SetValue("", "");
            regkey.Close();

            // regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\");
            // regkey.SetValue();
            // regkey.SetValue();
            // regkey.SetValue();
            // regkey.Close();
            // regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\\command");
            // regkey.SetValue("", "\"" + path + "\" -d \"%V\"");
            // regkey.Close();

        }
        static void UninstallReg()
        {
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\at_2_snapshot");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\at_3_restore");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\at_4_backup");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\at_8_tagging");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\at_9_comment");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_1_datefolder");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_2_snapshot");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_8_tagging");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_9_comment");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\Background\shell\at_1_datefolder");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\Directory\Background\shell\at_2_hashfile");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.at");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\at");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.sum");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.md5");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.sha1");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.sha256");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.sha384");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\.sha512");
            Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\hashfile");
        }
    }
}
