# update-packages.ps1
param (
    [switch]$Preview
)

$webApiPath = "..\..\"  # adjust path as needed
Push-Location $webApiPath

function Update-ProjectPackages {
    param (
        [string]$ProjectPath,
        [bool]$IncludePrerelease
    )
    
    Write-Host "`nUpdating packages for project: $ProjectPath" -ForegroundColor Cyan
    
    # Get all packages
    $packages = dotnet list $ProjectPath package | Select-String -Pattern "^      > "
    
    foreach ($package in $packages) {
        $packageName = ($package -split ">")[1].Trim().Split(" ")[0]
        
        Write-Host "Updating $packageName..." -ForegroundColor Yellow
        
        if ($IncludePrerelease) {
            dotnet add $ProjectPath package $packageName --prerelease
        }
        else {
            dotnet add $ProjectPath package $packageName
        }
    }
}

# Get all .csproj files
$projects = Get-ChildItem -Recurse -Filter *.csproj

foreach ($project in $projects) {
    Update-ProjectPackages -ProjectPath $project.FullName -IncludePrerelease $Preview.IsPresent
}

Write-Host "`nAll packages have been updated!" -ForegroundColor Green