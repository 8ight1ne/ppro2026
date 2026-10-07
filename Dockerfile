FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Restore dependencies
COPY src/AkademieTrutnov.Core/AkademieTrutnov.Core.csproj src/AkademieTrutnov.Core/
COPY src/AkademieTrutnov.Infrastructure/AkademieTrutnov.Infrastructure.csproj src/AkademieTrutnov.Infrastructure/
COPY src/AkademieTrutnov.Web/AkademieTrutnov.Web.csproj src/AkademieTrutnov.Web/
RUN dotnet restore src/AkademieTrutnov.Web/AkademieTrutnov.Web.csproj

# Build and Publish
COPY src/ src/
WORKDIR /app/src/AkademieTrutnov.Web
RUN dotnet publish -c Release -o /out

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AkademieTrutnov.Web.dll"]
