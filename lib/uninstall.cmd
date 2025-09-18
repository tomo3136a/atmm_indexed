@echo off
pushd %~dp0
echo uninstalling...
set p=c:\opt\bin
if exist install_task.cmd (
    call install_task.cmd -Clean -Pass
)
if exist %p%\indexed.exe (
%p%\indexed.exe -u1
del %p%\indexed.exe
)
popd
pause
