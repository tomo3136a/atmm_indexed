using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Tmm
{
    public partial class ItemManager
    {
        public const string hash_name = @"digest.sum";

        static readonly HashAlgorithm hashProvider = new MD5CryptoServiceProvider();

        public static string GetFileHash(string path, string algorithm = "MD5")
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var provider = hashProvider;
                switch (algorithm)
                {
                    case "SHA1": provider = new SHA1CryptoServiceProvider(); break;
                    case "SHA256": provider = new SHA256CryptoServiceProvider(); break;
                    case "SHA384": provider = new SHA384CryptoServiceProvider(); break;
                    case "SHA512": provider = new SHA512CryptoServiceProvider(); break;
                }
                var bs = provider.ComputeHash(fs);
                return BitConverter.ToString(bs).ToLower().Replace("-", "");
            }
        }

        public static void WriteHashFile(string path, string name, string hash, bool bAppend=false)
        {
            var hash_path = path;
            if (Directory.Exists(hash_path))
            {
                hash_path = Path.Combine(path, hash_name);
            }
            using (var file = new StreamWriter(hash_path, bAppend))
            {
                file.WriteLineAsync(hash + "  " + name);
            }
        }

        public static Dictionary<string,string> ReadHashFile(string path)
        {
            var hash_path = path;
            if (Directory.Exists(hash_path))
            {
                hash_path = Path.Combine(path, hash_name);
            }
            var col = new Dictionary<string, string>();
            var lines = File.ReadAllLines(hash_path);
            foreach (var line in lines)
            {
                int sep = line.IndexOf(' ');
                if (sep > 0)
                {
                    var hash = line.Substring(0, sep).Trim();
                    var name = line.Substring(sep).Trim();
                    name = name.Replace('*', ' ');
                    name = name.Replace('/', '\\');
                    var file = new FileInfo(name);
                    name = file.Name;
                    col[name] = hash;
                }
            }
            return col;
        }

        public static string GetAlgorithm(string hash, string default_algorithm = "MD5")
        {
            var algorithm = default_algorithm;
            switch (hash.Length)
            {
                case 32: algorithm = "MD5"; break;
                case 40: algorithm = "SHA1"; break;
                case 64: algorithm = "SHA256"; break;
                case 96: algorithm = "SHA384"; break;
                case 128: algorithm = "SHA512"; break;
            }
            return algorithm;
        }

        public static string GetAlgorithm(Dictionary<string,string> col)
        {
            var kve = col.GetEnumerator();
            kve.MoveNext();
            return GetAlgorithm(kve.Current.Value);
        }

        /////////////////////////////////////////////////////////////////////
        // hashfile

        /// <summary>
        /// create hash file
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo CreateHashFile(DirectoryInfo src)
        {
            var hash_path = Path.Combine(src.FullName, hash_name);
            var hash_file = new FileInfo(hash_path);
            if (hash_file.Exists)
            {
                hash_file.Delete();
            }
            foreach (var fi in src.EnumerateFiles())
            {
                if (!"._@".Contains(""+fi.Name[0]))
                {
                    var hash = GetFileHash(fi.FullName);
                    using (var fo = new StreamWriter(hash_file.FullName, true))
                    {
                        fo.WriteLineAsync(hash + "  " + fi.Name);
                    }
                }
            }
            var dst = new FileInfo(src.FullName);
            return dst;
        }

        /// <summary>
        /// test hash file
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public FileInfo TestHashFile(FileInfo src)
        {
            var col = ReadHashFile(src.FullName);
            if (0 == col.Count)
            {
                return null;
            }
            var common_algorithm = GetAlgorithm(col);
            var txt = "";
            foreach (var fi in src.Directory.EnumerateFiles())
            {
                if (0 > "._@".IndexOf(fi.Name[0]))
                {
                    var res = "\u2010"; // not unknown
                    if (col.ContainsKey(fi.Name))
                    {
                        var algorithm = GetAlgorithm(col[fi.Name]);
                        var hash = GetFileHash(fi.FullName, algorithm);
                        res = (hash == col[fi.Name]) ? "\u3007" : "\u2715"; // ok, ng
                    }
                    else
                    {
                        var hash = GetFileHash(fi.FullName, common_algorithm);
                        if (col.ContainsValue(hash))
                        {
                            res = "\u25b3"; // question
                        }
                    }
                    txt += res + "\t" + fi.Name + "\r\n";
                }
            }
            MessageBox.Show(txt, "Sum Check");
            return src;
        }
    }
}
