param(
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [Parameter(Mandatory = $true)][string]$SymbolPackagePath,
    [Parameter(Mandatory = $true)][string]$ExpectedVersion
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

function Read-EntryText($archive, [string]$name) {
    $entry = $archive.GetEntry($name)
    if ($null -eq $entry) { throw "Missing package entry: $name" }
    $reader = [System.IO.StreamReader]::new($entry.Open())
    try { return $reader.ReadToEnd() }
    finally { $reader.Dispose() }
}

function Require-Entry($archive, [string]$name) {
    if ($null -eq $archive.GetEntry($name)) { throw "Missing package entry: $name" }
}

function Get-MetadataNode([xml]$manifest, [string]$name) {
    return $manifest.SelectSingleNode("/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$name']")
}

$package = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $PackagePath).Path)
$symbols = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $SymbolPackagePath).Path)
try {
    [xml]$manifest = Read-EntryText $package 'Kajabity.DocForms.nuspec'
    [xml]$symbolManifest = Read-EntryText $symbols 'Kajabity.DocForms.nuspec'

    if ((Get-MetadataNode $manifest 'id').InnerText -ne 'Kajabity.DocForms' -or
        (Get-MetadataNode $manifest 'version').InnerText -ne $ExpectedVersion) {
        throw 'Package ID or version does not match the expected release.'
    }
    if ((Get-MetadataNode $symbolManifest 'id').InnerText -ne 'Kajabity.DocForms' -or
        (Get-MetadataNode $symbolManifest 'version').InnerText -ne $ExpectedVersion) {
        throw 'Symbol package ID or version does not match the main package.'
    }
    $licence = Get-MetadataNode $manifest 'license'
    if ($licence.type -ne 'expression' -or $licence.InnerText -ne 'Apache-2.0') {
        throw 'Apache-2.0 licence metadata is missing.'
    }
    if ((Get-MetadataNode $manifest 'readme').InnerText -ne 'README.md' -or
        (Get-MetadataNode $manifest 'icon').InnerText -ne 'kajabity.docforms.logo.png') {
        throw 'README or icon metadata does not match the packed files.'
    }
    $repository = Get-MetadataNode $manifest 'repository'
    if ($repository.type -ne 'git' -or
        $repository.url -ne 'https://github.com/Kajabity/Kajabity.DocForms.git' -or
        $repository.commit -notmatch '^[a-f0-9]{40}$') {
        throw 'Repository URL or commit metadata is missing.'
    }

    foreach ($framework in @('net48', 'net10.0-windows7.0')) {
        Require-Entry $package "lib/$framework/Kajabity.DocForms.dll"
        Require-Entry $package "lib/$framework/Kajabity.DocForms.xml"
        Require-Entry $symbols "lib/$framework/Kajabity.DocForms.pdb"
    }
    Require-Entry $package 'README.md'
    $icon = $package.GetEntry('kajabity.docforms.logo.png')
    if ($null -eq $icon -or $icon.Length -lt 100 -or $icon.Length -gt 1048576) {
        throw 'Package icon is missing or has an unreasonable size.'
    }
    $iconStream = $icon.Open()
    try {
        $header = New-Object byte[] 8
        if ($iconStream.Read($header, 0, 8) -ne 8 -or
            [Convert]::ToHexString($header) -ne '89504E470D0A1A0A') {
            throw 'Package icon is not a PNG.'
        }
    }
    finally { $iconStream.Dispose() }

    $groups = @(Get-MetadataNode $manifest 'dependencies' | ForEach-Object { $_.ChildNodes })
    if ($groups.Count -ne 2 -or
        @($groups | Where-Object targetFramework -eq '.NETFramework4.8').Count -ne 1 -or
        @($groups | Where-Object targetFramework -eq 'net10.0-windows7.0').Count -ne 1 -or
        @($groups | Where-Object { $_.ChildNodes.Count -ne 0 }).Count -ne 0) {
        throw 'Unexpected package dependency groups or dependencies.'
    }
    $windowsForms = $manifest.SelectSingleNode("//*[local-name()='frameworkReferences']/*[local-name()='group'][@targetFramework='net10.0-windows7.0']/*[local-name()='frameworkReference'][@name='Microsoft.WindowsDesktop.App.WindowsForms']")
    if ($null -eq $windowsForms) { throw 'The .NET 10 Windows Forms framework reference is missing.' }
    if ($package.Entries.FullName -match '(^|/)(obj|bin)/|\.(csproj|snk|user|pdb|cs)$') {
        throw 'The main package contains a development file or PDB.'
    }

    foreach ($framework in @('net48', 'net10.0-windows7.0')) {
        $entry = $symbols.GetEntry("lib/$framework/Kajabity.DocForms.pdb")
        $memory = [System.IO.MemoryStream]::new()
        $stream = $entry.Open()
        try { $stream.CopyTo($memory) }
        finally { $stream.Dispose() }
        try {
            if (-not [Text.Encoding]::UTF8.GetString($memory.ToArray()).Contains('raw.githubusercontent.com/Kajabity/Kajabity.DocForms')) {
                throw "Source Link mapping is missing from $framework PDB."
            }
        }
        finally { $memory.Dispose() }
    }
    $symbolType = $symbolManifest.SelectSingleNode("//*[local-name()='packageType'][@name='SymbolsPackage']")
    if ($null -eq $symbolType) { throw 'Symbol package type is missing.' }

    Write-Host "Validated Kajabity.DocForms ${ExpectedVersion}: two assemblies, XML docs, icon, README, licence, dependencies, symbols and Source Link."
}
finally {
    $package.Dispose()
    $symbols.Dispose()
}
