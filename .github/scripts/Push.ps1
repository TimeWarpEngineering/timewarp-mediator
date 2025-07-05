$scriptName = $MyInvocation.MyCommand.Name
$Artifacts = "./Artifacts"

if ([string]::IsNullOrEmpty($Env:NUGET_API_KEY)) {
    Write-Host "${scriptName}: NUGET_API_KEY is empty or not set. Skipped pushing package(s)."
} else {
    Get-ChildItem $Artifacts -Filter "*.nupkg" | ForEach-Object {
        Write-Host "$($scriptName): Pushing $($_.Name)"
        dotnet nuget push $_ --source $Env:NUGET_URL --api-key $Env:NUGET_API_KEY
        if ($lastexitcode -ne 0) {
            throw ("Exec: " + $errorMessage)
        }
    }
}
