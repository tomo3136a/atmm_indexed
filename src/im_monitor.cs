using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;

namespace Tmm
{
    public partial class ItemManager
    {
        public const string monitor_conf = @"monitor.ini";
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
                    LoadMonitor();
                    break;
                }
                default:
                    break;
            }
            return src;
        }

        public DirectoryInfo Monitor(DirectoryInfo src, int level, CallBack proc)
        {
            var name = src.FullName;
            if (SetSource(src.Name, 0, 0))
            {
                // name = _name;
            }
            proc(this, name);
            return src;
        }

        string GetMonitorConfigPath(bool flag = false)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = System.IO.Path.Combine(path, monitor_conf);
            if (!flag) return path;
            if (File.Exists(path)) return path;
            using (var fo = new StreamWriter(path))
            {
                fo.WriteLineAsync("");
            }
            return path;
        }

        string GetMonitorDataPath(bool flag = false)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = System.IO.Path.Combine(path, monitor_name);
            if (!flag) return path;
            if (File.Exists(path)) return path;
            using (var fo = new StreamWriter(path))
            {
                fo.WriteLineAsync("");
            }
            return path;
        }

        public string AddMonitor(string name)
        {
            if (File.Exists(name))
            {
                FileInfo src = new FileInfo(name);
                var ptn = "*" + _name + "*" + _ext;
                var path = GetMonitorConfigPath();
                if (File.Exists(path))
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        var ss = line.Split('\t');
                        if (ss.Length < 3) continue;
                        if (string.Compare(src.DirectoryName, ss[1]) != 0) continue;
                        if (string.Compare(ptn, ss[2]) == 0) return src.FullName;
                    }
                }
                using (var fo = new StreamWriter(path, true))
                {
                    var line = src.DirectoryName + "\t" + ptn;
                    fo.WriteLineAsync(_name + "\t" + line);
                }
                return src.FullName;
            }
            if (Directory.Exists(name))
            {
                var ptn = "*.*";
                var path = GetMonitorConfigPath();
                if (File.Exists(path))
                {
                    foreach (var line in File.ReadAllLines(path))
                    {
                        var ss = line.Split('\t');
                        if (ss.Length < 3) continue;
                        if (string.Compare(name, ss[1]) != 0) continue;
                        if (string.Compare(ptn, ss[2]) == 0) return name;
                    }
                }
                using (var fo = new StreamWriter(path, true))
                {
                    var line = name + "\t" + ptn;
                    fo.WriteLineAsync(FileName + "\t" + line);
                }
                return name;
            }
            return name;
        }

        public void LoadMonitor()
        {
            Dictionary<string,string> dic = new Dictionary<string, string>();
            string path = GetMonitorDataPath();
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    var ss = line.Split('\t');
                    if (ss.Length < 2) continue;
                    dic.Add(ss[0], ss[1]);
                }
            }

            string msg = "";
            path = GetMonitorConfigPath();
            if (!File.Exists(path)) return;
            foreach (var line in File.ReadAllLines(path))
            {
                var ss = line.Split('\t');
                if (ss.Length < 3) continue;
                var name = ss[0];
                var dir = ss[1];
                var ptn = ss[2];
                DirectoryInfo di = new DirectoryInfo(dir);
                FileInfo[] fis = di.GetFiles(ptn, System.IO.SearchOption.AllDirectories);
                string date = "";
                if (dic.ContainsKey(name)) date = dic[name];
                string last = date;
                string file = "";
                foreach (FileInfo fi in fis)
                {
                    if (IsIgnoreName(fi.Name) == false)
                    {
                        string dt = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
                        last = (string.Compare(last, dt) < 0) ? dt : last;
                        file = fi.Name;
                    }
                }
                if (string.Compare(last, date) != 0)
                {
                    msg += "name=" + name + "\ndir=" + dir + "\nptn=" + ptn + "\ndate="+ date + " -> " + last + "\n" + file + "\n\n";
                    dic[name] = last;
                }
            }

            if (msg.Length > 0)
            {
                path = GetMonitorDataPath(true);
                bool append = false;
                foreach (var k in dic.Keys)
                {
                    using (var fo = new StreamWriter(path, append))
                    {
                        fo.WriteLineAsync(k + "\t" + dic[k]);
                        append = true;
                    }
                }
            }
        }
    }
}
