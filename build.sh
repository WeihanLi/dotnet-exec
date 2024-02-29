#!/bin/sh

dotnet build -c Release ./src/dotnet-exec/dotnet-exec.csproj -f net9.0 -o ./artifacts/out

./artifacts/out/dotnet-exec --info

echo "dotnet-exec ./build/build.cs --args $@"
./artifacts/out/dotnet-exec ./build/build.cs --args "$@"
