FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
WORKDIR /app    
EXPOSE 443
EXPOSE 8080
#EXPOSE 7089
#EXPOSE 5025

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug
ARG GITHUB_TOKEN
WORKDIR /src
COPY . .
WORKDIR "/src/SubsTracker.API"
RUN dotnet nuget add source https://nuget.pkg.github.com/androdenby/index.json -u androdenby -p ${GITHUB_TOKEN} --store-password-in-clear-text
RUN dotnet build "SubsTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=${BUILD_CONFIGURATION}
RUN dotnet publish "SubsTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubsTracker.API.dll"]
