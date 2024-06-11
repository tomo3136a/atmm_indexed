@echo off
pushd %~dp0
set p=c:\opt\bin
if exist %p% (
%p%\indexed.exe -u1
del %p%\indexed.exe
)
popd
