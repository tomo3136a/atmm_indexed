@echo off
pushd %~dp0\src
set OPT=-Sta -NonInteractive -NoProfile -NoLogo -ExecutionPolicy RemoteSigned
powershell.exe %OPT% ./setup.ps1 %*
popd
