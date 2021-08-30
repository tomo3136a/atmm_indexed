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
                s = BuildName();
                src.MoveTo(s);
                src = new DirectoryInfo(s);
            }
            return src;
        }
    }
}
