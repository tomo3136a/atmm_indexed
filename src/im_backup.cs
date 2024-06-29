using System;
using System.Collections.Generic;
using System.IO;

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
        /// <returns>backupped file info</returns>
        public FileInfo BackupTo(FileInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            var dp = Path.Combine(src.DirectoryName, backup_name);
            var di = new DirectoryInfo(dp);
            if (!di.Exists)
            {
                di.Create();
            }
            var s = Path.Combine(dp, src.Name);
            var dst = new FileInfo(s);
            while (dst.Exists && (myCallBack != null))
            {
                var r = myCallBack(this, dst.Name);
                if (r == "*")
                {
                    dst.Delete();
                    break;
                }
                if (r == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(dp, r);
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
        /// <returns>backupped directory info</returns>
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
                var r = myCallBack(this, dst.Name);
                if (r == "*")
                {
                    dst.Delete(true);
                    break;
                }
                if (r == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, r);
                dst = new DirectoryInfo(s);
            }
            src.MoveTo(s);
            dst = new DirectoryInfo(s);
            return dst;
        }

        /////////////////////////////////////////////////////////////////////
        // restore

        /// <summary>
        /// restore file
        /// </summary>
        /// <param name="src"></param>
        /// <returns>restored file info</returns>
        public FileInfo RestoreFrom(FileInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            var p = src.Directory.Parent.FullName;
            var s = System.IO.Path.Combine(p, src.Name);
            var dst = new FileInfo(s);
            while (dst.Exists && (myCallBack != null)) {
                var r = myCallBack(this, dst.Name);
                if (r == "*")
                {
                    dst.Delete();
                    break;
                }
                if (r == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, r);
                dst = new FileInfo(s);
            }
            src.CopyTo(s);
            return dst;
        }

        /// <summary>
        /// restore directory
        /// </summary>
        /// <param name="src"></param>
        /// <returns>restored directory info</returns>
        public DirectoryInfo RestoreFrom(DirectoryInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            var p = src.Parent.FullName;
            var s = System.IO.Path.Combine(p, src.Name);
            var dst = new DirectoryInfo(s);
            while (dst.Exists && (myCallBack != null)) {
                var r = myCallBack(this, dst.Name);
                if (r == "*")
                {
                    dst.Delete();
                    break;
                }
                if (r == null)
                {
                    return null;
                }
                s = System.IO.Path.Combine(p, r);
                dst = new DirectoryInfo(s);
            }
            CopyAll(src, dst);
            return dst;
        }

    }
}
