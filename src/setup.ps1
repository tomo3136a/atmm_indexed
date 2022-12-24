#installer for indexed.exe
#
$AppName = "indexed.exe"
$OutputPath = "c:/opt/bin"

$Path = "indexed.cs", "im.cs", "im_index.cs", "im_backup.cs", `
  "im_checkout.cs", "im_comment.cs"
$ReferencedAssemblies = "System.Drawing", "System.Windows.Forms"
$orgs = "setup1.org", "setup2.org", "setup3.org"

if ( $Args[0] -ne "-u" ) {
  Write-Host "install start." -ForegroundColor Yellow

  if (-not (Test-Path $OutputPath)) {
    New-Item -Path $OutputPath -ItemType Directory | Out-Null
    Write-Output "make directory."
  }
  $OutputAssembly = Join-Path (Resolve-Path -Path $OutputPath) $AppName

  Write-Output "build program:  $AppName"
  Write-Output "    Path:       $Path"
  Write-Output "    Output:     $OutputAssembly"
  Write-Output "    References: $ReferencedAssemblies"
  Add-Type -OutputType WindowsApplication `
    -Path $Path -OutputAssembly $OutputAssembly `
    -ReferencedAssemblies $ReferencedAssemblies
  Write-Output "build completed."

  foreach ($org in $orgs) {
    $reg = $org -replace ".org$", ".reg"
    $bin = $OutPutAssembly -replace "\\", "\\"
    Get-Content $org | ForEach-Object { $_ -replace "{program}", $bin } > "$reg"
    reg import $reg
    Remove-Item $reg | Out-Null
  }

  Write-Host "install completed." -ForegroundColor Yellow
  $host.UI.RawUI.ReadKey() | Out-Null
  return
}

#uninstall
Write-Host "uninstall start." -ForegroundColor Yellow
reg delete HKCU\Software\Classes\*\shell\atmm_3_restore /f
reg delete HKCU\Software\Classes\*\shell\atmm_4_snapshot /f
reg delete HKCU\Software\Classes\*\shell\atmm_6_backup /f
reg delete HKCU\Software\Classes\*\shell\atmm_7_checkout1 /f
reg delete HKCU\Software\Classes\*\shell\atmm_7_checkout2 /f
reg delete HKCU\Software\Classes\*\shell\atmm_7_checkin /f
reg delete HKCU\Software\Classes\*\shell\atmm_8_tagging /f
reg delete HKCU\Software\Classes\*\shell\atmm_9_comment /f
reg delete HKCU\Software\Classes\Directory\shell\atmm_4_datefolder /f
reg delete HKCU\Software\Classes\Directory\shell\atmm_8_tagging /f
reg delete HKCU\Software\Classes\Directory\shell\atmm_9_comment /f
reg delete HKCU\Software\Classes\Directory\Background\shell\atmm_4_datefolder /f
reg delete HKCU\Software\Classes\.tmm /f
reg delete HKCU\Software\Classes\atmm /f
if (Test-Path $OutputPath) {
  $OutputAssembly = Join-Path (Resolve-Path -Path $OutputPath) $AppName
  if (Test-Path $OutputAssembly) { Remove-Item -Path $OutputAssembly | Out-Null }
}
Write-Host "uninstall completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null