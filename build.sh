#!/bin/sh

dotnet publish ./src/dotnet-exec/dotnet-exec.csproj -p VersionSuffix=dev -f net10.0 -o ./artifacts/out/build -p UseAppHost=true

ls ./artifacts/out/build

dotnet_exec="./artifacts/out/build/dotnet-exec"

$dotnet_exec --info

echo "dotnet-exec ./build/build.cs --args $@"
$dotnet_exec ./build/build.cs --args "$@"
