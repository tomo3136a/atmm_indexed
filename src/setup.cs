using System;
using Microsoft.Win32;
using System.Reflection;

namespace Tmm
{
    partial class Program
    {
        static void InstallReg()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var cu = Registry.CurrentUser;

            var regkey = cu.CreateSubKey(@"Software\Classes\atmm");
            regkey.SetValue("", "Advainced T's Manipulator Modules");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\atmm\current");
            regkey.SetValue("", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\atmm\tag");
            regkey.SetValue("", "abcd");
            regkey.SetValue("a", "参考");
            regkey.SetValue("b", "編集中");
            regkey.SetValue("c", "済み");
            regkey.SetValue("d", "破棄");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\atmm\tag\recent");
            regkey.SetValue("", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\atmm\note");
            regkey.SetValue("", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\atmm\note\recent");
            regkey.SetValue("", "");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_2_snapshot");
            regkey.SetValue("MUIVerb", "スナップショット(&H)");
            regkey.SetValue("Description", "日付を付けたファイルを作成します。");
            regkey.SetValue("AppliesTo", "NOT system.filename:~\"*_20??????*\"");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_2_snapshot\command");
            regkey.SetValue("", "\"" + path + "\" -s \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_2_snapshot2");
            regkey.SetValue("MUIVerb", "日付削除(&H)");
            regkey.SetValue("Description", "ファイルから日付を削除します。");
            regkey.SetValue("AppliesTo", "system.filename:~\"*_20??????*\"");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_2_snapshot2\command");
            regkey.SetValue("", "\"" + path + "\" -s1 \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_3_restore");
            regkey.SetValue("MUIVerb", "バックアップから戻す(&B)");
            regkey.SetValue("Description", "OLDフォルダから戻す。");
            regkey.SetValue("AppliesTo", @"folder:~>\_old");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_3_restore\command");
            regkey.SetValue("", "\"" + path + "\" -r \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_4_backup");
            regkey.SetValue("MUIVerb", "バックアップへ移動(&B)");
            regkey.SetValue("Description", "OLDフォルダに移動します。");
            regkey.SetValue("AppliesTo", @"NOT folder:~=\_old");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_4_backup\command");
            regkey.SetValue("", "\"" + path + "\" -b \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_8_tagging");
            regkey.SetValue("MUIVerb", "タグ編集(&T)");
            regkey.SetValue("Description", "ファイル名タグを編集する。");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_8_tagging\command");
            regkey.SetValue("", "\"" + path + "\" -t \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_9_comment");
            regkey.SetValue("MUIVerb", "コメント(&A)");
            regkey.SetValue("Description", "ファイル名にコメントを追加・編集する。");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_9_comment\command");
            regkey.SetValue("", "\"" + path + "\" -c \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder");
            regkey.SetValue("MUIVerb", "日付フォルダに変更(&H)");
            regkey.SetValue("Description", "日付付きフォルダに変更する。");
            regkey.SetValue("AppliesTo", "NOT system.filename:~<\"20\"");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder\command");
            regkey.SetValue("", "\"" + path + "\" -d \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder2");
            regkey.SetValue("MUIVerb", "日付フォルダを戻す(&H)");
            regkey.SetValue("Description", "日付付きフォルダから日付を削除する。");
            regkey.SetValue("AppliesTo", "system.filename:~<\"20\"");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_1_datefolder2\command");
            regkey.SetValue("", "\"" + path + "\" -d1 \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_2_snapshot");
            regkey.SetValue("MUIVerb", "スナップショットに変更(&H)");
            regkey.SetValue("Description", "スナップショットフォルダに変更する。");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_2_snapshot\command");
            regkey.SetValue("", "\"" + path + "\" -s \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_4_backup");
            regkey.SetValue("MUIVerb", "バックアップへ移動(&B)");
            regkey.SetValue("Description", "OLDフォルダに移動します。");
            regkey.SetValue("AppliesTo", "NOT folder:~=\\_old");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_4_backup\command");
            regkey.SetValue("", "\"" + path + "\" -b \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_8_tagging");
            regkey.SetValue("MUIVerb", "タグ追加(&T)");
            regkey.SetValue("Description", "フォルダ名にタグを追加する。");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_8_tagging\command");
            regkey.SetValue("", "\"" + path + "\" -t \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_9_comment");
            regkey.SetValue("MUIVerb", "コメント追加(&A)");
            regkey.SetValue("Description", "フォルダ名にコメントを追加する。");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_9_comment\command");
            regkey.SetValue("", "\"" + path + "\" -c \"%V\"");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_1_datefolder");
            regkey.SetValue("MUIVerb", "日付フォルダ作成(&H)");
            regkey.SetValue("Description", "日付付きフォルダを作成する。");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_1_datefolder\command");
            regkey.SetValue("", "\"" + path + "\" -d");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_2_hashfile");
            regkey.SetValue("MUIVerb", "ハッシュファイル作成(&H)");
            regkey.SetValue("Description", "ハッシュファイルを作成する。");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\Background\shell\at_2_hashfile\command");
            regkey.SetValue("", "\"" + path + "\" -h");
            regkey.Close();

            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_9_monitor");
            regkey.SetValue("MUIVerb", "モニタ登録");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\*\shell\at_9_monitor\command");
            regkey.SetValue("", "\"" + path + "\" -m \"%V\"");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_9_monitor");
            regkey.SetValue("MUIVerb", "モニタ登録");
            regkey.SetValue("Extended", "");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\Directory\shell\at_9_monitor\command");
            regkey.SetValue("", "\"" + path + "\" -m \"%V\"");
            regkey.Close();


            regkey = cu.CreateSubKey(@"Software\Classes\hashfile\shell\test");
            regkey.SetValue("MUIVerb", "ハッシュ値テスト(&H)");
            regkey.SetValue("Description", "ハッシュ値をテストする。");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\hashfile\shell\test\command");
            regkey.SetValue("", "\"" + path + "\" -h \"%V\"");
            regkey.Close();
            regkey = cu.CreateSubKey(@"Software\Classes\.sum");
            regkey.SetValue("", "hashfile");
            regkey = cu.CreateSubKey(@"Software\Classes\.md5");
            regkey.SetValue("", "hashfile");
            regkey = cu.CreateSubKey(@"Software\Classes\.sha1");
            regkey.SetValue("", "hashfile");
            regkey = cu.CreateSubKey(@"Software\Classes\.sha256");
            regkey.SetValue("", "hashfile");
            regkey = cu.CreateSubKey(@"Software\Classes\.sha384");
            regkey.SetValue("", "hashfile");
            regkey = cu.CreateSubKey(@"Software\Classes\.sha512");
            regkey.SetValue("", "hashfile");

            regkey = cu.CreateSubKey(@"Software\Classes\.tmm");
            regkey.SetValue("", "atmm");
            regkey = cu.CreateSubKey(@"Software\Classes\.tmm\ShellNew");
            regkey.SetValue("MenuText", "日付フォルダ");
            regkey.SetValue("ItemName", @"@%SystemRoot%\system32\notepad.exe,-470");
            regkey.SetValue("", "\"" + path + "\" -d");
            regkey.Close();
        }

        static void UninstallReg()
        {
            var cu = Registry.CurrentUser;
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_2_snapshot", true);
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_2_snapshot2", true);
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_3_restore", true);
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_4_backup", true);
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_8_tagging", true);
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_9_comment", true);
            cu.DeleteSubKeyTree(@"Software\Classes\*\shell\at_9_monitor", true);

            cu.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_1_datefolder", true);
            cu.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_1_datefolder2", true);
            cu.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_2_snapshot", true);
            cu.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_4_backup", true);
            cu.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_8_tagging", true);
            cu.DeleteSubKeyTree(@"Software\Classes\Directory\shell\at_9_comment", true);

            cu.DeleteSubKeyTree(@"Software\Classes\Directory\Background\shell\at_1_datefolder", true);
            cu.DeleteSubKeyTree(@"Software\Classes\Directory\Background\shell\at_2_hashfile", true);

            cu.DeleteSubKeyTree(@"Software\Classes\.sum", true);
            cu.DeleteSubKeyTree(@"Software\Classes\.md5", true);
            cu.DeleteSubKeyTree(@"Software\Classes\.sha1", true);
            cu.DeleteSubKeyTree(@"Software\Classes\.sha256", true);
            cu.DeleteSubKeyTree(@"Software\Classes\.sha384", true);
            cu.DeleteSubKeyTree(@"Software\Classes\.sha512", true);
            cu.DeleteSubKeyTree(@"Software\Classes\hashfile", true);

            cu.DeleteSubKeyTree(@"Software\Classes\.tmm", true);
            cu.DeleteSubKeyTree(@"Software\Classes\atmm", true);
        }
    }
}
