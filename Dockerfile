ARG RuntimeImageRepo=runtime

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-preview-alpine AS build-env
ARG TARGETARCH
WORKDIR /app

COPY ./.editorconfig ./
COPY ./src/ ./src/
COPY ./build/ ./build/
COPY ./Directory.Build.props ./
COPY ./Directory.Build.targets ./
COPY ./Directory.Packages.props ./
COPY ./nuget.config ./

WORKDIR /app/src/dotnet-exec/
ENV HUSKY=0
RUN dotnet publish -f net10.0 -a $TARGETARCH -o /app/out/

FROM mcr.microsoft.com/dotnet/${RuntimeImageRepo}:10.0-preview-alpine AS final
ARG WebReferenceEnabled=false
# https://github.com/opencontainers/image-spec/blob/main/annotations.md
LABEL org.opencontainers.image.authors="WeihanLi"
LABEL org.opencontainers.image.source="https://github.com/WeihanLi/dotnet-exec"
LABEL org.opencontainers.image.title="dotnet-exec"
LABEL org.opencontainers.image.description="dotnet-exec, a command-line tool for executing C# program without a project file, and you can have your custom entry point other than the Main method"

ENV DOTNET_EXEC_WEB_REF_ENABLED=${WebReferenceEnabled}
WORKDIR /app
COPY --from=build-env /app/out/ ./
ENV PATH="/app:${PATH}"
RUN chmod +x /app/dotnet-exec
ENTRYPOINT [ "/app/dotnet-exec" ]
CMD [ "--help" ]
