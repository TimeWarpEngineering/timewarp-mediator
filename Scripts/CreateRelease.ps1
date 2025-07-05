<#
.SYNOPSIS
    Automates Git tags and GitHub release for stable versions of TimeWarp.Mediator and TimeWarp.Mediator.Contracts.

.DESCRIPTION
    Reads versions from .csproj files, checks for new commits, increments versions for stable releases based on PR labels
    or commit messages (falling back to user input), updates .csproj files, creates tags, and creates a GitHub release with
    custom title and body. Warns if run during alpha/beta phase, as CI handles prereleases.

.PARAMETER Commit
    The commit to tag (e.g., HEAD or a commit SHA). Default: HEAD.

.PARAMETER ReleaseTitle
    The title for the GitHub release (optional, defaults to "Release Mediator vX.Y.Z and Contracts vA.B.C").

.PARAMETER ReleaseBody
    The body/notes for the GitHub release (optional, defaults to a summary of versions and changes).

.EXAMPLE
    .\create-release.ps1 -Commit HEAD -ReleaseTitle "Release v14.0.1 and v3.0.1" -ReleaseBody "New features and fixes."
    # Creates stable release with incremented versions
#>

param (
    [string]$Commit = "HEAD",
    [string]$ReleaseTitle,
    [string]$ReleaseBody
)

# Check if gh CLI is installed
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Error "GitHub CLI (gh) is not installed. Please install it: https://cli.github.com/"
    exit 1
}

# Check if git is installed
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Error "Git is not installed. Please install it."
    exit 1
}

# Paths to .csproj files
$mediatorCsproj = "./src/TimeWarp.Mediator/TimeWarp.Mediator.csproj"
$contractsCsproj = "./src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj"

# Function to extract <Version> from .csproj
function Get-CsprojVersion {
    param ([string]$CsprojPath)
    if (-not (Test-Path $CsprojPath)) {
        Write-Error "Could not find .csproj file at $CsprojPath"
        exit 1
    }
    $xml = [xml](Get-Content $CsprojPath)
    $version = $xml.Project.PropertyGroup.Version
    if (-not $version) {
        Write-Error "No <Version> tag found in $CsprojPath"
        exit 1
    }
    return $version
}

# Function to check if a tag exists
function Test-TagExists {
    param ([string]$Tag)
    $exists = git tag -l | Select-String "^$Tag$" -Quiet
    return $exists
}

# Function to check if a package version is published
function Test-PackagePublished {
    param ([string]$PackageId, [string]$Version, [string]$Source)
    try {
        $output = & dotnet nuget list id $PackageId --source $Source --non-interactive --prerelease 2>&1
        return $output -like "*$PackageId $Version*"
    }
    catch {
        Write-Warning "Failed to check if $PackageId $Version is published: $_"
        return $false
    }
}

# Function to get commits since last tag
function Get-CommitsSinceTag {
    param ([string]$TagPrefix)
    $latestTag = git tag -l "$TagPrefix*" | Sort-Object -Descending | Select-Object -First 1
    if (-not $latestTag) {
        return git log --oneline $Commit
    }
    return git log "$latestTag..$Commit" --oneline
}

# Function to determine version increment
function Get-VersionIncrement {
    param ([string]$Commits, [string]$PackageName)
    
    # Check for conventional commits
    if ($Commits -match '^feat:|^feat\(.+\):') {
        return "minor"
    }
    elseif ($Commits -match '^fix:|^fix\(.+\):') {
        return "patch"
    }
    elseif ($Commits -match 'BREAKING CHANGE:') {
        return "major"
    }

    # Check PR labels
    try {
        $prInfo = gh pr list --state merged --head $Commit --json labels | ConvertFrom-Json
        $labels = $prInfo.labels.name
        if ($labels -contains "major") {
            return "major"
        }
        elseif ($labels -contains "minor") {
            return "minor"
        }
        elseif ($labels -contains "patch") {
            return "patch"
        }
        elseif ($labels -contains "alpha" -or $labels -contains "beta") {
            Write-Warning "$PackageName appears to be in alpha/beta phase (PR labels: $labels). Use CI for prerelease builds."
            return "none"
        }
    }
    catch {
        Write-Warning "Failed to fetch PR labels: $_"
    }

    # Prompt user
    Write-Host "No clear version increment found for $PackageName. Commits:"
    Write-Host $Commits
    $increment = Read-Host "Enter version increment for $PackageName (major/minor/patch, or 'none' for alpha/beta)"
    while ($increment -notin @("major", "minor", "patch", "none")) {
        Write-Host "Invalid input. Please enter 'major', 'minor', 'patch', or 'none'."
        $increment = Read-Host "Enter version increment for $PackageName (major/minor/patch, or 'none' for alpha/beta)"
    }
    return $increment
}

# Function to increment version
function Increment-Version {
    param ([string]$Version, [string]$Increment)
    $parts = $Version -split '\.'
    $major, $minor, $patch = [int]$parts[0], [int]$parts[1], [int]$parts[2]
    switch ($Increment) {
        "major" { $major++; $minor = 0; $patch = 0 }
        "minor" { $minor++; $patch = 0 }
        "patch" { $patch++ }
    }
    return "$major.$minor.$patch"
}

