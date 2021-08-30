#installer for indexed.exe
#
$AppName = "indexed.exe"
$OutputPath = "../bin"

$Path = "src/indexed.cs", "src/im.cs", "src/im_index.cs", "src/im_backup.cs", `
  "src/im_checkout.cs", "src/im_comment.cs"
$ReferencedAssemblies = "System.Drawing", "System.Windows.Forms"
$orgs = "setup1.org", "setup2.org", "setup3.org"

if (-not (Test-Path "$OutputPath")) {
  New-Item "$OutputPath" -ItemType Directory
}
$OutputPath = Join-Path (Resolve-Path "$OutputPath") "$AppName"

if ( $Args[0] -ne "-u" ) {
  Write-Host "install start." -ForegroundColor Yellow

  $OutputDir = Resolve-Path (Join-Path "$OutputPath" "..")
  if (-not (Test-Path "$OutputDir")) {
    New-Item "$OutputDir" -ItemType Directory
    Write-Output "make directory."
  }

  Write-Output "build appication: $AppName"
  Write-Output "    Path:       $Path"
  Write-Output "    OutputPath: $OutputPath"
  Write-Output "    References: $ReferencedAssemblies"
  Add-Type -OutputType WindowsApplication `
    -Path $Path -OutputAssembly $OutputPath `
    -ReferencedAssemblies $ReferencedAssemblies
  Write-Output "build completed."

  foreach ($org in $orgs) {
    $reg = $org -replace ".org$", ".reg"
    $bin = "$OutPutPath" -replace "\\", "\\"
    Get-Content $org | ForEach-Object { $_ -replace "{program}", $bin } > "$reg"
    reg import $reg
    Remove-Item $reg
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
if (Test-Path $OutputPath) { Remove-Item $OutputPath }
Write-Host "uninstall completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null
