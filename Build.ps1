# Modified by Steven T. Cramer
# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

$Solution = "timewarp-mediator.slnx"
$Artifacts = ".\Artifacts"

if(Test-Path $Artifacts) { Remove-Item $Artifacts -Force -Recurse }

exec { & dotnet clean $Solution -c Release }

exec { & dotnet build $Solution -c Release }

exec { & dotnet test $Solution -c Release --no-build -l trx --verbosity=normal }

exec { & dotnet pack .\src\TimeWarp.Mediator\TimeWarp.Mediator.csproj -c Release -o $Artifacts --no-build }

exec { & dotnet pack .\src\TimeWarp.Mediator.Contracts\TimeWarp.Mediator.Contracts.csproj -c Release -o $Artifacts --no-build }

exec { & dotnet pack .\src\TimeWarp.Mediator.Analyzers\TimeWarp.Mediator.Analyzers.csproj -c Release -o $Artifacts --no-build }

exec { & dotnet pack .\src\TimeWarp.Mediator.Generators\TimeWarp.Mediator.Generators.csproj -c Release -o $Artifacts --no-build }

function Assert-NupkgContains
{
    param(
        [Parameter(Mandatory = $true)][string]$PackageId,
        [Parameter(Mandatory = $true)][string]$Entry
    )

    $nupkg = Get-ChildItem -Path $Artifacts -Filter "$PackageId.*.nupkg" |
        Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
        Select-Object -First 1
    if (-not $nupkg)
    {
        throw "No nupkg for $PackageId under $Artifacts"
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
    try
    {
        $found = $zip.Entries | Where-Object { $_.FullName -eq $Entry }
        if (-not $found)
        {
            $names = ($zip.Entries | ForEach-Object { $_.FullName }) -join "`n  "
            throw "$($nupkg.Name) is missing '$Entry'. Entries:`n  $names"
        }
    }
    finally
    {
        $zip.Dispose()
    }
}

Assert-NupkgContains "TimeWarp.Mediator.Analyzers" "analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll"
Assert-NupkgContains "TimeWarp.Mediator.Generators" "analyzers/dotnet/cs/TimeWarp.Mediator.Generators.dll"
Assert-NupkgContains "TimeWarp.Mediator.Generators" "analyzers/dotnet/cs/TimeWarp.Mediator.Analyzers.dll"

