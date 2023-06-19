#installer for indexed.exe
#
$AppName = "indexed.exe"
$OutputPath = "c:/opt/bin"

$Path = "*indexed.cs", "im*.cs"
$ReferencedAssemblies = "System.Drawing", "System.Windows.Forms"
$orgs = "setup1.org", "setup2.org", "setup3.org"

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

Write-Host "build completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null
