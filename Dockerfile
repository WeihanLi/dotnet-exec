ARG RuntimeImageRepo=runtime

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build-env
ARG TARGETARCH
WORKDIR /app
COPY ./.editorconfig ./
COPY ./src/ ./src/
COPY ./build/ ./build/
COPY ./Directory.Build.props ./
COPY ./Directory.Build.targets ./
COPY ./Directory.Packages.props ./
WORKDIR /app/src/dotnet-exec/
ENV HUSKY=0
RUN dotnet publish -f net9.0 -a $TARGETARCH -o /app/out/

FROM mcr.microsoft.com/dotnet/${RuntimeImageRepo}:9.0-alpine AS final
ARG WebReferenceEnabled=false
LABEL Maintainer="WeihanLi"
LABEL Repository="https://github.com/WeihanLi/dotnet-exec"
ENV DOTNET_EXEC_WEB_REF_ENABLED=${WebReferenceEnabled}
WORKDIR /app
COPY --from=build-env /app/out/ ./
ENV PATH="/app:${PATH}"
RUN chmod +x /app/dotnet-exec
ENTRYPOINT [ "/app/dotnet-exec" ]
CMD [ "--help" ]
