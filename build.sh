#!/bin/sh

dotnet build -c Release -p VersionSuffix=dev ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out/build

dotnet_exec="./artifacts/out/build/dotnet-exec"

$dotnet_exec --info

echo "dotnet-exec ./build/build.cs --args $@"
$dotnet_exec ./build/build.cs --args "$@"
