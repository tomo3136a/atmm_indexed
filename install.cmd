@pushd %~dp0
@set PS=powershell.exe -Sta -NonInteractive -NoProfile -NoLogo -ExecutionPolicy RemoteSigned
@%PS% ./setup.ps1 %*
