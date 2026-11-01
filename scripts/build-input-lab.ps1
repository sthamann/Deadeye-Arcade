param([string]$OutputDirectory="release/Deadeye-InputLab-Windows-x64")
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot
$sdk=Join-Path $root 'work/input-lab-sdk'
$archive=Join-Path $sdk 'HIDMaestro-v2.1.0.zip'
New-Item -ItemType Directory $sdk -Force|Out-Null
$expected='7fe349f163864b8dfc36117af260d51bac94a2263b6cdfa719cb364a808397b4'
if(!(Test-Path $archive)){
    Invoke-WebRequest 'https://github.com/hifihedgehog/HIDMaestro/releases/download/v2.1.0/HIDMaestro-v2.1.0.zip' -OutFile $archive
}
if((Get-FileHash $archive -Algorithm SHA256).Hash.ToLower()-ne $expected){throw 'HIDMaestro archive checksum mismatch'}
Expand-Archive $archive (Join-Path $sdk 'extracted') -Force
Copy-Item (Join-Path $sdk 'extracted/HIDMaestro.Core.dll') $sdk -Force
foreach($name in @('Microsoft.Windows.SDK.NET.dll','WinRT.Runtime.dll')){
    Copy-Item (Join-Path $sdk ('extracted/HIDMaestroTest/'+$name)) $sdk -Force
}
$output=[IO.Path]::GetFullPath((Join-Path $root $OutputDirectory))
dotnet publish (Join-Path $root 'tools/Deadeye.InputLab') -c Release -r win-x64 --self-contained true -o $output "-p:HIDMaestroDir=$sdk"
if($LASTEXITCODE-ne 0){throw 'Input lab publish failed'}
Copy-Item (Join-Path $sdk 'extracted/LICENSE') (Join-Path $output 'HIDMaestro-LICENSE.txt') -Force
Copy-Item (Join-Path $sdk 'extracted/THIRD-PARTY-NOTICES.txt') (Join-Path $output 'HIDMaestro-THIRD-PARTY-NOTICES.txt') -Force
Copy-Item (Join-Path $root 'tools/Deadeye.InputLab/README.md') $output -Force
Copy-Item (Join-Path $root 'LICENSE') (Join-Path $output 'Deadeye-LICENSE.txt') -Force
Copy-Item (Join-Path $root 'THIRD-PARTY.md') $output -Force
New-Item -ItemType Directory (Join-Path $output 'licenses') -Force|Out-Null
Copy-Item (Join-Path $root 'licenses/input-lab-*') (Join-Path $output 'licenses') -Force
Write-Host ('Built optional input lab in '+$output+'. Building does not install its driver.')
