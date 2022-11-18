FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
LABEL Maintainer="WeihanLi"

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env

WORKDIR /app
COPY ./src/ ./src/
COPY ./build/ ./build/
COPY ./Directory.Build.props ./
COPY ./Directory.Build.targets ./
COPY ./Directory.Packages.props ./
WORKDIR /app/src/dotnet-exec/
RUN dotnet publish -f net7.0 -c Release -o /app/artifacts

FROM base AS final
WORKDIR /app
COPY --from=build-env /app/artifacts/ ./
ENV PATH="/app:${PATH}"
