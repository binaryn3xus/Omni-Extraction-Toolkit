param(
    [Alias("b")]
    [switch]$Build
)

# Omni-Extraction Toolkit Packaging Script
# This script creates a Thunderstore-ready .zip file

$projectDir = "OmniExtractionToolkit"
$projectFile = "$projectDir/OmniExtractionToolkit.csproj"
$buildDir = "$projectDir/bin/Debug/net48"
$distDir = "dist"
$manifestPath = "$projectDir/manifest.json"

# 1. Build if requested
if ($Build.IsPresent) {
    Write-Host "--- Building Project ---" -ForegroundColor Yellow
    dotnet build $projectFile
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed! Aborting packaging."
        exit $LASTEXITCODE
    }
} else {
    Write-Host "--- Skipping Build (Use -Build to compile) ---" -ForegroundColor Gray
}

# 2. Read version from manifest.json
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath | ConvertFrom-Json
    $version = $manifest.version_number
    $modName = $manifest.name
} else {
    Write-Error "manifest.json not found!"
    exit 1
}

$zipName = "${modName}_v${version}.zip"

Write-Host "--- Packaging $modName v$version ---" -ForegroundColor Cyan

# 3. Clean and create staging directory
if (Test-Path $distDir) { Remove-Item $distDir -Recurse -Force }
New-Item -ItemType Directory -Path "$distDir/plugins" | Out-Null

# 4. Copy files
Write-Host "Copying assets..."
Copy-Item "$projectDir/manifest.json" -Destination $distDir
Copy-Item "$projectDir/README.md" -Destination $distDir
if (Test-Path "$projectDir/icon.png") {
    Copy-Item "$projectDir/icon.png" -Destination $distDir
} else {
    Write-Warning "icon.png not found in $projectDir!"
}

Write-Host "Copying DLL..."
$dllPath = "$buildDir/OmniExtractionToolkit.dll"
if (Test-Path $dllPath) {
    Copy-Item $dllPath -Destination "$distDir/plugins"
} else {
    Write-Error "DLL not found at $dllPath! Did you build the project?"
    exit 1
}

# 5. Create ZIP
if (Test-Path $zipName) { Remove-Item $zipName -Force }
Write-Host "Creating $zipName..." -ForegroundColor Green
Compress-Archive -Path "$distDir/*" -DestinationPath $zipName

# 6. Cleanup
Remove-Item $distDir -Recurse -Force

Write-Host "--- Packaging Complete! ---" -ForegroundColor Cyan
Write-Host "Your mod is ready for upload: $zipName"
