FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
WORKDIR /app
RUN dotnet tool install --global dotnet-execute 
ENV PATH="/app:${PATH}"
