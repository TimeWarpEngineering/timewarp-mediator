#!/usr/bin/env pwsh

param(
    [string]$FileListPath = "files-to-modify-filtered.txt",
    [switch]$TestMode = $false,
    [int]$TestFileCount = 10
)

# Function to add attribution comment to a file
function Add-AttributionComment {
    param(
        [string]$FilePath
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Warning "File not found: $FilePath"
        return $false
    }
    
    try {
        # Read the file content
        $content = Get-Content -Path $FilePath -Raw -Encoding UTF8
        
        # Determine the appropriate comment syntax based on file type
        $extension = [System.IO.Path]::GetExtension($FilePath).ToLower()
        $fileName = [System.IO.Path]::GetFileName($FilePath).ToLower()
        
        # Determine comment style
        $commentStyle = switch -Regex ($fileName) {
            '^\.editorconfig$' { '#' }
            '^\.gitattributes$' { '#' }
            '^\.gitignore$' { '#' }
            default {
                switch ($extension) {
                    ".ps1" { '#' }
                    ".props" { 'xml' }
                    ".csproj" { 'xml' }
                    ".xml" { 'xml' }
                    ".cs" { '//' }
                    ".json" { '//' }
                    default { '//' }
                }
            }
        }
        
        # Create the attribution comment with appropriate syntax
        $attributionComment = switch ($commentStyle) {
            '#' { "# Modified by Steven T. Cramer" }
            'xml' { "<!-- Modified by Steven T. Cramer -->" }
            default { "// Modified by Steven T. Cramer" }
        }
        
        # Check if attribution comment already exists
        if ($content.Contains("Modified by Steven T. Cramer")) {
            Write-Host "Attribution already exists in: $FilePath" -ForegroundColor Yellow
            return $true
        }
        
        # Handle different file types
        switch ($extension) {
            ".csproj" {
                # For XML project files, add after XML declaration if present
                if ($content.StartsWith("<?xml")) {
                    $xmlDeclarationEnd = $content.IndexOf("?>") + 2
                    $beforeXml = $content.Substring(0, $xmlDeclarationEnd)
                    $afterXml = $content.Substring($xmlDeclarationEnd)
                    $newContent = $beforeXml + "`n" + $attributionComment + $afterXml
                } else {
                    $newContent = $attributionComment + "`n" + $content
                }
            }
            ".props" {
                # For XML props files, add as second line after <Project> tag
                if ($content.StartsWith("<Project>")) {
                    $newContent = "<Project>`n  " + $attributionComment + $content.Substring(9)
                } else {
                    $newContent = $attributionComment + "`n" + $content
                }
            }
            ".cs" {
                # For C# files, add as first line
                $newContent = $attributionComment + "`n" + $content
            }
            ".ps1" {
                # For PowerShell files, add after shebang if present
                if ($content.StartsWith("#!")) {
                    $firstLineEnd = $content.IndexOf("`n")
                    if ($firstLineEnd -eq -1) {
                        $newContent = $content + "`n" + $attributionComment
                    } else {
                        $shebang = $content.Substring(0, $firstLineEnd + 1)
                        $afterShebang = $content.Substring($firstLineEnd + 1)
                        $newContent = $shebang + $attributionComment + "`n" + $afterShebang
                    }
                } else {
                    $newContent = $attributionComment + "`n" + $content
                }
            }
            default {
                # For all other text files, add as first line
                $newContent = $attributionComment + "`n" + $content
            }
        }
        
        # Write the modified content back
        Set-Content -Path $FilePath -Value $newContent -Encoding UTF8 -NoNewline
        Write-Host "✓ Added attribution to: $FilePath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Error "Failed to process $FilePath : $_"
        return $false
    }
}

# Main script
$files = Get-Content -Path $FileListPath

if ($TestMode) {
    Write-Host "`nRunning in TEST MODE - Processing first $TestFileCount files only" -ForegroundColor Cyan
    $files = $files | Select-Object -First $TestFileCount
}

$totalFiles = $files.Count
$successCount = 0
$failureCount = 0
$skippedCount = 0

Write-Host "`nProcessing $totalFiles files..." -ForegroundColor Green

foreach ($file in $files) {
    $result = Add-AttributionComment -FilePath $file
    if ($result -eq $true) {
        $successCount++
    } elseif ($result -eq $false) {
        $failureCount++
    } else {
        $skippedCount++
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Total files: $totalFiles"
Write-Host "Successfully modified: $successCount" -ForegroundColor Green
Write-Host "Failed: $failureCount" -ForegroundColor Red
Write-Host "Skipped (already has attribution): $skippedCount" -ForegroundColor Yellow