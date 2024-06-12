#!/bin/sh

dotnet build -c Release -p VersionSuffix=dev ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out/build

export d-exe = ./artifacts/out/build/dotnet-exec

d-exe --info

echo "dotnet-exec ./build/build.cs --args $@"
d-exe ./build/build.cs --args "$@"
