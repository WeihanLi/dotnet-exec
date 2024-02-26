ARG SdkImage=mcr.microsoft.com/dotnet/sdk:9.0-preview-alpine
ARG RuntimeImage=mcr.microsoft.com/dotnet/runtime:9.0-preview-alpine

FROM --platform=$BUILDPLATFORM $SdkImage AS build-env
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

FROM --platform=$BUILDPLATFORM $RuntimeImage AS final
LABEL Maintainer="WeihanLi"
LABEL Repository="https://github.com/WeihanLi/dotnet-exec"
WORKDIR /app
COPY --from=build-env /app/artifacts/ ./
ENV PATH="/app:${PATH}"
RUN chmod +x ./dotnet-exec
ENTRYPOINT [ "dotnet-exec" ]
CMD [ "--help" ]
