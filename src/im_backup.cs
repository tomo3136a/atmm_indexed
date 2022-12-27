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
        public FileInfo BackupTo(FileInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            var p = Path.Combine(src.DirectoryName, backup_name);
            var di = new DirectoryInfo(p);
            if (!di.Exists)
            {
                di.Create();
            }
            var s = Path.Combine(p, src.Name);
            var dst = new FileInfo(s);
            while (dst.Exists && (myCallBack != null))
            {
                s = myCallBack(this, src.Name);
                if (s == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, s);
                dst = new FileInfo(s);
            }
            src.MoveTo(s);
            dst = new FileInfo(s);
            return dst;
        }

        /// <summary>
        /// backup directory
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public DirectoryInfo BackupTo(DirectoryInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            var p = Path.Combine(src.Parent.FullName, backup_name);
            var di = new DirectoryInfo(p);
            if (!di.Exists)
            {
                di.Create();
            }
            var s = Path.Combine(p, src.Name);
            var dst = new DirectoryInfo(s);
            while (dst.Exists && (myCallBack != null))
            {
                s = myCallBack(this, src.Name);
                if (s == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, s);
                dst = new DirectoryInfo(s);
            }
            src.MoveTo(s);
            dst = new DirectoryInfo(s);
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
            var p = src.Directory.Parent.FullName;
            var s = System.IO.Path.Combine(p, src.Name);
            var dst = new FileInfo(s);
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
            var p = src.Parent.FullName;
            var s = System.IO.Path.Combine(p, src.Name);
            var dst = new DirectoryInfo(s);
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
            return dst;
        }
    }
}
