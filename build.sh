#!/bin/sh

dotnet publish ./src/dotnet-exec/dotnet-exec.csproj -p VersionSuffix=dev -f net10.0 -o ./artifacts/dist -p UseAppHost=true

ls ./artifacts/dist

dotnet_exec="./artifacts/dist/dotnet-exec"

$dotnet_exec --info

echo "dotnet-exec ./build/build.cs --args $@"
$dotnet_exec ./build/build.cs --args "$@"
