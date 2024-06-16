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
        const char _tag_center = '\u25a0';

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
            if (false == SetSource(src.Name)) {
                FileNameShow();
            }
            string s = (myCallBack == null) ? _note : myCallBack(this, _note);
            if (s != null)
            {
                _note = s.Trim().TrimStart(new char[]{'_'});
                UpdateNote(_note);
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
            if (false == SetSource(src.Name)) {
                FileNameShow();
            }
            string s = (myCallBack == null) ? _note : myCallBack(this, _note);
            if (s != null)
            {
                _note = s.Trim().TrimStart(new char[]{'_'});
                UpdateNote(_note);
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
            if (false == SetSource(src.Name)) {
                FileNameShow();
            }
            string s = TrimTag(_tag);
            s = (myCallBack == null) ? s : myCallBack(this, s);
            if (s != null)
            {
                _tag = BuildTagName(s);
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
            if (false == SetSource(src.Name)) {
                FileNameShow();
            }
            string s = TrimTag(_tag);
            s = (myCallBack == null) ? s : myCallBack(this, s);
            if (s != null)
            {
                _tag = BuildTagName(s);
                UpdateTag(_tag);
                s = BuildName();
                src.MoveTo(s);
                src = new DirectoryInfo(s);
            }
            return src;
        }

        /////////////////////////////////////////////////////////////////////
        // sub

        void UpdateNote(string s)
        {
            if (0 == s.Length) return;
            Config.AddValue(@"note\recent", s);
        }

        void UpdateTag(string s)
        {
            s = TrimTag(s).TrimStart(new char[]{ '-' });
            if (0 == s.Length) return;
            Config.AddValue(@"tag\recent", s);
        }

        string TrimTag(string s)
        {
            s = s.Replace(_tag_left.ToString(), "");
            s = s.Replace(_tag_right.ToString(), "");
            s = s.Replace(_tag_center.ToString(), "-");
            return s;
        }

        string BuildTagName(string s)
        {
            s = s.Trim(new char[]{' ','\t','\v','_',_tag_left,_tag_right});
            s = (s.Length > 0) ? (_tag_left + s + _tag_right) : "";
            if (s == "-" || s == "" + _tag_center) { s = "" + _tag_center; }
            return s;
        }
    }
}
