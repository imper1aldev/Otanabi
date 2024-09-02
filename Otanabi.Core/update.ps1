Write-Host "Updating..."

# 1) Close Otanabi.exe if it's running
$processName = "Otanabi"
$process = Get-Process -Name $processName -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Name $processName -Force
    Write-Host "Closed Otanabi.exe"
} else {
    Write-Host "Otanabi.exe is not running"
}

# 2) Delete all items and folders excluding update.ps1
Write-Host "Cleaning installation folder"

Start-Sleep -Seconds 4

# 1) recheck if is running 
$processName = "Otanabi"
$process = Get-Process -Name $processName -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Name $processName -Force
    Write-Host "Closed Otanabi.exe"
} else {
    Write-Host "Otanabi.exe is not running"
}
Start-Sleep -Seconds 3


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
# 5) Open Otanabi.exe in the current path
Write-Host "Opening Otanabi...."
$OtanabiPath = "$destinationPath\Otanabi.exe"

if (Test-Path $OtanabiPath) {
    Start-Process -FilePath $OtanabiPath
    Write-Host "Opened Otanabi.exe"
} else {
    Write-Host "Otanabi.exe not found in $destinationPath"
}

exit