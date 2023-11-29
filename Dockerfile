ARG DOTNET_TARGET

# always use SDK 6
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS generator
WORKDIR /build
COPY Generator/Generator.csproj Generator.csproj
COPY Generator/Program.cs Program.cs
COPY Directory.Build.props Directory.Build.props
RUN dotnet build
RUN dotnet run --no-build > signed.xml 2> key.pem

# builds a self-contained app
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_TARGET} AS verifier
ARG CONFIGURATION=Release
ARG NUGET_VERSION
ARG DOTNET_TARGET
WORKDIR /build
COPY Verifier/Verifier.csproj Verifier.csproj
COPY Directory.Build.props Directory.Build.props
ARG DOTNET_CLI_PROPS="-p:TargetFrameworks=net${DOTNET_TARGET} -p:NugetVersion=${NUGET_VERSION} --runtime linux-x64"
RUN dotnet restore $DOTNET_CLI_PROPS
COPY Verifier/Program.cs Program.cs
RUN dotnet publish --no-restore --configuration ${CONFIGURATION} ${DOTNET_CLI_PROPS} --framework net${DOTNET_TARGET} --output /app

FROM ubuntu:jammy
ARG NUGET_VERSION
ENV XMLSEC_VERFIY_OPTS ""
ENV DEBIAN_FRONTEND=noninteractive
ENV NUGET_VERSION=${NUGET_VERSION}
RUN apt update && apt install -y xmlsec1 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=generator /build/signed.xml signed.xml
COPY --from=generator /build/key.pem key.pem
COPY --from=verifier /app .
CMD xmlsec1 verify $XMLSEC_VERFIY_OPTS signed.xml && /app/Verifier signed.xml key.pem