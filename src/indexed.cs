/**
 * Advanced T's Manipulater Module
 *
 * Command line options:
 *    -s  snapshot
 *    -d  date-indexed
 *    -b  backup
 *    -r  restore
 *    -h  hashfile
 *    -o  checkout
 *    -i  checkin
 *    -t  tagging
 *    -c  add comment
 *    -p  program
 *    -z  archive
 *    -v  verbose
 *    -u  update
 *    -m  monitor
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
using System.Windows.Forms;
using System.Threading;

namespace Tmm
{
    partial class Program
    {
        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////
        //main

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
            try
            {
                AppMain(args);
            }
            catch
            {
                MessageBox.Show("operation error.", 
                    AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// Applicaton Main
        static void AppMain(string[] args)
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
                    MessageBox.Show("option error(" + arg + ")", 
                        AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (0 != (di.Attributes & FileAttributes.System))
                    {
                        MessageBox.Show("not support system attribulte.\n" + di.Name, 
                            AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show("not support system file.\n" + fi.Name, 
                            AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            //setup
            if (job.HasMode(ItemJob.Mode.UpdateMode))
            {
                if (1 == job.GetLevel())
                {
                    UninstallReg();
                    job.Verbose("uninstalled.");
                }
                else
                {
                    InstallReg();
                    job.Verbose("installed.");
                }
                return;
            }

            //target directory background
            if (no_target)
            {
                DirectoryInfo di = new DirectoryInfo(".");
                string index = ItemManager.DateTimeFormat(job.StartTicks);
                job.ExecuteBackground(di, index);
                return;
            }
        }

        /////////////////////////////////////////////////////////////////////

        public class ItemJob
        {
            public long StartTicks;
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
                    src = im.Indexed(src, index, 0, _level, false);
                }
                else if (HasMode(Mode.SnapshotMode))
                {
                    src = im.Indexed(src, index, 0, _level, false);
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
                if (null == src) return null;
                //action monitor
                if (HasMode(Mode.MonitorMode))
                {
                    src = im.Monitor(src, _level, MonitorProc);
                }
                if (null == src) return null;
                return src;
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
                //action monitor
                if (HasMode(Mode.MonitorMode))
                {
                    src = im.Monitor(src, _level, MonitorProc);
                }
                if (null == src) return null;
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
                //action monitor
                if (HasMode(Mode.MonitorMode))
                {
                    ItemManager im = new ItemManager();
                    im.Monitor(dst, _level, MonitorProc);
                }
            }

            /////////////////////////////////////////////////////////////////////
            /// callback procedure

            public static string RenameProc(ItemManager im, string name)
            {
                var msg = "ファイル、またはフォルダが存在します。上書きしますか？";
                var res = MessageBox.Show(msg, AppName, MessageBoxButtons.YesNoCancel);
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
                string res = "";
                using (mutexObject = new Mutex(false, JobName))
                {
                    if (!mutexObject.WaitOne(100, true))
                    {
                        if (!mutexObject.WaitOne(100000, true))
                        {
                            MessageBox.Show("タイムアウトしました。\n" + JobName, 
                                AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mutexObject.Close();
                            return res;
                        }
                        res = Config.GetValue(@"current", "note");
                    }
                    else
                    {
                        res = CommentDialog(comment, src);
                        if (res == null)
                        {
                            Config.RemoveValue(@"current", "note");
                        }
                        else
                        {
                            Config.SetValue(@"current", "note", res);
                        }
                    }
                    mutexObject.ReleaseMutex();
                }
                mutexObject.Close();
                return res;
            }

            public static string TaggingProc(ItemManager im, string tag)
            {
                var src = im.FileName;
                string res = "";
                using (mutexObject = new Mutex(false, JobName))
                {
                    if (!mutexObject.WaitOne(100, true))
                    {
                        if (!mutexObject.WaitOne(100000, true))
                        {
                            MessageBox.Show("タイムアウトしました。\n" + JobName, 
                                AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            mutexObject.Close();
                            return res;
                        }
                        res = Config.GetValue(@"current", "tag");
                    }
                    else
                    {
                        res = TaggingDialog(tag, src);
                        if (res == null)
                        {
                            Config.RemoveValue(@"current", "tag");
                        }
                        else
                        {
                            Config.SetValue(@"current", "tag", res);
                        }
                    }
                    mutexObject.ReleaseMutex();
                }
                mutexObject.Close();
                return res;
            }

            public static string MonitorProc(ItemManager im, string name)
            {
                string res = "";
                using (mutexObject = new Mutex(false, JobName))
                {
                    if (!mutexObject.WaitOne(1000, true))
                    {
                        MessageBox.Show("タイムアウトしました。\n" + JobName, 
                            AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        mutexObject.Close();
                        return res;
                    }
                    res = im.AddMonitor(name);
                    mutexObject.ReleaseMutex();
                }
                mutexObject.Close();
                return res;
            }

            /////////////////////////////////////////////////////////////////////
            // mode

            public enum Mode
            {
                SnapshotMode,       // s
                IndexedMode,        // d
                BackupMode,         // b
                RestoreMode,        // r
                HashFileMode,       // h
                TaggingMode,        // t
                CommentMode,        // c
                OpenMode,           // p
                ZipMode,            // z
                VerboseMode,        // v
                UpdateMode,         // u
                MonitorMode,        // m
                ZzzMode
            };

            List<Mode> _mode = new List<Mode>();

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

            /////////////////////////////////////////////////////////////////////
            // level

            int _level;
            public void SetLevel(int level)
            {
                _level = level;
            }
            public int GetLevel()
            {
                return _level;
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
                        case 'm':   //monitor
                            ToggleMode(Mode.MonitorMode);
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
                        case '0':   //level(default)
                            SetLevel(Int32.Parse("" + c));
                            break;
                        default:
                            return false;
                    }
                }
                return true;
            }

            /////////////////////////////////////////////////////////////////////
            // verbose

            /// <summary>
            /// verbose
            /// </summary>
            public void Verbose(string s)
            {
                if (HasMode(Mode.VerboseMode))
                {
                    MessageBox.Show(s, AppName);
                }
            }

        }
    }
}
