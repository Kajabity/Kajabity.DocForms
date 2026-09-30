$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$packagesDirectory = Join-Path $repositoryRoot 'packages'

# Restore the sample projects that still use packages.config explicitly.
$packageManifests = @(
    'Samples/CsvEditor/packages.config'
    'Samples/JavaPropertiesEditor/packages.config'
)

foreach ($manifest in $packageManifests) {
    nuget restore (Join-Path $repositoryRoot $manifest) -PackagesDirectory $packagesDirectory -NonInteractive
    if ($LASTEXITCODE -ne 0) {
        throw "NuGet restore failed for $manifest."
    }
}

# Restore PackageReference dependencies and generate the MSBuild assets files.
nuget restore (Join-Path $repositoryRoot 'Kajabity.DocForms.sln') -NonInteractive
if ($LASTEXITCODE -ne 0) {
    throw 'NuGet solution restore failed.'
}
