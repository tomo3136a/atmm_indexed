using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Tmm
{
    public partial class ItemManager
    {
        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// checkout
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo CheckOutFrom(FileInfo src, int level, CallBack proc)
        {
            if (! SetSource(src.Name, 0, 0))
            {
                MessageBox.Show("no match: "+src.Name);
                return null;
            }
            myCallBack = new CallBack(proc);
            if (level == 1)
            {
                _tag = "【編集中】";
                _index = "";
                _n_rev = 0;
                _note = "";
            }
            if (level == 2)
            {
                _tag = "【参考】";
            }
            string n = BuildName();

            string p = System.Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop);
            p = System.IO.Path.Combine(p, src.Directory.Name);
            DirectoryInfo di = new DirectoryInfo(p);
            if (!di.Exists)
            {
                di.Create();
            }
            string s = System.IO.Path.Combine(p, n);
            FileInfo dst = new FileInfo(s);
            while (dst.Exists && (myCallBack != null)) {
                s = myCallBack(this, n);
                if (s == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, s);
                dst = new FileInfo(s);
            }
            src.CopyTo(s);
            dst.Attributes &= (~FileAttributes.ReadOnly);
            return dst;
        }


        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// checkin
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo CheckInTo(FileInfo src, CallBack proc)
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
