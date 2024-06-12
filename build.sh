#!/bin/sh

dotnet build -c Release -p VersionSuffix=dev ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out/build

./artifacts/out/build/dotnet-exec --info

echo "dotnet-exec ./build/build.cs --args $@"
./artifacts/out/dotnet-exec ./build/build.cs --args "$@"
