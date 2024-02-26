FROM  --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:9.0-preview-alpine AS base
LABEL Maintainer="WeihanLi"

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview-alpine AS build-env
ARG TARGETARCH
WORKDIR /app
COPY ./src/ ./src/
COPY ./build/ ./build/
COPY ./Directory.Build.props ./
COPY ./Directory.Build.targets ./
COPY ./Directory.Packages.props ./
WORKDIR /app/src/dotnet-exec/
ENV HUSKY=0
RUN dotnet publish -f net9.0 -a $TARGETARCH -o /app/artifacts

FROM base AS final
WORKDIR /app
COPY --from=build-env /app/artifacts/ ./
ENV PATH="/app:${PATH}"
RUN chmod +x ./dotnet-exec
ENTRYPOINT [ "dotnet-exec" ]
CMD [ "--help" ]
