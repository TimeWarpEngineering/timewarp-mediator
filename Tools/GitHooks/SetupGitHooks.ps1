# 🔄 AUTO-SYNCED FILE - DO NOT EDIT DIRECTLY
# Source: TimeWarpEngineering/timewarp-flow/Tools/GitHooks/SetupGitHooks.ps1
# This file is automatically synced from the master repository
# To modify: Edit the source file in timewarp-flow and sync will update all repos

# Git Hooks Setup Script
# This script installs git hooks that prevent direct commits to the master branch
# Run this script once in each repository to set up the hooks

Write-Host "🔧 Setting up git hooks..." -ForegroundColor Cyan

# Check if we're in a git repository
if (-not (& git rev-parse --is-inside-work-tree 2>$null)) {
    Write-Error "❌ This script must be run inside a git repository"
    exit 1
}

# Check if this is a worktree (not the main repository)
if (Test-Path ".git" -PathType Leaf) {
    Write-Error "❌ This appears to be a git worktree. Please run this script from the main repository location."
    Write-Host "💡 You can find the main repository with: git rev-parse --show-superproject-working-tree" -ForegroundColor Yellow
    exit 1
}

# Ensure we're at the repository root with a .git directory
if (-not (Test-Path ".git" -PathType Container)) {
    $gitRoot = & git rev-parse --show-toplevel
    Write-Host "📂 Changing to repository root: $gitRoot" -ForegroundColor Yellow
    Set-Location $gitRoot
    
    if (-not (Test-Path ".git" -PathType Container)) {
        Write-Error "❌ Could not find .git directory at repository root"
        exit 1
    }
}

# Create .git/hooks directory if it doesn't exist
$hooksDir = ".git/hooks"
if (-not (Test-Path $hooksDir)) {
    New-Item -ItemType Directory -Path $hooksDir -Force | Out-Null
}

# Check if source hook files exist
$sourcePreCommit = "Tools/GitHooks/pre-commit"
$sourcePreCommitPs1 = "Tools/GitHooks/PreCommit.ps1"

if (-not (Test-Path $sourcePreCommit)) {
    Write-Error "❌ Source file not found: $sourcePreCommit"
    Write-Host "💡 Make sure the repository has been synced with the latest configuration files" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $sourcePreCommitPs1)) {
    Write-Error "❌ Source file not found: $sourcePreCommitPs1"
    Write-Host "💡 Make sure the repository has been synced with the latest configuration files" -ForegroundColor Yellow
    exit 1
}

# Copy hook files to .git/hooks
try {
    Copy-Item $sourcePreCommit "$hooksDir/pre-commit" -Force
    Copy-Item $sourcePreCommitPs1 "$hooksDir/PreCommit.ps1" -Force
    
    # Make pre-commit executable on Unix-like systems
    if ($IsLinux -or $IsMacOS) {
        chmod +x "$hooksDir/pre-commit"
    }
    
    Write-Host "✅ Git hooks installed successfully!" -ForegroundColor Green
    Write-Host "📋 The following hooks are now active:" -ForegroundColor Cyan
    Write-Host "   • pre-commit: Prevents direct commits to master branch" -ForegroundColor Gray
    Write-Host ""
    Write-Host "💡 To disable these hooks temporarily, use: git commit --no-verify" -ForegroundColor Yellow
    
} catch {
    Write-Error "❌ Failed to install git hooks: $_"
    exit 1
}