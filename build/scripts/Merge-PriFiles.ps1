Param(
    [Parameter(Mandatory,
      HelpMessage="List of PRI files or XML dumps (detailed only) to merge")]
    [string[]]
    $Path,

    [Parameter(Mandatory,
      HelpMessage="Output Path")]
    [string]
    $OutputPath,

    [Parameter(HelpMessage="Name of index in output file; defaults to 'Application'")]
    [string]
    $IndexName = "Application",

    [Parameter(HelpMessage="Path to makepri.exe")]
    [ValidateScript({Test-Path $_ -Type Leaf})]
    [string]
    $MakePriPath
)

$ErrorActionPreference = 'Stop'

If (-not $MakePriPath) {
  $winSdk10Root = $(Get-ItemPropertyValue -Path "HKLM:\Software\Microsoft\Windows Kits\Installed Roots" -Name "KitsRoot10")
  $MakePriPath = "$winSdk10Root\bin\10.0.26100.0\x64\MakePri.exe"
}

If ($null -Eq (Get-Item $MakePriPath -EA:SilentlyContinue)) {
    Write-Error "Could not find MakePriPath.exe at `"$MakePriPath`".`nMake sure that -MakePriPath points to a valid SDK."
    Exit 1
}

$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "tmp$([Convert]::ToString((Get-Random 65535),16).PadLeft(4,'0')).tmp"
New-Item -ItemType Directory -Path $tempDir | Out-Null
$priConfig = Join-Path $tempDir "priconfig.xml"
$priListFile = Join-Path $tempDir "pri.resfiles"
$dumpListFile = Join-Path $tempDir "dump.resfiles"

$utf8 = New-Object System.Text.UTF8Encoding($false)
$priConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<resources targetOsVersion="10.0.0" majorVersion="1">
  <index root="\" startIndexAt="dump.resfiles">
    <default>
      <qualifier name="Language" value="en-US" />
      <qualifier name="Contrast" value="standard" />
      <qualifier name="Scale" value="200" />
      <qualifier name="HomeRegion" value="001" />
      <qualifier name="TargetSize" value="256" />
      <qualifier name="LayoutDirection" value="LTR" />
      <qualifier name="DXFeatureLevel" value="DX9" />
      <qualifier name="Configuration" value="" />
      <qualifier name="AlternateForm" value="" />
      <qualifier name="Platform" value="UAP" />
    </default>
    <indexer-config type="PRIINFO" />
    <indexer-config type="RESFILES" qualifierDelimiter="." />
  </index>
  <index root="\" startIndexAt="pri.resfiles">
    <default>
      <qualifier name="Language" value="en-US" />
      <qualifier name="Contrast" value="standard" />
      <qualifier name="Scale" value="200" />
      <qualifier name="HomeRegion" value="001" />
      <qualifier name="TargetSize" value="256" />
      <qualifier name="LayoutDirection" value="LTR" />
      <qualifier name="DXFeatureLevel" value="DX9" />
      <qualifier name="Configuration" value="" />
      <qualifier name="AlternateForm" value="" />
      <qualifier name="Platform" value="UAP" />
    </default>
    <indexer-config type="PRI" />
    <indexer-config type="RESFILES" qualifierDelimiter="." />
  </index>
</resources>
"@
[System.IO.File]::WriteAllText($priConfig, $priConfigContent, $utf8)

$priPaths = @($Path | Where-Object { $_ -Like "*.pri" } | ForEach-Object {
    (Get-Item $_).FullName
})
[System.IO.File]::WriteAllLines($priListFile, [string[]]$priPaths, $utf8)

$dumpPaths = @($Path | Where-Object { $_ -Like "*.xml" } | ForEach-Object {
    (Get-Item $_).FullName
})
[System.IO.File]::WriteAllLines($dumpListFile, [string[]]$dumpPaths, $utf8)

& $MakePriPath new /pr $tempDir /cf $priConfig /o /in $IndexName /of $OutputPath
If ($LASTEXITCODE -Ne 0) { throw 'Could not merge PRI resources.' }

Remove-Item -Recurse -Force $tempDir
