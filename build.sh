#!/bin/sh

# Install cake.tool
dotnet tool install --global cake.tool
# Install dotnet-exec tool
dotnet tool update --global dotnet-execute --prerelease

export PATH="$PATH:$HOME/.dotnet/tools"

# Execute CSharp script
Write-Host "dotnet-exec ./build/build.cs --args $ARGS" -ForegroundColor GREEN
dotnet-exec ./build/build.cs --args '--target hello'

# Start Cake
echo "dotnet cake $SCRIPT $@"
dotnet cake $SCRIPT "$@"