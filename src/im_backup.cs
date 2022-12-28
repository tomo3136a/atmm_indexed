using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Tmm
{
    public partial class ItemManager
    {
        /// <summary>
        /// backup folder name
        /// </summary>
        public const string backup_name = @"_old";

        /// <summary>
        /// backup folder name
        /// </summary>
        public const string backup_hash = @"_backup.sum";

        /// <summary>
        /// backup file size max
        /// </summary>
        public const int backup_hash_calc_max = 32*1024*1024;

        // /// <summary>
        // /// log file name
        // /// </summary>
        // public const string hash_name = @"_hashfile.sum";

        // static readonly HashAlgorithm hashProvider = new MD5CryptoServiceProvider();

        // public static string GetFileHash(string path)
        // {
        //     using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //     {
        //         var bs = hashProvider.ComputeHash(fs);
        //         return BitConverter.ToString(bs).ToLower().Replace("-", "");
        //     }
        // }

        // public static void WriteLog(string path, string name, string hash)
        // {
        //     var p = Path.Combine(path, hash_name);
        //     using (var file = new StreamWriter(p, true))
        //     {
        //         file.WriteLineAsync(hash + " * " + name);
        //     }
        // }

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
            var sh = GetFileHash(src.FullName);
            var dp = Path.Combine(src.DirectoryName, backup_name);
            var di = new DirectoryInfo(dp);
            if (di.Exists)
            {
                var lst = new List<FileInfo>();
                foreach (var fi in di.EnumerateFiles())
                {
                    if (src.Length == fi.Length)
                    {
                        if (src.Length < backup_hash_calc_max)
                        {
                            var dh = GetFileHash(fi.FullName);
                            if (sh == dh)
                            {
                                lst.Add(fi);
                            }
                        }
                        else
                        {
                            if (src.LastWriteTime == fi.LastWriteTime)
                            {
                                lst.Add(fi);
                            }
                        }
                    }
                }
                if (lst.Count > 0)
                {
                    var msg = "行き先に同じファイルがあります。";
                    msg += " ("+lst.Count+")\r\n";
                    msg += "移動元：\t"+src.Name+"\r\n\r\n";
                    msg += "移動先：\t";
                    foreach (var fi in lst)
                    {
                        msg += fi.Name + "\r\n" + "\t";
                    }
                    MessageBox.Show(msg, "indexed");
                }
            }
            else
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
            s = Path.Combine(dp, backup_hash);
            WriteHashFile(s, src.Name, sh, true);
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
            dst = new FileInfo(s);
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
            dst = new DirectoryInfo(s);
            return dst;
        }
    }
}
