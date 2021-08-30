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
        /// <summary>
        /// backup folder name
        /// </summary>
        public const string backup_name = @"_old";

        /////////////////////////////////////////////////////////////////////
        // backup

        /// <summary>
        /// backup file
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo BackupTo(FileInfo src)
        {
            string p = Path.Combine(src.DirectoryName, backup_name);
            DirectoryInfo di = new DirectoryInfo(p);
            if (!di.Exists)
            {
                di.Create();
            }
            p = Path.Combine(p, src.Name);
            src.MoveTo(p);
            FileInfo dst = new FileInfo(p);
            //dst.Attributes |= FileAttributes.ReadOnly;
            return dst;
        }

        /// <summary>
        /// backup directory
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public DirectoryInfo BackupTo(DirectoryInfo src)
        {
            string p = Path.Combine(src.Parent.FullName, backup_name);
            DirectoryInfo di = new DirectoryInfo(p);
            if (!di.Exists)
            {
                di.Create();
            }
            p = Path.Combine(p, src.Name);
            src.MoveTo(p);
            DirectoryInfo dst = new DirectoryInfo(p);
            //dst.Attributes |= FileAttributes.ReadOnly;
            return dst;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// restore file
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo RestoreFrom(FileInfo src, CallBack proc)
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

        /// <summary>
        /// restore directory
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public DirectoryInfo RestoreFrom(DirectoryInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            string p = src.Parent.FullName;
            string s = System.IO.Path.Combine(p, src.Name);
            DirectoryInfo dst = new DirectoryInfo(s);
            while (dst.Exists && (myCallBack != null)) {
                s = myCallBack(this, src.Name);
                if (s == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, s);
                dst = new DirectoryInfo(s);
            }
            CopyAll(src, dst);
            //dst.Attributes &= (~FileAttributes.ReadOnly);
            return dst;
        }
    }
}