# Function to update <Version> in .csproj
function Update-CsprojVersion {
    param ([string]$CsprojPath, [string]$NewVersion)
    $xml = [xml](Get-Content $CsprojPath)
    $xml.Project.PropertyGroup.Version = $NewVersion
    $xml.Save($CsprojPath)
}

# Get current versions
$mediatorVersion = Get-CsprojVersion $mediatorCsproj
$contractsVersion = Get-CsprojVersion $contractsCsproj

# Validate SemVer format
$semverRegex = '^\d+\.\d+\.\d+
if ($mediatorVersion -notmatch $semverRegex -or $contractsVersion -notmatch $semverRegex) {
    Write-Error "Versions in .csproj must be in SemVer format (e.g., 14.0.0, 3.0.0)"
    exit 1
}

# Check if versions need incrementing
$mediatorTag = "mediator/v$mediatorVersion"
$contractsTag = "contracts/v$contractsVersion"
$needsRelease = $false

# Check Mediator
$mediatorCommits = Get-CommitsSinceTag "mediator/v"
if ($mediatorCommits) {
    $increment = Get-VersionIncrement $mediatorCommits "TimeWarp.Mediator"
    if ($increment -eq "none") {
        Write-Host "Skipping stable release for TimeWarp.Mediator; CI will handle prerelease builds."
    }
    elseif ((Test-TagExists $mediatorTag) -and (Test-PackagePublished "TimeWarp.Mediator" $mediatorVersion "https://api.nuget.org/v3/index.json")) {
        $mediatorVersion = Increment-Version $mediatorVersion $increment
        $mediatorTag = "mediator/v$mediatorVersion"
        Update-CsprojVersion $mediatorCsproj $mediatorVersion
        $needsRelease = $true
        Write-Host "Incremented TimeWarp.Mediator to $mediatorVersion ($increment)"
    }
}

# Check Contracts
$contractsCommits = Get-CommitsSinceTag "contracts/v"
if ($contractsCommits) {
    $increment = Get-VersionIncrement $contractsCommits "TimeWarp.Mediator.Contracts"
    if ($increment -eq "none") {
        Write-Host "Skipping stable release for TimeWarp.Mediator.Contracts; CI will handle prerelease builds."
    }
    elseif ((Test-TagExists $contractsTag) -and (Test-PackagePublished "TimeWarp.Mediator.Contracts" $contractsVersion "https://api.nuget.org/v3/index.json")) {
        $contractsVersion = Increment-Version $contractsVersion $increment
        $contractsTag = "contracts/v$contractsVersion"
        Update-CsprojVersion $contractsCsproj $contractsVersion
        $needsRelease = $true
        Write-Host "Incremented TimeWarp.Mediator.Contracts to $contractsVersion ($increment)"
    }
}

# Update dependency range in TimeWarp.Mediator if Contracts version changed
if ($needsRelease -and $contractsCommits -and $increment -ne "none") {
    $xml = [xml](Get-Content $mediatorCsproj)
    $packageRef = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "TimeWarp.Mediator.Contracts" }
    if ($packageRef) {
        $newMajor = [int]($contractsVersion -split '\.')[0]
        $packageRef.Version = "[$contractsVersion,$($newMajor + 1).0.0)"
        $xml.Save($mediatorCsproj)
        Write-Host "Updated TimeWarp.Mediator dependency to [$contractsVersion,$($newMajor + 1).0.0)"
    }
}

# Commit .csproj changes
if ($needsRelease) {
    git add $mediatorCsproj $contractsCsproj
    git commit -m "Update .csproj versions to $mediatorVersion and $contractsVersion" --no-verify
    git push origin
}

# Create and push tags
if ($needsRelease) {
    Write-Host "Creating tags: $mediatorTag, $contractsTag on commit $Commit"
    try {
        git tag $mediatorTag $Commit
        git tag $contractsTag $Commit
        git push origin $mediatorTag $contractsTag
    }
    catch {
        Write-Error "Failed to create or push tags: $_"
        exit 1
    }
}
else {
    Write-Host "No new commits requiring a stable release. CI will handle prerelease builds with -alpha.<commit-height>."
    exit 0
}

# Set default release title and body
if (-not $ReleaseTitle) {
    $ReleaseTitle = "Release Mediator v$mediatorVersion and Contracts v$contractsVersion"
}
if (-not $ReleaseBody) {
    $ReleaseBody = "Released TimeWarp.Mediator v$mediatorVersion and TimeWarp.Mediator.Contracts v$contractsVersion`n`nChanges:`n- Mediator:`n$($mediatorCommits -replace '^', '  - ')`n- Contracts:`n$($contractsCommits -replace '^', '  - ')"
}

Write-Host "Creating GitHub release with tag: $mediatorTag"

# Create GitHub release
try {
    gh release create $mediatorTag `
        --title $ReleaseTitle `
        --notes $ReleaseBody `
        --target $Commit
}
catch {
    Write-Error "Failed to create GitHub release: $_"
    exit 1
}

Write-Host "Release created successfully. Workflow should now trigger to publish packages."