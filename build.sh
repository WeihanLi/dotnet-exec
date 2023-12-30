#!/bin/sh

# Install dotnet-exec tool
dotnet tool update --global dotnet-execute --prerelease

# configure dotnet tool path
export PATH="$PATH:$HOME/.dotnet/tools"

echo "dotnet-exec ./build/build.cs --args $@"
dotnet-exec ./build/build.cs --args "$@"
