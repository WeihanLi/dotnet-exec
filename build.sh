#!/bin/sh

# Install dotnet-exec tool
dotnet tool update --global dotnet-execute --prerelease

# configure dotnet tool path
export PATH="$PATH:$HOME/.dotnet/tools"

# Execute CSharp script
Write-Host "dotnet-exec ./build/build.cs --args $ARGS" -ForegroundColor GREEN
dotnet-exec ./build/build.cs --args "$ARGS"
