ARG RuntimeImageRepo=runtime-deps

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-preview-alpine AS build-env
ARG TARGETARCH
WORKDIR /app
COPY ./src/ ./src/
COPY ./build/ ./build/
COPY ./Directory.Build.props ./
COPY ./Directory.Build.targets ./
COPY ./Directory.Packages.props ./
WORKDIR /app/src/dotnet-exec/
ENV HUSKY=0
RUN dotnet publish -f net9.0 -a $TARGETARCH -o /app/out/

FROM mcr.microsoft.com/dotnet/${RuntimeImageRepo}:9.0-preview-alpine AS final
LABEL Maintainer="WeihanLi"
LABEL Repository="https://github.com/WeihanLi/dotnet-exec"
WORKDIR /app
COPY --from=build-env /app/out/ ./
ENV PATH="/app:${PATH}"
RUN chmod +x /app/dotnet-exec
ENTRYPOINT [ "/app/dotnet-exec" ]
CMD [ "--help" ]
