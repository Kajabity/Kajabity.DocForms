$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$packagesDirectory = Join-Path $repositoryRoot 'packages'

# Solution restore ignores packages.config in projects that also use PackageReference.
# Restore these manifests explicitly so a clean checkout has the legacy assemblies
# and the NUnit adapter in the repository's packages directory.
$packageManifests = @(
    'Kajabity.DocForms.Test/packages.config'
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
