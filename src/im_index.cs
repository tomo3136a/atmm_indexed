using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;

namespace Tmm
{    public partial class ItemManager
    {
        /////////////////////////////////////////////////////////////////////

        bool NextIndexedItem(string file_name)
        {
            Match m1 = re_name.Match(file_name);
            if (!m1.Success)
            {
                return false;
            }
            string name = m1.Groups["name"].Value;
            string ext = m1.Groups["ext"].Value;
            if (String.Compare(_ext, ext, false) != 0)
            {
                return false;
            }

            Match m2 = re_id.Match(name);
            string idx = "";
            string rev = "";
            string note = "";
            if (m2.Success)
            {
                idx = m2.Groups["idx"].Value;
                rev = m2.Groups["rev"].Value;
                if (_mode == 0)
                {
                    note = name.Substring(m2.Index + m2.Length);
                    name = name.Substring(0, m2.Index);
                }
                else
                {
                    name = name.Substring(m2.Index + m2.Length);
                    note = "";
                }
            }

            if (String.Compare(_name, name, false) != 0)
            {
                return false;
            }

            int n_rev = ToRevisionNumber(rev);
            int i = String.Compare(idx, _index);
            if ((i < 0) || ((i == 0) && (n_rev < _n_rev)))
            {
                return false;
            }
            _index = idx;
            _n_rev = n_rev + 1;
            return true;
        }

        bool NextIndexedItem(FileInfo fi, string file_name)
        {
            long v = fi.LastWriteTime.Ticks - _last;
            if (v > -10 && v < 10)
            {
                if (fi.Name == file_name)
                {
                    return true;
                }
                string msg = "同じファイルがあります。\n" + fi.Name;
                msg += "\n\n操作を取りやめますか？";
                DialogResult result = MessageBox.Show(msg, "確認", 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    return false;
                }
            }
            NextIndexedItem(fi.Name);
            return true;
        }

        bool NextIndexedItem(DirectoryInfo di, string file_name)
        {
            long v = di.LastWriteTime.Ticks - _last;
            if (v > -10 && v < 10)
            {
                if (di.Name == file_name)
                {
                    return true;
                }
                string msg = "同じディレクトリがあります。\n" + di.Name;
                msg += "\n\n操作を取りやめますか？";
                DialogResult result = MessageBox.Show(msg, "確認", 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    return false;
                }
            }
            NextIndexedItem(di.Name);
            return true;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// rename to indexed file
        /// </summary>
        /// <param name="src"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public FileInfo Indexed(FileInfo src, string index, int n_rev, bool move=true)
        {
            if (! SetSource(src.Name, src.Length, src.LastWriteTime.Ticks))
            {
                MessageBox.Show("no match: "+src.Name);
                return null;
            }

            SetIndex(index, n_rev);
            FileNameShow();
            string search = GetSearchName();
            foreach (FileInfo fi in src.Directory.GetFiles(search))
            {
                if (!NextIndexedItem(fi, src.Name))
                {
                    return null;
                }
            }
            foreach (DirectoryInfo di in src.Directory.GetDirectories(backup_name))
            {
                foreach (FileInfo fi in di.GetFiles(search))
                {
                    if (!NextIndexedItem(fi, src.Name))
                    {
                        return null;
                    }
                }
            }
            string s = BuildName();

            if (move)
            {
                src.MoveTo(s);
            }
            else
            {
                src.CopyTo(s);
            }
            FileInfo dst = new FileInfo(s);
            dst.Attributes |= FileAttributes.ReadOnly;
            return dst;
        }

        /// <summary>
        /// rename to indexed directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public DirectoryInfo Indexed(DirectoryInfo src, string index, int n_rev, bool move=true)
        {
            if (! SetSource(src.Name, src.GetFileSystemInfos().Length, src.LastWriteTime.Ticks))
            {
                MessageBox.Show("no match: "+src.Name);
                return src;
            }

            SetIndex(index, n_rev);
            FileNameShow();
            string search = GetSearchName();
            foreach (DirectoryInfo di in src.Parent.GetDirectories(search))
            {
                if (!NextIndexedItem(di, src.Name))
                {
                    return null;
                }
            }
            foreach (DirectoryInfo di in src.GetDirectories(backup_name))
            {
                foreach (DirectoryInfo di2 in di.GetDirectories(search))
                {
                    if (!NextIndexedItem(di2, src.Name))
                    {
                        return null;
                    }
                }
            }
            string s = BuildName();

            DirectoryInfo dst = new DirectoryInfo(s);
            if (move)
            {
                src.MoveTo(s);
            }
            else
            {
                CopyAll(src, dst);
            }
            return dst;
        }

        /// <summary>
        /// create idexed dirctory
        /// </summary>
        /// <param name="parent">base path</param>
        /// <param name="name">name</param>
        /// <param name="index">index</param>
        /// <returns></returns>
        public DirectoryInfo NewIndexed(DirectoryInfo parent, string name)
        {
            SetSource(name, 0, 0);
            FileNameShow();
            string search = GetSearchName();
            foreach (DirectoryInfo di in parent.GetDirectories(search))
            {
                NextIndexedItem(di.Name);
            }
            string dst = BuildName();

            DirectoryInfo item = new DirectoryInfo(dst);
            item.Create();
            return item;
        }
    }
}
