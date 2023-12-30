# work around for local push, should be removed when push package using CI
[System.Environment]::SetEnvironmentVariable('CI', 'true')

# Install the lastest dotnet-execute tool
dotnet tool update --global dotnet-execute --prerelease

# Execute CSharp script
Write-Host "dotnet-exec ./build/build.cs --args $ARGS" -ForegroundColor GREEN
dotnet-exec ./build/build.cs --args "$ARGS"
