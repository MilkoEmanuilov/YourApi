# migration.ps1
$webApiPath = "..\..\"  # adjust path as needed
Push-Location $webApiPath

function Get-ProjectNames {
    # Find the first .sln file in the current directory
    $solutionFile = Get-ChildItem -Filter "*.sln" | Select-Object -First 1
    if (-not $solutionFile) {
        Write-Host "No solution file found in the current directory!" -ForegroundColor Red
        exit 1
    }

    # Find Infrastructure and API projects
    $projects = Get-ChildItem -Recurse -Filter "*.csproj"
    $infrastructureProject = $projects | Where-Object { $_.Name -like "*Infrastructure.csproj" } | Select-Object -First 1
    $apiProject = $projects | Where-Object { $_.Name -like "*Api.csproj" -or $_.Name -like "*API.csproj" } | Select-Object -First 1

    if (-not $infrastructureProject -or -not $apiProject) {
        Write-Host "Could not find Infrastructure or API projects!" -ForegroundColor Red
        exit 1
    }

    return @{
        InfrastructureProject = $infrastructureProject.BaseName
        WebApiProject         = $apiProject.BaseName
        InfrastructurePath    = $infrastructureProject.Directory.FullName
        WebApiPath            = $apiProject.Directory.FullName
    }
}

$projects = Get-ProjectNames
$infrastructureProject = $projects.InfrastructureProject
$webApiProject = $projects.WebApiProject
$startupProjectPath = $projects.WebApiPath
$infrastructureProjectPath = $projects.InfrastructurePath

Write-Host "Found projects:" -ForegroundColor Cyan
Write-Host "Infrastructure: $infrastructureProject" -ForegroundColor Gray
Write-Host "API: $webApiProject" -ForegroundColor Gray
Write-Host

function Show-Menu {
    Clear-Host
    Write-Host "=== Database Migration Tool ===" -ForegroundColor Cyan
    Write-Host "1: Add new migration" -ForegroundColor Green
    Write-Host "2: List all migrations" -ForegroundColor Green
    Write-Host "3: Revert last migration" -ForegroundColor Yellow
    Write-Host "4: Remove specific migration" -ForegroundColor Yellow
    Write-Host "5: Exit" -ForegroundColor Red
    Write-Host
}

function Test-DatabaseMigrations {
    try {
        $hasMigrations = dotnet ef migrations list --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
        return $hasMigrations -ne $null -and $hasMigrations -ne ""
    }
    catch {
        return $false
    }
}

function Add-NewMigration {
    $migrationName = Read-Host "Enter migration name"
    if ([string]::IsNullOrWhiteSpace($migrationName)) {
        Write-Host "Migration name cannot be empty!" -ForegroundColor Red
        return
    }
    
    Write-Host "Adding new migration: $migrationName" -ForegroundColor Cyan
    dotnet ef migrations add $migrationName --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration $migrationName added successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "Failed to add migration $migrationName" -ForegroundColor Red
    }
    Pause
}

function List-Migrations {
    if (-not (Test-DatabaseMigrations)) {
        Write-Host "No migrations exist. Add your first migration!" -ForegroundColor Yellow
        Pause
        return
    }
    Write-Host "Listing all migrations:" -ForegroundColor Cyan
    dotnet ef migrations list --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
    Pause
}

function Remove-LastMigration {
    if (-not (Test-DatabaseMigrations)) {
        Write-Host "No migrations to revert." -ForegroundColor Yellow
        Pause
        return
    }

    Write-Host "Checking current migration..." -ForegroundColor Yellow
    $migrations = dotnet ef migrations list --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
    $previousMigration = $migrations | 
    Select-String "\[\s\]" | 
    Select-Object -Last 1 | 
    ForEach-Object { $_.Line.Trim().Split(' ')[0] }

    if ($previousMigration) {
        Write-Host "Reverting to previous migration: $previousMigration" -ForegroundColor Yellow
        dotnet ef database update $previousMigration --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to revert database changes!" -ForegroundColor Red
            Pause
            return
        }
    }

    Write-Host "Removing migration files..." -ForegroundColor Yellow
    dotnet ef migrations remove --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Last migration removed successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "Failed to remove migration files" -ForegroundColor Red
    }
    Pause
}

function Remove-SpecificMigration {
    if (-not (Test-DatabaseMigrations)) {
        Write-Host "No migrations to remove." -ForegroundColor Yellow
        Pause
        return
    }
    
    Write-Host "Current migrations:" -ForegroundColor Cyan
    dotnet ef migrations list --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
    
    $migrationName = Read-Host "Enter migration name to remove"
    if ([string]::IsNullOrWhiteSpace($migrationName)) {
        Write-Host "Migration name cannot be empty!" -ForegroundColor Red
        return
    }
    
    Write-Host "Removing migration $migrationName..." -ForegroundColor Yellow
    dotnet ef database update $migrationName --project $infrastructureProjectPath --startup-project $startupProjectPath --no-build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration $migrationName removed successfully!" -ForegroundColor Green
    }
    else {
        Write-Host "Failed to remove migration $migrationName" -ForegroundColor Red
    }
    Pause
}

# Build solution first
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Please fix the build errors and try again." -ForegroundColor Red
    exit 1
}

do {
    Show-Menu
    $selection = Read-Host "Enter your choice"
    
    switch ($selection) {
        '1' { Add-NewMigration }
        '2' { List-Migrations }
        '3' { Remove-LastMigration }
        '4' { Remove-SpecificMigration }
        '5' { return }
        default { 
            Write-Host "Invalid selection. Please try again." -ForegroundColor Red 
            Pause
        }
    }
} while ($true)

function Pause {
    Write-Host "`nPress any key to continue..."
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
}