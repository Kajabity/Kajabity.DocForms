$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path

# All projects use PackageReference.
dotnet restore (Join-Path $repositoryRoot 'Kajabity.DocForms.sln')
if ($LASTEXITCODE -ne 0) {
    throw 'Solution restore failed.'
}
