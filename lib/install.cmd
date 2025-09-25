@echo off
pushd %~dp0
echo installing...
set p=c:\opt\bin
if not exist %p% mkdir %p%
if exist ..\bin\indexed.exe (
  copy ..\bin\indexed.exe %p%
)
if exist %p%\indexed.exe (
  %p%\indexed.exe -u
)
popd
pause
