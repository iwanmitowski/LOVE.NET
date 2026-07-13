# Build context is the repository root.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NuGet.config .
COPY src/Directory.Build.props src/
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Notifications/ src/Services/Notifications/
RUN dotnet publish src/Services/Notifications/LoveNet.Notifications.Api/LoveNet.Notifications.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# curl is used by the compose healthcheck.
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LoveNet.Notifications.Api.dll"]
