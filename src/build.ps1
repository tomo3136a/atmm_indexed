#installer for indexed.exe
#
$AppName = "indexed.exe"
$OutputPath = "../bin"

$Path = "*indexed.cs", "im*.cs", "config.cs", "ui.cs", "setup.cs"
$ReferencedAssemblies = "System.Drawing", "System.Windows.Forms"

Write-Host "build start." -ForegroundColor Yellow

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

Copy-Item -Force ./install.cmd $OutputPath
Copy-Item -Force ./install-u.cmd $OutputPath

Write-Host "build completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null
