# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs


FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ui/PDFDict.SDK.Sharp.Web/PDFDict.SDK.Sharp.Web.Server/PDFDict.SDK.Sharp.Web.Server.csproj", "ui/PDFDict.SDK.Sharp.Web/PDFDict.SDK.Sharp.Web.Server/"]
COPY ["pdfdict.sdk.sharp.web.client/pdfdict.sdk.sharp.web.client.esproj", "pdfdict.sdk.sharp.web.client/"]
RUN dotnet restore "./ui/PDFDict.SDK.Sharp.Web/PDFDict.SDK.Sharp.Web.Server/PDFDict.SDK.Sharp.Web.Server.csproj"
COPY . .
WORKDIR "/src/ui/PDFDict.SDK.Sharp.Web/PDFDict.SDK.Sharp.Web.Server"
RUN dotnet build "./PDFDict.SDK.Sharp.Web.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PDFDict.SDK.Sharp.Web.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PDFDict.SDK.Sharp.Web.Server.dll"]
