FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app    
EXPOSE 443
EXPOSE 8081
EXPOSE 5025

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
WORKDIR "/src/SubsTracker.API"
RUN dotnet build "SubsTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SubsTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubsTracker.API.dll"]
