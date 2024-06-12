# work around for local push, should be removed when push package using CI
[System.Environment]::SetEnvironmentVariable('CI', 'true')

dotnet build -c Release -p VersionSuffix=dev ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out/build
./artifacts/out/build/dotnet-exec --info

# Execute CSharp script
./artifacts/out/dotnet-exec.exe ./build/build.cs --args "$ARGS"
