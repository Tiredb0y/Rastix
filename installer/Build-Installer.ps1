$ErrorActionPreference = 'Stop'
# Do not leave hashes or artifacts from an earlier successful run after a failure.
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$version = '0.1.2'
$packageRoot = Join-Path $repo 'src\cascadia\CascadiaPackage\AppPackages'
$outputRoot = Join-Path $repo 'out\release'
$payloadRoot = Join-Path $outputRoot 'payload'
$unpackedRoot = Join-Path $outputRoot 'unpacked'
$portableZip = Join-Path $outputRoot "Rastix-Portable-v$version.zip"
$tar = Join-Path $env:SystemRoot 'System32\tar.exe'

New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null
Remove-Item (Join-Path $outputRoot 'SHA256SUMS.txt'), (Join-Path $outputRoot "Rastix-Setup-v$version.exe"), $portableZip -Force -ErrorAction SilentlyContinue
if (-not (Test-Path $packageRoot)) { throw "Build the Release x64 package first; package directory is missing: $packageRoot" }
Remove-Item -Recurse -Force $payloadRoot, $unpackedRoot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $payloadRoot, $unpackedRoot | Out-Null

$packageVersion = '0.1.2.0'
$msix = Get-ChildItem $packageRoot -Filter '*.msix' -Recurse | Where-Object { $_.Name -match "_$([regex]::Escape($packageVersion))_x64\.msix$" } | Select-Object -First 1
if (-not $msix) { throw "Could not find the unsigned x64 MSIX under $packageRoot" }
$xaml = Get-ChildItem (Join-Path $repo 'src\cascadia\CascadiaPackage\AppPackages') -Filter 'Microsoft.UI.Xaml*.appx' -Recurse | Where-Object { $_.FullName -match '\\Dependencies\\x64\\' } | Select-Object -First 1
if (-not $xaml) { throw 'Could not find the x64 Microsoft.UI.Xaml dependency AppX needed to create the unpackaged layout.' }

$manifest = Join-Path $payloadRoot 'AppxManifest.xml'
& $tar -x -f $msix.FullName -C $payloadRoot AppxManifest.xml
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $manifest)) { throw 'Failed to extract AppxManifest.xml from the build package.' }
[xml]$xml = Get-Content $manifest
if ($xml.Package.Identity.Name -ne 'Rastix') { throw "Unexpected package identity: $($xml.Package.Identity.Name)" }
if ($xml.Package.Identity.Version -ne $packageVersion -or $xml.Package.Identity.ProcessorArchitecture -ne 'x64') { throw "Unexpected package version/architecture: $($xml.Package.Identity.Version)/$($xml.Package.Identity.ProcessorArchitecture)" }
if ($xml.Package.Identity.Publisher -like '*Microsoft Corporation*') { Write-Host 'Note: The unsigned MSIX is only an internal build input; the published distribution is unpackaged and carries no MSIX identity.' }

& (Join-Path $repo 'build\scripts\New-UnpackagedTerminalDistribution.ps1') -TerminalAppX $msix.FullName -XamlAppX $xaml.FullName -Destination $payloadRoot -PortableMode
if ($LASTEXITCODE -ne 0) { throw 'The repository unpackaged distribution script failed.' }
$distributionName = '{0}_{1}_{2}.zip' -f $xml.Package.Identity.Name, $xml.Package.Identity.Version, $xml.Package.Identity.ProcessorArchitecture
$distribution = Get-Item (Join-Path $payloadRoot $distributionName) -ErrorAction SilentlyContinue
if (-not $distribution) { throw "Expected unpackaged portable ZIP was not produced: $distributionName" }

