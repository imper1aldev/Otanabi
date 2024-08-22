Write-Host "Updating..."

# 1) Close Animewatcher.exe if it's running
$processName = "Animewatcher"
$process = Get-Process -Name $processName -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Name $processName -Force
    Write-Host "Closed Animewatcher.exe"
} else {
    Write-Host "Animewatcher.exe is not running"
}

# 2) Delete all items and folders excluding update.ps1
Write-Host "Cleaning installation folder"

Start-Sleep -Seconds 4

$excludeFile = "update.ps1"

$destinationPath = (Get-Location).Path

$itemsToDelete = Get-ChildItem -Path $destinationPath -Recurse | Where-Object { $_.Name -ne $excludeFile }

foreach ($item in $itemsToDelete) {
    if ($item.PSIsContainer) {
        if (Test-Path $item.FullName) {
            Remove-Item -Path $item.FullName -Recurse -Force
        }
    } else {
        if (Test-Path $item.FullName) {
            Remove-Item -Path $item.FullName -Force
        }
    }
}

Write-Host "Cleaning Completed"
Start-Sleep -Seconds 2.5
# 3) Extract the zip file located in the Windows temporary folder to the current path
$zipFilePath = "$env:TEMP\animeupdate.zip"


if (Test-Path $zipFilePath) {
    Expand-Archive -Path $zipFilePath -DestinationPath $destinationPath -Force
    Write-Host "Extracted animeupdate.zip to $destinationPath"
} else {
    Write-Host "animeupdate.zip not found in $env:TEMP"
}

# 4) Remove the update zip file
Write-Host "Cleaning downloaded file... "
if (Test-Path $zipFilePath) {
    Remove-Item -Path $zipFilePath -Force
    Write-Host "Cleaning Completed"
}
Start-Sleep -Seconds 2.5
# 5) Open Animewatcher.exe in the current path
Write-Host "Opening Animewatcher...."
$animeWatcherPath = "$destinationPath\AnimeWatcher.exe"

if (Test-Path $animeWatcherPath) {
    Start-Process -FilePath $animeWatcherPath
    Write-Host "Opened Animewatcher.exe"
} else {
    Write-Host "Animewatcher.exe not found in $destinationPath"
}

exit