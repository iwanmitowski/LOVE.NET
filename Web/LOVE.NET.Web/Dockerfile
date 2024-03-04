#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Web/LOVE.NET.Web/LOVE.NET.Web.csproj", "Web/LOVE.NET.Web/"]
COPY ["LOVE.NET.Common/LOVE.NET.Common.csproj", "LOVE.NET.Common/"]
COPY ["Data/LOVE.NET.Data.Models/LOVE.NET.Data.Models.csproj", "Data/LOVE.NET.Data.Models/"]
COPY ["Services/LOVE.NET.Services.Mapping/LOVE.NET.Services.Mapping.csproj", "Services/LOVE.NET.Services.Mapping/"]
COPY ["Data/LOVE.NET.Data.Common/LOVE.NET.Data.Common.csproj", "Data/LOVE.NET.Data.Common/"]
COPY ["Data/LOVE.NET.Data/Files/*", "YourDestinationPathInDockerImage/"]
COPY ["Data/LOVE.NET.Data/LOVE.NET.Data.csproj", "Data/LOVE.NET.Data/"]
COPY ["Services/LOVE.NET.Services.Messaging/LOVE.NET.Services.Messaging.csproj", "Services/LOVE.NET.Services.Messaging/"]
COPY ["Services/LOVE.NET.Services/LOVE.NET.Services.csproj", "Services/LOVE.NET.Services/"]
COPY ["Web/LOVE.NET.Web.ViewModels/LOVE.NET.Web.ViewModels.csproj", "Web/LOVE.NET.Web.ViewModels/"]
COPY ["Web/LOVE.NET.Web.Infrastructure/LOVE.NET.Web.Infrastructure.csproj", "Web/LOVE.NET.Web.Infrastructure/"]
RUN dotnet restore "./Web/LOVE.NET.Web/LOVE.NET.Web.csproj"
COPY . .
WORKDIR "/src/Web/LOVE.NET.Web"
RUN dotnet build "./LOVE.NET.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LOVE.NET.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LOVE.NET.Web.dll"]