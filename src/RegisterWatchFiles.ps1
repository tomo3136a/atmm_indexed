# remove duplication task-name
try {
    #Get-ScheduledTask -TaskName $TaskName | Unregister-ScheduledTask
    Unregister-ScheduledTask -TaskName "WatchFiles"
}
catch {}

# configure
$ExecutePath = "c:\opt\bin\indexed.exe"
$WorkingDirectory = "%USERPROFILE%\desktop"

# registration task schedule
$Trigger = @()
$Trigger += New-ScheduledTaskTrigger `
    -Once -at "08:00:00" `
    -RepetitionInterval "00:01:00"

$Action = New-ScheduledTaskAction `
    -Execute $ExecutePath `
    -Argument "-m1 %USERPROFILE%\documents\monitor.ini" `
    -WorkingDirectory $WorkingDirectory

Register-ScheduledTask `
    -TaskName "WatchFiles" `
    -Action $Action `
    -Trigger $Trigger

# display next scedule information
Get-ScheduledTaskInfo -TaskName "WatchFiles"
#$host.UI.RawUI.ReadKey() | Out-Null
