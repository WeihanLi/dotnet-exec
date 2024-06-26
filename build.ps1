# work around for local push, should be removed when push package using CI
[System.Environment]::SetEnvironmentVariable('CI', 'true')

dotnet build -c Release -p VersionSuffix=dev ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out/build
Set-Alias -Name d_exe -Value ./artifacts/out/build/dotnet-exec.exe

d_exe --info

# Execute CSharp script
d_exe ./build/build.cs --args "$ARGS"
