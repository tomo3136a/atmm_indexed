using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;

namespace Tmm
{
    public partial class ItemManager
    {
        /////////////////////////////////////////////////////////////////////
        // datetime

        const string dtfmt = @"yyyyMMdd";

        /// <summary>
        /// make date format string
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static string DateTimeFormat(long ticks)
        {
            DateTime dt = new DateTime(ticks);
            return dt.ToString(dtfmt);
        }

        /////////////////////////////////////////////////////////////////////
        // pattern

        /// <summary>
        /// filename pattern
        /// </summary>
        const string ptn_name = @"^(?<tag>(?:\u25a0|\u3010[^\u3011]*\u3011))?(?<name>[^.]+)(?<ext>[.].+)?$";
        static Regex re_name = new Regex(ptn_name,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// index pattern
        /// </summary>
        const string ptn_id = @"(^|[_-])(?<idx>\d{8})(?<rev>[a-z]+)?([_-]|$)";
        static Regex re_id = new Regex(ptn_id,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /////////////////////////////////////////////////////////////////////
        // revision

        /// <summary>
        /// revision charactor table
        /// </summary>
        const string _revtbl = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// get revision number
        /// </summary>
        /// <returns></returns>
        public static int ToRevisionNumber(string s)
        {
            int n = 0;
            foreach (char c in s)
            {
                n *= 26;
                n += _revtbl.IndexOf(c) + 1;
            }
            return n;
        }

        /// <summary>
        /// get revision string
        /// </summary>
        /// <returns></returns>
        public static string ToRevisionString(int n)
        {
            string s = "";
            while (n > 0)
            {
                s = _revtbl[(n - 1) % 26] + s;
                n = (n - 1)/26;
            }
            return s;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 推奨する名前に変換
        ///   先頭・末尾のスペースは削除
        ///   スペースはアンダーバーに置き換え
        ///   大なり、小なりはカッコに置き換え
        ///   イコールはハイフンに置き換え
        ///   ダブルクオート、クオート、バッククオート、スラッシュ、円マーク、パイプは削除
        ///   ピリオド、カンマ、セミコロン、コロンはアンダーバーに置き換え
        ///   末尾のドル、チルダは削除
        ///   連続したアンダーバーは1つに置き換え
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string RecommendName(string name)
        {
            string s = name.Trim().Replace('<', '(').Replace('>', ')').Replace('=', '-');
            s = Regex.Replace(s, @"\s+", "_");
            s = Regex.Replace(s, @"[""'`/\\|*?]+", "");
            s = Regex.Replace(s, @"[.,;:]+", "_");
            s = Regex.Replace(s, @"[\$~]+$", "");        //special
            s = Regex.Replace(s, @"_+", "_");
            return s;
        }

        /// <summary>
        /// test ignore file name
        /// '_','.','@','~' start-with/end-with is true
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool IsIgnoreName(string name)
        {
            string ptn = "~_.@";
            return ((name.Length > 0) &&
                    ((ptn.IndexOf(name[0]) >= 0) ||
                     (ptn.IndexOf(name[name.Length - 1]) >= 0)));
        }

        /// <summary>
        /// サブフォルダも含めてフォルダ内に最新書き込み日を取得
        /// ただし、対象外の名前は除く
        /// </summary>
        /// <param name="info"></param>
        /// <param name="org"></param>
        /// <returns></returns>
        public static long GetLastTimeInFolder(DirectoryInfo info, long org)
        {
            long last = 0;
            FileInfo[] fis = info.GetFiles(
                    "*", System.IO.SearchOption.AllDirectories);
            foreach (FileInfo fi in fis)
            {
                if (IsIgnoreName(fi.Name) == false)
                {
                    long dt = fi.LastWriteTime.Ticks;
                    last = (last < dt) ? dt : last;
                }
            }
            return (last != 0) ? last : org;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// copy directory
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void CopyAll(DirectoryInfo from, DirectoryInfo to)
        {
            if (!to.Exists)
            {
                to.Create();
            }
            foreach (FileInfo fi in from.GetFiles())
            {
                fi.CopyTo(Path.Combine(to.ToString(), fi.Name), true);
            }
            foreach (DirectoryInfo di in from.GetDirectories())
            {
                DirectoryInfo ndi = to.CreateSubdirectory(di.Name);
                CopyAll(di, ndi);
            }
        }

        /////////////////////////////////////////////////////////////////////
        public ItemManager(int mode = 0, string index = "",int n_rev = 0)
        {
            _mode = mode;
            _index = index;
            _n_rev = n_rev;
        }

        /////////////////////////////////////////////////////////////////////
        public delegate string CallBack(ItemManager im, string name);

        CallBack myCallBack;

        public void SetCallBack(CallBack proc)
        {
            myCallBack = new CallBack(proc);
        }

        /////////////////////////////////////////////////////////////////////

        int _mode = 0;      //0: last-index, 1: first-index
        string _index;
        int _n_rev;

        /// <summary>
        /// set mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public void SetMode(int mode)
        {
            _mode = mode;
        }

        /// <summary>
        /// set index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="n_rev"></param>
        /// <returns></returns>
        public void SetIndex(string index = "",int n_rev = 0)
        {
            _index = index;
            _n_rev = n_rev;
        }

        /////////////////////////////////////////////////////////////////////
        // file name

        string _src;
        string _tag;
        string _name;
        string _note;
        string _ext;
        long _last;
        long _size;

        public string FileName { get{ return _src; } }
        public string Tag { get{ return _tag; } }
        public string Name { get{ return _name; } }
        public string Note { get{ return _note; } }
        public string Ext { get{ return _ext; } }
        public long LastTime { get{ return _last; } }
        public long FileSize { get{ return _size; } }


        public void FileNameShow()
        {
            var s1 = "mode="+_mode+"\rindex="+_index+" "+_n_rev;
            var s2 = "name="+_name+"\rext="+_ext;
            var s3 = "tag="+_tag+"\rnote="+_note;
            var s4 = "last="+_last+"\rsize="+_size;
            MessageBox.Show(s1+"\r"+s2+"\r"+s3+"\r"+s4, "FileName");
        }

        /// <summary>
        /// set sorce information
        /// </summary>
        /// <param name="name"></param>
        /// <param name="size"></param>
        /// <param name="last"></param>
        /// <returns></returns>
        bool SetSource(string name, long size=0, long last=0)
        {
            _tag = "";
            _name = "";
            _note = "";
            _ext = "";
            _size = size;
            _last = last;
            _src = name;
            
            Match m1 = re_name.Match(name);
            if (! m1.Success) return false;
            _tag = m1.Groups["tag"].Value;
            _name = m1.Groups["name"].Value;
            _ext = m1.Groups["ext"].Value;

            Match m2 = re_id.Match(_name);
            if (! m2.Success) return true;
            _mode = 0;
            _index = m2.Groups["idx"].Value;
            _n_rev = ToRevisionNumber(m2.Groups["rev"].Value);
            _note = _name.Substring(m2.Index + m2.Length);
            _name = _name.Substring(0, m2.Index);
            if (m2.Index == 0)
            {
                _mode = 1;
                _name = _note;
                _note = "";
            }
            return true;
        }

        /// <summary>
        /// get search name
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string GetSearchName()
        {
            string s = "*" + _ext;
            if (_name.Length > 0)
            {
                s = "*" + _name + s;
            }
            return s;
        }

        /// <summary>
        /// build filename
        /// </summary>
        /// <returns></returns>
        string BuildName()
        {
            string index = _index + ToRevisionString(_n_rev);
            string sep = ((_name.Length > 0) && (index.Length > 0)) ? "_" : "";
            string s = _tag;
            s += (_mode == 0) ? (_name + sep + index) : (index + sep + _name);
            s += (_note.Length > 0) ? ("_" + _note) : "";
            s += _ext;
            _src = s;
            return s;
        }

        /// <summary>
        /// get restore name of file
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public string OriginalName(FileInfo src)
        {
            string p = src.Directory.Parent.FullName;
            SetSource(src.Name, 0, 0);
            _index = "";
            _n_rev = 0;
            _tag = "";
            string s = BuildName();
            return System.IO.Path.Combine(p, s);
        }
    }
}
