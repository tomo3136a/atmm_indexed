///ファイル監視

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace Tmm
{
    public partial class ItemManager
    {
        enum FileType {
            CONFIG,
            DATA,
            MESSAGE,
            DOCUMENT
        };
        public const string monitor_path = @"monitor";
        public const string monitor_name = @"monitor.txt";

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// monitor
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo Monitor(FileInfo src, int level, CallBack proc)
        {
            switch (level) {
                case 0: {
                    var name = src.FullName;
                    if (SetSource(src.Name, 0, 0))
                    {
                        // name = _name;
                    }
                    proc(this, name);
                    break;
                }
                case 1: {
                    InvokeMonitor();
                    break;
                }
                default:
                    break;
            }
            return src;
        }

        public DirectoryInfo Monitor(DirectoryInfo src, int level, CallBack proc)
        {
            switch (level) {
                case 0: {
                var name = src.FullName;
                if (SetSource(src.Name, 0, 0))
                {
                    // name = _name;
                }
                proc(this, name);
                    break;
                }
                case 1: {
                    InvokeMonitor();
                    break;
                }
                default:
                    break;
            }
            return src;
        }

        /////////////////////////////////////////////////////////////////////

        /// モニタ用パス取得、flag=trueの場合、ファイルが存在しなければからファイル作成
        static string GetMonitorPath(FileType ft, bool flag = false, string opt = "")
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = System.IO.Path.Combine(path, monitor_path);
            if (! Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string name = monitor_name;
            string ext = Path.GetExtension(name);
            switch (ft)
            {
                case FileType.CONFIG: ext = "ini"; break;
                case FileType.DATA: ext = "txt"; break;
                case FileType.MESSAGE: ext = "msg"; break;
                case FileType.DOCUMENT: ext = "xml"; break;
            }
            name = Path.ChangeExtension(name, ext);
            name = name.Replace(".", opt + ".");
            path = System.IO.Path.Combine(path, name);
            if (flag)
            {
                if (! File.Exists(path))
                {
                    using (var fo = new StreamWriter(path))
                    {
                        fo.WriteLineAsync("");
                    }
                }
            }
            return path;
        }

        /// モニタ対象の追加
        public string AddMonitor(string path)
        {
            //モニタ対象がファイルの場合
            if (File.Exists(path))
            {
                FileInfo src = new FileInfo(path);
                var dir = src.DirectoryName;
                var ptn = "*" + _name + "*" + _ext;
                var name = Program.AddMonitorDialog(_name, dir, ptn);

                var conf = GetMonitorPath(FileType.CONFIG);
                //設定ファイルがある場合、設定ファイルに登録済みなら終了
                if (File.Exists(conf))
                {
                    foreach (var line in File.ReadAllLines(conf))
                    {
                        var ss = line.Split('\t');
                        if (ss.Length < 3) continue;
                        if (string.Compare(dir, ss[1]) != 0) continue;
                        if (string.Compare(ptn, ss[2]) == 0) return src.FullName;
                    }
                }
                //設定ファイルにモニタ対象を追加
                using (var fo = new StreamWriter(conf, true))
                {
                    var line = dir + "\t" + ptn;
                    fo.WriteLineAsync(name + "\t" + line);
                }
                return src.FullName;
            }

            //モニタ対象がディレクトリの場合
            if (Directory.Exists(path))
            {
                DirectoryInfo src = new DirectoryInfo(path);
                var dir = src.FullName;
                var ptn = "*.*";
                var conf = GetMonitorPath(FileType.CONFIG);
                var name = Program.AddMonitorDialog(FileName, dir, ptn);

                //設定ファイルがある場合、設定ファイルに登録済みならなにもせず終了
                if (File.Exists(conf))
                {
                    foreach (var line in File.ReadAllLines(conf))
                    {
                        var ss = line.Split('\t');
                        if (ss.Length < 3) continue;
                        if (string.Compare(dir, ss[1]) != 0) continue;
                        if (string.Compare(ptn, ss[2]) == 0) return src.FullName;
                    }
                }
                //設定ファイルにモニタ対象を追加
                using (var fo = new StreamWriter(conf, true))
                {
                    var line = dir + "\t" + ptn;
                    fo.WriteLineAsync(name + "\t" + line);
                }
                return src.FullName;
            }
            return path;
        }

        ///ファイル変更モニタ実施
        public void InvokeMonitor()
        {
            //設定ファイルが無ければ終了
            string conf = GetMonitorPath(FileType.CONFIG);
            if (! File.Exists(conf)) return;

            //前回のモニタ結果がある場合、timに読み込み
            Dictionary<string,string> tim = LoadData();
            bool update = false;

            Dictionary<string,string> dic = new Dictionary<string, string>();
            int idx = 0;
            int cnt = 0;

            //設定ごとに調査
            foreach (var line in File.ReadAllLines(conf))
            {
                var ss = line.Split('\t');
                if (ss.Length < 3) continue;
                var name = ss[0];
                var dir = ss[1];
                var ptn = ss[2];
                idx ++;

                List<string> filelist = new List<string>();
                string file = "";
                string date = "";
                if (tim.ContainsKey(name)) date = tim[name];
                string last = date;

                //ディレクトリを調査し更新さえれたファイルリストを取得
                DirectoryInfo di = new DirectoryInfo(dir);
                FileInfo[] fis = di.GetFiles(ptn, System.IO.SearchOption.AllDirectories);
                foreach (FileInfo fi in fis)
                {
                    if (IsIgnoreName(fi.Name)) continue;
                    string dt = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
                    if (string.Compare(date, dt) >= 0) continue;
                    if (! dic.ContainsKey(fi.FullName)) filelist.Add(fi.FullName);
                    dic[fi.FullName] = name;
                    if (string.Compare(last, dt) >= 0) continue;
                    last = dt;
                    file = fi.Name;
                }
                //更新ファイルがる場合、トースト通知
                if (filelist.Count > 0) {
                    tim[name] = last;
                    update = true;
                    string s = "";
                    foreach(var s1 in filelist)
                    {
                        s += " " + s1.Substring(1 + dir.Length);
                    }
                    string v = "";
                    if (cnt++ > 0) v = cnt.ToString();
                    string path = GetMonitorPath(FileType.DOCUMENT,false,v);
                    CreateMessage(idx, path, name, s, file, dir);
                    ToastOut(path);
                }
            }

            //データが変更されたらデータファイルに保存
            if (update)
            {
                SaveData(tim);
            }
        }

        /////////////////////////////////////////////////////////////////////

        //データファイル読み込み
        static Dictionary<string,string> LoadData()
        {
            var tim = new Dictionary<string, string>();
            string data = GetMonitorPath(FileType.DATA);
            if (File.Exists(data))
            {
                foreach (var line in File.ReadAllLines(data))
                {
                    var ss = line.Split('\t');
                    if (ss.Length < 2) continue;
                    tim.Add(ss[0], ss[1]);
                }
            }
            return tim;
        }

        //データファイル書き出し
        static void SaveData(Dictionary<string,string> tim)
        {
            string path = GetMonitorPath(FileType.DATA, true);
            bool append = false;
            foreach (var k in tim.Keys)
            {
                using (var fo = new StreamWriter(path, append))
                {
                    fo.WriteLineAsync(k + "\t" + tim[k]);
                    append = true;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////

        //無効ファイルのチェック
        static bool IgnoreFileName(string name)
        {
            switch (System.IO.Path.GetExtension(name).ToLower())
            {
                case ".exe": return true;
                case ".bat": return true;
                case ".cmd": return true;
                case ".ps1": return true;
            }
            return false;
        }

        //トーストメッセージファイル作成
        static void CreateMessage(int idx, string path, string msg1, string msg2, string action, string folder)
        {
            action = @"file:///" + Path.Combine(folder, action).Replace("\\","/");
            folder = @"file:///" + folder.Replace("\\","/");
            string launch = folder;
            XElement actions = null;
            if (! IgnoreFileName(action))
            {
                launch = action;
                actions = new XElement("actions",
                    new XElement("action",
                        new XAttribute("activationType", "protocol"),
                        new XAttribute("arguments", action),
                        new XAttribute("content", "開く")
                    ),
                    new XElement("action",
                        new XAttribute("activationType", "protocol"),
                        new XAttribute("arguments", folder),
                        new XAttribute("content", "フォルダ")
                    )
                );
            }
            XElement elm = new XElement("toast",
                new XAttribute("activationType", "protocol"),
                new XAttribute("launch", launch),
                new XElement("visual",
                    new XAttribute("branding", "name"),
                    new XElement("binding",
                        new XAttribute("template", "ToastGeneric"),
                        new XElement("text",
                            new XAttribute("placement", "attribution"),
                            "ファイル変更通知"),
                        new XElement("text", msg1),
                        new XElement("text", msg2)
                    )
                ),
                actions
            );
            (new XDocument(elm)).Save(path);
        } 

        //トーストメッセージファイルを通知
        static void ToastOut(string path)
        {
            string s = @"$doc = Get-Content """ + path + @""" -Encoding UTF8;";
            s += @"[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null;";
            s += @"[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom, ContentType = WindowsRuntime] > $null;";
            s += @"$xml = New-Object Windows.Data.Xml.Dom.XmlDocument; $xml.LoadXml($doc);";
            s += @"$toast = [Windows.UI.Notifications.ToastNotification]::new($xml);";
            s += @"[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('Microsoft.Windows.Explorer').Show($toast);";
            Process cmd = new Process();
            cmd.StartInfo.FileName = "PowerShell.exe";
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; 
            cmd.StartInfo.Arguments = s;
            cmd.Start();
        }
    }
}