@echo off
pushd %~dp0
if exist src pushd %~dp0\src
set OPT=-Sta -NonInteractive -NoProfile -NoLogo -ExecutionPolicy RemoteSigned
powershell.exe %OPT% ./%~n0.ps1 %*
popd
