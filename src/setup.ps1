#installer for indexed.exe
#
$AppName = "indexed.exe"
$OutputPath = "c:/opt/bin"

$Path = "*indexed.cs", "im*.cs"
$ReferencedAssemblies = "System.Drawing", "System.Windows.Forms"
$orgs = "setup1.org", "setup2.org", "setup3.org", "setup4.org"

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
reg delete HKCU\Software\Classes\*\shell\at_2_snapshot /f
reg delete HKCU\Software\Classes\*\shell\at_3_restore /f
reg delete HKCU\Software\Classes\*\shell\at_4_backup /f
# reg delete HKCU\Software\Classes\*\shell\at_5_checkout1 /f
# reg delete HKCU\Software\Classes\*\shell\at_6_checkout2 /f
# reg delete HKCU\Software\Classes\*\shell\at_7_checkin /f
reg delete HKCU\Software\Classes\*\shell\at_8_tagging /f
reg delete HKCU\Software\Classes\*\shell\at_9_comment /f
reg delete HKCU\Software\Classes\Directory\shell\at_1_datefolder /f
reg delete HKCU\Software\Classes\Directory\shell\at_2_snapshot /f
reg delete HKCU\Software\Classes\Directory\shell\at_8_tagging /f
reg delete HKCU\Software\Classes\Directory\shell\at_9_comment /f
reg delete HKCU\Software\Classes\Directory\Background\shell\at_1_datefolder /f
reg delete HKCU\Software\Classes\Directory\Background\shell\at_2_hashfile /f
reg delete HKCU\Software\Classes\.at /f
reg delete HKCU\Software\Classes\at /f
reg delete HKCU\Software\Classes\.sum /f
reg delete HKCU\Software\Classes\.md5 /f
reg delete HKCU\Software\Classes\.sha1 /f
reg delete HKCU\Software\Classes\.sha256 /f
reg delete HKCU\Software\Classes\.sha384 /f
reg delete HKCU\Software\Classes\.sha512 /f
reg delete HKCU\Software\Classes\hashfile /f

if (Test-Path $OutputPath) {
  $OutputAssembly = Join-Path (Resolve-Path -Path $OutputPath) $AppName
  if (Test-Path $OutputAssembly) { Remove-Item -Path $OutputAssembly | Out-Null }
}
Write-Host "uninstall completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null
