$destinationDir = "SFLoaderRelease"

if (!(Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir | Out-Null
}

Copy-Item -Path "Output\Release\MelonLoader" -Destination $destinationDir -Recurse
Copy-Item -Path "BaseLibs\dobby_x64.dll" -Destination "$destinationDir\dobby.dll"
Copy-Item -Path "Prebuilt\version.dll" -Destination "$destinationDir\"
Copy-Item -Path "Prebuilt\Bootstrap.dll" -Destination "$destinationDir\MelonLoader\Dependencies\"

Set-Location -Path $destinationDir
Get-ChildItem -Path . | Compress-Archive -DestinationPath "..\$destinationDir.zip"
Set-Location -Path ..
Remove-Item -Path $destinationDir -Recurse -Force

Write-Host "Process completed successfully."