Expand-Archive -LiteralPath $distribution.FullName -DestinationPath $unpackedRoot -Force
$terminalExe = Get-ChildItem $unpackedRoot -Filter WindowsTerminal.exe -Recurse | Select-Object -First 1
if (-not $terminalExe) { throw 'WindowsTerminal.exe is missing from unpackaged distribution.' }
$appDirectory = $terminalExe.Directory.FullName
if (-not (Test-Path (Join-Path $appDirectory '.portable'))) { throw 'Portable marker missing from app layout.' }
if (-not (Test-Path (Join-Path $appDirectory 'resources.pri'))) { throw 'Merged resources.pri missing from app layout.' }
if (-not (Test-Path (Join-Path $appDirectory 'Microsoft.UI.Xaml.dll'))) { throw 'Microsoft.UI.Xaml.dll missing from app layout.' }
if (-not (Test-Path (Join-Path $appDirectory 'rx.exe'))) { throw 'The Rastix rx.exe command shim is missing from the unpackaged layout.' }

Remove-Item $portableZip -ErrorAction SilentlyContinue
$portableFolderName = Split-Path $appDirectory -Leaf
& $tar -c --format=zip -f $portableZip -C $unpackedRoot $portableFolderName
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $portableZip)) { throw 'Failed to create portable ZIP.' }

$compiler = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vs = & $compiler -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $vs) { throw 'Could not locate Visual Studio C# compiler.' }
$csc = Get-ChildItem (Join-Path $vs 'MSBuild\Current\Bin\Roslyn\csc.exe') -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $csc) { throw 'C# compiler csc.exe was not found.' }

$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$assembly = Join-Path $outputRoot "Rastix-Setup-v$version.exe"
$uninstaller = Join-Path $outputRoot 'Rastix-Uninstaller.exe'
$uninstallerArgs = @(
    '/nologo', '/target:winexe', '/platform:anycpu', '/optimize+',
    ('/out:' + $uninstaller),
    ('/win32icon:' + (Join-Path $repo 'res\terminal.ico')),
    ('/reference:' + (Join-Path $framework 'System.Windows.Forms.dll')),
    (Join-Path $PSScriptRoot 'Uninstaller.cs')
)
& $csc.FullName @uninstallerArgs
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $uninstaller)) { throw 'Failed to compile Rastix Uninstaller.' }

$compilerArgs = @(
    '/nologo', '/target:winexe', '/platform:anycpu', '/optimize+',
    ('/out:' + $assembly),
    ('/win32icon:' + (Join-Path $repo 'res\terminal.ico')),
    ('/reference:' + (Join-Path $framework 'System.Windows.Forms.dll')),
    ('/reference:' + (Join-Path $framework 'System.Drawing.dll')),
    ('/reference:' + (Join-Path $framework 'System.IO.Compression.dll')),
    ('/reference:' + (Join-Path $framework 'System.IO.Compression.FileSystem.dll')),
    ('/resource:' + $portableZip + ',RastixInstaller.Payload.zip'),
    ('/resource:' + $uninstaller + ',RastixInstaller.Uninstaller.exe'),
    (Join-Path $PSScriptRoot 'Installer.cs')
)
& $csc.FullName @compilerArgs
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $assembly)) { throw 'Failed to compile Rastix Setup.' }

$signature = Get-AuthenticodeSignature $assembly
if ($signature.Status -ne 'NotSigned') { throw "Expected unsigned output, got signature state $($signature.Status)." }
Remove-Item $uninstaller -Force
$verification = Start-Process -FilePath $assembly -ArgumentList '/verify' -Wait -PassThru
if ($verification.ExitCode -ne 0) { throw "Embedded portable payload verification failed (exit code $($verification.ExitCode))." }

$hashInstaller = (Get-FileHash $assembly -Algorithm SHA256).Hash.ToLowerInvariant()
$hashZip = (Get-FileHash $portableZip -Algorithm SHA256).Hash.ToLowerInvariant()
@("$hashInstaller  $([IO.Path]::GetFileName($assembly))", "$hashZip  $([IO.Path]::GetFileName($portableZip))") | Set-Content -Encoding ascii (Join-Path $outputRoot 'SHA256SUMS.txt')
Write-Host "Installer: $assembly"
Write-Host "Portable ZIP: $portableZip"
