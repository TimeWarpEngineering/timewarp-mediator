# 🔄 AUTO-SYNCED FILE - DO NOT EDIT DIRECTLY
# Source: TimeWarpEngineering/timewarp-flow/Tools/GitHooks/PreCommit.ps1
# This file is automatically synced from the master repository
# To modify: Edit the source file in timewarp-flow and sync will update all repos

$branch = & git rev-parse --abbrev-ref HEAD

if ($branch -eq "master") {
    Write-Host "❌ You can't commit directly to the master branch." -ForegroundColor Red
    Write-Host "💡 Please create a feature branch and submit a pull request." -ForegroundColor Yellow
    exit 1
}