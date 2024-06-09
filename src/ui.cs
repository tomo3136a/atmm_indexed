using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Tmm
{
    partial class Program
    {
        /////////////////////////////////////////////////////////////////////
        // dialog

        /// <summary>
        /// input dialog
        /// </summary>
        public class InputDialog : Form
        {
            Label textLabel = new Label();
            Button accept = new Button();
            Button cancel = new Button();
            ComboBox textBox = new ComboBox();
            Label srcLabel = new Label();
            Label dstLabel = new Label();
            Label modeLabel = new Label();
            ComboBox comboBox = new ComboBox();

            public InputDialog(string text, string caption)
            {
                int width = 400;
                int height = 190;
                this.Width = width;
                this.Height = height;
                //this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ShowIcon = false;
                this.Text = caption;
                this.MinimumSize = new Size(width, height);
                this.StartPosition = FormStartPosition.CenterScreen;
                int w = this.ClientRectangle.Width;
                int h = this.ClientRectangle.Height;

                textLabel.Left = 10;
                textLabel.Top = 10;
                textLabel.Text = text;
                textLabel.AutoSize = true;
                textLabel.MaximumSize = new Size(width - 20, 0);

                modeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                modeLabel.Text = "";
                modeLabel.Left = w - 10 - modeLabel.Width;
                modeLabel.Top = 10;
                modeLabel.AutoSize = true;
                modeLabel.Click += new EventHandler(on_mode);
                modeLabel.Visible = false;

                accept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                accept.Text = "Ok";
                accept.Width = 100;
                accept.Left = w - 2 * (10 + 100);
                accept.Top = h - 10 - 22;
                accept.AutoSize = true;
                accept.Top = h - 10 - accept.Height;
                accept.DialogResult = DialogResult.OK;
                accept.Click += new EventHandler(on_close);

                cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                cancel.Text = "Cancel";
                cancel.Width = 100;
                cancel.Left = w - 10 - 100;
                cancel.Top = h - 10 - cancel.Height;
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Click += new EventHandler(on_close);

                textBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
                    | AnchorStyles.Right;
                textBox.Width = w - 10 * 2;
                textBox.Left = 10;
                textBox.AutoSize = true;
                textBox.Top = this.ClientSize.Height - 7 * 10
                    - accept.Height - textBox.Height;

                srcLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                srcLabel.Left = 10;
                srcLabel.Top = h - 7*10 + 5 - accept.Height;;
                srcLabel.Text = "src:";
                srcLabel.AutoSize = true;
                srcLabel.MaximumSize = new Size(width - 20, 0);

                dstLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                dstLabel.Left = 10;
                dstLabel.Top = h - 5*10 + 10 - accept.Height;
                dstLabel.Text = "dst:";
                dstLabel.AutoSize = true;
                dstLabel.MaximumSize = new Size(width - 20, 0);

                comboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                //comboBox.Width = w - 10 * 2;
                comboBox.Left = 10;
                comboBox.Top = h - 10 - accept.Height;
                comboBox.AutoSize = true;
                comboBox.Visible = false;

                this.Controls.Add(textBox);
                this.Controls.Add(srcLabel);
                this.Controls.Add(dstLabel);
                this.Controls.Add(comboBox);
                this.Controls.Add(accept);
                this.Controls.Add(cancel);
                this.Controls.Add(textLabel);
                this.Controls.Add(modeLabel);
                this.AcceptButton = accept;
                this.CancelButton = cancel;
                this.Focus();
            }

            void on_close(Object sender, EventArgs e)
            {
                this.Close();
            }

            /////////////////////////////////////////////////////////////////////

            public string Value
            {
                get
                {
                    return textBox.Text;
                }
                set
                {
                    textBox.Text = value;
                }
            }

            public string SrcName
            {
                set
                {
                    srcLabel.Text = value;
                }
            }
            public string DstName
            {
                set
                {
                    dstLabel.Text = value;
                }
            }



            /////////////////////////////////////////////////////////////////////

            int mode;
            public List<string> ModeList = new List<string>();

            void on_mode(Object sender, EventArgs e)
            {
                if (ModeList.Count > 0)
                {

                    ModeIndex = (mode + 1) % ModeList.Count;
                }
            }

            public int ModeIndex
            {
                set
                {
                    if (value < ModeList.Count)
                    {
                        mode = value;
                        int w = ClientRectangle.Width;
                        modeLabel.Text = ModeList[mode];
                        modeLabel.Left = w - 10 - modeLabel.Width;
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////

            public void AddText(string s)
            {
                textBox.Items.Add(s);
            }


            /////////////////////////////////////////////////////////////////////
            public void AddFormatType(string s)
            {
                if (comboBox.Items.Count == 1)
                {
                    comboBox.Visible = true;
                    comboBox.SelectedIndex = 0;
                }
                comboBox.Items.Add(s);
            }

            public string FormatType
            {
                get
                {
                    return comboBox.Text;
                }
                set
                {
                    comboBox.Text = value;
                }
            }

            public int FormatTypeIndex
            {
                set
                {
                    comboBox.SelectedIndex = value;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// tag input dialog
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string TaggingDialog(string tag, string src)
        {
            string title = "indexed";
            string text = "タグを入れてください。";
            InputDialog dlg = new InputDialog(text, title);
            dlg.SrcName = "変更前: " + src;
            dlg.DstName = " ";

            var skey = @"SOFTWARE\Classes\atmm\tag";
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(skey);
            if (rkey != null) {
                string ks = (string)rkey.GetValue("");
                foreach(var k in ks) {
                    string v = (string)rkey.GetValue("" + k);
                    dlg.AddText(v);
                }
            }
            dlg.Value = tag;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.Value;
            }
            return null;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// comment input dialog
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static string CommentDialog(string comment, string src)
        {
            string title = "indexed";
            string text = "コメントを入れてください。";
            InputDialog dlg = new InputDialog(text, title);
            dlg.SrcName = "変更前: " + src;
            dlg.DstName = " ";

            var skey = @"SOFTWARE\Classes\atmm\note";
            RegistryKey rkey = Registry.CurrentUser.OpenSubKey(skey);
            if (rkey != null) {
                string ks = (string)rkey.GetValue("");
                foreach(var k in ks) {
                    string v = (string)rkey.GetValue("" + k);
                    dlg.AddText(v);
                }
            }
            dlg.Value = comment;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.Value;
            }
            return null;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// file rename input dialog
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RenameDialog(string s)
        {
            while (File.Exists(s) || Directory.Exists(s))
            {
                var p = Path.GetDirectoryName(s);
                var n = Path.GetFileName(s);
                var e = Path.GetExtension(s);
                n = n.Substring(0, n.Length - e.Length);
                var title = "indexed";
                var msg = "フォルダ、またはファイルが存在します。";
                msg += "別名で保存してください。\r\n";
                msg += n + e + "  -> *" + e;
                var dlg = new InputDialog(msg, title);
                dlg.Value = n; //.Substring(0, n.Length - e.Length);
                DialogResult res = dlg.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return null;
                }
                s = Path.Combine(p, dlg.Value + e);
            }
            return s;
        }

        /// <summary>
        /// file save input dialog
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SaveDialog(string s)
        {
            while (File.Exists(s) || Directory.Exists(s))
            {
                string p = Path.GetDirectoryName(s);
                string n = Path.GetFileName(s);
                string e = Path.GetExtension(s);
                n = n.Substring(0, n.Length - e.Length);
                string title = "indexed";
                string text = "フォルダ 、またはファイルが存在します。";
                text += "別名で保存してください。";
                InputDialog dlg = new InputDialog(text, title);
                dlg.Value = n;
                DialogResult res = dlg.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return null;
                }
                s = Path.Combine(p, dlg.Value + e);
            }
            return s;
        }

        /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// new name dialog
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static InputDialog NewNameDialog(string name, string dt)
        {
            string text = string.Format("説明を入力してください。(日付：{0})", dt);
            InputDialog dlg = new InputDialog(text, "日付フォルダ");
            dlg.Value = name;
            dlg.AddFormatType("<ディレクトリ>");
            dlg.FormatTypeIndex = 0;
            return dlg;
        }
    }
}
