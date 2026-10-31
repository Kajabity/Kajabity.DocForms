$ErrorActionPreference = 'Stop'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$reportDirectory = Join-Path $repositoryRoot 'vulnerability-reports'
New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null

Push-Location $repositoryRoot
try {
    $trackedFiles = @(git ls-files --cached)
    if ($LASTEXITCODE -ne 0) {
        throw 'Could not list tracked files for the NuGet audit.'
    }

    $legacyManifests = @($trackedFiles | Where-Object { $_ -match '(^|/)packages\.config$' })
    if ($legacyManifests.Count -gt 0) {
        throw "Legacy packages.config files are not audited: $($legacyManifests -join ', '). Migrate them to PackageReference first."
    }

    $projects = @($trackedFiles | Where-Object { $_ -match '\.csproj$' })
    if ($projects.Count -eq 0) {
        throw 'No tracked projects were found to audit.'
    }

    function Test-NodeForVulnerabilities {
        param($Node)

        if ($null -eq $Node -or $Node -is [string] -or $Node -is [ValueType]) {
            return $false
        }

        if ($Node -is [System.Collections.IEnumerable]) {
            foreach ($item in $Node) {
                if (Test-NodeForVulnerabilities $item) { return $true }
            }
            return $false
        }

        foreach ($property in $Node.PSObject.Properties) {
            if ($property.Name -eq 'vulnerabilities' -and $property.Value -and $property.Value.Count -gt 0) {
                return $true
            }
            if (Test-NodeForVulnerabilities $property.Value) { return $true }
        }

        return $false
    }

    $vulnerabilitiesFound = $false
    foreach ($project in $projects) {
        $projectPath = (Resolve-Path -LiteralPath (Join-Path $repositoryRoot $project)).Path

        # Force a fresh restore so NuGet audits every target framework and reports
        # unavailable or unsupported vulnerability feeds as errors.
        dotnet restore $projectPath --force `
            '-p:NuGetAudit=true' '-p:NuGetAuditMode=all' '-p:NuGetAuditLevel=low' `
            '-p:WarningsAsErrors=NU1900%3BNU1905'
        if ($LASTEXITCODE -ne 0) {
            throw "NuGet restore or vulnerability-feed audit failed for $project."
        }

        $output = dotnet list $projectPath package --vulnerable --include-transitive --format json --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "NuGet vulnerability report failed for $project."
        }

        $report = ($output -join "`n") | ConvertFrom-Json
        $reportedProjects = @($report.projects | Where-Object { $null -ne $_ })
        $reportedSources = @($report.sources | Where-Object { $null -ne $_ })
        if ($reportedProjects.Count -ne 1 -or
            [string]::IsNullOrWhiteSpace($reportedProjects[0].path) -or
            [IO.Path]::GetFullPath($reportedProjects[0].path) -ne [IO.Path]::GetFullPath($projectPath) -or
            $reportedSources.Count -eq 0) {
            throw "NuGet returned an incomplete vulnerability report for $project."
        }

        $reportName = ($project -replace '[\\/]', '_') -replace '\.csproj$', '.json'
        $report | ConvertTo-Json -Depth 100 |
            Set-Content -LiteralPath (Join-Path $reportDirectory $reportName) -Encoding utf8
        Write-Host "Audited $project."

        if (Test-NodeForVulnerabilities $report) {
            $vulnerabilitiesFound = $true
        }
    }

    if ($vulnerabilitiesFound) {
        throw 'Vulnerable NuGet dependencies were found. See the uploaded reports.'
    }

    Write-Host "No known vulnerabilities found in $($projects.Count) audited projects."
}
finally {
    Pop-Location
}
