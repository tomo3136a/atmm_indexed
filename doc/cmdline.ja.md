# コマンドライン

本ツールは、エクスプローラのコンテキストで使用することをメインとしていますが、コマンドラインで使用することも可能です。バッチファイルで使用することにより、エクスプローラのコンテキストで動作させることと同等の動作をさせることが可能です。

ツールは、 `C:\OPT\BIN` で使用することを基本としています。どこのフォルダにおいても動作は可能にしていますが検証はしていません。

コマンドラインは、次の形式で実行します。  
`{オプション}` は、実行する機能を指定します。`{ファイル/フォルダのパス}` は、対象を指定します。

    C:\OPT\BIN\INDEXED.EXE {オプション} {ファイル/フォルダのパス}

オプション一覧：

オプションは次の指定ができます。

* -s  snapshot スナップショット機能
* -s1 snapshot スナップショットで付けた日付を削除したファイルの複製
* -d  date-indexed 日付フォルダ機能
* -d1 date-indexed 日付フォルダから日付を削除したフォルダ名に変更
* -b  backup バックアップ機能
* -h  hashfile ハッシュファイル機能
* -t  tagging タグ機能
* -c  add comment コメント機能
* -u  update 設定更新機能

そのほか、以下のオプションがありますが、評価用であり使用しないでください。

* -r  restore
* -o  checkout
* -i  checkin
* -f  folder open
* -p  program
* -z  archive
* -v  verbose
* -m  monitor

## 設定更新機能

設定更新機能 (-u) は、コマンドラインでのみ対応しています。  
本ツールをエクスプローラで使用するにあたって、レジストリに初期設定を行います。また、レジストリから登録を削除します。