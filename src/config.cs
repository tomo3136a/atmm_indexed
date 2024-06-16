using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Tmm
{
    public class Config
    {
        public static List<string> GetValues(string kw)
        {
            var skey = @"SOFTWARE\Classes\atmm\" + kw;
            var lst = new List<string>();
            var rkey = Registry.CurrentUser.OpenSubKey(skey);
            if (rkey != null) {
                string ks = (string)rkey.GetValue("");
                foreach(var k in ks) {
                    string v = (string)rkey.GetValue("" + k);
                    lst.Add(v.Trim());
                }
            }
            return lst;
        }

        public static void AddValue(string kw, string s)
        {
            var skey = @"SOFTWARE\Classes\atmm\" + kw;
            var rkey = Registry.CurrentUser.OpenSubKey(skey, true);
            if (rkey != null) {
                var klst = "abcdef";
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
