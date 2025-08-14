# work around for local push, should be removed when push package using CI
[System.Environment]::SetEnvironmentVariable('CI', 'true')

dotnet publish ./src/dotnet-exec/dotnet-exec.csproj -p VersionSuffix=dev -f net10.0 -o ./artifacts/tmp -p UseAppHost=true

Get-ChildItem ./artifacts/tmp

Set-Alias -Name dotnet_exec -Value ./artifacts/tmp/dotnet-exec.exe

dotnet_exec info

dotnet_exec ./build/build.cs --args "$ARGS"
