# work around for local push, should be removed when push package using CI
[System.Environment]::SetEnvironmentVariable('CI', 'true')

dotnet publish ./src/dotnet-exec/dotnet-exec.csproj -p VersionSuffix=dev -f net10.0 -o ./artifacts/dist -p UseAppHost=true

Get-ChildItem ./artifacts/dist

Set-Alias -Name dotnet_exec -Value ./artifacts/dist/dotnet-exec.exe

dotnet_exec info

dotnet_exec ./build/build.cs --args "$ARGS"
