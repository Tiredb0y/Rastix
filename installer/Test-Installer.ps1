$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$testRoot = Join-Path $env:TEMP ('Rastix-review-tests-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $testRoot | Out-Null
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vs = & $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.Roslyn.Compiler -property installationPath
if (-not $vs) { throw 'Visual Studio with the Roslyn C# compiler is required.' }
$csc = Join-Path $vs 'MSBuild\Current\Bin\Roslyn\csc.exe'
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$references = @('System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll') | ForEach-Object { '/reference:' + (Join-Path $framework $_) }
foreach ($file in @('installer\Build-Installer.ps1','build\scripts\New-UnpackagedTerminalDistribution.ps1','build\scripts\Merge-TerminalAndXamlResources.ps1','build\scripts\Merge-PriFiles.ps1')) {
    $tokens = $null; $errors = $null
    $null = [System.Management.Automation.Language.Parser]::ParseFile((Join-Path $repo $file), [ref]$tokens, [ref]$errors)
    if ($errors.Count) { throw ($errors | Out-String) }
}
'PASS: PowerShell syntax (4 scripts)'
& $csc /nologo /target:winexe ('/out:' + (Join-Path $testRoot 'Uninstaller.exe')) @references (Join-Path $repo 'installer\Uninstaller.cs')
if ($LASTEXITCODE -ne 0) { throw 'Uninstaller compilation failed' }
'PASS: Uninstaller compilation'
$required = @('WindowsTerminal.exe', 'resources.pri', '.portable', 'Microsoft.UI.Xaml.dll', 'rx.exe')
$cases = @('valid', 'missing-rx', 'missing-xaml', 'missing-marker', 'missing-pri', 'duplicate-exe', 'traversal')
foreach ($case in $cases) {
    $zip = Join-Path $testRoot ($case + '.zip')
    $archive = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in $required) {
            if (($case -eq 'missing-rx' -and $file -eq 'rx.exe') -or ($case -eq 'missing-xaml' -and $file -eq 'Microsoft.UI.Xaml.dll') -or ($case -eq 'missing-marker' -and $file -eq '.portable') -or ($case -eq 'missing-pri' -and $file -eq 'resources.pri')) { continue }
            $null = $archive.CreateEntry('terminal-0.1.2.0/' + $file)
        }
        if ($case -eq 'duplicate-exe') { $null = $archive.CreateEntry('another/WindowsTerminal.exe') }
        if ($case -eq 'traversal') { $null = $archive.CreateEntry('../escape.txt') }
    } finally { $archive.Dispose() }
    $exe = Join-Path $testRoot ($case + '.exe')
    & $csc /nologo /target:winexe ('/out:' + $exe) @references ('/resource:' + $zip + ',RastixInstaller.Payload.zip') (Join-Path $repo 'installer\Installer.cs')
    if ($LASTEXITCODE -ne 0) { throw "Installer compilation failed: $case" }
    $process = Start-Process $exe -ArgumentList '/verify' -PassThru
    if (-not $process.WaitForExit(15000)) { $process.Kill(); throw "Verification hung: $case" }
    $process.Refresh()
    $expected = if ($case -eq 'valid') { 0 } else { 1 }
    if ($process.ExitCode -ne $expected) { throw "$case returned $($process.ExitCode), expected $expected" }
    "PASS: /verify $case (exit $expected)"
}
[xml]$manifest = Get-Content (Join-Path $repo 'src\cascadia\CascadiaPackage\Package-Dev.appxmanifest')
if ($manifest.Package.Identity.Name -ne 'Rastix' -or $manifest.Package.Identity.Version -ne '0.1.2.0') { throw 'Unexpected manifest identity/version' }
'PASS: Manifest XML/identity/version'
"Fixture outputs (not release files): $testRoot"
