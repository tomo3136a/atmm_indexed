using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Tmm
{
    public partial class ItemManager
    {
        const char _tag_left = '\u3010';
        const char _tag_right = '\u3011';

        /////////////////////////////////////////////////////////////////////
        // comment-note

        /// <summary>
        /// modified comment to file
        /// </summary>
        /// <param name="src"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        public FileInfo Comment(FileInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            SetSource(src.Name);
            string s = (myCallBack == null) ? _note : myCallBack(this, _note);
            if (s != null)
            {
                _note = s.Trim().TrimStart(new char[]{'_'});
                s = BuildName();
                src.MoveTo(s);
                src = new FileInfo(s);
            }
            return src;
        }

        /// <summary>
        /// modified comment to directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        public DirectoryInfo Comment(DirectoryInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            SetSource(src.Name);
            string s = (myCallBack == null) ? _note : myCallBack(this, _note);
            if (s != null)
            {
                _note = s.Trim().TrimStart(new char[]{'_'});
                s = BuildName();
                src.MoveTo(s);
                src = new DirectoryInfo(s);
            }
            return src;
        }

        /////////////////////////////////////////////////////////////////////
        // comment-tag

        /// <summary>
        /// modified tag to file
        /// </summary>
        /// <param name="src"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        public FileInfo Tagging(FileInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            SetSource(src.Name);
            string s = (myCallBack == null) ? _tag : myCallBack(this, _tag);
            if (s != null)
            {
                //TODO: reject charactor from tag-string inner
                s = s.Trim(new char[]{' ','\t','\v','_',_tag_left,_tag_right});
                _tag = (s.Length > 0) ? (_tag_left + s + _tag_right) : "";
                UpdateTag(_tag);
                s = BuildName();
                src.MoveTo(s);
                src = new FileInfo(s);
            }
            return src;
        }

        /// <summary>
        /// modified tag to directory
        /// </summary>
        /// <param name="src"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        public DirectoryInfo Tagging(DirectoryInfo src, CallBack proc)
        {
            myCallBack = new CallBack(proc);
            SetSource(src.Name);
            string s = (myCallBack == null) ? _tag : myCallBack(this, _tag);
            if (s != null)
            {
                //TODO: reject charactor from tag-string inner
                s = s.Trim(new char[]{' ','\t','\v','_',_tag_left,_tag_right});
                _tag = (s.Length > 0) ? (_tag_left + s + _tag_right) : "";
                UpdateTag(_tag);
                s = BuildName();
                src.MoveTo(s);
                src = new DirectoryInfo(s);
            }
            return src;
        }

        void UpdateTag(string s)
        {
            var skey = @"SOFTWARE\Classes\atmm\tag";
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(skey, true);
            if (rkey != null) {
                var klst = "abcd";
                var ks = (string)rkey.GetValue("");
                foreach(var k in ks) {
                    var v = (string)rkey.GetValue("" + k);
                    if (s == v) {
                        var i = ks.IndexOf(k);
                        if (0 < i) {
                            ks = "" + k + ks.Remove(i, 1);
                            rkey.SetValue("", ks);
                        }
                        return;
                    }
                    var j = klst.IndexOf(k);
                    if (0 <= j) {
                        klst = klst.Remove(j, 1);
                    }
                }
                if (0 == klst.Length) {
                    var k = "" + ks[ks.Length-1];
                    rkey.DeleteValue(k);
                    ks = k + ks.Remove(ks.Length-1, 1);
                    rkey.SetValue("", ks);
                    rkey.SetValue(k, s);
                }
                else {
                    var k = "" + klst[0];
                    ks = k + ks;
                    rkey.SetValue("", ks);
                    rkey.SetValue(k, s);
                }
            }
        }
    }
}
