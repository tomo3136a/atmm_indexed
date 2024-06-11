@echo off
pushd %~dp0
set p=c:\opt\bin
if not exist %p% mkdir %p%
copy indexed.exe %p%
%p%\indexed.exe -u
popd
