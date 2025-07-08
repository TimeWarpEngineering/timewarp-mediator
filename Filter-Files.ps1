#!/usr/bin/env pwsh

# Read the original file list
$files = Get-Content -Path "files-to-modify.txt"

# Filter out the files we don't want to modify
$filteredFiles = $files | Where-Object {
    $_ -notmatch '\.sln$' -and          # Exclude .sln files
    $_ -notmatch '\.snk$' -and          # Exclude .snk files
    $_ -notmatch 'NuGet\.Config$' -and  # Exclude NuGet.Config
    $_ -notmatch 'README\.md$'          # Exclude README.md
}

# Save the filtered list
$filteredFiles | Out-File -FilePath "files-to-modify-filtered.txt" -Encoding utf8

Write-Host "Original file count: $($files.Count)"
Write-Host "Filtered file count: $($filteredFiles.Count)"
Write-Host "Files excluded: $($files.Count - $filteredFiles.Count)"