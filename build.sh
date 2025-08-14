#!/bin/sh

dotnet publish ./src/dotnet-exec/dotnet-exec.csproj -p VersionSuffix=dev -f net10.0 -o ./artifacts/tmp -p UseAppHost=true

ls ./artifacts/tmp

dotnet_exec="./artifacts/tmp/dotnet-exec"

$dotnet_exec --info

echo "dotnet-exec ./build/build.cs --args $@"
$dotnet_exec ./build/build.cs --args "$@"
