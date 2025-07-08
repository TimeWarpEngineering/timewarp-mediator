#!/usr/bin/env pwsh

# Create mapping from original MediatR files to current TimeWarp.Mediator files
$originalFiles = Get-Content -Path "original-fork-files.txt"
$mappedFiles = @()

foreach ($originalFile in $originalFiles) {
    # Replace MediatR with TimeWarp.Mediator in the path
    $mappedFile = $originalFile -replace 'MediatR', 'TimeWarp.Mediator'
    
    # Check if the mapped file exists
    if (Test-Path $mappedFile) {
        $mappedFiles += [PSCustomObject]@{
            OriginalPath = $originalFile
            CurrentPath = $mappedFile
            Exists = $true
        }
    } else {
        # File might have been moved or deleted
        $mappedFiles += [PSCustomObject]@{
            OriginalPath = $originalFile
            CurrentPath = $mappedFile
            Exists = $false
        }
    }
}

# Export the mapping to CSV for review
$mappedFiles | Export-Csv -Path "file-mapping.csv" -NoTypeInformation

# Show summary
$existingFiles = $mappedFiles | Where-Object { $_.Exists -eq $true }
$missingFiles = $mappedFiles | Where-Object { $_.Exists -eq $false }

Write-Host "File Mapping Summary:" -ForegroundColor Green
Write-Host "Total original files: $($originalFiles.Count)"
Write-Host "Files found in current repo: $($existingFiles.Count)"
Write-Host "Files not found: $($missingFiles.Count)"

if ($missingFiles.Count -gt 0) {
    Write-Host "`nMissing files:" -ForegroundColor Yellow
    $missingFiles | ForEach-Object { Write-Host "  - $($_.OriginalPath)" }
}

# Export list of existing files for the attribution script
$existingFiles | Select-Object -ExpandProperty CurrentPath | Out-File -FilePath "files-to-modify.txt" -Encoding utf8