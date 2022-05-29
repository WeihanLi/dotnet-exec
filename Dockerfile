FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine AS base
LABEL Maintainer="WeihanLi"

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env

WORKDIR /app
COPY ./src/ ./src/
COPY ./build/ ./build/
COPY ./Directory.*.props ./
WORKDIR /app/src/dotnet-exec/
RUN dotnet publish -c Release --self-contained --use-current-runtime -p:AssemblyName=dotnet-exec -p:PublishSingleFile=true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true -o /app/artifacts

FROM base AS final
COPY --from=build-env /app/artifacts/dotnet-exec /root/.dotnet/tools/dotnet-exec
ENV PATH="/root/.dotnet/tools:${PATH}"