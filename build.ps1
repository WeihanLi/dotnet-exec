# work around for local push, should be removed when push package using CI
[System.Environment]::SetEnvironmentVariable('CI', 'true')

dotnet build -c Release ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out

# Execute CSharp script
Write-Host "dotnet-exec ./build/build.cs --args $ARGS" -ForegroundColor GREEN
./artifacts/out/dotnet-exec.exe ./build/build.cs --args "$ARGS"
