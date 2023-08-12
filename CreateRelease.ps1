$destinationDir = "SFLoaderRelease"

if (!(Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir | Out-Null
}

dotnet build -p:Configuration=Release -p:Platform="Windows - x64"
cargo +nightly build --target x86_64-pc-windows-msvc --release

Copy-Item -Path "Output\Release\_SFLoader" -Destination $destinationDir -Recurse
Copy-Item -Path "BaseLibs\dobby_x64.dll" -Destination "$destinationDir\dobby.dll"
Copy-Item -Path "target\x86_64-pc-windows-msvc\release\version.dll" -Destination "$destinationDir\"
Copy-Item -Path "target\x86_64-pc-windows-msvc\release\Bootstrap.dll" -Destination "$destinationDir\_SFLoader\Dependencies\"

Set-Location -Path $destinationDir
Get-ChildItem -Path . | Compress-Archive -DestinationPath "..\$destinationDir.zip"
Set-Location -Path ..
Remove-Item -Path $destinationDir -Recurse -Force

Move-Item -Path "$destinationDir.zip" -Destination "SFLoader.zip" -Force

Write-Host "Process completed successfully."
