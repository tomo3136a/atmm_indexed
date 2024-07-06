param($Name = "ファイル監視")

# remove resistered task
if (Get-ScheduledTask | Where-Object {$_.TaskName -eq $Name}) {
    try {
        Write-Host "Stop ScheduleTask ${Name}." -ForegroundColor Yellow
        Stop-ScheduledTask  -TaskName $Name

        Write-Host "Unregister ScheduleTask ${Name}." -ForegroundColor Yellow
        Unregister-ScheduledTask -TaskName $Name -Confirm:$false
    }
    catch {}
}

# configure
$ExecutePath = "c:\opt\bin\indexed.exe"
$WorkingDirectory = "%USERPROFILE%\documents"

# registration task schedule
$Trigger = @()
$Trigger += New-ScheduledTaskTrigger `
    -Once -at "08:00:00" `
    -RepetitionInterval "00:01:00"

$Action = New-ScheduledTaskAction `
    -Execute $ExecutePath `
    -Argument "-m1" `
    -WorkingDirectory $WorkingDirectory

Write-Host "Register ScheduleTask ${Name}." -ForegroundColor Yellow
Register-ScheduledTask `
    -TaskName $Name `
    -Action $Action `
    -Trigger $Trigger | Out-Null

# display next scedule information
Write-Host "Display ScheduleTask ${Name}." -ForegroundColor Yellow
Get-ScheduledTaskInfo -TaskName $Name

# end of resister task
Write-Host "Registered ScheduleTask completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null
