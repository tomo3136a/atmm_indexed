using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
//using Windows.UI.Notifications.Management;
//using Windows.Foundation.Metadata;
//using Windows.UI.Notifications;
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
                        name = _name;
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

        public string AddMonitor(string name)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = System.IO.Path.Combine(path, monitor_conf);
            FileInfo src = new FileInfo(FileName);
            var ptn = "*" + _name + "*" + _ext;
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var ss = line.Split('\t');
                if (ss.Length < 3) continue;
                if (string.Compare(ptn, ss[2]) == 0) return src.FullName;
            }
            using (var fo = new StreamWriter(path, true))
            {
                // var line = src.DirectoryName + "\t" + ptn + "\t" + src.LastWriteTime;
                var line = src.DirectoryName + "\t" + ptn;
                fo.WriteLineAsync(name + "\t" + line);
            }
            return src.FullName;
        }

        public bool HasMonitor(string ptn)
        {
            FileInfo src = new FileInfo(FileName);
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = System.IO.Path.Combine(path, monitor_conf);
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var ss = line.Split('\t');
                if (ss.Length < 3) continue;
                if (string.Compare(ptn, ss[2]) == 0) return true;
            }
            return false;
        }


        public void LoadMonitor()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = System.IO.Path.Combine(path, monitor_name);
            Dictionary<string,string> dic = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(path))
            {
                var ss = line.Split('\t');
                if (ss.Length < 2) continue;
                dic.Add(ss[0], ss[1]);
            }

            path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            path = System.IO.Path.Combine(path, monitor_conf);
            string msg = "";
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
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
                        string dt = fi.LastWriteTime.ToString();
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
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                path = System.IO.Path.Combine(path, monitor_name);
                bool append = false;
                foreach (var k in dic.Keys)
                {
                    using (var fo = new StreamWriter(path, append))
                    {
                        fo.WriteLineAsync(k + "\t" + dic[k]);
                        append = true;
                    }
                }
                MessageBox.Show("検出：\n" + msg);
            }
        }

        public DirectoryInfo Monitor(DirectoryInfo src, int level, CallBack proc)
        {
            string p = System.Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop);
            p = System.IO.Path.Combine(p, "monitor.txt");

            foreach (var fi in src.EnumerateFiles())
            {
                using (var fo = new StreamWriter(p, true))
                {
                    fo.WriteLineAsync("*" + fi.Name);
                }
            }
            return src;

            //var dst = new FileInfo(src.FullName);
            //return dst;

            // myCallBack = new CallBack(proc);
            // if (level == 1)
            // {
            //     _tag = "【編集中】";
            //     _index = "";
            //     _n_rev = 0;
            //     _note = "";
            // }
            // if (level == 2)
            // {
            //     _tag = "【参考】";
            // }
            // string n = BuildName();

            // string p = System.Environment.GetFolderPath(
            //     Environment.SpecialFolder.Desktop);
            // p = System.IO.Path.Combine(p, src.Directory.Name);
            // DirectoryInfo di = new DirectoryInfo(p);
            // if (!di.Exists)
            // {
            //     di.Create();
            // }
            //string s = System.IO.Path.Combine(p, n);
            //FileInfo dst = new FileInfo(s);
            // while (dst.Exists && (myCallBack != null)) {
            //     s = myCallBack(this, n);
            //     if (s == null)
            //     {
            //         return null;
            //     }
            //     s = System.IO.Path.Combine(p, s);
            //     dst = new FileInfo(s);
            // }
            // src.CopyTo(s);
            //dst.Attributes &= (~FileAttributes.ReadOnly);
            //return dst;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// checkin
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo DeleteMonitor(FileInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            string p = src.Directory.Parent.FullName;
            string s = System.IO.Path.Combine(p, src.Name);
            FileInfo dst = new FileInfo(s);
            while (dst.Exists && (myCallBack != null)) {
                s = myCallBack(this, src.Name);
                if (s == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, s);
                dst = new FileInfo(s);
            }
            src.CopyTo(s);
            //dst.Attributes &= (~FileAttributes.ReadOnly);
            return dst;
        }
   }
}
